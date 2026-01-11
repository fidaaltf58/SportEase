using FluentAssertions;
using Moq;
using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Services.Implementations;
using Xunit;

namespace SportEase.Tests.Services
{
    public class ReservationServiceTests
    {
        private readonly Mock<IReservationRepository> _mockReservationRepository;
        private readonly Mock<ITerrainRepository> _mockTerrainRepository;
        private readonly ReservationService _service;

        public ReservationServiceTests()
        {
            _mockReservationRepository = new Mock<IReservationRepository>();
            _mockTerrainRepository = new Mock<ITerrainRepository>();
            _service = new ReservationService(_mockReservationRepository.Object, _mockTerrainRepository.Object);
        }

        #region CreateAsync

        [Fact]
        public async Task CreateAsync_ShouldCreateReservation_WhenValid()
        {
            // Arrange
            var userId = 1;
            var terrainId = 1;
            var date = DateTime.Today.AddDays(1);
            var startTime = new TimeSpan(10, 0, 0);
            var endTime = new TimeSpan(12, 0, 0);

            var model = new CreateReservationViewModel
            {
                TerrainId = terrainId,
                ReservationDate = date,
                StartTime = startTime,
                EndTime = endTime
            };

            var terrain = new Terrain
            {
                Id = terrainId,
                IsActive = true,
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0),
                PricePerHour = 100
            };

            _mockTerrainRepository.Setup(r => r.GetByIdAsync(terrainId)).ReturnsAsync(terrain);
            _mockReservationRepository.Setup(r => r.HasConflictAsync(terrainId, date, startTime, endTime, null)).ReturnsAsync(false);
            _mockReservationRepository.Setup(r => r.GetActiveReservationsCountAsync(userId)).ReturnsAsync(0);
            _mockReservationRepository.Setup(r => r.AddAsync(It.IsAny<Reservation>())).ReturnsAsync((Reservation r) => r);

            // Act
            var result = await _service.CreateAsync(model, userId);

            // Assert
            result.Should().NotBeNull();
            result.TerrainId.Should().Be(terrainId);
            result.UserId.Should().Be(userId);
            result.Status.Should().Be("Pending");
            result.TotalPrice.Should().Be(200); // 2 hours * 100
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenDateIsInPast()
        {
            // Arrange
            var userId = 1;
            var model = new CreateReservationViewModel
            {
                TerrainId = 1,
                ReservationDate = DateTime.Today.AddDays(-1),
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(11, 0, 0)
            };

            _mockTerrainRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Terrain { IsActive = true });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(model, userId));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenTimeOutsideOpeningHours()
        {
            // Arrange
            var userId = 1;
            var terrainId = 1;
            var model = new CreateReservationViewModel
            {
                TerrainId = terrainId,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(7, 0, 0),
                EndTime = new TimeSpan(9, 0, 0)
            };

            var terrain = new Terrain
            {
                Id = terrainId,
                IsActive = true,
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0)
            };

