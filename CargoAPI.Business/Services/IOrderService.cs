using CargoAPI.Entities;

namespace CargoAPI.Business.Services
{
    public interface IOrderService
    {
        Task<List<Order>> GetAllAsync();
        Task<string> AddAsync(int orderDesi);
        Task<string> DeleteAsync(int id);
    }
}
