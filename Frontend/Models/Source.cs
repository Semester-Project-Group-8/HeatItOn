using System;

namespace Frontend.Models
{
    public class Source
    {
        public Source(int id, DateTime timeFrom, DateTime timeTo, float heatDemand, float electricityPrice)
        {
            Id = id;
            TimeFrom = timeFrom;
            TimeTo = timeTo;
            HeatDemand = heatDemand;
            ElectricityPrice = electricityPrice;
        }

        public int Id { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public float HeatDemand { get; set; }
        public float ElectricityPrice {  get; set; }

    }
}
