using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.IO;
using Frontend.Models;
using Frontend.Interfaces;

namespace Frontend.Data.CSV
{
    public static class AssetCsvHandler
    {
        public static async Task ExportCsv(string location, IClient<Asset> assetClient)
        {
            List<Asset> assets = await assetClient.GetAll();

            if (assets.Any())
            {
                List<string> lines = new List<string>();

                lines.Add("Name,MaxHeat MW,Production Cost DKK/MWh(th),CO2 Emissions kg/MWh(th),Gas Consumption MW(th),Oil Consumption MW(th),Max Electricity MW(e)");

                foreach (var asset in assets)
                {
                    string csvLine = $"{asset.Name},{asset.MaxHeat.ToString(CultureInfo.InvariantCulture)},{asset.ProductionCost},{asset.CO2Emission},{asset.GasConsumption.ToString(CultureInfo.InvariantCulture)},{asset.OilConsumption.ToString(CultureInfo.InvariantCulture)},{asset.MaxElectricity.ToString(CultureInfo.InvariantCulture)}";
                    lines.Add(csvLine);
                }
                await File.WriteAllLinesAsync(location, lines);
                Console.WriteLine("completed | asset csv file export");
            }
            else
            {
                Console.WriteLine("error | no assets found for export");
            }
        }

        public static async Task ImportCsv(string location, IClient<Asset> assetClient)
        {
            if (!System.IO.File.Exists(location))
                throw new System.IO.FileNotFoundException($"Asset CSV file not found: {location}");

            List<Asset> Assets = new List<Asset>();

            using (TextFieldParser parser = new TextFieldParser(location))
            {
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                if (!parser.EndOfData)
                {
                    parser.ReadFields();
                }

                while (!parser.EndOfData)
                {
                    string[]? fields = parser.ReadFields();

                    if (fields != null && fields.Length >= 7)
                    {
                        Asset asset = AssetConverter(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6]);
                        assetsToImport.Add(asset);
                    }
                    else
                    {
                        Console.WriteLine("error | read failed >> missing fields");
                    }
                }
            }

            List<Task> tasks = assetsToImport.Select(assetClient.Post).ToList();
            await Task.WhenAll(tasks);

            Console.WriteLine("completed | asset csv file read and uploaded");
        }

        private static string? ImageParser(string name)
        {
            if (name.Contains("Gas Boiler", StringComparison.OrdinalIgnoreCase)) return "gb1.png";
            if (name.Contains("Oil Boiler", StringComparison.OrdinalIgnoreCase)) return "ob1.png";
            if (name.Contains("Electric Boiler", StringComparison.OrdinalIgnoreCase)) return "eb1.png";
            if (name.Contains("Gas Motor", StringComparison.OrdinalIgnoreCase)) return "gm1.png";
            return null;
        }

        private static Asset AssetConverter(string name, string maxHeat, string prodCost, string co2, string gas, string oil, string maxElec)
        {
            return new Asset()
            {
                Name = name,
                MaxHeat = float.Parse(maxHeat, CultureInfo.InvariantCulture),
                ProductionCost = int.Parse(prodCost),
                CO2Emission = int.Parse(co2),
                GasConsumption = float.Parse(gas, CultureInfo.InvariantCulture),
                OilConsumption = float.Parse(oil, CultureInfo.InvariantCulture),
                MaxElectricity = float.Parse(maxElec, CultureInfo.InvariantCulture),
                ImageName = ImageParser(name)
            };
        }
    }
}