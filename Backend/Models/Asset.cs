using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Asset
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Asset name is required")]
        [MinLength(1, ErrorMessage = "Asset name must be at least 1 characters long")]
        [MaxLength(30, ErrorMessage = "Asset name cannot exceed 30 characters")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Asset MaxHeat is required")]
        [Range(0, float.MaxValue,  ErrorMessage = "Asset MaxHeat cannot be negative")]
        public float MaxHeat { get; set; }
        [Required(ErrorMessage = "Asset ProductionCost is required")]
        [Range(0, int.MaxValue,  ErrorMessage = "Asset ProductionCost cannot be negative")]
        public int ProductionCost { get; set; }
        [Required(ErrorMessage = "Asset CO2Emmission is required")]
        [Range(0, int.MaxValue,   ErrorMessage = "Asset CO2Emission cannot be negative")]
        public int CO2Emission {  get; set; }
        [Required(ErrorMessage = "Asset GasConsumption is required")]
        [Range(0, float.MaxValue, ErrorMessage = "Asset GasConsumption cannot be negative")]
        public float GasConsumption { get; set; }
        [Required(ErrorMessage = "Asset OilConsumption is required")]
        [Range(0, float.MaxValue, ErrorMessage = "Asset OilConsumption cannot be negative")]
        public float OilConsumption { get; set; }
        [Required(ErrorMessage = "Asset MaxElectricity is required")]
        [Range(float.MinValue, float.MaxValue, ErrorMessage = "Asset MaxElectricity is out of range")]
        public float MaxElectricity { get; set; }
        [RegularExpression("^.+\\.png$")]
        public string? ImageName { get; set; } = null;
    }
}
