using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace back.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
            
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<EmailConfirmationToken> EmailConfirmationTokens => Set<EmailConfirmationToken>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<Question> Questions => Set<Question>();
        public DbSet<LegalReference> LegalReferences => Set<LegalReference>();
        public DbSet<RegulatoryUpdate> RegulatoryUpdates => Set<RegulatoryUpdate>();
        public DbSet<RegulatoryAlert> RegulatoryAlerts => Set<RegulatoryAlert>();
        public DbSet<Payment> Payments => Set<Payment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Content).IsRequired();
                entity.HasIndex(e => e.UploadedAt);
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(2000);
                entity.HasIndex(e => e.AskedAt);
                entity.HasIndex(e => e.UserId);
                
                entity.HasOne(e => e.Document)
                    .WithMany(d => d.Questions)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LegalReference>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ArticleOrSection).HasMaxLength(200);
                entity.Property(e => e.Jurisdiction).HasMaxLength(100);
                entity.HasIndex(e => e.DocumentId);

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.LegalReferences)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RegulatoryUpdate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(500);
                entity.Property(e => e.LawIdentifier).IsRequired().HasMaxLength(500);
                entity.HasIndex(e => e.IsProcessed);
                entity.HasIndex(e => e.PublishedAt);
            });

            modelBuilder.Entity<RegulatoryAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.DocumentId);
                entity.HasIndex(e => e.IsRead);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Document)
                    .WithMany(d => d.RegulatoryAlerts)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.RegulatoryUpdate)
                    .WithMany(r => r.Alerts)
                    .HasForeignKey(e => e.RegulatoryUpdateId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.LegalReference)
                    .WithMany()
                    .HasForeignKey(e => e.LegalReferenceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                entity.Property(e => e.Amount).HasPrecision(10, 2);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Code = "Admin", Name = "Administrator", Description = "Full system access", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 2, Code = "User", Name = "User", Description = "Regular user", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}