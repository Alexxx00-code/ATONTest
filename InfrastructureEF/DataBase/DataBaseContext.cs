using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureEF.DataBase
{
    public class DataBaseContext : DbContext
    {
        public DataBaseContext(DbContextOptions<DataBaseContext> options)
                    : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLazyLoadingProxies(true);

            }

            optionsBuilder.UseSeeding((context, _) =>
            {
                var testBlog = context.Set<User>().FirstOrDefault();
                if (testBlog == null)
                {
                    context.Set<User>().Add(new User
                    {
                        Guid = Guid.NewGuid(),
                        Gender = 1,
                        Login = "Admin",
                        Name = "Admin",
                        Password = BCrypt.Net.BCrypt.HashPassword("Admin"),
                        Admin = true,
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = "",
                    });
                    context.SaveChanges();
                }
            });
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            base.OnModelCreating(builder);
        }
    }
}
