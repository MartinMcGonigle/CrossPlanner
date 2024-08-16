namespace CrossPlanner.Domain.OtherModels
{
    public class AffiliateMembersViewModel
    {
        public string MemberId { get; set; }

        public string MemberFirstName { get; set; }

        public string MemberLastName { get; set; }

        public string MemberEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool MembershipActive { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string RoleName { get; set; }

        public bool AffiliateUserLink { get; set; }
    }
}