namespace Frontend.Models
{
    public class Asset
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public float MaxHeat { get; set; }
        public int ProductionCost { get; set; }
        public int CO2Emission {  get; set; }
        public float GasConsumption { get; set; }
        public float OilConsumption { get; set; }
        public float MaxElectricity { get; set; }
        public string ImageName { get; set; }
    }
}
