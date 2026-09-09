using System;
using System.Data.Entity;
using System.Threading.Tasks;
using NDLP_FIRST_API.Exceptions;
using NDLP_FIRST_API.Models;
using NDLP_FIRST_API.Models.DTOs;

namespace NDLP_FIRST_API.Services
{
    public interface IKycService
    {
        Task<KycResponseDto> GetKycByCustomerAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false);
        Task<KycResponseDto> UpdateKycAsync(string customerId, UpdateKycRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false);
        Task<KycResponseDto> ValidateKycAsync(string customerId, ValidateKycRequestDto validateDto, string requestingUserId, bool isAdminOrOfficer = false);
    }

    public class KycService : IKycService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICustomerService _customerService;
        private readonly IAuditService _auditService;

        public KycService(ApplicationDbContext db, ICustomerService customerService, IAuditService auditService)
        {
            _db = db;
            _customerService = customerService;
            _auditService = auditService;
        }

        public async Task<KycResponseDto> GetKycByCustomerAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false)
        {
            await _customerService.ValidateProfileOwnershipAsync(customerId, requestingUserId, isAdminOrOfficer);

            var kyc = await _db.KycRecords.FirstOrDefaultAsync(k => k.CustomerId == customerId);
            if (kyc == null)
                throw new EntityNotFoundException("KYC Record", customerId);

            return MapToResponseDto(kyc);
        }

        public async Task<KycResponseDto> UpdateKycAsync(string customerId, UpdateKycRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            await _customerService.ValidateProfileOwnershipAsync(customerId, requestingUserId, isAdminOrOfficer);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var kyc = await _db.KycRecords.FirstOrDefaultAsync(k => k.CustomerId == customerId);
                    object oldValues = kyc != null ? new { kyc.NationalId, kyc.KraPin, kyc.PassportNumber, kyc.KycStatus } : null;

                    if (kyc == null)
                    {
                        kyc = new KycRecord
                        {
                            KycId = Guid.NewGuid().ToString(),
                            CustomerId = customerId,
                            NationalId = requestDto.NationalId.Trim(),
                            KraPin = requestDto.KraPin?.Trim(),
                            PassportNumber = requestDto.PassportNumber?.Trim(),
                            KycStatus = "PENDING",
                            CreatedAt = DateTime.UtcNow,
                            LastUpdatedAt = DateTime.UtcNow
                        };
                        _db.KycRecords.Add(kyc);
                    }
                    else
                    {
                        if (kyc.KycStatus == "VERIFIED" && !isAdminOrOfficer)
                        {
                            throw new KycRestrictionException("Verified KYC records cannot be modified without administrative authorization.");
                        }

                        kyc.NationalId = requestDto.NationalId.Trim();
                        kyc.KraPin = requestDto.KraPin?.Trim();
                        kyc.PassportNumber = requestDto.PassportNumber?.Trim();
                        kyc.KycStatus = "PENDING"; // Reset to PENDING for re-verification upon update
                        kyc.LastUpdatedAt = DateTime.UtcNow;
                    }

                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    var newValues = new { kyc.NationalId, kyc.KraPin, kyc.PassportNumber, kyc.KycStatus };
                    await _auditService.LogAuditAsync("UPDATE_KYC", "KycRecord", kyc.KycId, customerId, requestingUserId, oldValues, newValues);

                    return MapToResponseDto(kyc);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<KycResponseDto> ValidateKycAsync(string customerId, ValidateKycRequestDto validateDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            if (!isAdminOrOfficer)
                throw new UnauthorizedAccessDomainException("Only authorized officers can validate KYC records.");

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var kyc = await _db.KycRecords.FirstOrDefaultAsync(k => k.CustomerId == customerId);
                    if (kyc == null)
                        throw new EntityNotFoundException("KYC Record", customerId);

                    var targetStatus = validateDto.Status.ToUpper().Trim();
                    if (targetStatus != "VERIFIED" && targetStatus != "REJECTED")
                    {
                        throw new BusinessException("INVALID_KYC_STATUS", "KYC validation status must be either 'VERIFIED' or 'REJECTED'.");
                    }

                    var oldValues = new { kyc.KycStatus, kyc.VerificationDate, kyc.RejectionReason };

                    kyc.KycStatus = targetStatus;
                    kyc.LastUpdatedAt = DateTime.UtcNow;
                    if (targetStatus == "VERIFIED")
                    {
                        kyc.VerificationDate = DateTime.UtcNow;
                        kyc.RejectionReason = null;
                    }
                    else
                    {
                        kyc.RejectionReason = validateDto.RejectionReason;
                    }

                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    var newValues = new { kyc.KycStatus, kyc.VerificationDate, kyc.RejectionReason };
                    await _auditService.LogAuditAsync("VALIDATE_KYC", "KycRecord", kyc.KycId, customerId, requestingUserId, oldValues, newValues);

                    return MapToResponseDto(kyc);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static KycResponseDto MapToResponseDto(KycRecord kyc)
        {
            if (kyc == null) return null;

            return new KycResponseDto
            {
                KycId = kyc.KycId,
                CustomerId = kyc.CustomerId,
                MaskedNationalId = MaskString(kyc.NationalId),
                MaskedKraPin = MaskString(kyc.KraPin),
                MaskedPassportNumber = MaskString(kyc.PassportNumber),
                KycStatus = kyc.KycStatus,
                VerificationDate = kyc.VerificationDate,
                RejectionReason = kyc.RejectionReason,
                CreatedAt = kyc.CreatedAt,
                LastUpdatedAt = kyc.LastUpdatedAt
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
