namespace Frontend.Models
{
    public class Result
    {
        public int Id { get; set; }
        public float HeatProduction { get; set; }
        public float Electricity { get; set; }
        public float ProductionCost { get; set; }
        public float PrimaryEnergyConsumed { get; set; }
        public int CO2Produced { get; set; }
        public int AssetId { get; set; }
        public Asset Asset { get; set; }
    }
}
