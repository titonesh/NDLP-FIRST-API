using System;
using System.ComponentModel.DataAnnotations;

namespace NDLP_FIRST_API.Models.DTOs
{
    public class CustomerProfileResponseDto
    {
        public string CustomerId { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public KycResponseDto KycSummary { get; set; }
    }

    public class UpdateCustomerProfileRequestDto
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(250)]
        public string Address { get; set; }
    }

    public class CustomerSearchRequestDto
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
