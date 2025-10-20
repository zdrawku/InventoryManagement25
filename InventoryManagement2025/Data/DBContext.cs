using InventoryManagement2025.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement2025.Data
{
    public class SchoolInventory : DbContext
    {
        public SchoolInventory(DbContextOptions<SchoolInventory> options)
            : base(options)
        {
        }

        public DbSet<Equipment> Equipment => Set<Equipment>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<ConditionLog> ConditionLogs => Set<ConditionLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Equipment>(entity =>
            {
                entity.ToTable("Equipment");
                entity.Property(e => e.Name)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.Property(e => e.Type)
                      .HasMaxLength(50);
                entity.Property(e => e.SerialNumber)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.HasIndex(e => e.SerialNumber)
                      .IsUnique();
                entity.Property(e => e.Condition)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(e => e.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(e => e.Location)
                      .HasMaxLength(100);
                entity.Property(e => e.PhotoUrl)
                      .HasMaxLength(2048);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(u => u.FirstName)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.LastName)
                      .IsRequired()
                      .HasMaxLength(50);
                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(100);
                entity.HasIndex(u => u.Email)
                      .IsUnique();
                entity.Property(u => u.PasswordHash)
                      .IsRequired()
                      .HasMaxLength(500);
                entity.Property(u => u.PhoneNumber)
                      .HasMaxLength(20);
                entity.Property(u => u.Role)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(u => u.Department)
                      .HasMaxLength(100);
                entity.Property(u => u.IsActive)
                      .HasDefaultValue(true);
                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(u => u.LastLoginAt)
                      .HasColumnType("TEXT")
                      .IsRequired(false);
                entity.Property(u => u.PasswordChangedAt)
                      .HasColumnType("TEXT")
                      .IsRequired(false);
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("Requests");
                entity.Property(r => r.Type)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(r => r.Status)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(r => r.RequestedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(r => r.Notes)
                      .HasMaxLength(500);

                entity.HasOne(r => r.Equipment)
                      .WithMany(e => e.Requests)
                      .HasForeignKey(r => r.EquipmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.User)
                      .WithMany(u => u.Requests)
                      .HasForeignKey(r => r.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ConditionLog>(entity =>
            {
                entity.ToTable("ConditionLogs");
                entity.Property(cl => cl.Condition)
                      .HasConversion<string>()
                      .HasMaxLength(20);
                entity.Property(cl => cl.LoggedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(cl => cl.Notes)
                      .HasMaxLength(500);

                entity.HasOne(cl => cl.Equipment)
                      .WithMany(e => e.ConditionLogs)
                      .HasForeignKey(cl => cl.EquipmentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cl => cl.LoggedBy)
                      .WithMany(u => u.ConditionLogs)
                      .HasForeignKey(cl => cl.LoggedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
