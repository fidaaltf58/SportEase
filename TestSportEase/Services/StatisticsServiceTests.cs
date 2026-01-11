using Moq;
using SportEase.Web.Models.Entities;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestSportEase.Services
{
    [TestFixture]
    public class StatisticsServiceTests
    {
        private Mock<IReservationRepository> _reservationRepositoryMock;
        private Mock<ITerrainRepository> _terrainRepositoryMock;
        private StatisticsService _statisticsService;

        [SetUp]
        public void Setup()
        {
            _reservationRepositoryMock = new Mock<IReservationRepository>();
            _terrainRepositoryMock = new Mock<ITerrainRepository>();

            _statisticsService = new StatisticsService(
                _reservationRepositoryMock.Object,
                _terrainRepositoryMock.Object);
        }

        // =========================
        // USER DASHBOARD TESTS
        // =========================

        [Test]
        public async Task GetUserDashboardAsync_ReturnsCorrectCounts()
        {
            // Arrange
            var userId = 1;
            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    UserId = userId,
                    Status = "Pending",
                    ReservationDate = DateTime.Today.AddDays(1),
                    StartTime = TimeSpan.FromHours(10),
                    EndTime = TimeSpan.FromHours(11),
                    TotalPrice = 100,
                    CreatedAt = DateTime.Now
                },
                new Reservation
                {
                    UserId = userId,
                    Status = "Completed",
                    ReservationDate = DateTime.Today.AddDays(-1),
                    TotalPrice = 150,
                    CreatedAt = DateTime.Now.AddDays(-1)
                }
            };

            _reservationRepositoryMock
                .Setup(r => r.GetByUserIdAsync(userId))
                .ReturnsAsync(reservations);

            // Act
            var dashboard = await _statisticsService.GetUserDashboardAsync(userId);

            // Assert
            Assert.AreEqual(2, dashboard.TotalReservations);
            Assert.AreEqual(1, dashboard.UpcomingReservations);
            Assert.AreEqual(1, dashboard.CompletedReservations);
            Assert.AreEqual(250, dashboard.TotalSpent);
            Assert.AreEqual(2, dashboard.RecentReservations.Count);
        }

        // =========================
        // ADMIN DASHBOARD TESTS
        // =========================

        [Test]
        public async Task GetAdminDashboardAsync_ReturnsCorrectStatistics()
        {
            // Arrange
            var adminId = 1;
            var now = DateTime.Now;

            var terrains = new List<Terrain>
            {
                new Terrain
                {
                    Id = 1,
                    AdminId = adminId,
                    IsActive = true,
                    Reservations = new List<Reservation>
                    {
                        new Reservation { Status = "Confirmed" },
                        new Reservation { Status = "Completed" }
                    }
                },
                new Terrain
                {
                    Id = 2,
                    AdminId = adminId,
                    IsActive = false,
                    Reservations = new List<Reservation>()
                }
            };

            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    Terrain = terrains[0],
                    ReservationDate = now.Date,
                    Status = "Pending",
                    TotalPrice = 100,
                    CreatedAt = now
                },
                new Reservation
                {
                    Terrain = terrains[0],
                    ReservationDate = now.Date.AddDays(-1),
                    Status = "Confirmed",
                    TotalPrice = 200,
                    CreatedAt = now
                }
            };

            _terrainRepositoryMock
                .Setup(t => t.GetByAdminIdAsync(adminId))
                .ReturnsAsync(terrains);

            _reservationRepositoryMock
                .Setup(r => r.GetByAdminIdAsync(adminId))
                .ReturnsAsync(reservations);

            // Act
            var dashboard = await _statisticsService.GetAdminDashboardAsync(adminId);

            // Assert
            Assert.AreEqual(2, dashboard.TotalTerrains);
            Assert.AreEqual(1, dashboard.ActiveTerrains);
            Assert.AreEqual(1, dashboard.TodayReservations);
            Assert.AreEqual(1, dashboard.PendingReservations);
            Assert.AreEqual(200, dashboard.MonthlyRevenue);
            Assert.AreEqual(200, dashboard.TotalRevenue);
            Assert.IsNotEmpty(dashboard.TopTerrains);
        }

        // =========================
        // RESERVATIONS BY MONTH
        // =========================

        [Test]
        public async Task GetReservationsByMonthAsync_ReturnsCorrectMonthCount()
        {
            // Arrange
            var adminId = 1;
            var now = DateTime.Now;

            var reservations = new List<Reservation>
            {
                new Reservation { CreatedAt = now },
                new Reservation { CreatedAt = now.AddMonths(-1) }
            };

            _reservationRepositoryMock
                .Setup(r => r.GetByAdminIdAsync(adminId))
                .ReturnsAsync(reservations);

            // Act
            var result = await _statisticsService.GetReservationsByMonthAsync(adminId, 2);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Values.All(v => v >= 0));
        }

        // =========================
        // REVENUE BY MONTH
        // =========================

        [Test]
        public async Task GetRevenueByMonthAsync_ReturnsCorrectRevenue()
        {
            // Arrange
            var adminId = 1;
            var now = DateTime.Now;

            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    CreatedAt = now,
                    Status = "Confirmed",
                    TotalPrice = 300
                },
                new Reservation
                {
                    CreatedAt = now,
                    Status = "Cancelled",
                    TotalPrice = 500
                }
            };

            _reservationRepositoryMock
                .Setup(r => r.GetByAdminIdAsync(adminId))
                .ReturnsAsync(reservations);

            // Act
            var result = await _statisticsService.GetRevenueByMonthAsync(adminId, 1);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(300, result.Values.First());
        }
    }
}

