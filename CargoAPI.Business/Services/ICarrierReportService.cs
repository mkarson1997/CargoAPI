using CargoAPI.Entities;

namespace CargoAPI.Business.Services
{
    public interface ICarrierReportService
    {
        Task GenerateReportsAsync();
        Task<List<CarrierReport>> GetAllReportsAsync();
    }
}
