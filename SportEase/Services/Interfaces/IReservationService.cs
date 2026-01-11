using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;

namespace SportEase.Web.Services.Interfaces
{
    public interface IReservationService
    {
        Task<Reservation?> GetByIdAsync(int id);
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Reservation>> GetByTerrainIdAsync(int terrainId);
        Task<IEnumerable<Reservation>> GetByAdminIdAsync(int adminId);
        Task<Reservation> CreateAsync(CreateReservationViewModel model, int userId);
        Task<bool> CancelAsync(int id, int userId, string reason);
        Task<bool> ConfirmAsync(int id, int adminId);
        Task<bool> RejectAsync(int id, int adminId, string reason);
        Task<bool> ValidateReservationAsync(int terrainId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null);
        Task<Dictionary<TimeSpan, bool>> GetAvailableSlotsAsync(int terrainId, DateTime date);
    }
}