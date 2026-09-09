using System;
using System.ComponentModel.DataAnnotations;

namespace NDLP_FIRST_API.Models.DTOs
{
    public class RegisteredDeviceResponseDto
    {
        public string DeviceId { get; set; }
        public string CustomerId { get; set; }
        public string DeviceIdentifier { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string OperatingSystem { get; set; }
        public bool IsTrusted { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime LastActiveAt { get; set; }
        public DateTime? DeactivatedAt { get; set; }
        public string DeactivatedBy { get; set; }
    }

    public class RegisterDeviceRequestDto
    {
        [Required]
        [StringLength(128)]
        public string DeviceIdentifier { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceType { get; set; }

        [StringLength(50)]
        public string OperatingSystem { get; set; }

        public bool IsTrusted { get; set; }
    }

    public class UpdateDeviceRequestDto
    {
        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; }

        [StringLength(50)]
        public string OperatingSystem { get; set; }

        public bool IsTrusted { get; set; }
    }

    public class DeactivateDeviceRequestDto
    {
        [StringLength(250)]
        public string Reason { get; set; }
    }
}
