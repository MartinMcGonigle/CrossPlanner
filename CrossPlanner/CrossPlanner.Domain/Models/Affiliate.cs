using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("Affiliate")]
    public class Affiliate
    {
        [Key]
        public int AffiliateId { get; set; }

        [Required]
        [MaxLength(250)]
        [Display(Name = "Affiliate Name")]
        public string Name { get; set; }

        [MaxLength(500)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Phone Number")]
        public string Phone {  get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        [Display(Name = "Affiliate Email Address")]
        public string Email { get; set; }

        public bool IsActive { get; set; } = true;

        public string? ConnectedAccountId { get; set; }
    }
}