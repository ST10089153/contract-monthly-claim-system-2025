using CMCS.Models;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ApprovalHistory> ApprovalHistory { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<LecturerProfile> LecturerProfiles { get; set; } // ✔ FIXED

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany()
                .HasForeignKey(c => c.LecturerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Claim>()
                .HasMany(c => c.Documents)
                .WithOne(d => d.Claim)
                .HasForeignKey(d => d.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApprovalHistory>()
                .HasOne(a => a.Claim)
                .WithMany(c => c.ApprovalHistories)
                .HasForeignKey(a => a.ClaimId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ApprovalHistory>()
                .HasOne(a => a.Approver)
                .WithMany()
                .HasForeignKey(a => a.ApproverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Claim)
                .WithOne()
                .HasForeignKey<Invoice>(i => i.ClaimId);
        }
    }
}
