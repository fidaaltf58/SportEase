using Microsoft.EntityFrameworkCore;
using SportEase.Web.Data;
using SportEase.Web.Models.Entities;

namespace SportEase.Web.Repositories.Interfaces
{
    public interface ITerrainRepository
    {
        Task<Terrain?> GetByIdAsync(int id);
        Task<IEnumerable<Terrain>> GetAllAsync();
        Task<IEnumerable<Terrain>> GetAllActiveAsync();
        Task<IEnumerable<Terrain>> GetByAdminIdAsync(int adminId);
        Task<Terrain> AddAsync(Terrain terrain);
        Task UpdateAsync(Terrain terrain);
        Task DeleteAsync(int id);
        Task<IEnumerable<Terrain>> SearchAsync(string? sportType, string? city, decimal? minPrice, decimal? maxPrice, int? minCapacity, bool? hasLighting, bool? hasParking, bool? hasChangingRoom);
    }
}