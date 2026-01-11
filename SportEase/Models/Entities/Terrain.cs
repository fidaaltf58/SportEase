using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportEase.Web.Models.Entities
{
    [Table("Terrains")]
    public class Terrain
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Admin")]
        public int AdminId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string SportType { get; set; } = string.Empty; // Football, Basketball, Tennis, Volleyball

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [Range(2, 100)]
        public int Capacity { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 10000)]
        public decimal PricePerHour { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool HasLighting { get; set; } = false;

        public bool HasParking { get; set; } = false;

        public bool HasChangingRoom { get; set; } = false;

        [Required]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(8, 0, 0); // 08:00

        [Required]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0); // 22:00

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual User Admin { get; set; } = null!;
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        // Computed Properties
        [NotMapped]
        public string FeaturesDisplay
        {
            get
            {
                var features = new List<string>();
                if (HasLighting) features.Add("Éclairage");
                if (HasParking) features.Add("Parking");
                if (HasChangingRoom) features.Add("Vestiaires");
                return string.Join(", ", features);
            }
        }

        [NotMapped]
        public string FullAddress => $"{Address}, {City}";
    }
}