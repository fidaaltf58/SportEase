using Microsoft.AspNetCore.Mvc;
using SportEase.Web.Services.Interfaces;
using SportEase.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR; // Add SignalR
using SportEase.Web.Hubs;

namespace SportEase.Web.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly ITerrainService _terrainService;
        private readonly IStatisticsService _statisticsService;
        private readonly IHubContext<ReservationHub> _hubContext;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(
            IReservationService reservationService,
            ITerrainService terrainService,
            IStatisticsService statisticsService,
            IHubContext<ReservationHub> hubContext,
            ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _terrainService = terrainService;
            _statisticsService = statisticsService;
            _hubContext = hubContext;
            _logger = logger;
        }

        // GET: /Reservation/Create/5
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            if (!IsUserLoggedIn())
            {
                TempData["ErrorMessage"] = "Veuillez vous connecter pour réserver";
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Reservation/Create/{id}" });
            }

            if (IsAdmin())
            {
                TempData["ErrorMessage"] = "Les administrateurs ne peuvent pas faire de réservations";
                return RedirectToAction("Details", "Terrain", new { id });
            }

            var terrain = await _terrainService.GetByIdAsync(id);

            if (terrain == null || !terrain.IsActive)
            {
                TempData["ErrorMessage"] = "Terrain introuvable";
                return RedirectToAction("Index", "Terrain");
            }

            var model = new CreateReservationViewModel
            {
                TerrainId = id,
                Terrain = terrain,
                ReservationDate = DateTime.Today.AddDays(1)
            };

            // Get available slots for tomorrow
            model.AvailableSlots = await _reservationService.GetAvailableSlotsAsync(id, model.ReservationDate);

            // Get next 30 days
            model.Next30Days = Enumerable.Range(0, 30)
                .Select(offset => DateTime.Today.AddDays(offset + 1))
                .ToList();

            return View(model);
        }

        // POST: /Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationViewModel model)
        {
            _logger.LogInformation("Create POST called. TerrainId: {TerrainId}, Date: {Date}", model.TerrainId, model.ReservationDate);

            if (!IsUserLoggedIn())
            {
                _logger.LogWarning("User not logged in.");
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState Invalid.");
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogWarning("Error in {Field}: {Error}", state.Key, error.ErrorMessage);
                    }
                }

                var terrain = await _terrainService.GetByIdAsync(model.TerrainId);
                model.Terrain = terrain!;
                model.AvailableSlots = await _reservationService.GetAvailableSlotsAsync(
                    model.TerrainId,
                    model.ReservationDate);
                return View(model);
            }

            try
            {
                var userId = GetCurrentUserId()!.Value;
                _logger.LogInformation("Creating reservation for User {UserId}", userId);
                var reservation = await _reservationService.CreateAsync(model, userId);
                _logger.LogInformation("Reservation created: {ReservationId}", reservation.Id);

                // Wait for DB to commit and notify clients via SignalR
                await Task.Delay(200);
                await _hubContext.Clients.Group("Admins").SendAsync("ReservationUpdated", "Une nouvelle réservation a été effectuée !");

                TempData["SuccessMessage"] = "Réservation créée avec succès! En attente de confirmation.";
                return RedirectToAction("MyBookings");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "InvalidOperationException during creation: {Message}", ex.Message);
                ModelState.AddModelError(string.Empty, ex.Message);
                var terrain = await _terrainService.GetByIdAsync(model.TerrainId);
                model.Terrain = terrain!;
                model.AvailableSlots = await _reservationService.GetAvailableSlotsAsync(
                    model.TerrainId,
                    model.ReservationDate);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during reservation creation");
                ModelState.AddModelError(string.Empty, "Une erreur est survenue");
                var terrain = await _terrainService.GetByIdAsync(model.TerrainId);
                model.Terrain = terrain!;
                return View(model);
            }
        }

        // GET: /Reservation/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId()!.Value;
            var allReservations = (await _reservationService.GetByUserIdAsync(userId)).ToList();
            var now = DateTime.Now;

            var viewModel = new ReservationListViewModel
            {
                UpcomingReservations = allReservations
                    .Where(r => r.Status == "Confirmed" &&
                               (r.ReservationDate > now.Date ||
                                (r.ReservationDate == now.Date && r.EndTime > now.TimeOfDay)))
                    .OrderBy(r => r.ReservationDate)
                    .ThenBy(r => r.StartTime)
                    .ToList(),

                PastReservations = allReservations
                    .Where(r => r.ReservationDate < now.Date ||
                               (r.ReservationDate == now.Date && r.EndTime <= now.TimeOfDay))
                    .OrderByDescending(r => r.ReservationDate)
                    .ThenByDescending(r => r.StartTime)
                    .ToList(),

                PendingReservations = allReservations
                    .Where(r => r.Status == "Pending")
                    .OrderBy(r => r.ReservationDate)
                    .ToList(),

                CancelledReservations = allReservations
                    .Where(r => r.Status == "Cancelled")
                    .OrderByDescending(r => r.UpdatedAt)
                    .ToList(),

                TotalReservations = allReservations.Count,
                ActiveReservations = allReservations.Count(r => r.Status == "Pending" || r.Status == "Confirmed"),
                TotalSpent = allReservations.Where(r => r.Status != "Cancelled").Sum(r => r.TotalPrice)
            };

            return View(viewModel);
        }

        // GET: /Reservation/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _reservationService.GetByIdAsync(id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Réservation introuvable";
                return RedirectToAction("MyBookings");
            }

            var userId = GetCurrentUserId()!.Value;

            // Check authorization
            if (reservation.UserId != userId && reservation.Terrain.AdminId != userId)
            {
                TempData["ErrorMessage"] = "Non autorisé";
                return RedirectToAction("MyBookings");
            }

            var reservationDateTime = reservation.ReservationDate.Add(reservation.StartTime);
            var hoursUntilReservation = (int)(reservationDateTime - DateTime.Now).TotalHours;

            var viewModel = new ReservationDetailsViewModel
            {
                Reservation = reservation,
                CanCancel = reservation.CanBeCancelled,
                CanModify = reservation.CanBeModified,
                HoursUntilReservation = hoursUntilReservation
            };

            return View(viewModel);
        }

        // GET: /Reservation/Cancel/5
        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var reservation = await _reservationService.GetByIdAsync(id);

            if (reservation == null)
            {
                TempData["ErrorMessage"] = "Réservation introuvable";
                return RedirectToAction("MyBookings");
            }

            var userId = GetCurrentUserId()!.Value;

            if (reservation.UserId != userId)
            {
                TempData["ErrorMessage"] = "Non autorisé";
                return RedirectToAction("MyBookings");
            }

            if (!reservation.CanBeCancelled)
            {
                TempData["ErrorMessage"] = "Cette réservation ne peut plus être annulée";
                return RedirectToAction("Details", new { id });
            }

            var viewModel = new CancelReservationViewModel
            {
                ReservationId = id,
                Reservation = reservation
            };

            return View(viewModel);
        }

        // POST: /Reservation/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(CancelReservationViewModel model)
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            if (!ModelState.IsValid)
            {
                var reservation = await _reservationService.GetByIdAsync(model.ReservationId);
                model.Reservation = reservation!;
                return View("Cancel", model);
            }

            try
            {
                var userId = GetCurrentUserId()!.Value;
                var success = await _reservationService.CancelAsync(
                    model.ReservationId,
                    userId,
                    model.CancellationReason);

                if (success)
                {
                    // Notify clients via SignalR
                    await _hubContext.Clients.Group("Admins").SendAsync("ReservationUpdated", "Une réservation a été annulée.");

                    TempData["SuccessMessage"] = "Réservation annulée avec succès";
                    return RedirectToAction("MyBookings");
                }

                TempData["ErrorMessage"] = "Erreur lors de l'annulation";
                return RedirectToAction("Details", new { id = model.ReservationId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", new { id = model.ReservationId });
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Non autorisé";
                return RedirectToAction("MyBookings");
            }
        }

        // GET: /Reservation/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsUserLoggedIn())
            {
                return RedirectToAction("Login", "Account");
            }

            var userId = GetCurrentUserId()!.Value;
            var dashboard = await _statisticsService.GetUserDashboardAsync(userId);

            return View(dashboard);
        }

        // GET: /Reservation/GetAvailableSlots (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int terrainId, DateTime date)
        {
            var slots = await _reservationService.GetAvailableSlotsAsync(terrainId, date);

            var result = slots.Select(s => new
            {
                time = s.Key.ToString(@"hh\:mm"),
                available = s.Value,
                displayTime = $"{s.Key:hh\\:mm} - {s.Key.Add(TimeSpan.FromHours(1)):hh\\:mm}"
            });

            return Json(result);
        }

        // Helper Methods
        private bool IsUserLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}