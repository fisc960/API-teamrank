using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GemachApp.Data
{

    [Table("clients")]
    public class Client
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClientId { get; set; }
        [Required(ErrorMessage = "you must enter First Name")]
        [MaxLength(40, ErrorMessage = "The First Name is too long (max 40 chars)")]
        [MinLength(3, ErrorMessage = "The First Name is too short  (at least 3 chars)")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "The first name must contain only letters and spaces.")]
        public string ClientFirstName { get; set; }
        [Required(ErrorMessage = "you must enter Last Name")]
        [MaxLength(40, ErrorMessage = "The Last Name is too long (max 40 chars)")]
        [MinLength(3, ErrorMessage = "The Last Name is too short  (at least 3 chars)")]
        [RegularExpression(@"^[A-Za-z\s]+$", ErrorMessage = "The last name must contain only letters and spaces.")]
        public string ClientLastName { get; set; }

        [Required(ErrorMessage = "you must enter Phone number")]
        [MaxLength(18, ErrorMessage = "The Phone number is too long (max 18 chars)")]
        [MinLength(10, ErrorMessage = "The First Name is too short  (at least 10 chars)")]
        public string Phonenumber { get; set; } = string.Empty;
        public DateTime ClientOpenDate { get; set; }
        public bool Urav {  get; set; }
        public string Comments { get; set; } = string.Empty;
        [MinLength(6, ErrorMessage = "The Password is too short  (at least 6 chars)")]
        public string? ClientPassword { get; set; }

        public bool UpdateByEmail { get; set; }
        public string? Email { get; set; }

        // Selected position (from the dropdown)
        public string SelectedPosition { get; set; }

        // Available positions (dropdown options)
        public static List<string> AvailablePositions { get; } = new List<string>
    {
        "בעל הבית",       
        "אברך",      
        "בחור",      
        "אשה"         
    };
        // Navigation property to access related Transactions
        public ICollection<Transaction>? Transactions { get; set; } = new List<Transaction>();  // Empty list as default
        public Account? Account { get; set; } // One-to-one relation
    public string Agent {  get; set; }
    }

}
