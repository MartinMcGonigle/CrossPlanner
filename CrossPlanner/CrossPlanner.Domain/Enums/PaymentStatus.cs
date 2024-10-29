using System.ComponentModel;

namespace CrossPlanner.Domain.Enums
{
    public enum PaymentStatus
    {
        [Description("Paid")]
        Paid,

        [Description("Refunded")]
        Refunded,
    }
}