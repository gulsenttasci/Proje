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
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 
            
            builder.Entity<Comments>()
                .HasOne(c => c.Caterer)
                .WithMany()
                .HasForeignKey(c => c.CatererId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            builder.Entity<Caterer>().HasQueryFilter(c => !c.IsDeleted);
            builder.Entity<MenuItem>().HasQueryFilter(m => !m.IsDeleted);
            builder.Entity<Comments>().HasQueryFilter(com => !com.IsDeleted);
            builder.Entity<CustomizationGroup>().HasQueryFilter(g => !g.IsDeleted);
            builder.Entity<CustomizationOption>().HasQueryFilter(o => !o.IsDeleted);
        }
    }
}