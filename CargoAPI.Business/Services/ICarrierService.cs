using CargoAPI.Entities;

namespace CargoAPI.Business.Services
{
    public interface ICarrierService
    {
        Task<List<Carrier>> GetAllAsync();
        Task<string> AddAsync(Carrier carrier);
        Task<string> UpdateAsync(Carrier carrier);
        Task<string> DeleteAsync(int id);
    }
}
