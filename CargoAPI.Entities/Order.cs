using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CargoAPI.Entities
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        public int OrderDesi { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OrderCarrierCost { get; set; }

        [Required]
        public int CarrierId { get; set; }

        [ForeignKey("CarrierId")]
        [JsonIgnore]
        public Carrier Carrier { get; set; } = null!;
    }
}
