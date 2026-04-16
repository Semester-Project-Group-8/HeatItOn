using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Frontend.Models;

namespace Frontend.Data.CSV
{
    public static class ResultCsvHandler
    {
        public static async void ExportCsv(string location, ResultListClient resultListClient)
        {
            List<ResultList>? resultLists = await resultListClient.ListResultLists();

            if (resultLists != null && resultLists.Count > 0)
            {
                List<string> lines = new List<string>();

                lines.Add("Hour,ActiveAssets,HeatProduced MW,Electricity MW,Co2Produced kg");

                foreach (var resultList in resultLists)
                {
                    string hour = resultList.TimeFrom.ToString("yyyy.MM.dd HH:mm", CultureInfo.InvariantCulture);

                    string activeAssets = string.Join(" | ", resultList.Results.Select(r => r.AssetId.ToString()));

                    float heatProduced = resultList.Results.Sum(r => r.HeatProduction);
                    float electricity = resultList.Results.Sum(r => r.Electricity);
                    int co2Produced = resultList.Results.Sum(r => r.CO2Produced);

                    string csvLine = $"{hour},{activeAssets},{heatProduced.ToString(CultureInfo.InvariantCulture)},{electricity.ToString(CultureInfo.InvariantCulture)},{co2Produced}";
                    lines.Add(csvLine);
                }

                System.IO.File.WriteAllLines(location, lines);
                Console.WriteLine("completed | optimized result csv file export");
            }
            else
            {
                Console.WriteLine("error | no results found for export");
            }
        }
    }
}