namespace Frontend.Models;

public class ResultTableRow
{
    public string Hour { get; set; } = string.Empty;
    public string ActiveAssets { get; set; } = string.Empty;
    public float HeatProduced { get; set; }
    public float Electricity { get; set; }
    public string ElectricityColor => Electricity < 0 ? "Red" : "Green";
    public int Co2Produced { get; set; }
    public float ProductionCost { get; set; }
}