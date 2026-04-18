using System.ComponentModel.DataAnnotations;
namespace Backend.Models
{
    public class Asset
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public float MaxHeat { get; set; }
        [Required]
        public int ProductionCost { get; set; }
        [Required]
        public int CO2Emission {  get; set; }
        [Required]
        public float GasConsumption { get; set; }
        [Required]
        public float OilConsumption { get; set; }
        [Required]
        public float MaxElectricity { get; set; }

        public string? ImageName { get; set; } = null;
    }
}
