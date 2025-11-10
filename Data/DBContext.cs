using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;

namespace GemachApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Agent> Agents { get; set; }
        public DbSet<UpdateLog> Updates { get; set; }
        public DbSet<Check> Checks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Client
            modelBuilder.Entity<Client>(e =>
            {
                e.Property(c => c.ClientFirstName).IsRequired();
                e.Property(c => c.ClientLastName).IsRequired();
                e.Property(c => c.Phonenumber).HasMaxLength(18).IsRequired();
                e.Property(c => c.Comments).HasMaxLength(4000);
                e.Property(c => c.Email).HasMaxLength(255);
                e.Property(c => c.SelectedPosition).HasMaxLength(30);
            });

            //  Transaction ↔ Client (many-to-one)
            modelBuilder.Entity<Transaction>(e =>
            {
                e.HasKey(t => t.TransId);
                e.Property(t => t.Agent).HasMaxLength(40).IsRequired();
                e.Property(t => t.Added).HasPrecision(18, 2);
                e.Property(t => t.Subtracted).HasPrecision(18, 2);
                e.Property(t => t.TotalAdded).HasPrecision(18, 2);
                e.Property(t => t.TotalSubtracted).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Client)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            //  Account ↔ Client (one-to-one)
            modelBuilder.Entity<Account>(e =>
            {
                e.HasKey(a => a.AccountId);
                e.Property(a => a.TotalAmount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Client)
                .WithOne(c => c.Account)
                .HasForeignKey<Account>(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            //  Agent
            modelBuilder.Entity<Agent>(e =>
            {
                e.Property(a => a.AgentName).HasMaxLength(40).IsRequired();
                e.Property(a => a.AgentPassword).HasMaxLength(40).IsRequired();
            });

            //  Admin
            /*modelBuilder.Entity<Admin>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.Password).HasMaxLength(128);
                e.Property(a => a.PasswordHash).HasMaxLength(256);
            });*/

            //  Checks
            modelBuilder.Entity<Check>(e =>
            {
                e.HasKey(c => c.CheckId);
                e.Property(c => c.CheckId).ValueGeneratedNever(); // No auto-increment
                e.Property(c => c.ClientName).HasMaxLength(100);
                e.Property(c => c.OrderTo).HasMaxLength(100);
                e.Property(c => c.AgentName).HasMaxLength(100);
            });

            //  UpdateLog
            modelBuilder.Entity<UpdateLog>(e =>
            {
                e.Property(u => u.TableName).HasMaxLength(128);
                e.Property(u => u.ObjectId).HasMaxLength(64);
                e.Property(u => u.ColumName).HasMaxLength(128);
                e.Property(u => u.PrevVersion).HasMaxLength(4000);
                e.Property(u => u.UpdatedVersion).HasMaxLength(4000);
                e.Property(u => u.Agent).HasMaxLength(100);
            });


            // --- lowercase convention for Postgres ---
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // lowercase table name
                entity.SetTableName(entity.GetTableName()?.ToLower());

                foreach (var property in entity.GetProperties())
                {
                     var tableIdentifier = StoreObjectIdentifier.Table(entity.GetTableName(), null);
        property.SetColumnName(property.GetColumnName(tableIdentifier)?.ToLower());
                }
            }
           

        }
    }
}

/*
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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>(entity =>
            {
                entity.Property(e => e.ClientFirstName)
                    .HasColumnType("text")
                    .IsRequired();

                entity.Property(e => e.ClientLastName)
                    .HasColumnType("text")
                    .IsRequired();

                entity.Property(e => e.Phonenumber)
                    .HasColumnType("text")
                    .IsRequired();
            });

            modelBuilder.Entity<Transaction>(e =>
            {
                e.HasKey(t => t.TransId);
                e.Property(t => t.Agent).HasMaxLength(40).IsRequired();   // align with AgentName rule
                e.Property(t => t.Added).HasPrecision(18, 2);
                e.Property(t => t.Subtracted).HasPrecision(18, 2);
                e.Property(t => t.TotalAdded).HasPrecision(18, 2);
                e.Property(t => t.TotalSubtracted).HasPrecision(18, 2);
            });

            // Transaction ↔ Client (many-to-one)
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Client)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client
            modelBuilder.Entity<Client>(e =>
            {
                e.Property(c => c.Phonenumber).HasMaxLength(18).IsRequired();
                e.Property(c => c.Comments).HasMaxLength(4000);   // optional cap so Postgres uses varchar
                e.Property(c => c.Email).HasMaxLength(255);
                e.Property(c => c.SelectedPosition).HasMaxLength(30);
            });

            // Account
            modelBuilder.Entity<Account>(e =>
            {
                e.Property(a => a.TotalAmount).HasPrecision(18, 2);
                e.HasKey(a => a.AccountId);
            });
            // Account ↔ Client (one-to-one)
            /*modelBuilder.Entity<Account>()
                .HasKey(a => a.AccountId);*//*

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Client)
                .WithOne(c => c.Account)
                .HasForeignKey<Account>(a => a.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Agent
            modelBuilder.Entity<Agent>(e =>
            {
                e.Property(a => a.AgentName).HasMaxLength(40).IsRequired();
                e.Property(a => a.AgentPassword).HasMaxLength(40).IsRequired();
            });

            // Admin
            modelBuilder.Entity<Admin>(e =>
            {
                e.Property(a => a.Password).HasMaxLength(128);
                e.Property(a => a.PasswordHash).HasMaxLength(256);
                e.HasKey(a => a.Id);
            });

            // Configure the Admin entity if needed
            /*modelBuilder.Entity<Admin>()
                .HasKey(a => a.Id);*//*

            // Check
            modelBuilder.Entity<Check>(e =>
            {
                e.Property(c => c.ClientName).HasMaxLength(100);
                e.Property(c => c.OrderTo).HasMaxLength(100);
                e.Property(c => c.AgentName).HasMaxLength(100);
            });

            modelBuilder.Entity<Check>()
    .Property(c => c.CheckId)
    .ValueGeneratedNever(); // disables auto-increment

            // UpdateLog (existing 'Updates' table)
            modelBuilder.Entity<UpdateLog>(e =>
            {
                e.Property(u => u.TableName).HasMaxLength(128);
                e.Property(u => u.ObjectId).HasMaxLength(64);
                e.Property(u => u.ColumName).HasMaxLength(128);
                e.Property(u => u.PrevVersion).HasMaxLength(4000);
                e.Property(u => u.UpdatedVersion).HasMaxLength(4000);
                e.Property(u => u.Agent).HasMaxLength(100);
                // Keep C# default DateTime.Now (no provider-specific SQL default)
            });
        }

            // Or globally configure all strings to use text
/*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(connectionString);
            }
        }*//*

    }

}*/


/*

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
*/
