using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SportEase.Web.Data;
using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Implementations;
using SportEase.Web.Services.Implementations;
using Xunit;

namespace SportEase.Tests.Integration
{
    public class ReservationFlowTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;

            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task FullReservationFlow_ShouldWorkWithDatabase()
        {
            // Arrange
            using var context = GetInMemoryContext();
            
            // Seed data
            var admin = new User 
            { 
                Id = 100, 
                Email = "admin@test.com", 
                PasswordHash = "hash", 
                Role = "Admin",
                FirstName = "Admin",
                LastName = "User",
                Phone = "1234567890"
            };
            var user = new User 
            { 
                Id = 101, 
                Email = "user@test.com", 
                PasswordHash = "hash", 
                Role = "User",
                FirstName = "Normal",
                LastName = "User",
                Phone = "0987654321"
            };
            
            var terrain = new Terrain
            {
                Id = 1,
                Name = "Test Field",
                AdminId = admin.Id,
                Admin = admin,
                PricePerHour = 100,
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0),
                IsActive = true,
                SportType = "Football",
                Address = "Test St",
                City = "Test City",
                Capacity = 10
            };

            context.Users.AddRange(admin, user);
            context.Terrains.Add(terrain);
            await context.SaveChangesAsync();

            // Setup dependencies
            var reservationRepo = new ReservationRepository(context);
            var terrainRepo = new TerrainRepository(context);
            var service = new ReservationService(reservationRepo, terrainRepo);

            // Act - 1. Create Reservation
            var createModel = new CreateReservationViewModel
            {
                TerrainId = terrain.Id,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(16, 0, 0)
            };

            var createdReservation = await service.CreateAsync(createModel, user.Id);

            // Assert - 1
            createdReservation.Should().NotBeNull();
            createdReservation.Id.Should().BeGreaterThan(0);
            createdReservation.Status.Should().Be("Pending");
            
            // Verify DB state
            var dbReservation = await context.Reservations.FindAsync(createdReservation.Id);
            dbReservation.Should().NotBeNull();
            dbReservation!.Status.Should().Be("Pending");

            // Act - 2. Confirm Reservation (Simulating Admin Action - assuming logic is in service)
            // Note: ReservationService has ConfirmAsync
            var confirmResult = await service.ConfirmAsync(createdReservation.Id, admin.Id);

            // Assert - 2
            confirmResult.Should().BeTrue();
            
            // Reload from DB to verify update
            await context.Entry(dbReservation).ReloadAsync();
            dbReservation.Status.Should().Be("Confirmed");

            // Act - 3. Try Create Overlapping Reservation
             var overlappingModel = new CreateReservationViewModel
            {
                TerrainId = terrain.Id,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(15, 0, 0), // Overlaps with 14-16
                EndTime = new TimeSpan(17, 0, 0)
            };

            // Assert - 3
            await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(overlappingModel, user.Id));
        }
    }
}
