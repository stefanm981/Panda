using System.Linq;
using Microsoft.EntityFrameworkCore;
using PandaWebApp.Models;

namespace PandaWebApp.Data
{
    public class PandaDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Receipt> Receipts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-MIMIHGQ\SQLEXPRESS;Database=Panda;Integrated Security=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasMany(u => u.Receipts)
                .WithOne(r => r.Recipient)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Packages)
                .WithOne(p => p.Recipient)
                .HasForeignKey(u => u.RecipientId);

            modelBuilder.Entity<Package>()
                .HasOne(p => p.Receipt)
                .WithOne(r => r.Package)
                .HasForeignKey<Receipt>(p => p.PackageId);

            //modelBuilder.Entity<Receipt>()
            //    .HasOne(r => r.Recipient)
            //    .WithMany(r => r.Receipts)
            //    .HasForeignKey(r => r.RecipientId)
            //    .OnDelete(DeleteBehavior.Restrict);

            ////modelBuilder.Entity<Receipt>()
            ////    .HasOne(r => r.Package)
            ////    .WithMany(r => r.Receipts)
            ////    .HasForeignKey(r => r.PackageId)
            ////    .OnDelete(DeleteBehavior.Restrict);

            //foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            //{
            //    relationship.DeleteBehavior = DelseteBehavior.Cascade;
            //}
        }
    }
}