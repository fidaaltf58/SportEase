using System;

namespace SportEase.ViewModels
{
    public class ReservationDetailsViewModel
    {
        public ReservationViewModel Reservation { get; set; }
        public bool CanCancel { get; set; }
        public int HoursUntilReservation { get; set; }
    }

    // Exemple de classe ReservationViewModel pour éviter d'autres erreurs de compilation.
    // À adapter selon votre modèle réel.
    public class ReservationViewModel
    {
        public int Id { get; set; }
        public string StatusBadgeClass { get; set; }
        public string StatusDisplayText { get; set; }
        public string FormattedDate { get; set; }
        public string TimeSlot { get; set; }
        public int DurationHours { get; set; }
        public decimal TotalPrice { get; set; }
        public string CancellationReason { get; set; }
        public TerrainViewModel Terrain { get; set; }
        public int TerrainId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsPending { get; set; }
        public bool IsConfirmed { get; set; }
    }

    public class TerrainViewModel
    {
        public string Name { get; set; }
        public string FullAddress { get; set; }
        public string SportType { get; set; }
        public int Capacity { get; set; }
        public AdminViewModel Admin { get; set; }
    }

    public class AdminViewModel
    {
        public string FullName { get; set; }
        public string Phone { get; set; }
    }
}