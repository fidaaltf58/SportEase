
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;
using SportEase.Web.Services.Interfaces;
namespace SportEase.Web.Services.Implementations
{
    public class TerrainService : ITerrainService
    {
        private readonly ITerrainRepository _terrainRepository;
        private readonly IReservationRepository _reservationRepository;
        private readonly IWebHostEnvironment _environment;

        public TerrainService(
            ITerrainRepository terrainRepository,
            IReservationRepository reservationRepository,
            IWebHostEnvironment environment)
        {
            _terrainRepository = terrainRepository;
            _reservationRepository = reservationRepository;
            _environment = environment;
        }

        public async Task<Terrain?> GetByIdAsync(int id)
        {
            return await _terrainRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Terrain>> GetAllAsync()
        {
            return await _terrainRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Terrain>> GetAllActiveAsync()
        {
            return await _terrainRepository.GetAllActiveAsync();
        }

        public async Task<IEnumerable<Terrain>> GetByAdminIdAsync(int adminId)
        {
            return await _terrainRepository.GetByAdminIdAsync(adminId);
        }

        public async Task<Terrain> CreateAsync(CreateTerrainViewModel model, int adminId)
        {
            // Validate times
            if (model.ClosingTime <= model.OpeningTime)
            {
                throw new InvalidOperationException("L'heure de fermeture doit être après l'heure d'ouverture");
            }

            // Save image if provided
            string? imageUrl = null;
            if (model.ImageFile != null)
            {
                imageUrl = await SaveImageAsync(model.ImageFile);
            }

            var terrain = new Terrain
            {
                AdminId = adminId,
                Name = model.Name,
                SportType = model.SportType,
                Address = model.Address,
                City = model.City,
                Capacity = model.Capacity,
                PricePerHour = model.PricePerHour,
                Description = model.Description,
                ImageUrl = imageUrl,
                HasLighting = model.HasLighting,
                HasParking = model.HasParking,
                HasChangingRoom = model.HasChangingRoom,
                OpeningTime = model.OpeningTime,
                ClosingTime = model.ClosingTime,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            return await _terrainRepository.AddAsync(terrain);
        }

        public async Task<bool> UpdateAsync(EditTerrainViewModel model)
        {
            var terrain = await _terrainRepository.GetByIdAsync(model.Id);
            if (terrain == null)
            {
                return false;
            }

            // Validate times
            if (model.ClosingTime <= model.OpeningTime)
            {
                throw new InvalidOperationException("L'heure de fermeture doit être après l'heure d'ouverture");
            }

            // Update image if new file provided
            if (model.ImageFile != null)
            {
                terrain.ImageUrl = await SaveImageAsync(model.ImageFile);
            }

            terrain.Name = model.Name;
            terrain.SportType = model.SportType;
            terrain.Address = model.Address;
            terrain.City = model.City;
            terrain.Capacity = model.Capacity;
            terrain.PricePerHour = model.PricePerHour;
            terrain.Description = model.Description;
            terrain.HasLighting = model.HasLighting;
            terrain.HasParking = model.HasParking;
            terrain.HasChangingRoom = model.HasChangingRoom;
            terrain.OpeningTime = model.OpeningTime;
            terrain.ClosingTime = model.ClosingTime;
            terrain.IsActive = model.IsActive;

            await _terrainRepository.UpdateAsync(terrain);
            return true;
        }

        public async Task<bool> DeleteAsync(int id, int adminId)
        {
            var terrain = await _terrainRepository.GetByIdAsync(id);
            if (terrain == null || terrain.AdminId != adminId)
            {
                return false;
            }

            await _terrainRepository.DeleteAsync(id);
            return true;
        }

        public async Task<IEnumerable<Terrain>> SearchAsync(TerrainSearchViewModel model)
        {
            return await _terrainRepository.SearchAsync(
                model.SportType,
                model.City,
                model.MinPrice,
                model.MaxPrice,
                model.MinCapacity,
                model.HasLighting,
                model.HasParking,
                model.HasChangingRoom
            );
        }

        public async Task<string?> SaveImageAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return null;
            }

            // Validate file
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Type de fichier non autorisé. Utilisez: JPG, PNG ou GIF");
            }

            if (file.Length > 5242880) // 5MB
            {
                throw new InvalidOperationException("Le fichier est trop volumineux. Taille maximale: 5MB");
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "terrains");

            // Create directory if it doesn't exist
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/terrains/{fileName}";
        }

        public async Task<Dictionary<TimeSpan, bool>> GetAvailableSlotsAsync(int terrainId, DateTime date)
        {
            var terrain = await _terrainRepository.GetByIdAsync(terrainId);
            if (terrain == null)
            {
                return new Dictionary<TimeSpan, bool>();
            }

            var slots = new Dictionary<TimeSpan, bool>();
            var currentTime = terrain.OpeningTime;

            while (currentTime < terrain.ClosingTime)
            {
                var endTime = currentTime.Add(TimeSpan.FromHours(1));

                // Check if slot is available
                var hasConflict = await _reservationRepository.HasConflictAsync(
                    terrainId,
                    date,
                    currentTime,
                    endTime
                );

                slots[currentTime] = !hasConflict; // true = available, false = occupied

                currentTime = endTime;
            }

            return slots;
        }
    }
}