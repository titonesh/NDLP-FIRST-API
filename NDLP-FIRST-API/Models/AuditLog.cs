using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDLP_FIRST_API.Models
{
    [Table("AuditLogs")]
    public class AuditLog
    {
        [Key]
        [StringLength(128)]
        public string AuditId { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; }

        [StringLength(128)]
        public string EntityId { get; set; }

        [StringLength(128)]
        public string CustomerId { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }

        public DateTime Timestamp { get; set; }

        public string OldValues { get; set; }

        public string NewValues { get; set; }

        [StringLength(128)]
        public string CorrelationId { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        [StringLength(20)]
        public string Status { get; set; } // SUCCESS, FAILED
    }
}
