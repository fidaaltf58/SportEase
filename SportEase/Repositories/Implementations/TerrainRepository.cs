using Microsoft.EntityFrameworkCore;
using SportEase.Web.Data;
using SportEase.Web.Models.Entities;
using SportEase.Web.Repositories.Interfaces;
namespace SportEase.Web.Repositories.Implementations
{
    public class TerrainRepository : ITerrainRepository
    {
        private readonly ApplicationDbContext _context;

        public TerrainRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Terrain?> GetByIdAsync(int id)
        {
            return await _context.Terrains
                .Include(t => t.Admin)
                .Include(t => t.Reservations)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Terrain>> GetAllAsync()
        {
            return await _context.Terrains
                .Include(t => t.Admin)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Terrain>> GetAllActiveAsync()
        {
            return await _context.Terrains
                .Include(t => t.Admin)
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Terrain>> GetByAdminIdAsync(int adminId)
        {
            return await _context.Terrains
                .Include(t => t.Reservations)
                .Where(t => t.AdminId == adminId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Terrain> AddAsync(Terrain terrain)
        {
            _context.Terrains.Add(terrain);
            await _context.SaveChangesAsync();
            return terrain;
        }

        public async Task UpdateAsync(Terrain terrain)
        {
            _context.Terrains.Update(terrain);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var terrain = await _context.Terrains.FindAsync(id);
            if (terrain != null)
            {
                terrain.IsActive = false; // Soft delete
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Terrain>> SearchAsync(
            string? sportType,
            string? city,
            decimal? minPrice,
            decimal? maxPrice,
            int? minCapacity,
            bool? hasLighting,
            bool? hasParking,
            bool? hasChangingRoom)
        {
            var query = _context.Terrains
                .Include(t => t.Admin)
                .Where(t => t.IsActive)
                .AsQueryable();

            if (!string.IsNullOrEmpty(sportType))
            {
                query = query.Where(t => t.SportType == sportType);
            }

            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(t => t.City.Contains(city));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(t => t.PricePerHour >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(t => t.PricePerHour <= maxPrice.Value);
            }

            if (minCapacity.HasValue)
            {
                query = query.Where(t => t.Capacity >= minCapacity.Value);
            }

            if (hasLighting.HasValue && hasLighting.Value)
            {
                query = query.Where(t => t.HasLighting);
            }

            if (hasParking.HasValue && hasParking.Value)
            {
                query = query.Where(t => t.HasParking);
            }

            if (hasChangingRoom.HasValue && hasChangingRoom.Value)
            {
                query = query.Where(t => t.HasChangingRoom);
            }

            return await query.OrderBy(t => t.Name).ToListAsync();
        }
    }
}