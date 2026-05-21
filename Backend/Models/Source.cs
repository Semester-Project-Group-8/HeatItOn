using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Source
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Source TimeFrom is required")]
        [DataType(DataType.DateTime, ErrorMessage = "Invalid date")]
        public DateTime TimeFrom { get; set; }
        [Required(ErrorMessage = "Source TimeTo is required")]
        [DataType(DataType.DateTime,  ErrorMessage = "Invalid date")]
        public DateTime TimeTo { get; set; }
        [Required(ErrorMessage = "Source HeatDemand is required")]
        [Range(0, float.MaxValue, ErrorMessage = "Source HeatDemand cannot be negative")]
        public float HeatDemand { get; set; }
        [Required(ErrorMessage = "Source ElectricityPrice is required")]
        [Range(float.MinValue, float.MaxValue, ErrorMessage = "Source ElectricityPrice is out of range")]
        public float ElectricityPrice {  get; set; }

    }
}
