using System.ComponentModel.DataAnnotations;
namespace Backend.Models
{
    public class Source
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TimeFrom { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime TimeTo { get; set; }
        [Required]
        public float HeatDemand { get; set; }
        [Required]
        public float ElectricityPrice {  get; set; }

    }
}
