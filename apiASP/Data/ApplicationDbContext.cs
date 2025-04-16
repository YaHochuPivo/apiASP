using Microsoft.EntityFrameworkCore;
using apiASP.Models;

namespace apiASP.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Psychologist> Psychologists { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Конфигурация User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Конфигурация Psychologist
            modelBuilder.Entity<Psychologist>(entity =>
            {
                entity.HasKey(e => e.PsychologistId);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.PhotoUrl).HasMaxLength(255);
            });

            // Конфигурация Service
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.ServiceId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
                entity.Property(e => e.DurationMinutes).IsRequired();
                entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
                
                // Настройка связи с Psychologist
                entity.HasOne(s => s.Psychologist)
                      .WithMany(p => p.Services)
                      .HasForeignKey(s => s.PsychologistId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Конфигурация Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // Конфигурация Payment
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Amount).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50);
                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // Конфигурация Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.AppointmentId);
                entity.Property(e => e.Status).HasMaxLength(20);
            });

            // Конфигурация Review
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.ReviewId);
                entity.Property(e => e.Rating).IsRequired();
            });
        }
    }
} 