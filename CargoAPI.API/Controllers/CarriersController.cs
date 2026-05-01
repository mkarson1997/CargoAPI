using CargoAPI.Business.Services;
using CargoAPI.Entities;
using CargoAPI.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CargoAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarriersController : ControllerBase
    {
        private readonly ICarrierService _carrierService;

        public CarriersController(ICarrierService carrierService)
        {
            _carrierService = carrierService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var carriers = await _carrierService.GetAllAsync();
            return Ok(carriers);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CarrierCreateDto dto)
        {
            var carrier = new Carrier
            {
                CarrierName = dto.CarrierName,
                CarrierIsActive = dto.CarrierIsActive,
                CarrierPlusDesiCost = dto.CarrierPlusDesiCost,
                CarrierConfigurationId = dto.CarrierConfigurationId
            };
            var result = await _carrierService.AddAsync(carrier);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] CarrierUpdateDto dto)
        {
            var carrier = new Carrier
            {
                CarrierId = dto.CarrierId,
                CarrierName = dto.CarrierName,
                CarrierIsActive = dto.CarrierIsActive,
                CarrierPlusDesiCost = dto.CarrierPlusDesiCost,
                CarrierConfigurationId = dto.CarrierConfigurationId
            };
            var result = await _carrierService.UpdateAsync(carrier);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _carrierService.DeleteAsync(id);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }
    }
}
