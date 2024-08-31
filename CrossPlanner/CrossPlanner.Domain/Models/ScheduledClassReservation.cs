using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("ScheduledClassReservation")]
    public class ScheduledClassReservation
    {
        [Key]
        public int ScheduledClassReservationId { get; set; }

        public int? MembershipId { get; set; }

        public int? ScheduledClassId { get; set; }

        public DateTime ReservationDate { get; set; }

        public bool? IsPresent { get; set; }
        
        [ForeignKey("MembershipId")]
        public Membership? Membership { get; set; }

        [ForeignKey("ScheduledClassId")]
        public ScheduledClass? ScheduledClass { get; set; }
    }
}