using Frontend.Data;
using Frontend.Data.CSV;
using Frontend.Models;

namespace FrontendTest
{
    public class CsvTests
    {
        private readonly IClient<Asset> _assetClient = new InMemoryAssetClient();
        private readonly IClient<Source> _sourceClient = new InMemorySourceClient();

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

            await _assetClient.Post(new Asset { Id = 1, Name = "Gas Boiler", MaxHeat = 12.5f, ProductionCost = 500, CO2Emission = 200, GasConsumption = 6.2f, OilConsumption = 0, MaxElectricity = 0 });
            CsvHandler.ExportAsset(path, _assetClient);

            await WaitUntilAsync(() => Task.FromResult(File.Exists(path)&& new FileInfo(path).Length > 0));

            var lines = await File.ReadAllLinesAsync(path);

            Assert.NotEmpty(lines);
            Assert.StartsWith("Name,MaxHeat", lines[0]);
        }

        [Fact]
        public async Task ImportAsset_ReadsCsvAndInsertsAsset()
        {
            var path = Path.GetTempFileName();

            await File.WriteAllLinesAsync(path, new[]
            {
                "Name,MaxHeat MW,Production Cost DKK/MWh(th),CO2 Emissions kg/MWh(th),Gas Consumption MW(th),Oil Consumption MW(th),Max Electricity MW(e)",
                "Gas Boiler Test,12.5,500,200,6.2,0,0"
            });

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

            await WaitUntilAsync(() => Task.FromResult(File.Exists(path) && new FileInfo(path).Length > 0));

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
            var cols = lines[1].Split(',');
            var csvCulture = new System.Globalization.CultureInfo("da-DK");
            var from = DateTime.Parse(cols[0], csvCulture);
            var to = DateTime.Parse(cols[1], csvCulture);
            Assert.Equal(new DateTime(2024,1,1,0,0,0), from);
            Assert.Equal(new DateTime(2024,1,1,1,0,0), to);
            Assert.Contains("10.25", lines[1], StringComparison.InvariantCulture);
        }
    }
    class InMemoryAssetClient : IClient<Asset>
    {
        private readonly List<Asset> _items = new List<Asset>();
        public Task Delete(int id)
        {
            _items.RemoveAll(a => a.Id == id);
            return Task.CompletedTask;
        }

        public Task<Asset?> Get(int id)
        {
            return Task.FromResult(_items.FirstOrDefault(a => a.Id == id));
        }

        public Task<List<Asset>> GetAll()
        {
            return Task.FromResult(_items.ToList());
        }

        public Task Post(Asset item)
        {
            if (item.Id == 0)
                item.Id = _items.Count + 1;
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task Put(Asset item)
        {
            var existing = _items.FirstOrDefault(a => a.Id == item.Id);
            if (existing != null)
            {
                _items.Remove(existing);
                _items.Add(item);
            }
            return Task.CompletedTask;
        }
    }

    class InMemorySourceClient : IClient<Source>
    {
        private readonly List<Source> _items = new List<Source>();
        public Task Delete(int id)
        {
            _items.RemoveAll(s => s.Id == id);
            return Task.CompletedTask;
        }

        public Task<Source?> Get(int id)
        {
            return Task.FromResult(_items.FirstOrDefault(s => s.Id == id));
        }

        public Task<List<Source>> GetAll()
        {
            return Task.FromResult(_items.ToList());
        }

        public Task Post(Source item)
        {
            if (item.Id == 0)
                item.Id = _items.Count + 1;
            _items.Add(item);
            return Task.CompletedTask;
        }

        public Task Put(Source item)
        {
            var existing = _items.FirstOrDefault(s => s.Id == item.Id);
            if (existing != null)
            {
                _items.Remove(existing);
                _items.Add(item);
            }
            return Task.CompletedTask;
        }
    }
}