using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NDLP_FIRST_API.Models
{
    [Table("CustomerProfiles")]
    public class CustomerProfile
    {
        [Key]
        [StringLength(128)]
        public string CustomerId { get; set; }

        [StringLength(128)]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        public string LastName { get; set; }

        [Required]
        [StringLength(256)]
        public string Email { get; set; }

        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(250)]
        public string Address { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } // ACTIVE, SUSPENDED, BLOCKED

        public DateTime CreatedAt { get; set; }

        public DateTime LastUpdatedAt { get; set; }

        public virtual KycRecord KycRecord { get; set; }
    }
}
