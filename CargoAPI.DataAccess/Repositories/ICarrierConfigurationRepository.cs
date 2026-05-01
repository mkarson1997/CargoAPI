using CargoAPI.Entities;

namespace CargoAPI.DataAccess.Repositories
{
    public interface ICarrierConfigurationRepository : IGenericRepository<CarrierConfiguration>
    {
        Task<List<CarrierConfiguration>> GetByDesiRangeAsync(int orderDesi);
        Task<CarrierConfiguration?> GetClosestConfigAsync(int orderDesi);
    }
}
