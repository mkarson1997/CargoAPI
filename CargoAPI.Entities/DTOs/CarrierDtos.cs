namespace CargoAPI.Entities.DTOs
{
    public class CarrierCreateDto
    {
        public string CarrierName { get; set; } = null!;
        public bool CarrierIsActive { get; set; }
        public int CarrierPlusDesiCost { get; set; }
        public int CarrierConfigurationId { get; set; }
    }

    public class CarrierUpdateDto
    {
        public int CarrierId { get; set; }
        public string CarrierName { get; set; } = null!;
        public bool CarrierIsActive { get; set; }
        public int CarrierPlusDesiCost { get; set; }
        public int CarrierConfigurationId { get; set; }
    }
}
