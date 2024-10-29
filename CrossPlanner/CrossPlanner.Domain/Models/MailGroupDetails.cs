using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("MailGroupDetails")]
    public class MailGroupDetails
    {
        [Key]
        public int MailGroupDetailsId { get; set; }

        public int MailGroupId { get; set; }

        public int MailDetailsId { get; set; }

        [ForeignKey("MailGroupId")]
        public MailGroup MailGroup { get; set; }

        [ForeignKey("MailDetailsId")]
        public MailDetails MailDetails { get; set; }
    }
}