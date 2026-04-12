using System;

namespace Frontend.Models
{
    public class Source
    {
        public int Id { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public float HeatDemand { get; set; }
        public float ElectricityPrice {  get; set; }
        public string? FileName { get; set; }

    }
}
