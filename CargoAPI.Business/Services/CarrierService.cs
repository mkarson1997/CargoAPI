using CargoAPI.DataAccess.Repositories;
using CargoAPI.Entities;

namespace CargoAPI.Business.Services
{
    public class CarrierService : ICarrierService
    {
        private readonly IGenericRepository<Carrier> _carrierRepository;

        public CarrierService(IGenericRepository<Carrier> carrierRepository)
        {
            _carrierRepository = carrierRepository;
        }

        public async Task<List<Carrier>> GetAllAsync()
        {
            return await _carrierRepository.GetAllAsync();
        }

        public async Task<string> AddAsync(Carrier carrier)
        {
            var error = ValidateCarrier(carrier);
            if (error != null) return error;

            await _carrierRepository.AddAsync(carrier);
            await _carrierRepository.SaveAsync();
            return "Kargo firması eklendi.";
        }

        public async Task<string> UpdateAsync(Carrier carrier)
        {
            var error = ValidateCarrier(carrier);
            if (error != null) return error;

            var existing = await _carrierRepository.GetByIdAsync(carrier.CarrierId);
            if (existing == null)
                return "Hata: Kargo firması bulunamadı.";

            existing.CarrierName = carrier.CarrierName;
            existing.CarrierIsActive = carrier.CarrierIsActive;
            existing.CarrierPlusDesiCost = carrier.CarrierPlusDesiCost;
            existing.CarrierConfigurationId = carrier.CarrierConfigurationId;

            _carrierRepository.Update(existing);
            await _carrierRepository.SaveAsync();
            return $"{carrier.CarrierId} ID'li kargo firması güncellendi.";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var carrier = await _carrierRepository.GetByIdAsync(id);
            if (carrier == null)
                return "Hata: Kargo firması bulunamadı.";

            _carrierRepository.Delete(carrier);
            await _carrierRepository.SaveAsync();
            return $"{id} ID'li kargo firması silindi.";
        }
        private string? ValidateCarrier(Carrier carrier)
        {
            if (string.IsNullOrWhiteSpace(carrier.CarrierName))
                return "Hata: CarrierName boş olamaz.";
            if (carrier.CarrierPlusDesiCost < 0)
                return "Hata: CarrierPlusDesiCost 0 veya daha büyük olmalıdır.";
            return null;
        }
    }
}
