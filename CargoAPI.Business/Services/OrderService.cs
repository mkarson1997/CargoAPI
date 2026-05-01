using CargoAPI.DataAccess.Repositories;
using CargoAPI.Entities;
using Microsoft.Extensions.Logging;

namespace CargoAPI.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IGenericRepository<Order> _orderRepository;
        private readonly ICarrierConfigurationRepository _configRepository;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IGenericRepository<Order> orderRepository,
            ICarrierConfigurationRepository configRepository,
            ILogger<OrderService> logger)
        {
            _orderRepository = orderRepository;
            _configRepository = configRepository;
            _logger = logger;
        }

        public async Task<List<Order>> GetAllAsync()
        {
            return await _orderRepository.GetAllAsync();
        }

        public async Task<string> AddAsync(int orderDesi)
        {
            if (orderDesi <= 0)
            {
                _logger.LogWarning("Invalid OrderDesi provided: {OrderDesi}", orderDesi);
                return "Hata: OrderDesi 0'dan büyük olmalıdır.";
            }

            // Case 1: Desi falls within a carrier range — pick cheapest
            var matchingConfigs = await _configRepository.GetByDesiRangeAsync(orderDesi);

            int carrierId;
            decimal carrierCost;

            if (matchingConfigs.Any())
            {
                var cheapest = matchingConfigs.OrderBy(c => c.CarrierCost).First();
                carrierId = cheapest.CarrierId;
                carrierCost = cheapest.CarrierCost;
            }
            else
            {
                // Case 2: Desi outside all ranges — find closest and calculate extra cost
                // Formula: CarrierCost + (PlusDesiCost × (OrderDesi - MaxDesi))
                var closestConfig = await _configRepository.GetClosestConfigAsync(orderDesi);
                if (closestConfig == null)
                    return "Hata: Uygun kargo firması bulunamadı.";

                int extraDesi = Math.Abs(orderDesi - closestConfig.CarrierMaxDesi);
                carrierCost = closestConfig.CarrierCost + (closestConfig.Carrier.CarrierPlusDesiCost * extraDesi);
                carrierId = closestConfig.CarrierId;
            }

            // Create and save order
            var order = new Order
            {
                OrderDesi = orderDesi,
                OrderDate = DateTime.Now,
                OrderCarrierCost = carrierCost,
                CarrierId = carrierId
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveAsync();

            _logger.LogInformation("Order created: OrderDesi={OrderDesi}, CarrierId={CarrierId}, Cost={OrderCarrierCost}",
                order.OrderDesi, order.CarrierId, order.OrderCarrierCost);

            return $"Sipariş eklendi. Kargo ücreti: {carrierCost}₺";
        }

        public async Task<string> DeleteAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return "Hata: Sipariş bulunamadı.";

            _orderRepository.Delete(order);
            await _orderRepository.SaveAsync();
            return $"{id} ID'li sipariş silindi.";
        }
    }
}
