using Microsoft.AspNetCore.Mvc;
using SportEase.Web.Services.Interfaces;
using SportEase.Web.Models.ViewModels;

namespace SportEase.Web.Controllers
{
    public class TerrainController : Controller
    {
        private readonly ITerrainService _terrainService;
        private readonly IReservationService _reservationService;

        public TerrainController(
            ITerrainService terrainService,
            IReservationService reservationService)
        {
            _terrainService = terrainService;
            _reservationService = reservationService;
        }

        // GET: /Terrain
        public async Task<IActionResult> Index()
        {
            var terrains = await _terrainService.GetAllActiveAsync();
            return View(terrains);
        }

        // GET: /Terrain/Search
        [HttpGet]
        public async Task<IActionResult> Search(TerrainSearchViewModel model)
        {
            if (model == null)
            {
                model = new TerrainSearchViewModel();
            }

            var results = await _terrainService.SearchAsync(model);

            // Apply sorting
            results = model.SortBy switch
            {
                "PriceAsc" => results.OrderBy(t => t.PricePerHour),
                "PriceDesc" => results.OrderByDescending(t => t.PricePerHour),
                "Capacity" => results.OrderByDescending(t => t.Capacity),
                _ => results.OrderBy(t => t.Name)
            };

            model.Results = results.ToList();
            model.TotalResults = model.Results.Count;

            return View(model);
        }

        // GET: /Terrain/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var terrain = await _terrainService.GetByIdAsync(id);

            if (terrain == null || !terrain.IsActive)
            {
                TempData["ErrorMessage"] = "Terrain introuvable";
                return RedirectToAction("Index");
            }

            var viewModel = new TerrainDetailsViewModel
            {
                Terrain = terrain,
                CanReserve = IsUserLoggedIn() && !IsAdmin()
            };

            // Get available slots for today
            var today = DateTime.Today;
            viewModel.TodaySlots = await _terrainService.GetAvailableSlotsAsync(id, today);

            // Get next 7 days for calendar
            viewModel.AvailableDates = Enumerable.Range(0, 7)
                .Select(offset => today.AddDays(offset))
                .ToList();

            // Get upcoming reservations for this terrain (for display)
            var allReservations = await _reservationService.GetByTerrainIdAsync(id);
            viewModel.UpcomingReservations = allReservations
                .Where(r => (r.Status == "Confirmed" || r.Status == "Pending") &&
                           r.ReservationDate >= today)
                .OrderBy(r => r.ReservationDate)
                .ThenBy(r => r.StartTime)
                .Take(5)
                .ToList();

            return View(viewModel);
        }

        // GET: /Terrain/GetSlots (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetSlots(int terrainId, DateTime date)
        {
            var slots = await _terrainService.GetAvailableSlotsAsync(terrainId, date);

            var result = slots.Select(s => new
            {
                time = s.Key.ToString(@"hh\:mm"),
                available = s.Value
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