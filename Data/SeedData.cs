// Data/SeedData.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AleksandarIvanov_NexaCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AleksandarIvanov_NexaCore.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var um = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // 1) Apply any pending migrations
            await ctx.Database.MigrateAsync();

            //
            // 2) Seed the admin Identity user
            //
            var adminUserName = "admin";
            var adminEmail = "admin@nexacore.com";
            var admin = await um.FindByNameAsync(adminUserName);
            if (admin == null)
            {
                admin = new User
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,   // allow immediate login
                    Address = "Head Office",
                    IsAdmin = true
                };
                await um.CreateAsync(admin, "P@ssw0rd!");
                // add the IsAdmin claim too
                await um.AddClaimAsync(admin, new Claim("IsAdmin", "True"));
            }

            //
            // 2a) Ensure there’s a corresponding AdminUser record
            //
            if (!ctx.AdminUsers.Any(au => au.UserId == admin.Id))
            {
                ctx.AdminUsers.Add(new AdminUser
                {
                    UserId = admin.Id,
                    Role = "Administrator"
                });
                await ctx.SaveChangesAsync();
            }

            //
            // 3) Seed 5 subscription plans
            //
            if (!ctx.SubscriptionPlans.Any())
            {
                ctx.SubscriptionPlans.AddRange(
                    new SubscriptionPlan { PlanName = "Starter", Price = 4.99m, Duration = 30, Features = "Basic dashboard, Email support, Single user" },
                    new SubscriptionPlan { PlanName = "Basic", Price = 9.99m, Duration = 30, Features = "Standard analytics, Email support, Up to 3 users" },
                    new SubscriptionPlan { PlanName = "Standard", Price = 29.99m, Duration = 30, Features = "Advanced analytics, Priority email support, Up to 10 users" },
                    new SubscriptionPlan { PlanName = "Premium", Price = 49.99m, Duration = 30, Features = "All analytics, Phone & email support, Unlimited users, Custom branding" },
                    new SubscriptionPlan { PlanName = "Enterprise", Price = 99.99m, Duration = 30, Features = "Dedicated account manager, 99.9% SLA, API access, SSO, Custom integrations" }
                );
                await ctx.SaveChangesAsync();
            }

            //
            // 4) Seed 5 regular users + their Customer profiles
            //
            if (!ctx.Users.Any(u => !u.IsAdmin))
            {
                var regularUsers = new List<User>();
                for (int i = 1; i <= 5; i++)
                {
                    var email = $"user{i}@example.com";
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        Address = $"123 Sample St. Apt {i}",
                        IsAdmin = false
                    };
                    await um.CreateAsync(user, "User!234");
                    regularUsers.Add(user);
                }

                // 4a) Customer record (PK = FK to User.Id)
                foreach (var u in regularUsers)
                {
                    ctx.Customers.Add(new Customer
                    {
                        Id = u.Id,
                        IsActive = true
                    });
                }
                await ctx.SaveChangesAsync();

                //
                // 5) For each customer, create a subscription + invoice
                //
                var plans = ctx.SubscriptionPlans.ToList();
                var seedSubs = new List<(Subscription sub, decimal price)>();
                var today = DateTime.Today;

                for (int idx = 0; idx < regularUsers.Count; idx++)
                {
                    var cust = regularUsers[idx];
                    var plan = plans[idx % plans.Count];

                    var start = today.AddDays(-10 * idx);
                    var end = start.AddDays(plan.Duration);

                    var sub = new Subscription
                    {
                        CustomerId = cust.Id,
                        PlanId = plan.PlanId,
                        StartDate = start,
                        EndDate = end
                    };
                    ctx.Subscriptions.Add(sub);
                    seedSubs.Add((sub, plan.Price));
                }
                await ctx.SaveChangesAsync();

                // 5a) Now seed one invoice per subscription
                foreach (var (sub, price) in seedSubs)
                {
                    ctx.Invoices.Add(new Invoice
                    {
                        SubscriptionId = sub.SubscriptionId,
                        Amount = price,
                        DueDate = sub.EndDate,
                        PaymentStatus = "Paid"
                    });
                }
                await ctx.SaveChangesAsync();
            }
        }
    }
}
