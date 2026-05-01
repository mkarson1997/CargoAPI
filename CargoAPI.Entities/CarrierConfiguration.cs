using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CargoAPI.Entities
{
    public class CarrierConfiguration
    {
        [Key]
        public int CarrierConfigurationId { get; set; }

        [Required]
        public int CarrierId { get; set; }

        [ForeignKey("CarrierId")]
        [JsonIgnore]
        public Carrier Carrier { get; set; } = null!;

        [Required]
        public int CarrierMaxDesi { get; set; }

        [Required]
        public int CarrierMinDesi { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CarrierCost { get; set; }
    }
}
