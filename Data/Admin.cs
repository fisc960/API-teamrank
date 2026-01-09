using System.ComponentModel.DataAnnotations.Schema;


namespace GemachApp.Data
{

    [Table("admins")]
    public class Admin
    {
            public int Id { get; set; }
        public string Name { get; set; }
            public string PasswordHash { get; set; } // Store the hashed password
    }
}