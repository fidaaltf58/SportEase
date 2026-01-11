using Moq;
using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
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
    public class ReservationServiceTests
    {
        private Mock<IReservationRepository> _reservationRepoMock;
        private Mock<ITerrainRepository> _terrainRepoMock;
        private ReservationService _service;

        [SetUp]
        public void Setup()
        {
            _reservationRepoMock = new Mock<IReservationRepository>();
            _terrainRepoMock = new Mock<ITerrainRepository>();

            _service = new ReservationService(
                _reservationRepoMock.Object,
                _terrainRepoMock.Object);
        }

        [Test]
        public void CreateAsync_TerrainNotActive_ThrowsException()
        {
            // Arrange
            var model = new CreateReservationViewModel
            {
                TerrainId = 1,
                ReservationDate = DateTime.Today.AddDays(1),
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11)
            };

            _terrainRepoMock
                .Setup(t => t.GetByIdAsync(1))
                .ReturnsAsync(new Terrain { IsActive = false });

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await _service.CreateAsync(model, 1));
        }
    }
}
