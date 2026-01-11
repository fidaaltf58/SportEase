using Microsoft.EntityFrameworkCore;
using SportEase.Web.Data;
using SportEase.Web.Models.Entities;

namespace SportEase.Web.Repositories.Interfaces
{
    public interface IReservationRepository
    {
        Task<Reservation?> GetByIdAsync(int id);
        Task<IEnumerable<Reservation>> GetAllAsync();
        Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Reservation>> GetByTerrainIdAsync(int terrainId);
        Task<IEnumerable<Reservation>> GetByAdminIdAsync(int adminId);
        Task<Reservation> AddAsync(Reservation reservation);
        Task UpdateAsync(Reservation reservation);
        Task DeleteAsync(int id);
        Task<bool> HasConflictAsync(int terrainId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null);
        Task<int> GetActiveReservationsCountAsync(int userId);
        Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync(int userId);
        Task<IEnumerable<Reservation>> GetPastReservationsAsync(int userId);
    }
}
