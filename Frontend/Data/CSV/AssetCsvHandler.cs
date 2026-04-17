using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Frontend.Models;
using Frontend.Data;

namespace Frontend.Data.CSV
{
    public static class AssetCsvHandler
    {
        public static async void ExportCsv(string location, AssetClient assetClient)
        {
            List<Asset>? assets = await assetClient.GetAll();

            if (assets != null && assets.Count > 0)
            {
                List<string> lines = new List<string>();

                lines.Add("Name,MaxHeat MW,Production Cost DKK/MWh(th),CO2 Emissions kg/MWh(th),Gas Consumption MW(th),Oil Consumption MW(th),Max Electricity MW(e)");

                foreach (var asset in assets)
                {
                    string csvLine = $"{asset.Name},{asset.MaxHeat.ToString(CultureInfo.InvariantCulture)},{asset.ProductionCost},{asset.CO2Emission},{asset.GasConsumption.ToString(CultureInfo.InvariantCulture)},{asset.OilConsumption.ToString(CultureInfo.InvariantCulture)},{asset.MaxElectricity.ToString(CultureInfo.InvariantCulture)}";
                    lines.Add(csvLine);
                }

                System.IO.File.WriteAllLines(location, lines);
                Console.WriteLine("completed | asset csv file export");
            }
            else
            {
                Console.WriteLine("error | no assets found for export");
            }
        }

        public static async void ImportCsv(string location, AssetClient assetClient)
        {
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
                        Assets.Add(asset);
                    }
                    else
                    {
                        Console.WriteLine("error | read failed >> missing fields");
                    }
                }
                Console.WriteLine("completed | asset csv file read");
            }

            List<Task> insertedAssets = new List<Task>();
            foreach (var asset in Assets)
            {
                insertedAssets.Add(assetClient.Post(asset));
            }
            await Task.WhenAll(insertedAssets);
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
                MaxElectricity = float.Parse(maxElec, CultureInfo.InvariantCulture)
            };
        }
    }
}