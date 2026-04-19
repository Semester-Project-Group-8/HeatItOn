using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Frontend.Models;

namespace Frontend.Data.CSV
{
    public static class ResultCsvHandler
    {
        public static async void ExportCsv(string location, List<ResultTableRow>? results)
        {

            if (results is { Count: > 0 })
            {
                List<string> lines =
                [
                    "Hour,ActiveAssets,HeatProduced MW,Electricity MW,Co2Produced kg"
                ];
                lines.AddRange(results.Select(row => $"{row.Hour},{row.ActiveAssets},{row.HeatProduced.ToString(CultureInfo.InvariantCulture)},{row.Electricity.ToString(CultureInfo.InvariantCulture)},{row.Co2Produced}"));

                await System.IO.File.WriteAllLinesAsync(location, lines);
                Console.WriteLine("completed | optimized result csv file export");
            }
            else
            {
                Console.WriteLine("error | no results found for export");
            }
        }
    }
}