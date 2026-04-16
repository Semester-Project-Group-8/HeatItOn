using System;
using System.Collections.Generic;
using System.Globalization;
using Frontend.Models;

namespace Frontend.Data.CSV
{
    public static class ResultCsvHandler
    {
        public static async void ExportCsv(string location, ResultClient resultClient)
        {
            List<Result>? results = await resultClient.GetAll();

            if (results != null && results.Count > 0)
            {
                List<string> lines = new List<string>();

                lines.Add("AssetId,HeatProduction MW,Electricity MW,ProductionCost DKK,PrimaryEnergyConsumed MW,CO2Produced kg");

                foreach (var result in results)
                {
                    string csvLine = $"{result.AssetId},{result.HeatProduction.ToString(CultureInfo.InvariantCulture)},{result.Electricity.ToString(CultureInfo.InvariantCulture)},{result.ProductionCost.ToString(CultureInfo.InvariantCulture)},{result.PrimaryEnergyConsumed.ToString(CultureInfo.InvariantCulture)},{result.CO2Produced}";
                    lines.Add(csvLine);
                }

                System.IO.File.WriteAllLines(location, lines);
                Console.WriteLine("completed | result csv file export");
            }
            else
            {
                Console.WriteLine("error | no results found for export");
            }
        }
    }
}