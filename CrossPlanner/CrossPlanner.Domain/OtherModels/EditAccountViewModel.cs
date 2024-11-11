using System.ComponentModel.DataAnnotations;

namespace CrossPlanner.Domain.OtherModels
{
    public class EditAccountViewModel
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

        public bool DisplayNameVisibility { get; set; }

        public bool ProfilePictureVisibility { get; set; }
    }
}