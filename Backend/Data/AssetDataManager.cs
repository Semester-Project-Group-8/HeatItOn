using System.Globalization;
using Backend.Models;
using Backend.Services;
using Microsoft.VisualBasic.FileIO;

namespace Backend.Data
{
    class ReadAssetCsv
    {
        private static readonly CultureInfo CsvCulture = new CultureInfo("da-DK");

        private readonly string _location;
        private readonly AssetsService _assetsService;

        public ReadAssetCsv(AssetsService assetsService, string location)
        {
            _assetsService = assetsService;
            _location = location;
        }

        public async Task<int> ImportCsv()
        {
            if (!File.Exists(_location))
            {
                Console.WriteLine($"warning | assets csv not found at {_location}");
                return 0;
            }

            var assets = new List<Asset>();

            using (var parser = new TextFieldParser(_location))
            {
                parser.SetDelimiters(",", ";");
                parser.HasFieldsEnclosedInQuotes = true;

                var hasHeader = false;
                var indexMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();
                    if (fields == null || fields.Length == 0)
                        continue;

                    if (!hasHeader)
                    {
                        indexMap = BuildIndexMap(fields);
                        hasHeader = true;
                        continue;
                    }

                    var asset = ParseAsset(fields, indexMap);
                    if (asset != null)
                        assets.Add(asset);
                }
            }

            if (assets.Count == 0)
            {
                Console.WriteLine("warning | no assets parsed from csv");
                return 0;
            }

            var inserted = await _assetsService.AddAssets(assets);
            Console.WriteLine($"completed | inserted {inserted} asset rows from csv");
            return inserted;
        }

        private static Dictionary<string, int> BuildIndexMap(string[] headers)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < headers.Length; i++)
            {
                var key = (headers[i] ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(key))
                    map[key] = i;
            }

            return map;
        }

        private static Asset? ParseAsset(string[] fields, Dictionary<string, int> map)
        {
            var name = GetString(fields, map, "Name");
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var asset = new Asset
            {
                Name = name,
                MaxHeat = GetFloat(fields, map, "MaxHeat"),
                ProductionCost = GetInt(fields, map, "ProductionCost"),
                CO2Emission = GetInt(fields, map, "CO2Emission"),
                GasConsumption = GetFloat(fields, map, "GasConsumption"),
                OilConsumption = GetFloat(fields, map, "OilConsumption"),
                MaxElectricity = GetFloat(fields, map, "MaxElectricity")
            };

            var id = GetNullableInt(fields, map, "Id");
            if (id.HasValue)
                asset.Id = id.Value;

            return asset;
        }

        private static string GetString(string[] fields, Dictionary<string, int> map, string column)
        {
            if (!map.TryGetValue(column, out var index) || index >= fields.Length)
                return string.Empty;

            return (fields[index] ?? string.Empty).Trim();
        }

        private static int GetInt(string[] fields, Dictionary<string, int> map, string column)
        {
            var raw = GetString(fields, map, column);
            if (int.TryParse(raw, NumberStyles.Integer, CsvCulture, out var value))
                return value;
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return value;

            return 0;
        }

        private static int? GetNullableInt(string[] fields, Dictionary<string, int> map, string column)
        {
            var raw = GetString(fields, map, column);
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (int.TryParse(raw, NumberStyles.Integer, CsvCulture, out var value))
                return value;
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return value;

            return null;
        }

        private static float GetFloat(string[] fields, Dictionary<string, int> map, string column)
        {
            var raw = GetString(fields, map, column);
            if (float.TryParse(raw, NumberStyles.Float, CsvCulture, out var value))
                return value;
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;

            return 0f;
        }
    }
}