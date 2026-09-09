using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDLP_FIRST_API.Models
{
    [Table("KycRecords")]
    public class KycRecord
    {
        [Key]
        [StringLength(128)]
        public string KycId { get; set; }

        [Required]
        [StringLength(128)]
        public string CustomerId { get; set; }

        [ForeignKey("CustomerId")]
        public virtual CustomerProfile Customer { get; set; }

        [Required]
        [StringLength(50)]
        public string NationalId { get; set; }

        [StringLength(50)]
        public string KraPin { get; set; }

        [StringLength(50)]
        public string PassportNumber { get; set; }

        [Required]
        [StringLength(30)]
        public string KycStatus { get; set; } // PENDING, VERIFIED, REJECTED, EXPIRED

        public DateTime? VerificationDate { get; set; }

        [StringLength(250)]
        public string RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }
    }
}
