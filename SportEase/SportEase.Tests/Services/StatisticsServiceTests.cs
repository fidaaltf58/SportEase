using FluentAssertions;
using Moq;
using SportEase.Web.Models.Entities;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Services.Implementations;
using Xunit;

namespace SportEase.Tests.Services
{
    public class StatisticsServiceTests
    {
        private readonly Mock<IReservationRepository> _mockReservationRepository;
        private readonly Mock<ITerrainRepository> _mockTerrainRepository;
        private readonly StatisticsService _service;

        public StatisticsServiceTests()
        {
            _mockReservationRepository = new Mock<IReservationRepository>();
            _mockTerrainRepository = new Mock<ITerrainRepository>();
            _service = new StatisticsService(_mockReservationRepository.Object, _mockTerrainRepository.Object);
        }

        [Fact]
        public async Task GetUserDashboardAsync_ShouldCalculateTotalsCorrectly()
        {
            // Arrange
            var userId = 1;
            var now = DateTime.Now;
            var reservations = new List<Reservation>
            {
                new Reservation { Status = "Pending", UserId = userId, ReservationDate = now.AddDays(1), StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, TotalPrice = 50 },
                new Reservation { Status = "Confirmed", UserId = userId, ReservationDate = now.AddDays(2), StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, TotalPrice = 100 },
                new Reservation { Status = "Completed", UserId = userId, ReservationDate = now.AddDays(-1), StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, TotalPrice = 75 },
                new Reservation { Status = "Cancelled", UserId = userId, ReservationDate = now.AddDays(3), StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, TotalPrice = 200 }
            };

            _mockReservationRepository.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(reservations);

            // Act
            var result = await _service.GetUserDashboardAsync(userId);

            // Assert
            result.TotalReservations.Should().Be(4);
            result.UpcomingReservations.Should().Be(2); // Pending + Confirmed in future
            result.CompletedReservations.Should().Be(1);
            result.TotalSpent.Should().Be(225); // 50 + 100 + 75 (Cancelled excluded)
        }

        [Fact]
        public async Task GetAdminDashboardAsync_ShouldVerifyRevenueCalculations()
        {
            // Arrange
            var adminId = 1;
            var now = DateTime.Now;

            var terrains = new List<Terrain>
            {
                new Terrain { Id = 1, AdminId = adminId, IsActive = true, Reservations = new List<Reservation>() },
                new Terrain { Id = 2, AdminId = adminId, IsActive = false, Reservations = new List<Reservation>() }
            };

            var reservations = new List<Reservation>
            {
                // Today Pending
                new Reservation { Status = "Pending", ReservationDate = now.Date, StartTime = TimeSpan.Zero, EndTime = TimeSpan.Zero, TotalPrice = 50 },
                
                // This Month Confirmed
                new Reservation 
                { 
                    Status = "Confirmed", 
                    ReservationDate = now.Date,
                    StartTime = TimeSpan.Zero, 
                    EndTime = TimeSpan.Zero, 
                    TotalPrice = 100,
                    CreatedAt = now 
                },

                // This Month Completed
                new Reservation 
                { 
                    Status = "Completed", 
                    ReservationDate = now.AddDays(-1),
                    StartTime = TimeSpan.Zero, 
                    EndTime = TimeSpan.Zero, 
                    TotalPrice = 75,
                    CreatedAt = now 
                },

                // Old Completed (Total Revenue but not Monthly Revenue)
                new Reservation 
                { 
                    Status = "Completed", 
                    ReservationDate = now.AddMonths(-10),
                    StartTime = TimeSpan.Zero, 
                    EndTime = TimeSpan.Zero, 
                    TotalPrice = 200,
                    CreatedAt = now.AddMonths(-10)
                }
            };
            
            // Note: Update Reservation Model if AdminId isn't on it (it wasn't in the model I saw earlier `Reservation.cs`, it has TerrainId).
            // The service calls `GetByAdminIdAsync`.
            // Let's assume the mocking of the repository handles the "returning the right reservations".
            // However, inside `GetAdminDashboardAsync`, it uses LINQ logic.
            // Wait, looking at `GetAdminDashboardAsync` logic:
            // var allReservations = await _reservationRepository.GetByAdminIdAsync(adminId);
            // It relies on the repository to return the right reservations.
            // But the logic inside uses CreatedAt for revenue calculations.

            _mockTerrainRepository.Setup(r => r.GetByAdminIdAsync(adminId)).ReturnsAsync(terrains);
            _mockReservationRepository.Setup(r => r.GetByAdminIdAsync(adminId)).ReturnsAsync(reservations);

            // Act
            var result = await _service.GetAdminDashboardAsync(adminId);

            // Assert
            result.TotalTerrains.Should().Be(2);
            result.ActiveTerrains.Should().Be(1);
            result.TodayReservations.Should().Be(2); // Pending + Confirmed today
            result.PendingReservations.Should().Be(1);
            result.MonthlyRevenue.Should().Be(175); // 100 + 75
            result.TotalRevenue.Should().Be(375); // 100 + 75 + 200
        }
    }
}
