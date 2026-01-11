using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportEase.Web.Models.Entities
{
    [Table("Reservations")]
    public class Reservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Terrain")]
        public int TerrainId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Terrain Terrain { get; set; } = null!;
        public virtual User User { get; set; } = null!;

        // Computed Properties
        [NotMapped]
        public int DurationHours => (int)(EndTime - StartTime).TotalHours;

        [NotMapped]
        public bool IsPending => Status == "Pending";

        [NotMapped]
        public bool IsConfirmed => Status == "Confirmed";

        [NotMapped]
        public bool IsCancelled => Status == "Cancelled";

        [NotMapped]
        public bool IsCompleted => Status == "Completed";

        [NotMapped]
        public bool CanBeCancelled
        {
            get
            {
                if (IsCancelled || IsCompleted) return false;
                var reservationDateTime = ReservationDate.Add(StartTime);
                return reservationDateTime.Subtract(DateTime.Now).TotalHours >= 24;
            }
        }

        [NotMapped]
        public bool CanBeModified => CanBeCancelled;

        [NotMapped]
        public string TimeSlot => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";

        [NotMapped]
        public string FormattedDate => ReservationDate.ToString("dd/MM/yyyy");

        [NotMapped]
        public string StatusBadgeClass
        {
            get
            {
                return Status switch
                {
                    "Pending" => "badge bg-warning",
                    "Confirmed" => "badge bg-success",
                    "Cancelled" => "badge bg-danger",
                    "Completed" => "badge bg-secondary",
                    _ => "badge bg-primary"
                };
            }
        }

        [NotMapped]
        public string StatusDisplayText
        {
            get
            {
                return Status switch
                {
                    "Pending" => "En attente",
                    "Confirmed" => "Confirmée",
                    "Cancelled" => "Annulée",
                    "Completed" => "Terminée",
                    _ => Status
                };
            }
        }
    }
}