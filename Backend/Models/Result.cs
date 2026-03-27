using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [ForeignKey("Asset")]
        public int AssetId { get; set; }
        
        public Asset? Asset { get; set; }

        [ForeignKey("ResultList")]
        public int? ResultListId { get; set; }

        public ResultList? ResultList { get; set; }
    }
}
