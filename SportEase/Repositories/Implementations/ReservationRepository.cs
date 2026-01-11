using Microsoft.EntityFrameworkCore;
using SportEase.Web.Data;
using SportEase.Web.Models.Entities;
using SportEase.Web.Repositories.Interfaces;
namespace SportEase.Web.Repositories.Implementations
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly ApplicationDbContext _context;

        public ReservationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _context.Reservations
                .Include(r => r.Terrain)
                    .ThenInclude(t => t.Admin)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<IEnumerable<Reservation>> GetAllAsync()
        {
            return await _context.Reservations
                .Include(r => r.Terrain)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Terrain)
                    .ThenInclude(t => t.Admin)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByTerrainIdAsync(int terrainId)
        {
            return await _context.Reservations
                .Include(r => r.User)
                .Where(r => r.TerrainId == terrainId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetByAdminIdAsync(int adminId)
        {
            return await _context.Reservations
                .Include(r => r.Terrain)
                .Include(r => r.User)
                .Where(r => r.Terrain.AdminId == adminId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<Reservation> AddAsync(Reservation reservation)
        {
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();
            return reservation;
        }

        public async Task UpdateAsync(Reservation reservation)
        {
            reservation.UpdatedAt = DateTime.Now;
            _context.Reservations.Update(reservation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasConflictAsync(int terrainId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null)
        {
            var query = _context.Reservations
                .Where(r => r.TerrainId == terrainId &&
                           r.ReservationDate.Date == date.Date &&
                           (r.Status == "Pending" || r.Status == "Confirmed") &&
                           ((startTime >= r.StartTime && startTime < r.EndTime) ||
                            (endTime > r.StartTime && endTime <= r.EndTime) ||
                            (startTime <= r.StartTime && endTime >= r.EndTime)));

            if (excludeReservationId.HasValue)
            {
                query = query.Where(r => r.Id != excludeReservationId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> GetActiveReservationsCountAsync(int userId)
        {
            var now = DateTime.Now;
            return await _context.Reservations
                .CountAsync(r => r.UserId == userId &&
                                (r.Status == "Pending" || r.Status == "Confirmed") &&
                                (r.ReservationDate > now.Date ||
                                 (r.ReservationDate == now.Date && r.EndTime > now.TimeOfDay)));
        }

        public async Task<IEnumerable<Reservation>> GetUpcomingReservationsAsync(int userId)
        {
            var now = DateTime.Now;
            return await _context.Reservations
                .Include(r => r.Terrain)
                .Where(r => r.UserId == userId &&
                           (r.Status == "Pending" || r.Status == "Confirmed") &&
                           (r.ReservationDate > now.Date ||
                            (r.ReservationDate == now.Date && r.EndTime > now.TimeOfDay)))
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Reservation>> GetPastReservationsAsync(int userId)
        {
            var now = DateTime.Now;
            return await _context.Reservations
                .Include(r => r.Terrain)
                .Where(r => r.UserId == userId &&
                           (r.ReservationDate < now.Date ||
                            (r.ReservationDate == now.Date && r.EndTime <= now.TimeOfDay)))
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.StartTime)
                .ToListAsync();
        }
    }
}