using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CargoAPI.Entities
{
    public class Carrier
    {
        [Key]
        public int CarrierId { get; set; }

        [Required]
        public string CarrierName { get; set; } = null!;

        [Required]
        public bool CarrierIsActive { get; set; }

        [Required]
        public int CarrierPlusDesiCost { get; set; }

        [Required]
        public int CarrierConfigurationId { get; set; }

        [JsonIgnore]
        public ICollection<CarrierConfiguration> CarrierConfigurations { get; set; } = new List<CarrierConfiguration>();
    }
}
