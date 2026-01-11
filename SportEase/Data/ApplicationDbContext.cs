using Microsoft.EntityFrameworkCore;
using SportEase.Web.Models.Entities;

namespace SportEase.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Terrain> Terrains { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User Entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Role);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Configure Terrain Entity
            modelBuilder.Entity<Terrain>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.AdminId);
                entity.HasIndex(e => e.City);
                entity.HasIndex(e => e.SportType);
                entity.HasIndex(e => new { e.IsActive, e.SportType, e.City });

                entity.Property(e => e.PricePerHour).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                // Configure relationship
                entity.HasOne(e => e.Admin)
                      .WithMany(u => u.Terrains)
                      .HasForeignKey(e => e.AdminId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Reservation Entity
            modelBuilder.Entity<Reservation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TerrainId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => new { e.TerrainId, e.ReservationDate, e.Status });

                entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETDATE()");

                // Configure relationships
                entity.HasOne(e => e.Terrain)
                      .WithMany(t => t.Reservations)
                      .HasForeignKey(e => e.TerrainId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reservations)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Admin User (Password: Admin@123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@sportease.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FirstName = "Admin",
                    LastName = "SportEase",
                    Phone = "+216 20 123 456",
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 10, 1)
                }
            );

            // Seed Regular User (Password: User@123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 2,
                    Email = "user@sportease.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                    FirstName = "Mohamed",
                    LastName = "Ben Ali",
                    Phone = "+216 25 987 654",
                    Role = "User",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 10, 5)
                }
            );

            // Seed Sample Terrains
            modelBuilder.Entity<Terrain>().HasData(
                new Terrain
                {
                    Id = 1,
                    AdminId = 1,
                    Name = "Stade Municipal Nord",
                    SportType = "Football",
                    Address = "Avenue Habib Bourguiba",
                    City = "La Marsa",
                    Capacity = 22,
                    PricePerHour = 45.00m,
                    Description = "Terrain de football professionnel avec pelouse naturelle. Idéal pour les matchs et entraînements.",
                    ImageUrl = "/images/terrains/football1.jpg",
                    HasLighting = true,
                    HasParking = true,
                    HasChangingRoom = true,
                    OpeningTime = new TimeSpan(8, 0, 0),
                    ClosingTime = new TimeSpan(22, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 10, 10)
                },
                new Terrain
                {
                    Id = 2,
                    AdminId = 1,
                    Name = "Court Central Tennis",
                    SportType = "Tennis",
                    Address = "Route de la Goulette",
                    City = "Carthage",
                    Capacity = 4,
                    PricePerHour = 35.00m,
                    Description = "Court de tennis en terre battue avec éclairage nocturne.",
                    ImageUrl = "/images/terrains/tennis1.jpg",
                    HasLighting = true,
                    HasParking = false,
                    HasChangingRoom = true,
                    OpeningTime = new TimeSpan(7, 0, 0),
                    ClosingTime = new TimeSpan(21, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 10, 12)
                },
                new Terrain
                {
                    Id = 3,
                    AdminId = 1,
                    Name = "Arena Basketball Pro",
                    SportType = "Basketball",
                    Address = "Rue de la République",
                    City = "Ariana",
                    Capacity = 10,
                    PricePerHour = 40.00m,
                    Description = "Terrain de basketball couvert avec parquet professionnel.",
                    ImageUrl = "/images/terrains/basketball1.jpg",
                    HasLighting = true,
                    HasParking = true,
                    HasChangingRoom = false,
                    OpeningTime = new TimeSpan(9, 0, 0),
                    ClosingTime = new TimeSpan(23, 0, 0),
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 10, 15)
                }
            );
        }
    }
}