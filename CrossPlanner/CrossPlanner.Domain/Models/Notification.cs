using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("Notification")]
    public class Notification
    {
        public Notification()
        {
            UserGrantAcess = new List<string>();
        }

        [Key]
        public int NotificationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public string UserCreated { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Please select users who can see this notification")]
        public string UserAccess {  get; set; }

        public int AffiliateId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        [ForeignKey("UserCreated")]
        public ApplicationUser? ApplicationUser { get; set; }

        [ForeignKey("AffiliateId")]
        public Affiliate? Affiliate { get; set; }

        [NotMapped]
        public List<string> UserGrantAcess { get; set; }
    }
}