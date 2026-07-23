using BloodCenterOS.Core.Models;

namespace BloodCenterOS.API.Repositories;

public interface ICampOrganizerRepository
{
    Task<long> CreateAsync(CampOrganizer organizer);
    Task UpdateAsync(CampOrganizer organizer);
    Task<CampOrganizer?> GetByIdAsync(long id);
    Task<IEnumerable<CampOrganizer>> GetAllByCenterAsync(long centerId);
    Task DeleteAsync(long id);
}
