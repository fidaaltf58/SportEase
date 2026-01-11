using Microsoft.AspNetCore.Mvc;
using SportEase.Web.Services.Interfaces;
using SportEase.Web.Models.ViewModels;
using System.Diagnostics;

namespace SportEase.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ITerrainService _terrainService;

        public HomeController(ITerrainService terrainService)
        {
            _terrainService = terrainService;
        }
        public IActionResult Test()
        {
            return View("Index"); // Force le rendu de Index.cshtml
        }
        // GET: /
        public async Task<IActionResult> Index()
        {
            // Get featured/popular terrains for homepage
            var terrains = await _terrainService.GetAllActiveAsync();
            var featuredTerrains = terrains.Take(6).ToList();

            ViewBag.FeaturedTerrains = featuredTerrains;
            ViewBag.TotalTerrains = terrains.Count();

            return View();
        }

        // GET: /Home/About
        public IActionResult About()
        {
            return View();
        }

        // GET: /Home/Contact
        public IActionResult Contact()
        {
            return View();
        }

        // GET: /Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: /Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }

    // Error ViewModel
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}