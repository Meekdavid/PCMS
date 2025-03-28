using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Concrete;
using Persistence.DBModels;
using Persistence.Enums;
using System.Linq.Expressions;

namespace Persistence.DBContext
{
    public class ApplicationDbContext : IdentityDbContext<Member, Role, string>
    {
        private readonly IConfiguration _configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;

            // Log connection string to verify it's being read
            Console.WriteLine("Connection String: " + _configuration.GetConnectionString("DefaultConnection"));
        }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<EligibilityRule> EligibilityRules { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<BackgroundJob> BackgroundJobs { get; set; }
        public DbSet<BenefitEligibility> BenefitEligibilities { get; set; }
        public DbSet<Contribution> Contributions { get; set; }
        public DbSet<Employer> Employers { get; set; }
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<RequestResponseLog> RequestResponseLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account entity configuration
            modelBuilder.Entity<Account>()
                .HasKey(a => a.AccountId);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Member)
                .WithMany(m => m.Accounts)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Employer)
                .WithMany()
                .HasForeignKey(a => a.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Admin entity configuration
            modelBuilder.Entity<Admin>()
                .HasKey(a => a.AdminId);

            modelBuilder.Entity<Admin>()
                .HasOne(a => a.Member)
                .WithOne()
                .HasForeignKey<Admin>(a => a.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuditLog entity configuration
            modelBuilder.Entity<AuditLog>()
                .HasKey(al => al.AuditLogId);

            // BenefitEligibility entity configuration
            modelBuilder.Entity<BenefitEligibility>()
                .HasKey(be => be.BenefitEligibilityId);

            modelBuilder.Entity<BenefitEligibility>()
                .HasOne(be => be.Member)
                .WithOne()
                .HasForeignKey<BenefitEligibility>(be => be.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Contribution entity configuration
            modelBuilder.Entity<Contribution>()
                .HasKey(c => c.ContributionId);

            modelBuilder.Entity<Contribution>()
                .HasOne(c => c.Member)
                .WithMany()
                .HasForeignKey(c => c.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Employer entity configuration
            modelBuilder.Entity<Employer>()
                .HasKey(e => e.EmployerId);

            modelBuilder.Entity<Employer>()
                .HasMany(e => e.Employees)
                .WithOne(m => m.Employer)
                .HasForeignKey(m => m.EmployerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Member entity configuration
            modelBuilder.Entity<Member>()
                .HasKey(m => m.Id);

            modelBuilder.Entity<Member>()
                .HasMany(m => m.Accounts)
                .WithOne(a => a.Member)
                .HasForeignKey(a => a.MemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification entity configuration
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Member)
                .WithMany()
                .HasForeignKey(n => n.MemberId)
                .OnDelete(DeleteBehavior.Cascade);

            // Transaction entity configuration
            //modelBuilder.Entity<Transaction>()
            //    .HasOne(t => t.Account)
            //    .WithMany()
            //    .HasForeignKey(t => t.AccountId)
            //    .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Transaction>()
            //    .HasOne(t => t.Member)
            //    .WithMany()
            //    .HasForeignKey(t => t.MemberId)
            //    .OnDelete(DeleteBehavior.NoAction);

            //modelBuilder.Entity<Transaction>()
            //    .HasOne(t => t.Contribution)
            //    .WithMany()
            //    .HasForeignKey(t => t.ContributionId)
            //    .OnDelete(DeleteBehavior.NoAction);



            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // This is to configure properties for all model primary keys
                var entityName = entityType.ClrType.Name;
                var primaryKeyName = $"{entityName}Id";

                var property = entityType.FindProperty(primaryKeyName);
                if (property != null && property.ClrType == typeof(string))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property(primaryKeyName)
                                .HasMaxLength(50);
                }

                // This is to configure a consistent date type for all date properties
                foreach (var component in entityType.GetProperties())
                {
                    if (component.ClrType == typeof(DateTime) || component.ClrType == typeof(DateTime?))
                    {
                        component.SetValueConverter(new ValueConverter<DateTime, DateTime>(
                            v => v.ToUniversalTime(),
                            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
                        ));
                    }
                }
            }


            // This is to ensure soft deleted records are exempted from the records returned to the client. It is enforced on all objects inheriting from 'BaseModel' class
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseModel).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "x");
                    var property = Expression.Property(parameter, "Status");
                    var deletedStatus = Expression.Constant(Status.Deleted);
                    var condition = Expression.NotEqual(property, deletedStatus);

                    var lambda = Expression.Lambda(condition, parameter);

                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            //string connectionString = ConfigSettings.ConnectionString.DefaultConnection;
            string connectionString = _configuration["ConnectionStrings:DefaultConnection"];

            // Define log file path
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Error", "OnBuild", "log.txt");

            // Ensure the directory exists
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Create a delegate-based logging mechanism to avoid locking issues
            optionsBuilder.UseSqlServer(connectionString)
                .EnableSensitiveDataLogging()
                .LogTo(Console.WriteLine, LogLevel.Information) // Log to Console
                .LogTo(message =>
                {
                    try
                    {
                        // Append text without locking the file
                        using (StreamWriter writer = File.AppendText(logFilePath))
                        {
                            writer.WriteLine(message);
                        }
                    }
                    catch (IOException ex)
                    {
                        Console.WriteLine("Logging error: " + ex.Message);
                    }
                }, LogLevel.Information);

        }
    }

}
