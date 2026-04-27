using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Frontend.Models;
using Microsoft.VisualBasic.FileIO;

namespace Frontend.Data.CSV;

public static class CsvHandler
{
    // Asset
    public static async void ExportAsset(string location, AssetClient assetClient)
    {
        try
        {
            List<Asset>? assets = await assetClient.GetAll();

            if (assets is { Count: > 0 })
            {
                List<string> lines =
                [
                    "Name,MaxHeat MW,Production Cost DKK/MWh(th),CO2 Emissions kg/MWh(th),Gas Consumption MW(th),Oil Consumption MW(th),Max Electricity MW(e)"
                ];
                lines.AddRange(assets.Select(asset => $"{asset.Name},{asset.MaxHeat.ToString(CultureInfo.InvariantCulture)},{asset.ProductionCost},{asset.CO2Emission},{asset.GasConsumption.ToString(CultureInfo.InvariantCulture)},{asset.OilConsumption.ToString(CultureInfo.InvariantCulture)},{asset.MaxElectricity.ToString(CultureInfo.InvariantCulture)}"));

                await System.IO.File.WriteAllLinesAsync(location, lines);
                Console.WriteLine("completed | asset csv file export");
            }
            else
            {
                Console.WriteLine("error | no assets found for export");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"error | asset csv file export failed >> {e.Message}");
        }
    }

    public static async void ImportAsset(string location, AssetClient assetClient)
    {
        try
        {
            List<Asset> assets = new List<Asset>();

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

                    if (fields is { Length: >= 7 })
                    {
                        Asset asset = AssetConverter(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6]);
                        assets.Add(asset);
                    }
                    else
                    {
                        Console.WriteLine("error | read failed >> missing fields");
                    }
                }
                Console.WriteLine("completed | asset csv file read");
            }

            List<Task> insertedAssets = [];
            insertedAssets.AddRange(assets.Select(assetClient.Post));
            await Task.WhenAll(insertedAssets);
        }
        catch (Exception e)
        {
            Console.WriteLine($"error | asset csv file import failed >> {e.Message}");
        }
    }

    private static string? ImageParser(string name)
    {
        if (name.Contains("Gas Boiler", StringComparison.CurrentCultureIgnoreCase))
        {
            return "gb1.png";
        }
        else if (name.Contains("Oil Boiler", StringComparison.CurrentCultureIgnoreCase))
        {
            return "ob1.png";
        }
        else if (name.Contains("Electric Boiler", StringComparison.CurrentCultureIgnoreCase))
        {
            return "eb1.png";
        } 
        else if (name.Contains("Gas Motor",  StringComparison.CurrentCultureIgnoreCase))
        {
            return "gm1.png";
        }
        else
        {
            return null;
        }
    }

    private static Asset AssetConverter(string name, string maxHeat, string prodCost, string co2, string gas, string oil, string maxElec)
    {
        var asd = new Asset()
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
        return asd;
    }
    
    // Result
    public static async void ExportResult(string location, List<ResultTableRow>? results)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine($"error | optimized result csv file export failed >> {e.Message}");
        }
    }
    public static async Task ImportSource(string location, SourceClient sourceClient) 
    { 
        List<Source> sources = new List<Source>();
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
                            sources.Add(source);
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
        foreach (var source in sources)
        {
            insertedSources.Add(sourceClient.Post(source));
        }
        await Task.WhenAll(insertedSources);
    }
    private static bool IsDate(string value)
    {
        return DateTime.TryParse(value,new CultureInfo("da-DK"),DateTimeStyles.None, out _);
    }

    public static async void ExportSource(string location, List<Source>? sources)
    {
        try
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
        catch (Exception e)
        {
            Console.WriteLine($"error | csv file export failed >> {e.Message}");
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