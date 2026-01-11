
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Models.ViewModels;

namespace SportEase.Web.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<DashboardViewModel> GetUserDashboardAsync(int userId);
        Task<DashboardViewModel> GetAdminDashboardAsync(int adminId);
        Task<Dictionary<string, int>> GetReservationsByMonthAsync(int adminId, int months = 6);
        Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int adminId, int months = 6);
    }
}