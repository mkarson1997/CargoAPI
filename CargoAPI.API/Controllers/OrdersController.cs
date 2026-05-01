using CargoAPI.Business.Services;
using CargoAPI.Entities.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CargoAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderService.GetAllAsync();
            return Ok(orders);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] OrderCreateDto dto)
        {
            var result = await _orderService.AddAsync(dto.OrderDesi);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _orderService.DeleteAsync(id);
            if (result.StartsWith("Hata:")) return BadRequest(result);
            return Ok(result);
        }
    }
}
