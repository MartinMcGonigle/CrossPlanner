namespace CrossPlanner.Domain.OtherModels
{
    public class ProfileViewModel
    {
        public string ApplicationUserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string? ProfilePictureUrl { get; set; }

        public bool DisplayNameVisibility { get; set; }

        public bool ProfilePictureVisibility { get; set; }

        public string MembershipPlanTitle { get; set; }

        public bool MembershipAutoRenew { get; set; }

        public DateTime MembershipStartDate { get; set; }

        public DateTime? MembershipEndDate { get; set; }
    }
}