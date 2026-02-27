using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class Result
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public float HeatProduction { get; set; }
        [Required]
        public float Electricity { get; set; }
        [Required]
        public float ProductionCost { get; set; }
        [Required]
        public float PrimaryEnergyConsumed { get; set; }
        [Required]
        public int CO2Produced { get; set; }
        [Required]
        public int AssetId { get; set; }
        [Required]
        public Asset Asset { get; set; }
    }
}
