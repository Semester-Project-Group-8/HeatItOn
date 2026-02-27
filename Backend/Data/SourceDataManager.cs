using Backend.Models;
using Backend.Services;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
namespace Backend.Data
{
    class ReadCsv
    {
        readonly string location;
        SourceService _sourceService;
        public ReadCsv(SourceService sourceService, string location)
        {
            _sourceService = sourceService;
            this.location = location;
        }
        public async Task<int> ImportCsv()
        {
            List<Source> Sources = new List<Source>();
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
                                Source source = SourceConverter(fields[i], fields[i + 1], fields[i + 2], fields[i + 3]);
                                Sources.Add(source);
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
            var inserted = await _sourceService.AddSources(Sources);
            Console.WriteLine($"completed | inserted {inserted} lines of data");
            return inserted;
        }
        private bool IsDate(string value)
        {
            return DateTime.TryParse(value,new CultureInfo("da-DK"),DateTimeStyles.None, out _);
        }

        private static readonly CultureInfo CsvCulture = new CultureInfo("da-DK");
        private Source SourceConverter(string? startDate, string? endDate, string? heatDemand, string? electricityPrice)
        {
            startDate ??= "2000.01.01 00:00";
            endDate ??= "2000.01.01 01:00";
            heatDemand ??= "0";
            electricityPrice ??= "0";

            Source source = new Source()
            {
                //ID = id,
                TimeFrom = DateTime.Parse(startDate,CsvCulture),
                TimeTo = DateTime.Parse(endDate,CsvCulture),
                HeatDemand = float.Parse(heatDemand),
                ElectricityPrice = float.Parse(electricityPrice)
            };
            return source;
        }
    }
}
