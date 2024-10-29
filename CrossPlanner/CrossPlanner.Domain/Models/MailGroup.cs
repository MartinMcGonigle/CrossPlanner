using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("MailGroup")]
    public class MailGroup
    {
        [Key]
        public int MailGroupId { get; set; }

        public string MailGroupName { get; set; }

        public string MailAddress { get; set; }
    }
}