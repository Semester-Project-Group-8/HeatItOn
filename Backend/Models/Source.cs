using System.ComponentModel.DataAnnotations;
namespace Backend.Models
{
    public class Source
    {
        [Required]
        public int ID { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }
        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }
        [Required]
        public float HeatDemand { get; set; }
        [Required]
        public float ElectricityPrice {  get; set; }

    }
}
