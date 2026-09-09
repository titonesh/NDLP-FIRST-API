using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using NDLP_FIRST_API.Models;
using NDLP_FIRST_API.Models.DTOs;
using NDLP_FIRST_API.Services;

namespace NDLP_FIRST_API.Controllers
{
    [Authorize]
    [RoutePrefix("api/v1")]
    public class DeviceController : ApiController
    {
        private readonly IDeviceService _deviceService;

        public DeviceController()
        {
            var db = ApplicationDbContext.Create();
            var auditService = new AuditService(db);
            var customerService = new CustomerService(db, auditService);
            _deviceService = new DeviceService(db, customerService, auditService);
        }

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        // GET api/v1/customers/{customerId}/devices
        [HttpGet]
        [Route("customers/{customerId}/devices")]
        public async Task<IHttpActionResult> GetCustomerDevices(string customerId)
        {
            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var devices = await _deviceService.GetCustomerDevicesAsync(customerId, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<IEnumerable<RegisteredDeviceResponseDto>>.SuccessResponse(devices, "Customer devices retrieved successfully."));
        }

        // POST api/v1/customers/{customerId}/devices
        [HttpPost]
        [Route("customers/{customerId}/devices")]
        public async Task<IHttpActionResult> RegisterDevice(string customerId, [FromBody] RegisterDeviceRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var device = await _deviceService.RegisterDeviceAsync(customerId, requestDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<RegisteredDeviceResponseDto>.SuccessResponse(device, "Device registered successfully."));
        }

        // POST api/v1/devices/{deviceId}/update
        [HttpPost]
        [Route("devices/{deviceId}/update")]
        public async Task<IHttpActionResult> UpdateDevice(string deviceId, [FromBody] UpdateDeviceRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var updatedDevice = await _deviceService.UpdateDeviceAsync(deviceId, requestDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<RegisteredDeviceResponseDto>.SuccessResponse(updatedDevice, "Device updated successfully."));
        }

        // POST api/v1/devices/{deviceId}/deactivate
        [HttpPost]
        [Route("devices/{deviceId}/deactivate")]
        public async Task<IHttpActionResult> DeactivateDevice(string deviceId, [FromBody] DeactivateDeviceRequestDto requestDto)
        {
            var requestingUserId = User.Identity.GetUserId();
            var isAdminOrOfficer = User.IsInRole("Admin") || User.IsInRole("Officer");

            var deactivatedDevice = await _deviceService.DeactivateDeviceAsync(deviceId, requestDto, requestingUserId, isAdminOrOfficer);
            return Ok(ApiResponse<RegisteredDeviceResponseDto>.SuccessResponse(deactivatedDevice, "Device deactivated successfully."));
        }
    }
}
