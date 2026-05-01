using CargoAPI.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace CargoAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarrierReportsController : ControllerBase
    {
        private readonly ICarrierReportService _carrierReportService;

        public CarrierReportsController(ICarrierReportService carrierReportService)
        {
            _carrierReportService = carrierReportService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var reports = await _carrierReportService.GetAllReportsAsync();
            return Ok(reports);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate()
        {
            await _carrierReportService.GenerateReportsAsync();
            return Ok("Carrier reports generated successfully.");
        }
    }
}
