using CargoAPI.DataAccess.Repositories;
using CargoAPI.Entities;
using Microsoft.Extensions.Logging;

namespace CargoAPI.Business.Services
{
    public class CarrierConfigurationService : ICarrierConfigurationService
    {
        private readonly IGenericRepository<CarrierConfiguration> _configRepository;
        private readonly ILogger<CarrierConfigurationService> _logger;

        public CarrierConfigurationService(IGenericRepository<CarrierConfiguration> configRepository, ILogger<CarrierConfigurationService> logger)
        {
            _configRepository = configRepository;
            _logger = logger;
        }

        public async Task<List<CarrierConfiguration>> GetAllAsync()
        {
            return await _configRepository.GetAllAsync();
        }

        public async Task<string> AddAsync(CarrierConfiguration config)
        {
            var error = ValidateConfig(config);
            if (error != null) return error;

            await _configRepository.AddAsync(config);
            await _configRepository.SaveAsync();
            return "Kargo konfigürasyonu eklendi.";
        }

        public async Task<string> UpdateAsync(CarrierConfiguration config)
        {
            var error = ValidateConfig(config);
            if (error != null) return error;

            var existing = await _configRepository.GetByIdAsync(config.CarrierConfigurationId);
            if (existing == null)
                return "Hata: Kargo konfigürasyonu bulunamadı.";

            existing.CarrierId = config.CarrierId;
            existing.CarrierMaxDesi = config.CarrierMaxDesi;
            existing.CarrierMinDesi = config.CarrierMinDesi;
            existing.CarrierCost = config.CarrierCost;

            _configRepository.Update(existing);
            await _configRepository.SaveAsync();
            return $"{config.CarrierConfigurationId} ID'li kargo konfigürasyonu güncellendi.";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var config = await _configRepository.GetByIdAsync(id);
            if (config == null)
                return "Hata: Kargo konfigürasyonu bulunamadı.";

            _configRepository.Delete(config);
            await _configRepository.SaveAsync();
            return $"{id} ID'li kargo konfigürasyonu silindi.";
        }
        private string? ValidateConfig(CarrierConfiguration config)
        {
            if (config.CarrierMinDesi <= 0)
            {
                _logger.LogWarning("Invalid CarrierMinDesi: {CarrierMinDesi} (must be > 0)", config.CarrierMinDesi);
                return "Hata: CarrierMinDesi 0'dan büyük olmalıdır.";
            }
            if (config.CarrierMaxDesi < config.CarrierMinDesi)
            {
                _logger.LogWarning("Invalid CarrierMaxDesi: {CarrierMaxDesi} (less than CarrierMinDesi: {CarrierMinDesi})",
                    config.CarrierMaxDesi, config.CarrierMinDesi);
                return "Hata: CarrierMaxDesi, CarrierMinDesi'den küçük olamaz.";
            }
            if (config.CarrierCost <= 0)
            {
                _logger.LogWarning("Invalid CarrierCost: {CarrierCost} (must be > 0)", config.CarrierCost);
                return "Hata: CarrierCost 0'dan büyük olmalıdır.";
            }
            return null;
        }
    }
}
