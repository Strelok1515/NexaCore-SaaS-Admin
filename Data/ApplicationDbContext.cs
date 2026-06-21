using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AleksandarIvanov_NexaCore.Data;

public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    // DbSet properties for your entities
    public DbSet<User> User { get; set; }

    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AdminUser> AdminUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        base.OnModelCreating(modelBuilder);

        // Customer → User is a one‑to‑one where Customer.Id == User.Id
        modelBuilder.Entity<Customer>()
            .HasKey(c => c.Id);

        modelBuilder.Entity<Customer>()
            .HasOne(c => c.User)
            .WithOne(u => u.CustomerProfile)
            .HasForeignKey<Customer>(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Subscription → Customer is one‑to‑many
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Customer)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Subscription → SubscriptionPlan is many‑to‑one
        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.SubscriptionPlan)
            .WithMany(p => p.Subscriptions)
            .HasForeignKey(s => s.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        // Invoice → Subscription is one‑to‑many
        modelBuilder.Entity<Invoice>()
            .HasOne(i => i.Subscription)
            .WithMany(s => s.Invoices)
            .HasForeignKey(i => i.SubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);

        // AuditLog → AdminUser is many‑to‑one
        modelBuilder.Entity<AuditLog>()
            .HasOne(a => a.AdminUser)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.AdminUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}



