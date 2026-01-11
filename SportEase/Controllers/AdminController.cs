using Microsoft.AspNetCore.Mvc;
using SportEase.Web.Services.Interfaces;
using SportEase.Web.Models.ViewModels;
using Microsoft.AspNetCore.SignalR; // Add Import
using SportEase.Web.Hubs; // Add Import

namespace SportEase.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly ITerrainService _terrainService;
        private readonly IReservationService _reservationService;
        private readonly IStatisticsService _statisticsService;
        private readonly IHubContext<ReservationHub> _hubContext; // Add field

        public AdminController(
            ITerrainService terrainService,
            IReservationService reservationService,
            IStatisticsService statisticsService,
            IHubContext<ReservationHub> hubContext) // Inject HubContext
        {
            _terrainService = terrainService;
            _reservationService = reservationService;
            _statisticsService = statisticsService;
            _hubContext = hubContext;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                TempData["ErrorMessage"] = "Accès non autorisé";
                return RedirectToAction("Index", "Home");
            }

            var adminId = GetCurrentUserId()!.Value;
            var dashboard = await _statisticsService.GetAdminDashboardAsync(adminId);

            return View(dashboard);
        }

        #region Terrain Management

        // GET: /Admin/Terrains
        public async Task<IActionResult> Terrains()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var adminId = GetCurrentUserId()!.Value;
            var terrains = await _terrainService.GetByAdminIdAsync(adminId);

            var viewModel = new TerrainListViewModel
            {
                Terrains = terrains.ToList(),
                TotalTerrains = terrains.Count(),
                ActiveTerrains = terrains.Count(t => t.IsActive),
                InactiveTerrains = terrains.Count(t => !t.IsActive)
            };

            return View(viewModel);
        }

        // GET: /Admin/CreateTerrain
        [HttpGet]
        public IActionResult CreateTerrain()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new CreateTerrainViewModel());
        }

        // POST: /Admin/CreateTerrain
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTerrain(CreateTerrainViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var adminId = GetCurrentUserId()!.Value;
                var terrain = await _terrainService.CreateAsync(model, adminId);

                TempData["SuccessMessage"] = "Terrain créé avec succès";
                return RedirectToAction("Terrains");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue");
                return View(model);
            }
        }

        // GET: /Admin/EditTerrain/5
        [HttpGet]
        public async Task<IActionResult> EditTerrain(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var terrain = await _terrainService.GetByIdAsync(id);

            if (terrain == null)
            {
                TempData["ErrorMessage"] = "Terrain introuvable";
                return RedirectToAction("Terrains");
            }

            var adminId = GetCurrentUserId()!.Value;

            if (terrain.AdminId != adminId)
            {
                TempData["ErrorMessage"] = "Non autorisé";
                return RedirectToAction("Terrains");
            }

            var model = new EditTerrainViewModel
            {
                Id = terrain.Id,
                Name = terrain.Name,
                SportType = terrain.SportType,
                Address = terrain.Address,
                City = terrain.City,
                Capacity = terrain.Capacity,
                PricePerHour = terrain.PricePerHour,
                Description = terrain.Description,
                CurrentImageUrl = terrain.ImageUrl,
                HasLighting = terrain.HasLighting,
                HasParking = terrain.HasParking,
                HasChangingRoom = terrain.HasChangingRoom,
                OpeningTime = terrain.OpeningTime,
                ClosingTime = terrain.ClosingTime,
                IsActive = terrain.IsActive
            };

            return View(model);
        }

        // POST: /Admin/EditTerrain
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTerrain(EditTerrainViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var success = await _terrainService.UpdateAsync(model);

                if (success)
                {
                    TempData["SuccessMessage"] = "Terrain mis à jour avec succès";
                    return RedirectToAction("Terrains");
                }

                TempData["ErrorMessage"] = "Erreur lors de la mise à jour";
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Une erreur est survenue");
                return View(model);
            }
        }

        // POST: /Admin/DeleteTerrain/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTerrain(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var adminId = GetCurrentUserId()!.Value;
                var success = await _terrainService.DeleteAsync(id, adminId);

                if (success)
                {
                    TempData["SuccessMessage"] = "Terrain archivé avec succès";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erreur lors de l'archivage";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Une erreur est survenue";
            }

            return RedirectToAction("Terrains");
        }

        #endregion

        #region Reservation Management

        // GET: /Admin/Reservations
        public async Task<IActionResult> Reservations(AdminReservationListViewModel? filter)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            var adminId = GetCurrentUserId()!.Value;
            var allReservations = (await _reservationService.GetByAdminIdAsync(adminId)).ToList();
            var adminTerrains = await _terrainService.GetByAdminIdAsync(adminId);

            // Apply filters
            var filteredReservations = allReservations.AsEnumerable();

            if (filter?.FilterTerrainId.HasValue == true)
            {
                filteredReservations = filteredReservations
                    .Where(r => r.TerrainId == filter.FilterTerrainId.Value);
            }

            if (!string.IsNullOrEmpty(filter?.FilterStatus))
            {
                filteredReservations = filteredReservations
                    .Where(r => r.Status == filter.FilterStatus);
            }

            if (filter?.FilterStartDate.HasValue == true)
            {
                filteredReservations = filteredReservations
                    .Where(r => r.ReservationDate >= filter.FilterStartDate.Value);
            }

            if (filter?.FilterEndDate.HasValue == true)
            {
                filteredReservations = filteredReservations
                    .Where(r => r.ReservationDate <= filter.FilterEndDate.Value);
            }

            var viewModel = new AdminReservationListViewModel
            {
                AllReservations = filteredReservations
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList(),

                TodayReservations = allReservations
                    .Where(r => r.ReservationDate.Date == DateTime.Today)
                    .OrderBy(r => r.StartTime)
                    .ToList(),

                PendingReservations = allReservations
                    .Where(r => r.Status == "Pending")
                    .OrderBy(r => r.ReservationDate)
                    .ToList(),

                AdminTerrains = adminTerrains.ToList(),
                FilterTerrainId = filter?.FilterTerrainId,
                FilterStatus = filter?.FilterStatus,
                FilterStartDate = filter?.FilterStartDate,
                FilterEndDate = filter?.FilterEndDate,

                TotalReservations = allReservations.Count,
                TotalRevenue = allReservations
                    .Where(r => r.Status == "Confirmed" || r.Status == "Completed")
                    .Sum(r => r.TotalPrice)
            };

            return View(viewModel);
        }

        // POST: /Admin/ConfirmReservation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmReservation(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var adminId = GetCurrentUserId()!.Value;
                var success = await _reservationService.ConfirmAsync(id, adminId);

                if (success)
                {
                    // Wait for DB to commit and notify clients via SignalR
                    await Task.Delay(200);
                    // Notify the specific user
                    var reservation = await _reservationService.GetByIdAsync(id);
                    if (reservation != null)
                    {
                        await _hubContext.Clients.Group($"User_{reservation.UserId}").SendAsync("ReservationUpdated", "Votre réservation a été confirmée !");
                    }
                    // Notify other admins to sync their lists
                    await _hubContext.Clients.Group("Admins").SendAsync("ReservationUpdated", "");

                    TempData["SuccessMessage"] = "Réservation confirmée";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erreur lors de la confirmation";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Non autorisé";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Reservations");
        }

        // POST: /Admin/RejectReservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReservation(int id, string reason)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["ErrorMessage"] = "La raison du refus est requise";
                return RedirectToAction("Reservations");
            }

            try
            {
                var adminId = GetCurrentUserId()!.Value;
                var success = await _reservationService.RejectAsync(id, adminId, reason);

                if (success)
                {
                    // Notify clients via SignalR
                    var reservation = await _reservationService.GetByIdAsync(id);
                    if (reservation != null)
                    {
                        await _hubContext.Clients.Group($"User_{reservation.UserId}").SendAsync("ReservationUpdated", "Votre réservation a été refusée.");
                    }
                     // Notify other admins to sync their lists
                    await _hubContext.Clients.Group("Admins").SendAsync("ReservationUpdated", "");

                    TempData["SuccessMessage"] = "Réservation refusée";
                }
                else
                {
                    TempData["ErrorMessage"] = "Erreur lors du refus";
                }
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Non autorisé";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction("Reservations");
        }

        #endregion

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
            return IsUserLoggedIn() && HttpContext.Session.GetString("UserRole") == "Admin";
        }
    }
}