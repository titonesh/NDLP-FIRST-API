using System;
using System.ComponentModel.DataAnnotations;

namespace NDLP_FIRST_API.Models.DTOs
{
    public class KycResponseDto
    {
        public string KycId { get; set; }
        public string CustomerId { get; set; }
        public string MaskedNationalId { get; set; }
        public string MaskedKraPin { get; set; }
        public string MaskedPassportNumber { get; set; }
        public string KycStatus { get; set; }
        public DateTime? VerificationDate { get; set; }
        public string RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
    }

    public class UpdateKycRequestDto
    {
        [Required]
        [StringLength(50)]
        public string NationalId { get; set; }

        [StringLength(50)]
        public string KraPin { get; set; }

        [StringLength(50)]
        public string PassportNumber { get; set; }
    }

    public class ValidateKycRequestDto
    {
        [Required]
        [StringLength(30)]
        public string Status { get; set; } // VERIFIED, REJECTED

        [StringLength(250)]
        public string RejectionReason { get; set; }
    }
}
