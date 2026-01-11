using Microsoft.AspNetCore.Hosting;
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
    public class TerrainServiceTests
    {
        private Mock<ITerrainRepository> _terrainRepoMock;
        private Mock<IReservationRepository> _reservationRepoMock;
        private Mock<IWebHostEnvironment> _envMock;
        private TerrainService _service;

        [SetUp]
        public void Setup()
        {
            _terrainRepoMock = new Mock<ITerrainRepository>();
            _reservationRepoMock = new Mock<IReservationRepository>();
            _envMock = new Mock<IWebHostEnvironment>();

            _envMock.Setup(e => e.WebRootPath).Returns("wwwroot");

            _service = new TerrainService(
                _terrainRepoMock.Object,
                _reservationRepoMock.Object,
                _envMock.Object);
        }

        [Test]
        public async Task GetAllAsync_ReturnsTerrains()
        {
            // Arrange
            _terrainRepoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<Terrain> { new Terrain() });

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            Assert.IsNotEmpty(result);
        }
    }
}
