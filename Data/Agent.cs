using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemachApp.Data
{
   // [Table("agents")]
    public class Agent
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[Required(ErrorMessage = "you must enter Name")]
        public string AgentName { get; set; }
        [Required(ErrorMessage = "you must enter Password")]
        [MaxLength(40, ErrorMessage = "The Password is too long (max 40 chars)")]
        [MinLength(6, ErrorMessage = "The Password is too short  (at least 6 chars)")]
        public string AgentPassword { get; set; } //  hash this later
        public DateTime AgentOpenDate { get; set; } = DateTime.UtcNow;
    }
}
