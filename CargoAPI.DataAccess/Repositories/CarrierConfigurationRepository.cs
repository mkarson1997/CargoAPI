using CargoAPI.Entities;
using Microsoft.EntityFrameworkCore;

namespace CargoAPI.DataAccess.Repositories
{
    public class CarrierConfigurationRepository : GenericRepository<CarrierConfiguration>, ICarrierConfigurationRepository
    {
        private readonly AppDbContext _context;

        public CarrierConfigurationRepository(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<CarrierConfiguration>> GetByDesiRangeAsync(int orderDesi)
        {
            return await _context.CarrierConfigurations
                .Include(cc => cc.Carrier)
                .Where(cc => cc.Carrier.CarrierIsActive
                             && orderDesi >= cc.CarrierMinDesi
                             && orderDesi <= cc.CarrierMaxDesi)
                .ToListAsync();
        }

        public async Task<CarrierConfiguration?> GetClosestConfigAsync(int orderDesi)
        {
            return await _context.CarrierConfigurations
                .Include(cc => cc.Carrier)
                .Where(cc => cc.Carrier.CarrierIsActive)
                .OrderBy(cc => Math.Abs(cc.CarrierMaxDesi - orderDesi))
                .FirstOrDefaultAsync();
        }
    }
}
