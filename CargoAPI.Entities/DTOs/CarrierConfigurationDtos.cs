namespace CargoAPI.Entities.DTOs
{
    public class CarrierConfigurationCreateDto
    {
        public int CarrierId { get; set; }
        public int CarrierMaxDesi { get; set; }
        public int CarrierMinDesi { get; set; }
        public decimal CarrierCost { get; set; }
    }

    public class CarrierConfigurationUpdateDto
    {
        public int CarrierConfigurationId { get; set; }
        public int CarrierId { get; set; }
        public int CarrierMaxDesi { get; set; }
        public int CarrierMinDesi { get; set; }
        public decimal CarrierCost { get; set; }
    }
}
