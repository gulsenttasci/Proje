using Microsoft.EntityFrameworkCore;
using Hearty_Bites.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Hearty_Bites.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Caterer> Caterers { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Comments> Comments { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<CustomizationGroup> CustomizationGroups { get; set; }

        public DbSet<CustamizationOption> CustamizationOptions { get; set; }

        public DbSet<LogEntry> LogEntries { get; set; }

        public DbSet<OrderRating> OrderRatings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<CartItem>().ToTable("CartItems");
            builder.Entity<Comments>()
                .HasOne(c => c.Caterer)
                .WithMany()
                .HasForeignKey(c => c.CatererId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(o => o.MenuItem)
                .WithMany()
                .HasForeignKey(o => o.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderItem>()
                .HasOne(o => o.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o => o.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CartItem>()
                .HasOne(c => c.MenuItem)
                .WithMany()
                .HasForeignKey(c => c.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Caterer>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<MenuItem>().HasQueryFilter(m => !m.IsDeleted);
            builder.Entity<Comments>().HasQueryFilter(com => !com.IsDeleted);
            builder.Entity<CustomizationGroup>().HasQueryFilter(g => !g.IsDeleted);
            builder.Entity<CustamizationOption>().HasQueryFilter(o => !o.IsDeleted);
            
        }
    }
}