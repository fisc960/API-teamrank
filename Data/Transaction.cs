using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemachApp.Data
{
    public class Transaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TransId { get; set; }
        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        [Required]
        public DateTime TransDate { get; set; }
       [Required]
        public string? Agent  { get; set; }
        public decimal? Added { get; set; }
        public decimal? Subtracted { get; set; }
        public decimal? TotalAdded { get; set; }
        public decimal? TotalSubtracted { get; set; }
        public bool SendEmail { get; set; }  // Flag to determine if an email should be sent
    }
}