            _mockTerrainRepository.Setup(r => r.GetByIdAsync(terrainId)).ReturnsAsync(terrain);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(model, userId));
        }

        [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenConflictExists()
        {
             // Arrange
            var userId = 1;
            var terrainId = 1;
            var date = DateTime.Today.AddDays(1);
            var startTime = new TimeSpan(10, 0, 0);
            var endTime = new TimeSpan(11, 0, 0);

            var model = new CreateReservationViewModel
            {
                TerrainId = terrainId,
                ReservationDate = date,
                StartTime = startTime,
                EndTime = endTime
            };

             var terrain = new Terrain
            {
                Id = terrainId,
                IsActive = true,
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0),
                PricePerHour = 100
            };

            _mockTerrainRepository.Setup(r => r.GetByIdAsync(terrainId)).ReturnsAsync(terrain);
            _mockReservationRepository.Setup(r => r.HasConflictAsync(terrainId, date, startTime, endTime, null)).ReturnsAsync(true);

            // Act & Assert
             await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(model, userId));
        }

         [Fact]
        public async Task CreateAsync_ShouldThrowException_WhenUserExceedsMaxActiveReservations()
        {
             // Arrange
            var userId = 1;
            var terrainId = 1;
            var date = DateTime.Today.AddDays(1);
            var startTime = new TimeSpan(10, 0, 0);
            var endTime = new TimeSpan(11, 0, 0);

            var model = new CreateReservationViewModel
            {
                TerrainId = terrainId,
                ReservationDate = date,
                StartTime = startTime,
                EndTime = endTime
            };

             var terrain = new Terrain
            {
                Id = terrainId,
                IsActive = true,
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(22, 0, 0),
                PricePerHour = 100
            };

            _mockTerrainRepository.Setup(r => r.GetByIdAsync(terrainId)).ReturnsAsync(terrain);
            _mockReservationRepository.Setup(r => r.HasConflictAsync(terrainId, date, startTime, endTime, null)).ReturnsAsync(false);
            _mockReservationRepository.Setup(r => r.GetActiveReservationsCountAsync(userId)).ReturnsAsync(3); // Max is 3

            // Act & Assert
             await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(model, userId));
        }

        #endregion

        #region CancelAsync

        [Fact]
        public async Task CancelAsync_ShouldCancel_WhenWithinDeadline()
        {
             // Arrange
            var reservationId = 1;
            var userId = 1;
            var reason = "Change of plans";
            
            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                Status = "Pending",
                ReservationDate = DateTime.Today.AddDays(2),
                StartTime = new TimeSpan(10, 0, 0),
                Terrain = new Terrain { AdminId = 99 }
            };

            _mockReservationRepository.Setup(r => r.GetByIdAsync(reservationId)).ReturnsAsync(reservation);
            _mockReservationRepository.Setup(r => r.UpdateAsync(It.IsAny<Reservation>())).Returns(Task.CompletedTask);

             // Act
            var result = await _service.CancelAsync(reservationId, userId, reason);

            // Assert
            result.Should().BeTrue();
            reservation.Status.Should().Be("Cancelled");
            reservation.CancellationReason.Should().Be(reason);
        }

        [Fact]
        public async Task CancelAsync_ShouldThrow_WhenTooLate()
        {
            // Arrange
            var reservationId = 1;
            var userId = 1;
            
            var reservation = new Reservation
            {
                Id = reservationId,
                UserId = userId,
                Status = "Confirmed",
                ReservationDate = DateTime.Today, // Today!
                StartTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromHours(1)), // 1 hour from now
                Terrain = new Terrain { AdminId = 99 } 
            };

            _mockReservationRepository.Setup(r => r.GetByIdAsync(reservationId)).ReturnsAsync(reservation);

            // Act & Assert
             await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CancelAsync(reservationId, userId, "reason"));
        }

        #endregion

        #region GetAvailableSlotsAsync

        [Fact]
        public async Task GetAvailableSlotsAsync_ShouldCorrectlyCalculateFreeSlots()
        {
            // Arrange
            var terrainId = 1;
            var date = DateTime.Today.AddDays(1);
            var terrain = new Terrain
            {
                Id = terrainId,
                OpeningTime = new TimeSpan(8, 0, 0),
                ClosingTime = new TimeSpan(12, 0, 0) // 8-9, 9-10, 10-11, 11-12 (4 slots)
            };

            var reservations = new List<Reservation>
            {
                new Reservation
                {
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(10, 0, 0) // Fully occupies 9-10
                }
            };

            _mockTerrainRepository.Setup(r => r.GetByIdAsync(terrainId)).ReturnsAsync(terrain);
            _mockReservationRepository.Setup(r => r.GetByTerrainAndDateAsync(terrainId, date)).ReturnsAsync(reservations);

            // Act
            var result = await _service.GetAvailableSlotsAsync(terrainId, date);

            // Assert
            result.Should().HaveCount(4);
            result[new TimeSpan(8, 0, 0)].Should().BeTrue();
            result[new TimeSpan(9, 0, 0)].Should().BeFalse();
            result[new TimeSpan(10, 0, 0)].Should().BeTrue();
            result[new TimeSpan(11, 0, 0)].Should().BeTrue();
        }

        #endregion
    }
}
