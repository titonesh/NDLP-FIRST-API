using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using NDLP_FIRST_API.Exceptions;
using NDLP_FIRST_API.Models;
using NDLP_FIRST_API.Models.DTOs;

namespace NDLP_FIRST_API.Services
{
    public interface ICustomerService
    {
        Task<CustomerProfileResponseDto> GetProfileByIdAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false);
        Task<PagedResponseDto<CustomerProfileResponseDto>> SearchProfilesAsync(CustomerSearchRequestDto searchDto, string requestingUserId, bool isAdminOrOfficer = false);
        Task<CustomerProfileResponseDto> UpdateProfileAsync(string customerId, UpdateCustomerProfileRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false);
        Task ValidateProfileOwnershipAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditService _auditService;

        public CustomerService(ApplicationDbContext db, IAuditService auditService)
        {
            _db = db;
            _auditService = auditService;
        }

        public async Task ValidateProfileOwnershipAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new BusinessException("INVALID_CUSTOMER_ID", "Customer identifier is required.");

            var customer = await _db.CustomerProfiles.FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
                throw new EntityNotFoundException("Customer Profile", customerId);

            if (!isAdminOrOfficer && customer.UserId != requestingUserId)
            {
                throw new UnauthorizedAccessDomainException("You do not have access to this customer profile.");
            }
        }

        public async Task<CustomerProfileResponseDto> GetProfileByIdAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false)
        {
            await ValidateProfileOwnershipAsync(customerId, requestingUserId, isAdminOrOfficer);

            var customer = await _db.CustomerProfiles
                .Include(c => c.KycRecord)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            return MapToResponseDto(customer);
        }

        public async Task<PagedResponseDto<CustomerProfileResponseDto>> SearchProfilesAsync(CustomerSearchRequestDto searchDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            var pageNumber = searchDto.PageNumber <= 0 ? 1 : searchDto.PageNumber;
            var pageSize = searchDto.PageSize <= 0 ? 10 : Math.Min(searchDto.PageSize, 100); // Cap at 100 max page size

            IQueryable<CustomerProfile> query = _db.CustomerProfiles.Include(c => c.KycRecord);

            // Scope filter if not admin/officer
            if (!isAdminOrOfficer)
            {
                query = query.Where(c => c.UserId == requestingUserId);
            }

            if (!string.IsNullOrWhiteSpace(searchDto.Status))
            {
                query = query.Where(c => c.Status.Equals(searchDto.Status, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
            {
                var term = searchDto.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.CustomerId.ToLower().Contains(term) ||
                                         c.FirstName.ToLower().Contains(term) ||
                                         c.LastName.ToLower().Contains(term) ||
                                         c.Email.ToLower().Contains(term) ||
                                         c.PhoneNumber.Contains(term));
            }

            var totalRecords = await query.CountAsync();
            var items = await query
                .OrderBy(c => c.CustomerId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Select(MapToResponseDto).ToList();
            return new PagedResponseDto<CustomerProfileResponseDto>(dtos, pageNumber, pageSize, totalRecords);
        }

        public async Task<CustomerProfileResponseDto> UpdateProfileAsync(string customerId, UpdateCustomerProfileRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            await ValidateProfileOwnershipAsync(customerId, requestingUserId, isAdminOrOfficer);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var customer = await _db.CustomerProfiles
                        .Include(c => c.KycRecord)
                        .FirstOrDefaultAsync(c => c.CustomerId == customerId);

                    if (customer == null)
                        throw new EntityNotFoundException("Customer Profile", customerId);

                    // KYC status restriction rule
                    if (customer.KycRecord != null && (customer.KycRecord.KycStatus == "REJECTED" || customer.KycRecord.KycStatus == "BLOCKED"))
                    {
                        throw new KycRestrictionException($"Profile update is not permitted when KYC status is '{customer.KycRecord.KycStatus}'.");
                    }

                    var oldValues = new
                    {
                        customer.FirstName,
                        customer.LastName,
                        customer.Email,
                        customer.PhoneNumber,
                        customer.Address
                    };

                    // Update allow-listed fields explicitly
                    customer.FirstName = requestDto.FirstName.Trim();
                    customer.LastName = requestDto.LastName.Trim();
                    customer.Email = requestDto.Email.Trim();
                    customer.PhoneNumber = requestDto.PhoneNumber.Trim();
                    customer.Address = requestDto.Address?.Trim();
                    customer.LastUpdatedAt = DateTime.UtcNow;

                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    var newValues = new
                    {
                        customer.FirstName,
                        customer.LastName,
                        customer.Email,
                        customer.PhoneNumber,
                        customer.Address
                    };

                    await _auditService.LogAuditAsync("UPDATE_PROFILE", "CustomerProfile", customer.CustomerId, customer.CustomerId, requestingUserId, oldValues, newValues);

                    return MapToResponseDto(customer);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static CustomerProfileResponseDto MapToResponseDto(CustomerProfile customer)
        {
            if (customer == null) return null;

            return new CustomerProfileResponseDto
            {
                CustomerId = customer.CustomerId,
                UserId = customer.UserId,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                Address = customer.Address,
                Status = customer.Status,
                CreatedAt = customer.CreatedAt,
                LastUpdatedAt = customer.LastUpdatedAt,
                KycSummary = customer.KycRecord == null ? null : new KycResponseDto
                {
                    KycId = customer.KycRecord.KycId,
                    CustomerId = customer.KycRecord.CustomerId,
                    MaskedNationalId = MaskString(customer.KycRecord.NationalId),
                    MaskedKraPin = MaskString(customer.KycRecord.KraPin),
                    MaskedPassportNumber = MaskString(customer.KycRecord.PassportNumber),
                    KycStatus = customer.KycRecord.KycStatus,
                    VerificationDate = customer.KycRecord.VerificationDate,
                    RejectionReason = customer.KycRecord.RejectionReason,
                    CreatedAt = customer.KycRecord.CreatedAt,
                    LastUpdatedAt = customer.KycRecord.LastUpdatedAt
                }
            };
        }

        private static string MaskString(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length <= 4) return new string('*', value.Length);
            return new string('*', value.Length - 4) + value.Substring(value.Length - 4);
        }
    }
}
