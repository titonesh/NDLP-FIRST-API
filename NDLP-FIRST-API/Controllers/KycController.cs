using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using NDLP_FIRST_API.Models;
using NDLP_FIRST_API.Models.DTOs;
using NDLP_FIRST_API.Services;

namespace NDLP_FIRST_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1/customers/{customerId}/kyc")]
    public class KycController : ApiController
    {
        private readonly IKycService _kycService;

        public KycController()
        {
            var db = ApplicationDbContext.Create();
            var auditService = new AuditService(db);
            var customerService = new CustomerService(db, auditService);
            _kycService = new KycService(db, customerService, auditService);
        }

        public KycController(IKycService kycService)
        {
            _kycService = kycService;
        }

        // GET api/v1/customers/{customerId}/kyc
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> GetKyc(string customerId)
        {
            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var kyc = await _kycService.GetKycByCustomerAsync(customerId, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<KycResponseDto>.SuccessResponse(kyc, "KYC record retrieved successfully."));
        }

        // POST api/v1/customers/{customerId}/kyc
        [HttpPost]
        [Route("")]
        public async Task<IHttpActionResult> UpdateKyc(string customerId, [FromBody] UpdateKycRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var updatedKyc = await _kycService.UpdateKycAsync(customerId, requestDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<KycResponseDto>.SuccessResponse(updatedKyc, "KYC record updated successfully."));
        }

        // POST api/v1/customers/{customerId}/kyc/validate
        [HttpPost]
        [Route("validate")]
        public async Task<IHttpActionResult> ValidateKyc(string customerId, [FromBody] ValidateKycRequestDto validateDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var validatedKyc = await _kycService.ValidateKycAsync(customerId, validateDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<KycResponseDto>.SuccessResponse(validatedKyc, "KYC record validated successfully."));
        }
    }
}
