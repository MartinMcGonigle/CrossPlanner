using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrossPlanner.Domain.Models
{
    [Table("MailServer")]
    public class MailServer
    {
        [Key]
        public int MailServerId { get; set; }

        public string MailServerName { get; set; }

        public string MailServerIP { get; set; }

        public string MailServerUserName { get; set; }

        public string MailServerPassword { get; set;}

        public int MailServerPort { get; set; }

        public bool Active { get; set; }
    }
}