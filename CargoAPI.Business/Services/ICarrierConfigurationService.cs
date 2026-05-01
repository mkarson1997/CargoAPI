using CargoAPI.Entities;

namespace CargoAPI.Business.Services
{
    public interface ICarrierConfigurationService
    {
        Task<List<CarrierConfiguration>> GetAllAsync();
        Task<string> AddAsync(CarrierConfiguration config);
        Task<string> UpdateAsync(CarrierConfiguration config);
        Task<string> DeleteAsync(int id);
    }
}
