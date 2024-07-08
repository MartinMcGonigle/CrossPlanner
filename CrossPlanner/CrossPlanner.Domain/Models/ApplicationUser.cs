using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CrossPlanner.Domain.Models
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime DateOfBirth { get; set; }

        public bool IsActive { get; set; } = true;
    }
}