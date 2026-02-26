using Backend.Models;
using Backend.Services;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
namespace Backend.Data
{
    class ReadCsv
    {
        readonly string location;
        DemandService _demandService;
        public ReadCsv(DemandService demandService,string location)
        {
            _demandService = demandService;
            this.location = location;
        }
        public async Task<int> ImportCsv()
        {
            List<Demand> Demands = new List<Demand>();
            using (TextFieldParser parser = new TextFieldParser(location))
            {
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;
                while (!parser.EndOfData)
                {
                    string[]? fields = parser.ReadFields();
                    if (fields != null)
                    {
                        int i = 0;
                        while (i < fields.Length)
                        {
                            if (IsDate(fields[i]))
                            {
                                Demand demand = DemandConverter(fields[i], fields[i + 1], fields[i + 2], fields[i + 3]);
                                Demands.Add(demand);
                                i = i + 3;
                            }
                            i++;
                        }
                    }
                    else
                    {
                        Console.WriteLine("error | read failed >> no fields found");
                    }
                }
                Console.WriteLine("completed | csv file read");
            }
            var inserted = await _demandService.AddDemands(Demands);
            Console.WriteLine($"completed | inserted {inserted} lines of data");
            return inserted;
        }
        private bool IsDate(string value)
        {
            return DateTime.TryParse(value,new CultureInfo("de-DE"),DateTimeStyles.None, out _);
        }

        private static readonly CultureInfo CsvCulture = new CultureInfo("de-DE");
        private Demand DemandConverter(string? startDate, string? endDate, string? heatDemand, string? electricityPrice)
        {
            startDate ??= "2000.01.01 00:00";
            endDate ??= "2000.01.01 01:00";
            heatDemand ??= "0";
            electricityPrice ??= "0";

            Demand demand = new Demand()
            {
                //ID = id,
                StartTime = DateTime.Parse(startDate,CsvCulture),
                EndTime = DateTime.Parse(endDate,CsvCulture),
                HeatDemand = float.Parse(heatDemand),
                ElectricityPrice = float.Parse(electricityPrice)
            };
            return demand;
        }
    }
}
