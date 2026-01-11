using System.ComponentModel.DataAnnotations;
using SportEase.Web.Models.Entities;
using Microsoft.AspNetCore.Http;

namespace SportEase.Web.Models.ViewModels
{
    public class TerrainSearchViewModel
    {
        [Display(Name = "Type de sport")]
        public string? SportType { get; set; }

        [Display(Name = "Ville")]
        public string? City { get; set; }

        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Display(Name = "Heure")]
        [DataType(DataType.Time)]
        public TimeSpan? Time { get; set; }

        [Display(Name = "Prix minimum")]
        [Range(0, 10000)]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Prix maximum")]
        [Range(0, 10000)]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Capacité minimale")]
        [Range(2, 100)]
        public int? MinCapacity { get; set; }

        [Display(Name = "Avec éclairage")]
        public bool? HasLighting { get; set; }

        [Display(Name = "Avec parking")]
        public bool? HasParking { get; set; }

        [Display(Name = "Avec vestiaires")]
        public bool? HasChangingRoom { get; set; }

        [Display(Name = "Trier par")]
        public string SortBy { get; set; } = "Name"; // Name, PriceAsc, PriceDesc, Capacity

        public List<Terrain> Results { get; set; } = new List<Terrain>();
        public int TotalResults { get; set; }
    }

    public class TerrainDetailsViewModel
    {
        public Terrain Terrain { get; set; } = null!;
        public List<DateTime> AvailableDates { get; set; } = new List<DateTime>();
        public Dictionary<TimeSpan, bool> TodaySlots { get; set; } = new Dictionary<TimeSpan, bool>();
        public bool CanReserve { get; set; }
        public List<Reservation> UpcomingReservations { get; set; } = new List<Reservation>();
    }

    public class CreateTerrainViewModel
    {
        [Required(ErrorMessage = "Le nom du terrain est requis")]
        [StringLength(200)]
        [Display(Name = "Nom du terrain")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le type de sport est requis")]
        [Display(Name = "Type de sport")]
        public string SportType { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'adresse est requise")]
        [StringLength(500)]
        [Display(Name = "Adresse")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "La ville est requise")]
        [StringLength(100)]
        [Display(Name = "Ville")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "La capacité est requise")]
        [Range(2, 100, ErrorMessage = "La capacité doit être entre 2 et 100")]
        [Display(Name = "Capacité (nombre de joueurs)")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Le prix par heure est requis")]
        [Range(0.01, 10000, ErrorMessage = "Le prix doit être entre 0.01 et 10000")]
        [Display(Name = "Prix par heure (DT)")]
        public decimal PricePerHour { get; set; }

        [StringLength(2000)]
        [Display(Name = "Description")]
        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        [Display(Name = "Photo du terrain")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Éclairage disponible")]
        public bool HasLighting { get; set; }

        [Display(Name = "Parking disponible")]
        public bool HasParking { get; set; }

        [Display(Name = "Vestiaires disponibles")]
        public bool HasChangingRoom { get; set; }

        [Required]
        [Display(Name = "Heure d'ouverture")]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(8, 0, 0);

        [Required]
        [Display(Name = "Heure de fermeture")]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0);

        public List<string> SportTypes { get; set; } = new List<string>
        {
            "Football", "Basketball", "Tennis", "Volleyball", "Handball", "Badminton"
        };

        public List<string> Cities { get; set; } = new List<string>
        {
            "Tunis", "Ariana", "Ben Arous", "Manouba", "La Marsa", "Carthage",
            "Sidi Bou Said", "Gammarth", "Lac 1", "Lac 2", "Les Berges du Lac"
        };
    }

    public class EditTerrainViewModel : CreateTerrainViewModel
    {
        public int Id { get; set; }
        public string? CurrentImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TerrainListViewModel
    {
        public List<Terrain> Terrains { get; set; } = new List<Terrain>();
        public int TotalTerrains { get; set; }
        public int ActiveTerrains { get; set; }
        public int InactiveTerrains { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}