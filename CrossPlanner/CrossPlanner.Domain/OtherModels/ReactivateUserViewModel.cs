using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace CrossPlanner.Domain.OtherModels
{
    public class ReactivateUserViewModel
    {
        public string ApplicationUserId { get; set; }

        [MaxLength(100)]
        [Display(Name = "First Name")]
        [Required]
        public string FirstName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Last Name")]
        [Required]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date Of Birth")]
        public DateTime DateOfBirth { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Role")]
        public string Role { get; set; }

        public List<SelectListItem>? Roles { get; set; }

        public string ReturnController { get; set; }
    }
}