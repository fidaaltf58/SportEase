using SportEase.Web.Models.Entities;
using SportEase.Web.Models.ViewModels;
using SportEase.Web.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SportEase.Web.Services.Interfaces
{
    public interface ITerrainService
    {
        Task<Terrain?> GetByIdAsync(int id);
        Task<IEnumerable<Terrain>> GetAllAsync();
        Task<IEnumerable<Terrain>> GetAllActiveAsync();
        Task<IEnumerable<Terrain>> GetByAdminIdAsync(int adminId);
        Task<Terrain> CreateAsync(CreateTerrainViewModel model, int adminId);
        Task<bool> UpdateAsync(EditTerrainViewModel model);
        Task<bool> DeleteAsync(int id, int adminId);
        Task<IEnumerable<Terrain>> SearchAsync(TerrainSearchViewModel model);
        Task<string?> SaveImageAsync(IFormFile? file);
        Task<Dictionary<TimeSpan, bool>> GetAvailableSlotsAsync(int terrainId, DateTime date);
    }
}
