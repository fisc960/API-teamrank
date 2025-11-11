using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemachApp.Data
{
    [Table("checks")]
    public class Check
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // Allow manual value  (to match the check number)
        public int CheckId { get; set; }  // Auto-incremented 
        public string ClientName { get; set; }    // First + " " + Last name
        public int ClientId { get; set; }
        public string OrderTo { get; set; }
        public int Sum { get; set; }
        public int TransId { get; set; }
        public string AgentName { get; set; }
        public int AgentId { get; set; }
        public DateTime CheckIssuedDate { get; set; }

    }
}
