using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using NDLP_FIRST_API.Exceptions;
using NDLP_FIRST_API.Models;
using NDLP_FIRST_API.Models.DTOs;

namespace NDLP_FIRST_API.Services
{
    public interface IDeviceService
    {
        Task<IEnumerable<RegisteredDeviceResponseDto>> GetCustomerDevicesAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false);
        Task<RegisteredDeviceResponseDto> RegisterDeviceAsync(string customerId, RegisterDeviceRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false);
        Task<RegisteredDeviceResponseDto> UpdateDeviceAsync(string deviceId, UpdateDeviceRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false);
        Task<RegisteredDeviceResponseDto> DeactivateDeviceAsync(string deviceId, DeactivateDeviceRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false);
    }

    public class DeviceService : IDeviceService
    {
        private const int MAX_ACTIVE_DEVICES_PER_CUSTOMER = 3;
        private readonly ApplicationDbContext _db;
        private readonly ICustomerService _customerService;
        private readonly IAuditService _auditService;

        public DeviceService(ApplicationDbContext db, ICustomerService customerService, IAuditService auditService)
        {
            _db = db;
            _customerService = customerService;
            _auditService = auditService;
        }

        public async Task<IEnumerable<RegisteredDeviceResponseDto>> GetCustomerDevicesAsync(string customerId, string requestingUserId, bool isAdminOrOfficer = false)
        {
            await _customerService.ValidateProfileOwnershipAsync(customerId, requestingUserId, isAdminOrOfficer);

            var devices = await _db.RegisteredDevices
                .Where(d => d.CustomerId == customerId)
                .OrderByDescending(d => d.RegisteredAt)
                .ToListAsync();

            return devices.Select(MapToResponseDto).ToList();
        }

        public async Task<RegisteredDeviceResponseDto> RegisterDeviceAsync(string customerId, RegisterDeviceRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            await _customerService.ValidateProfileOwnershipAsync(customerId, requestingUserId, isAdminOrOfficer);

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Check duplicate device identifier for customer
                    var existingDevice = await _db.RegisteredDevices
                        .FirstOrDefaultAsync(d => d.CustomerId == customerId && d.DeviceIdentifier == requestDto.DeviceIdentifier);

                    if (existingDevice != null && existingDevice.IsActive)
                    {
                        throw new DuplicateDeviceException(requestDto.DeviceIdentifier);
                    }

                    // 2. Check active device limit
                    var activeDeviceCount = await _db.RegisteredDevices
                        .CountAsync(d => d.CustomerId == customerId && d.IsActive);

                    if (activeDeviceCount >= MAX_ACTIVE_DEVICES_PER_CUSTOMER)
                    {
                        throw new DeviceLimitExceededException(MAX_ACTIVE_DEVICES_PER_CUSTOMER);
                    }

                    RegisteredDevice device;
                    if (existingDevice != null)
                    {
                        // Reactivate existing inactive device
                        existingDevice.IsActive = true;
                        existingDevice.DeviceName = requestDto.DeviceName.Trim();
                        existingDevice.DeviceType = requestDto.DeviceType.Trim();
                        existingDevice.OperatingSystem = requestDto.OperatingSystem?.Trim();
                        existingDevice.IsTrusted = requestDto.IsTrusted;
                        existingDevice.LastActiveAt = DateTime.UtcNow;
                        existingDevice.DeactivatedAt = null;
                        existingDevice.DeactivatedBy = null;
                        device = existingDevice;
                    }
                    else
                    {
                        device = new RegisteredDevice
                        {
                            DeviceId = Guid.NewGuid().ToString(),
                            CustomerId = customerId,
                            DeviceIdentifier = requestDto.DeviceIdentifier.Trim(),
                            DeviceName = requestDto.DeviceName.Trim(),
                            DeviceType = requestDto.DeviceType.Trim(),
                            OperatingSystem = requestDto.OperatingSystem?.Trim(),
                            IsTrusted = requestDto.IsTrusted,
                            IsActive = true,
                            RegisteredAt = DateTime.UtcNow,
                            LastActiveAt = DateTime.UtcNow
                        };
                        _db.RegisteredDevices.Add(device);
                    }

                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    await _auditService.LogAuditAsync("REGISTER_DEVICE", "RegisteredDevice", device.DeviceId, customerId, requestingUserId, null, new { device.DeviceId, device.DeviceIdentifier, device.DeviceName, device.IsTrusted });

                    return MapToResponseDto(device);
                }
                catch (DbUpdateException dbEx)
                {
                    transaction.Rollback();
                    if (dbEx.InnerException != null && dbEx.InnerException.Message.Contains("IX_") || dbEx.Message.Contains("unique"))
                    {
                        throw new DuplicateDeviceException(requestDto.DeviceIdentifier);
                    }
                    throw;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<RegisteredDeviceResponseDto> UpdateDeviceAsync(string deviceId, UpdateDeviceRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            var device = await _db.RegisteredDevices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (device == null)
                throw new EntityNotFoundException("Registered Device", deviceId);

            await _customerService.ValidateProfileOwnershipAsync(device.CustomerId, requestingUserId, isAdminOrOfficer);

            if (!device.IsActive)
                throw new BusinessException("DEVICE_INACTIVE", "Cannot update an inactive device. Please re-register the device first.");

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var oldValues = new { device.DeviceName, device.OperatingSystem, device.IsTrusted };

                    device.DeviceName = requestDto.DeviceName.Trim();
                    device.OperatingSystem = requestDto.OperatingSystem?.Trim();
                    device.IsTrusted = requestDto.IsTrusted;
                    device.LastActiveAt = DateTime.UtcNow;

                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    var newValues = new { device.DeviceName, device.OperatingSystem, device.IsTrusted };
                    await _auditService.LogAuditAsync("UPDATE_DEVICE", "RegisteredDevice", device.DeviceId, device.CustomerId, requestingUserId, oldValues, newValues);

                    return MapToResponseDto(device);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public async Task<RegisteredDeviceResponseDto> DeactivateDeviceAsync(string deviceId, DeactivateDeviceRequestDto requestDto, string requestingUserId, bool isAdminOrOfficer = false)
        {
            var device = await _db.RegisteredDevices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
            if (device == null)
                throw new EntityNotFoundException("Registered Device", deviceId);

            await _customerService.ValidateProfileOwnershipAsync(device.CustomerId, requestingUserId, isAdminOrOfficer);

            if (!device.IsActive)
                throw new BusinessException("DEVICE_ALREADY_INACTIVE", "The requested device is already inactive.");

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var oldValues = new { device.IsActive, device.DeactivatedAt, device.DeactivatedBy };

                    device.IsActive = false;
                    device.DeactivatedAt = DateTime.UtcNow;
                    device.DeactivatedBy = requestingUserId;

                    await _db.SaveChangesAsync();
                    transaction.Commit();

                    var newValues = new { device.IsActive, device.DeactivatedAt, device.DeactivatedBy, Reason = requestDto?.Reason };
                    await _auditService.LogAuditAsync("DEACTIVATE_DEVICE", "RegisteredDevice", device.DeviceId, device.CustomerId, requestingUserId, oldValues, newValues);

                    return MapToResponseDto(device);
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private static RegisteredDeviceResponseDto MapToResponseDto(RegisteredDevice device)
        {
            if (device == null) return null;

            return new RegisteredDeviceResponseDto
            {
                DeviceId = device.DeviceId,
                CustomerId = device.CustomerId,
                DeviceIdentifier = device.DeviceIdentifier,
                DeviceName = device.DeviceName,
                DeviceType = device.DeviceType,
                OperatingSystem = device.OperatingSystem,
                IsTrusted = device.IsTrusted,
                IsActive = device.IsActive,
                RegisteredAt = device.RegisteredAt,
                LastActiveAt = device.LastActiveAt,
                DeactivatedAt = device.DeactivatedAt,
                DeactivatedBy = device.DeactivatedBy
            };
        }
    }
}
