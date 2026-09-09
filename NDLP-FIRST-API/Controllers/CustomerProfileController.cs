using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using NDLP_FIRST_API.Models;
using NDLP_FIRST_API.Models.DTOs;
using NDLP_FIRST_API.Services;

namespace NDLP_FIRST_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1/customers")]
    public class CustomerProfileController : ApiController
    {
        private readonly ICustomerService _customerService;

        public CustomerProfileController()
        {
            var db = ApplicationDbContext.Create();
            var auditService = new AuditService(db);
            _customerService = new CustomerService(db, auditService);
        }

        public CustomerProfileController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET api/v1/customers/{customerId}
        [HttpGet]
        [Route("{customerId}")]
        public async Task<IHttpActionResult> GetProfile(string customerId)
        {
            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var profile = await _customerService.GetProfileByIdAsync(customerId, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<CustomerProfileResponseDto>.SuccessResponse(profile, "Customer profile retrieved successfully."));
        }

        // GET api/v1/customers
        [HttpGet]
        [Route("")]
        public async Task<IHttpActionResult> SearchProfiles([FromUri] CustomerSearchRequestDto searchDto)
        {
            searchDto = searchDto ?? new CustomerSearchRequestDto();
            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var result = await _customerService.SearchProfilesAsync(searchDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<PagedResponseDto<CustomerProfileResponseDto>>.SuccessResponse(result, "Customer profiles retrieved successfully."));
        }

        // POST api/v1/customers/{customerId}/update
        [HttpPost]
        [Route("{customerId}/update")]
        public async Task<IHttpActionResult> UpdateProfile(string customerId, [FromBody] UpdateCustomerProfileRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var updatedProfile = await _customerService.UpdateProfileAsync(customerId, requestDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<CustomerProfileResponseDto>.SuccessResponse(updatedProfile, "Customer profile updated successfully."));
        }
    }
}
