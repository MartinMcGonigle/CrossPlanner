using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("MailDetails")]
    public class MailDetails
    {
        [Key]
        public int MailDetailsId { get; set; }

        public string MailSubject { get; set; }

        public string MailBody { get; set; }
    }
}