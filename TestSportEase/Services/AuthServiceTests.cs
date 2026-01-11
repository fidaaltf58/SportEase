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
    public class AuthServiceTests
    {
        private Mock<IUserRepository> _userRepositoryMock;
        private AuthService _authService;

        [SetUp]
        public void Setup()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authService = new AuthService(_userRepositoryMock.Object);
        }

        [Test]
        public async Task LoginAsync_InvalidEmail_ReturnsNull()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _authService.LoginAsync("test@test.com", "1234");

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsUser()
        {
            // Arrange
            var password = "1234";
            var user = new User
            {
                Email = "test@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true
            };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync(user.Email))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(user.Email, password);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(user.Email, result!.Email);
        }
    }
}
