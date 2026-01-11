using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Services.Interfaces;

namespace SportEase.Web.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;
        private readonly ITerrainRepository _terrainRepository;
        private const int MaxReservationsPerUser = 3;
        private const int MaxReservationHours = 3;
        private const int MinReservationHours = 1;
        private const int MaxAdvanceBookingDays = 30;
        private const int CancellationDeadlineHours = 24;

        public ReservationService(
            IReservationRepository reservationRepository,
            ITerrainRepository terrainRepository)
        {
            _reservationRepository = reservationRepository;
            _terrainRepository = terrainRepository;
        }

        public async Task<Reservation?> GetByIdAsync(int id)
        {
            return await _reservationRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(int userId)
        {
            return await _reservationRepository.GetByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Reservation>> GetByTerrainIdAsync(int terrainId)
        {
            return await _reservationRepository.GetByTerrainIdAsync(terrainId);
        }

        public async Task<IEnumerable<Reservation>> GetByAdminIdAsync(int adminId)
        {
            return await _reservationRepository.GetByAdminIdAsync(adminId);
        }

        public async Task<Reservation> CreateAsync(CreateReservationViewModel model, int userId)
        {
            // Validate terrain exists
            var terrain = await _terrainRepository.GetByIdAsync(model.TerrainId);
            if (terrain == null || !terrain.IsActive)
            {
                throw new InvalidOperationException("Ce terrain n'est pas disponible");
            }

            // Validate date
            if (model.ReservationDate.Date < DateTime.Today)
            {
                throw new InvalidOperationException("Impossible de réserver dans le passé");
            }

            if (model.ReservationDate.Date > DateTime.Today.AddDays(MaxAdvanceBookingDays))
            {
                throw new InvalidOperationException($"Réservation possible jusqu'à {MaxAdvanceBookingDays} jours à l'avance");
            }

            // Validate time
            if (model.StartTime < terrain.OpeningTime || model.EndTime > terrain.ClosingTime)
            {
                throw new InvalidOperationException($"Le terrain est ouvert de {terrain.OpeningTime:hh\\:mm} à {terrain.ClosingTime:hh\\:mm}");
            }

            if (model.EndTime <= model.StartTime)
            {
                throw new InvalidOperationException("L'heure de fin doit être après l'heure de début");
            }

            // Validate duration
            var duration = (model.EndTime - model.StartTime).TotalHours;
            if (duration < MinReservationHours)
            {
                throw new InvalidOperationException($"Durée minimum: {MinReservationHours} heure(s)");
            }

            if (duration > MaxReservationHours)
            {
                throw new InvalidOperationException($"Durée maximum: {MaxReservationHours} heures consécutives");
            }

            // Check for conflicts
            var hasConflict = await _reservationRepository.HasConflictAsync(
                model.TerrainId,
                model.ReservationDate,
                model.StartTime,
                model.EndTime
            );

            if (hasConflict)
            {
                throw new InvalidOperationException("Ce créneau horaire n'est plus disponible");
            }

            // Check user's active reservations limit
            var activeReservationsCount = await _reservationRepository.GetActiveReservationsCountAsync(userId);
            if (activeReservationsCount >= MaxReservationsPerUser)
            {
                throw new InvalidOperationException($"Vous ne pouvez avoir que {MaxReservationsPerUser} réservations actives maximum");
            }

            // Calculate total price
            var totalPrice = terrain.PricePerHour * (decimal)duration;

            // Create reservation
            var reservation = new Reservation
            {
                TerrainId = model.TerrainId,
                UserId = userId,
                ReservationDate = model.ReservationDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                TotalPrice = totalPrice,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return await _reservationRepository.AddAsync(reservation);
        }

        public async Task<bool> CancelAsync(int id, int userId, string reason)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                return false;
            }

            // Check if user owns this reservation or is admin
            if (reservation.UserId != userId && reservation.Terrain.AdminId != userId)
            {
                throw new UnauthorizedAccessException("Non autorisé");
            }

            // Check if already cancelled or completed
            if (reservation.Status == "Cancelled" || reservation.Status == "Completed")
            {
                throw new InvalidOperationException("Cette réservation ne peut pas être annulée");
            }

            // Check cancellation deadline (24 hours before)
            var reservationDateTime = reservation.ReservationDate.Add(reservation.StartTime);
            var hoursUntilReservation = (reservationDateTime - DateTime.Now).TotalHours;

            if (hoursUntilReservation < CancellationDeadlineHours)
            {
                throw new InvalidOperationException($"Annulation possible jusqu'à {CancellationDeadlineHours}h avant la réservation");
            }

            reservation.Status = "Cancelled";
            reservation.CancellationReason = reason;
            reservation.UpdatedAt = DateTime.Now;

            await _reservationRepository.UpdateAsync(reservation);
            return true;
        }

        public async Task<bool> ConfirmAsync(int id, int adminId)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                return false;
            }

            // Check if admin owns the terrain
            if (reservation.Terrain.AdminId != adminId)
            {
                throw new UnauthorizedAccessException("Non autorisé");
            }

            if (reservation.Status != "Pending")
            {
                throw new InvalidOperationException("Seules les réservations en attente peuvent être confirmées");
            }

            reservation.Status = "Confirmed";
            reservation.UpdatedAt = DateTime.Now;

            await _reservationRepository.UpdateAsync(reservation);
            return true;
        }

        public async Task<bool> RejectAsync(int id, int adminId, string reason)
        {
            var reservation = await _reservationRepository.GetByIdAsync(id);

            if (reservation == null)
            {
                return false;
            }

            // Check if admin owns the terrain
            if (reservation.Terrain.AdminId != adminId)
            {
                throw new UnauthorizedAccessException("Non autorisé");
            }

            if (reservation.Status != "Pending")
            {
                throw new InvalidOperationException("Seules les réservations en attente peuvent être refusées");
            }

            reservation.Status = "Cancelled";
            reservation.CancellationReason = $"Refusée par l'administrateur: {reason}";
            reservation.UpdatedAt = DateTime.Now;

            await _reservationRepository.UpdateAsync(reservation);
            return true;
        }

        public async Task<bool> ValidateReservationAsync(int terrainId, DateTime date, TimeSpan startTime, TimeSpan endTime, int? excludeReservationId = null)
        {
            return !await _reservationRepository.HasConflictAsync(terrainId, date, startTime, endTime, excludeReservationId);
        }

        public async Task<Dictionary<TimeSpan, bool>> GetAvailableSlotsAsync(int terrainId, DateTime date)
        {
            var terrain = await _terrainRepository.GetByIdAsync(terrainId);
            if (terrain == null)
            {
                return new Dictionary<TimeSpan, bool>();
            }

            // Optimization: Fetch all reservations for the day in one query
            var reservations = await _reservationRepository.GetByTerrainAndDateAsync(terrainId, date);

            var slots = new Dictionary<TimeSpan, bool>();
            var currentTime = terrain.OpeningTime;

            while (currentTime < terrain.ClosingTime)
            {
                var slotEndTime = currentTime.Add(TimeSpan.FromHours(1));

                // Check availability in memory
                var hasConflict = reservations.Any(r => 
                    (currentTime >= r.StartTime && currentTime < r.EndTime) ||
                    (slotEndTime > r.StartTime && slotEndTime <= r.EndTime) ||
                    (currentTime <= r.StartTime && slotEndTime >= r.EndTime));

                slots[currentTime] = !hasConflict;
                currentTime = slotEndTime;
            }

            return slots;
        }
    }
}