using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("MembershipPlan")]
    public class MembershipPlan
    {
        [Key]
        public int MembershipPlanId { get; set; }

        [MaxLength(100)]
        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Price")]
        [Range(minimum: 0.0, maximum: double.MaxValue, ErrorMessage = "Price must be at least 0.0")]
        public double Price { get; set; }

        [MaxLength(250)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Membership Type")]
        public int Type {  get; set; }

        [Display(Name = "Number of Classes")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of classes must be at least 1")]
        public int? NumberOfClasses { get; set; }

        [Display(Name = "Number of Months")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of months must be at least 1")]
        public int? NumberOfMonths { get; set; }

        [Required]
        public int AffiliateId { get; set; }

        [ForeignKey("AffiliateId")]
        public Affiliate? Affiliate { get; set; }
    }
}