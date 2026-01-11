using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Services.Interfaces;
namespace SportEase.Web.Services.Implementations
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITerrainRepository _terrainRepository;

        public StatisticsService(
            IReservationRepository reservationRepository,
            ITerrainRepository terrainRepository)
        {
            _reservationRepository = reservationRepository;
            _terrainRepository = terrainRepository;
        }

        public async Task<DashboardViewModel> GetUserDashboardAsync(int userId)
        {
            var allReservations = await _reservationRepository.GetByUserIdAsync(userId);
            var now = DateTime.Now;

            var upcomingReservations = allReservations
                .Where(r => (r.Status == "Confirmed" || r.Status == "Pending") &&
                           (r.ReservationDate > now.Date ||
                            (r.ReservationDate == now.Date && r.EndTime > now.TimeOfDay)))
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .ToList();

            var completedReservations = allReservations
                .Where(r => r.Status == "Completed")
                .ToList();

            var recentReservations = allReservations
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToList();

            var nextReservations = upcomingReservations
                .Take(3)
                .ToList();

            return new DashboardViewModel
            {
                TotalReservations = allReservations.Count(),
                UpcomingReservations = upcomingReservations.Count,
                CompletedReservations = completedReservations.Count,
                TotalSpent = allReservations.Where(r => r.Status != "Cancelled").Sum(r => r.TotalPrice),
                NextReservations = nextReservations,
                RecentReservations = recentReservations
            };
        }

        public async Task<DashboardViewModel> GetAdminDashboardAsync(int adminId)
        {
            var terrains = (await _terrainRepository.GetByAdminIdAsync(adminId)).ToList();
            var allReservations = await _reservationRepository.GetByAdminIdAsync(adminId);
            var now = DateTime.Now;

            var todayReservations = allReservations
                .Where(r => r.ReservationDate.Date == now.Date)
                .ToList();

            var pendingReservations = allReservations
                .Where(r => r.Status == "Pending")
                .ToList();

            var confirmedReservations = allReservations
                .Where(r => r.Status == "Confirmed" || r.Status == "Completed")
                .ToList();

            // Monthly revenue (current month)
            var monthlyRevenue = allReservations
                .Where(r => r.CreatedAt.Month == now.Month &&
                           r.CreatedAt.Year == now.Year &&
                           (r.Status == "Confirmed" || r.Status == "Completed"))
                .Sum(r => r.TotalPrice);

            // Total revenue (all time)
            var totalRevenue = allReservations
                .Where(r => r.Status == "Confirmed" || r.Status == "Completed")
                .Sum(r => r.TotalPrice);

            // Get reservations by month for chart
            var reservationsByMonth = await GetReservationsByMonthAsync(adminId, 6);
            var revenueByMonth = await GetRevenueByMonthAsync(adminId, 6);

            // Top terrains by reservations
            var topTerrains = terrains
                .OrderByDescending(t => t.Reservations.Count)
                .Take(5)
                .ToList();

            return new DashboardViewModel
            {
                TotalTerrains = terrains.Count,
                ActiveTerrains = terrains.Count(t => t.IsActive),
                TodayReservations = todayReservations.Count,
                PendingReservations = pendingReservations.Count,
                MonthlyRevenue = monthlyRevenue,
                TotalRevenue = totalRevenue,
                ReservationsByMonth = reservationsByMonth,
                RevenueByMonth = revenueByMonth,
                TopTerrains = topTerrains
            };
        }

        public async Task<Dictionary<string, int>> GetReservationsByMonthAsync(int adminId, int months = 6)
        {
            var allReservations = await _reservationRepository.GetByAdminIdAsync(adminId);
            var now = DateTime.Now;
            var result = new Dictionary<string, int>();

            for (int i = months - 1; i >= 0; i--)
            {
                var targetDate = now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy");

                var count = allReservations
                    .Count(r => r.CreatedAt.Month == targetDate.Month &&
                               r.CreatedAt.Year == targetDate.Year);

                result[monthName] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMonthAsync(int adminId, int months = 6)
        {
            var allReservations = await _reservationRepository.GetByAdminIdAsync(adminId);
            var now = DateTime.Now;
            var result = new Dictionary<string, decimal>();

            for (int i = months - 1; i >= 0; i--)
            {
                var targetDate = now.AddMonths(-i);
                var monthName = targetDate.ToString("MMM yyyy");

                var revenue = allReservations
                    .Where(r => r.CreatedAt.Month == targetDate.Month &&
                               r.CreatedAt.Year == targetDate.Year &&
                               (r.Status == "Confirmed" || r.Status == "Completed"))
                    .Sum(r => r.TotalPrice);

                result[monthName] = revenue;
            }

            return result;
        }
    }
}