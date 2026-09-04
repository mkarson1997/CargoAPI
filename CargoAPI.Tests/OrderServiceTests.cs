using CargoAPI.Business.Services;
using CargoAPI.DataAccess.Repositories;
using CargoAPI.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CargoAPI.Tests;

public class OrderServiceTests
{
    private readonly Mock<IGenericRepository<Order>> _orders = new();
    private readonly Mock<ICarrierConfigurationRepository> _configs = new();
    private readonly Mock<ILogger<OrderService>> _logger = new();

    private OrderService CreateService() =>
        new(_orders.Object, _configs.Object, _logger.Object);

    [Fact]
    public async Task AddAsync_WhenDesiIsNotPositive_DoesNotPersistOrder()
    {
        var service = CreateService();

        var result = await service.AddAsync(0);

        Assert.Contains("0'dan büyük", result);
        _configs.Verify(x => x.GetByDesiRangeAsync(It.IsAny<int>()), Times.Never);
        _orders.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orders.Verify(x => x.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task AddAsync_WhenMultipleConfigurationsMatch_SelectsCheapestCarrier()
    {
        var service = CreateService();
        Order? persisted = null;

        _configs
            .Setup(x => x.GetByDesiRangeAsync(5))
            .ReturnsAsync(new List<CarrierConfiguration>
            {
                new()
                {
                    CarrierId = 1,
                    CarrierMinDesi = 1,
                    CarrierMaxDesi = 10,
                    CarrierCost = 40m,
                    Carrier = new Carrier { CarrierId = 1, CarrierName = "A", CarrierIsActive = true }
                },
                new()
                {
                    CarrierId = 2,
                    CarrierMinDesi = 1,
                    CarrierMaxDesi = 10,
                    CarrierCost = 32m,
                    Carrier = new Carrier { CarrierId = 2, CarrierName = "B", CarrierIsActive = true }
                }
            });

        _orders
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => persisted = order)
            .Returns(Task.CompletedTask);
        _orders.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        var result = await service.AddAsync(5);

        Assert.NotNull(persisted);
        Assert.Equal(2, persisted!.CarrierId);
        Assert.Equal(32m, persisted.OrderCarrierCost);
        Assert.Equal(5, persisted.OrderDesi);
        Assert.Contains("32", result);
        _orders.Verify(x => x.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task AddAsync_WhenNoRangeMatches_AppliesExtraDesiCostToClosestConfiguration()
    {
        var service = CreateService();
        Order? persisted = null;

        _configs
            .Setup(x => x.GetByDesiRangeAsync(13))
            .ReturnsAsync(new List<CarrierConfiguration>());

        _configs
            .Setup(x => x.GetClosestConfigAsync(13))
            .ReturnsAsync(new CarrierConfiguration
            {
                CarrierId = 7,
                CarrierMinDesi = 1,
                CarrierMaxDesi = 10,
                CarrierCost = 32m,
                Carrier = new Carrier
                {
                    CarrierId = 7,
                    CarrierName = "Example Carrier",
                    CarrierIsActive = true,
                    CarrierPlusDesiCost = 4
                }
            });

        _orders
            .Setup(x => x.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => persisted = order)
            .Returns(Task.CompletedTask);
        _orders.Setup(x => x.SaveAsync()).Returns(Task.CompletedTask);

        var result = await service.AddAsync(13);

        Assert.NotNull(persisted);
        Assert.Equal(7, persisted!.CarrierId);
        Assert.Equal(44m, persisted.OrderCarrierCost);
        Assert.Contains("44", result);
    }

    [Fact]
    public async Task AddAsync_WhenNoCarrierConfigurationExists_DoesNotPersistOrder()
    {
        var service = CreateService();

        _configs
            .Setup(x => x.GetByDesiRangeAsync(13))
            .ReturnsAsync(new List<CarrierConfiguration>());
        _configs
            .Setup(x => x.GetClosestConfigAsync(13))
            .ReturnsAsync((CarrierConfiguration?)null);

        var result = await service.AddAsync(13);

        Assert.Contains("bulunamadı", result);
        _orders.Verify(x => x.AddAsync(It.IsAny<Order>()), Times.Never);
        _orders.Verify(x => x.SaveAsync(), Times.Never);
    }
}
