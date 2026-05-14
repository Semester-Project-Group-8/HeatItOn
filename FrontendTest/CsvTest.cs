using Frontend.Data;
using Frontend.Data.CSV;
using Frontend.Models;

namespace FrontendTest
{
    public class CsvTests
    {
        private static readonly HttpClient client = new HttpClient()
        {
            BaseAddress =  new Uri("https://localhost:8080/")
        };
        private readonly AssetClient _assetClient = new AssetClient(client);
        private readonly SourceClient _sourceClient = new SourceClient(client);

        // -------------------------
        // Helper
        // -------------------------
        private static async Task WaitUntilAsync(
            Func<Task<bool>> condition,
            int timeoutMs = 5000,
            int pollMs = 50)
        {
            var start = DateTime.UtcNow;
            while (!await condition())
            {
                if ((DateTime.UtcNow - start).TotalMilliseconds > timeoutMs)
                    throw new TimeoutException("Condition not met within timeout.");

                await Task.Delay(pollMs);
            }
        }

        // =========================
        // Asset CSV
        // =========================

        [Fact]
        public async Task ExportAsset_CreatesValidCsvFile()
        {
            var path = Path.GetTempFileName();

            CsvHandler.ExportAsset(path, _assetClient);

            await WaitUntilAsync(() => Task.FromResult(File.Exists(path)));

            var lines = await File.ReadAllLinesAsync(path);

            Assert.NotEmpty(lines);
            Assert.StartsWith("Name,MaxHeat", lines[0]);
        }

        [Fact]
        public async Task ImportAsset_ReadsCsvAndInsertsAsset()
        {
            var path = Path.GetTempFileName();

            await File.WriteAllLinesAsync(path, [
                "Name,MaxHeat MW,Production Cost DKK/MWh(th),CO2 Emissions kg/MWh(th),Gas Consumption MW(th),Oil Consumption MW(th),Max Electricity MW(e)",
                "Gas Boiler Test,12.5,500,200,6.2,0,0"
            ]);

            CsvHandler.ImportAsset(path, _assetClient);

            await WaitUntilAsync(async () =>
            {
                var assets = await _assetClient.GetAll();
                return assets.Any(a => a.Name == "Gas Boiler Test");
            });

            var asset = (await _assetClient.GetAll())
                .First(a => a.Name == "Gas Boiler Test");

            Assert.Equal(12.5f, asset.MaxHeat);
        }

        // =========================
        // Result CSV
        // =========================

        [Fact]
        public async Task ExportResult_WritesCorrectCsv()
        {
            var path = Path.GetTempFileName();

            var results = new List<ResultTableRow>
            {
                new ResultTableRow
                {
                    Hour = "1:00",
                    ActiveAssets = "Gas Boiler",
                    HeatProduced = 100.5f,
                    Electricity = 20.25f,
                    Co2Produced = 300
                }
            };

            CsvHandler.ExportResult(path, results);

            await WaitUntilAsync(() => Task.FromResult(File.Exists(path)));

            var lines = await File.ReadAllLinesAsync(path);

            Assert.Equal(2, lines.Length);
            Assert.StartsWith("Hour,ActiveAssets", lines[0]);
            Assert.Contains("100.5", lines[1], StringComparison.InvariantCulture);
        }

        // =========================
        // Source CSV Import
        // =========================

        [Fact]
        public async Task ImportSource_ParsesDanishDatesCorrectly()
        {
            var path = Path.GetTempFileName();

            await File.WriteAllLinesAsync(path, new[]
            {
                "2024.01.01 00:00,2024.01.01 01:00,45.5,1200",
                "2024.01.01 01:00,2024.01.01 02:00,50,1300"
            });

            await CsvHandler.ImportSource(path, _sourceClient);

            var sources = await _sourceClient.GetAll();

            Assert.True(sources.Count >= 2);

            var first = sources.First();
            Assert.Equal(45.5f, first.HeatDemand);
            Assert.Equal(1200f, first.ElectricityPrice);
        }

        // =========================
        // Source CSV Export
        // =========================

        [Fact]
        public async Task ExportSource_WritesCorrectCsv()
        {
            var path = Path.GetTempFileName();

            var sources = new List<Source>
            {
                new Source
                {
                    TimeFrom = new DateTime(2024, 1, 1, 0, 0, 0),
                    TimeTo = new DateTime(2024, 1, 1, 1, 0, 0),
                    HeatDemand = 10.25f,
                    ElectricityPrice = 900
                }
            };

            CsvHandler.ExportSource(path, sources);

            await WaitUntilAsync(() => Task.FromResult(File.Exists(path)));

            var lines = await File.ReadAllLinesAsync(path);

            Assert.Equal(2, lines.Length);
            Assert.Contains("2024.01.01 00:00", lines[1]);
            Assert.Contains("10.25", lines[1], StringComparison.InvariantCulture);
        }
    }
}