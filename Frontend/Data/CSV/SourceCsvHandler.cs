using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Frontend.Models;
using System;
using System.Linq;
using Frontend.Interfaces;

namespace Frontend.Data.CSV
{
    static class SourceCsvHandler
    { 
        
        public static async Task ImportCsv(string location, IClient<Source> sourceClient) 
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
            List<Task> insertedSources = new List<Task>();
            foreach (var source in Sources)
            {
                insertedSources.Add(sourceClient.Post(source));
            }
            await Task.WhenAll(insertedSources);
        }
        private static bool IsDate(string value)
        {
            return DateTime.TryParse(value,new CultureInfo("da-DK"),DateTimeStyles.None, out _);
        }

        public static async void ExportCsv(string location, List<Source>? sources)
        {
            if (sources != null)
            {   
                List<string> lines =
                [
                    "TimeFrom,TimeTo,HeatDemand,ElectricityPrice"
                ];
                lines.AddRange(sources.Select(source => $"{source.TimeFrom.ToString("yyyy.MM.dd HH:mm", CsvCulture)},{source.TimeTo.ToString("yyyy.MM.dd HH:mm", CsvCulture)},{source.HeatDemand.ToString("00.00", CultureInfo.InvariantCulture)},{source.ElectricityPrice.ToString(CultureInfo.InvariantCulture)}"));
                await System.IO.File.WriteAllLinesAsync(location, lines);
                Console.WriteLine("completed | csv file export");
            }
            else
            {
                Console.WriteLine("error | no sources found for export");
            }

            
        }
        private static readonly CultureInfo CsvCulture = new CultureInfo("da-DK");
        private static Source SourceConverter(string? startDate, string? endDate, string? heatDemand, string? electricityPrice)
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
