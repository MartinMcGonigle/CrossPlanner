namespace CrossPlanner.Domain.OtherModels
{
    public class AffiliateUsersViewModel
    {
        public string AspNetUserId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool ActiveMembership { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string RoleName { get; set; }

        public bool AffiliateUserLink {  get; set; }

        public bool MembershipHistory { get; set; }
    }
}