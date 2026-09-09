using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDLP_FIRST_API.Models
{
    [Table("RegisteredDevices")]
    public class RegisteredDevice
    {
        [Key]
        [StringLength(128)]
        public string DeviceId { get; set; }

        [Required]
        [StringLength(128)]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual CustomerProfile Customer { get; set; }

        [Required]
        [StringLength(128)]
        public string DeviceIdentifier { get; set; }

        [Required]
        [StringLength(100)]
        public string DeviceName { get; set; }

        [Required]
        [StringLength(50)]
        public string DeviceType { get; set; } // Mobile, Tablet, Web, Desktop

        [StringLength(50)]
        public string OperatingSystem { get; set; }

        public bool IsTrusted { get; set; }

        public bool IsActive { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime LastActiveAt { get; set; }

        public DateTime? DeactivatedAt { get; set; }

        [StringLength(128)]
        public string DeactivatedBy { get; set; }
    }
}
