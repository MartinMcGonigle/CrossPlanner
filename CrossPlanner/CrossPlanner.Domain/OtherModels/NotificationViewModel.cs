

using CrossPlanner.Domain.Models;

namespace CrossPlanner.Domain.OtherModels
{
    public class NotificationViewModel
    {
        public List<AffiliateUser>? AffiliateUsers { get; set; }

        public Notification Notification { get; set; }
    }
}