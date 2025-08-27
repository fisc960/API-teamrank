using Microsoft.EntityFrameworkCore;

namespace GemachApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Admin> Admins { get; set; } // Fix the property name to 'Admins' (plural)
        public DbSet<Agent> Agents { get; set; }
        public DbSet<UpdateLog> Updates { get; set; }
        public DbSet<Check> Checks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Transaction ↔ Client (many-to-one)
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.TransId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Client)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Account ↔ Client (one-to-one)
            modelBuilder.Entity<Account>()
                .HasKey(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Client)
                .WithOne(c => c.Account)
                .HasForeignKey<Account>(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure the Admin entity if needed
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.Id);

            modelBuilder.Entity<Check>()
    .Property(c => c.CheckId)
    .ValueGeneratedNever(); // disables auto-increment
        }

    }

}

