using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models;

public class Result
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Result HeatProduction is required")]
    [Range(0, float.MaxValue, ErrorMessage = "Result HeatProduction cannot be negative")]
    public float HeatProduction { get; set; }

    [Required(ErrorMessage = "Result Electricity is required")]
    [Range(float.MinValue, float.MaxValue, ErrorMessage = "Result Electricity is out of range")]
    public float Electricity { get; set; }

    [Required(ErrorMessage = "Result ProductionCost is required")]
    [Range(float.MinValue, float.MaxValue, ErrorMessage = "Result ProductionCost is out of range")]
    public float ProductionCost { get; set; }

    [Required(ErrorMessage = "Result PrimaryEnergyConsumed is required")]
    [Range(0, float.MaxValue, ErrorMessage = "Result PrimaryEnergyConsumed cannot be negative")]
    public float PrimaryEnergyConsumed { get; set; }

    [Required(ErrorMessage = "Result CO2Produced is required")]
    [Range(0, float.MaxValue, ErrorMessage = "Result CO2Produced cannot be negative")]
    public int CO2Produced { get; set; }

    [Required(ErrorMessage = "Result AssetId is required")]
    [ForeignKey("Asset")]
    public int AssetId { get; set; }

    public Asset? Asset { get; set; }
}