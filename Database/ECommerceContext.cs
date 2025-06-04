using Microsoft.EntityFrameworkCore;
using System;

namespace DbTests.Database
{
    class ECommerceContext : DbContext
    {

        public DbSet<User> Users { get; set; }

        public ECommerceContext() : base()
        {
        }

        // Constructor for dependency injection (used in web apps)
        public ECommerceContext(DbContextOptions<ECommerceContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<string>();
            });
        }
    }
}
