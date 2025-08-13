namespace GemachApp.Data
{
    public class Admin
    {
            public int Id { get; set; }
            public string Password { get; set; }
            public string PasswordHash { get; set; } // Store the hashed password
    }
}