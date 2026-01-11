using System.ComponentModel.DataAnnotations;
using SportEase.Web.Models.Entities;

namespace SportEase.Web.Models.ViewModels
{
    public class CreateReservationViewModel
    {
        public int TerrainId { get; set; }
        public Terrain Terrain { get; set; } = null!;

        [Required(ErrorMessage = "La date de réservation est requise")]
        [Display(Name = "Date de réservation")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "L'heure de début est requise")]
        [Display(Name = "Heure de début")]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "L'heure de fin est requise")]
        [Display(Name = "Heure de fin")]
        public TimeSpan EndTime { get; set; }

        public decimal TotalPrice { get; set; }
        public int DurationHours { get; set; }

        public Dictionary<TimeSpan, bool> AvailableSlots { get; set; } = new Dictionary<TimeSpan, bool>();
        public List<DateTime> Next30Days { get; set; } = new List<DateTime>();
    }

    public class ReservationListViewModel
    {
        public List<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
        public List<Reservation> PastReservations { get; set; } = new List<Reservation>();
        public List<Reservation> PendingReservations { get; set; } = new List<Reservation>();
        public List<Reservation> CancelledReservations { get; set; } = new List<Reservation>();

        public int TotalReservations { get; set; }
        public int ActiveReservations { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class ReservationDetailsViewModel
    {
        public Reservation Reservation { get; set; } = null!;
        public bool CanCancel { get; set; }
        public bool CanModify { get; set; }
        public int HoursUntilReservation { get; set; }
    }

    public class AdminReservationListViewModel
    {
        public List<Reservation> AllReservations { get; set; } = new List<Reservation>();
        public List<Reservation> TodayReservations { get; set; } = new List<Reservation>();
        public List<Reservation> PendingReservations { get; set; } = new List<Reservation>();

        [Display(Name = "Filtrer par terrain")]
        public int? FilterTerrainId { get; set; }

        [Display(Name = "Filtrer par statut")]
        public string? FilterStatus { get; set; }

        [Display(Name = "Date début")]
        [DataType(DataType.Date)]
        public DateTime? FilterStartDate { get; set; }

        [Display(Name = "Date fin")]
        [DataType(DataType.Date)]
        public DateTime? FilterEndDate { get; set; }

        public List<Terrain> AdminTerrains { get; set; } = new List<Terrain>();
        public int TotalReservations { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class CancelReservationViewModel
    {
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "Veuillez indiquer la raison de l'annulation")]
        [StringLength(500, ErrorMessage = "La raison ne doit pas dépasser 500 caractères")]
        [Display(Name = "Raison de l'annulation")]
        [DataType(DataType.MultilineText)]
        public string CancellationReason { get; set; } = string.Empty;

        public Reservation Reservation { get; set; } = null!;
    }

    public class DashboardViewModel
    {
        // User Dashboard
        public int TotalReservations { get; set; }
        public int UpcomingReservations { get; set; }
        public int CompletedReservations { get; set; }
        public decimal TotalSpent { get; set; }
        public List<Reservation> NextReservations { get; set; } = new List<Reservation>();
        public List<Reservation> RecentReservations { get; set; } = new List<Reservation>();

        // Admin Dashboard
        public int TotalTerrains { get; set; }
        public int ActiveTerrains { get; set; }
        public int TodayReservations { get; set; }
        public int PendingReservations { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal TotalRevenue { get; set; }

        public Dictionary<string, int> ReservationsByMonth { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByMonth { get; set; } = new Dictionary<string, decimal>();
        public List<Terrain> TopTerrains { get; set; } = new List<Terrain>();
    }
}