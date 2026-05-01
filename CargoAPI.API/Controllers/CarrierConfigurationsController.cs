using CargoAPI.Business.Services;
using CargoAPI.Entities;
using CargoAPI.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CargoAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarrierConfigurationsController : ControllerBase
    {
        private readonly ICarrierConfigurationService _configService;

        public CarrierConfigurationsController(ICarrierConfigurationService configService)
        {
            _configService = configService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _configService.GetAllAsync();
            return Ok(configs);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CarrierConfigurationCreateDto dto)
        {
            var config = new CarrierConfiguration
            {
                CarrierId = dto.CarrierId,
                CarrierMaxDesi = dto.CarrierMaxDesi,
                CarrierMinDesi = dto.CarrierMinDesi,
                CarrierCost = dto.CarrierCost
            };
            var result = await _configService.AddAsync(config);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CarrierConfigurationUpdateDto dto)
        {
            var config = new CarrierConfiguration
            {
                CarrierConfigurationId = dto.CarrierConfigurationId,
                CarrierId = dto.CarrierId,
                CarrierMaxDesi = dto.CarrierMaxDesi,
                CarrierMinDesi = dto.CarrierMinDesi,
                CarrierCost = dto.CarrierCost
            };
            var result = await _configService.UpdateAsync(config);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _configService.DeleteAsync(id);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }
    }
}
