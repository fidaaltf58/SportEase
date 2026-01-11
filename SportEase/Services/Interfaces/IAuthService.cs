using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;
using BCrypt.Net;

namespace SportEase.Web.Services.Interfaces
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string email, string password);
        Task<User> RegisterAsync(RegisterViewModel model);
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmailAsync(string email);
        Task<bool> UpdateProfileAsync(ProfileViewModel model);
        Task<bool> ChangePasswordAsync(int userId, string newPassword);
        bool ValidatePassword(string password, string hash);
    }
}