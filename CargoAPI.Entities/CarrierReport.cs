using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargoAPI.Entities
{
    public class CarrierReport
    {
        [Key]
        public int CarrierReportId { get; set; }

        [Required]
        public int CarrierId { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CarrierCost { get; set; }

        [Required]
        public DateTime CarrierReportDate { get; set; }
    }
}
