using CrossPlanner.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrossPlanner.Domain.Context
{
    public class ApplicationContext : IdentityDbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base (options)
        {
        }

        public DbSet<MailServer> MailServers { get; set; }

        public DbSet<Affiliate> Affiliates { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<AffiliateUser> AffiliateUsers { get; set; }

        public DbSet<MembershipPlan> MembershipPlans { get; set; }

        public DbSet<Membership> Memberships { get; set; }

        public DbSet<UserStripeAffiliate> UserStripeAffiliates { get; set; }
        
        public DbSet<Refund> Refunds { get; set; }

        public DbSet<ClassType> ClassTypes { get; set; }

        public DbSet<ScheduledClass> ScheduledClasses { get; set; }
        
        public DbSet<ScheduledClassReservation> ScheduledClassReservations { get; set; }
        
        public DbSet<Workout> Workouts { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Affiliate>()
                .HasIndex(a => a.Email)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<AffiliateUser>()
                .HasIndex(au => new { au.ApplicationUserId, au.IsActive })
                .IsUnique()
                .HasFilter("[IsActive] = 1");
            
            builder.Entity<Workout>()
                .HasIndex(w => new { w.ClassTypeId, w.Date })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
        }
    }
}