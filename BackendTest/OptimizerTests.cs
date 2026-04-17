using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;
using Backend.Services;

namespace BackendTest
{
    public class OptimizerTests : IDisposable
    {
        private readonly BackendDbContext _context;
        private readonly SourceService _sourceService;
        private readonly AssetsService _assetsService;
        private readonly ResultListService _resultListService;
        private readonly ResultService _resultService;
        private readonly OptimizerService _optimizerService;

        public OptimizerTests()
        {
            var options = new DbContextOptionsBuilder<BackendDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BackendDbContext(options);
            _sourceService = new SourceService(_context);
            _assetsService = new AssetsService(_context);
            _resultService = new ResultService(_context);
            _resultListService = new ResultListService(_context);
            _optimizerService = new OptimizerService(_assetsService, _sourceService, _resultService,_resultListService);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task CalculateNetProductionCost_PositiveCase()
        {
            var result = await _assetsService.AddAsset(
                id: 1,
                name: "Boiler",
                maxHeat: 100f,
                productionCost: 5000,
                co2Emission: 50,
                gasConsumption: 10f,
                oilConsumption: 5f,
                maxElectricity: 0f
            );
            Source s = new Source
            {
                Id = 1,
                TimeFrom = new DateTime(2018, 8, 11, 00, 00, 00),
                TimeTo = new DateTime(2018, 8, 11, 01, 00, 00),
                HeatDemand = 53f,
                ElectricityPrice = 1f
            };
            await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
            float correct = 500000;
            float received = await _optimizerService.CalculateNetProductionCost(1, s.TimeFrom);
            Assert.Equal(correct, received);
        }
        [Fact]
        public async Task CalculateNetProductionCost_PositiveCaseMakePower()
        {
            var result = await _assetsService.AddAsset(
                id: 1,
                name: "Boiler",
                maxHeat: 100f,
                productionCost: 5000,
                co2Emission: 50,
                gasConsumption: 10f,
                oilConsumption: 5f,
                maxElectricity: 10f
            );
            Source s = new Source
            {
                Id = 1,
                TimeFrom = new DateTime(2018, 8, 11, 00, 00, 00),
                TimeTo = new DateTime(2018, 8, 11, 01, 00, 00),
                HeatDemand = 53f,
                ElectricityPrice = 1f
            };
            await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
            float correct = 499990;
            float received = await _optimizerService.CalculateNetProductionCost(1, s.TimeFrom);
            Assert.Equal(correct, received);
        }
        [Fact]
        public async Task CalculateNetProductionCost_UsePowerPositiveCase()
        {
            var result = await _assetsService.AddAsset(
                id: 1,
                name: "Boiler",
                maxHeat: 100f,
                productionCost: 5000,
                co2Emission: 50,
                gasConsumption: 10f,
                oilConsumption: 5f,
                maxElectricity: -10f
            );
            Source s = new Source
            {
                Id = 1,
                TimeFrom = new DateTime(2018, 8, 11, 00, 00, 00),
                TimeTo = new DateTime(2018, 8, 11, 01, 00, 00),
                HeatDemand = 53f,
                ElectricityPrice = 1f
            };
            await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
            float correct = 500010;
            float received = await _optimizerService.CalculateNetProductionCost(1, s.TimeFrom);
            Assert.Equal(correct, received);
        }

        [Fact]
        public async Task CalculateNetProductionCost_NegativeCase()
        {
            var result = await _assetsService.AddAsset(
                id: 1,
                name: "Boiler",
                maxHeat: 100f,
                productionCost: 5000,
                co2Emission: 50,
                gasConsumption: 10f,
                oilConsumption: 5f,
                maxElectricity: 0f
            );
            Source s = new Source
            {
                Id = 1,
                TimeFrom = new DateTime(2018, 8, 11, 00, 00, 00),
                TimeTo = new DateTime(2018, 8, 11, 01, 00, 00),
                HeatDemand = 53f,
                ElectricityPrice = 245f
            };
            await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
            float correct = 5;
            float received = await _optimizerService.CalculateNetProductionCost(1, s.TimeFrom);
            Assert.NotEqual(correct, received);
        }

        [Fact]
        public async Task CalculateNetProductionCost_EdgeCase()
        {
            var result = await _assetsService.AddAsset(
                id: 1,
                name: "Boiler",
                maxHeat: 100f,
                productionCost: 0,
                co2Emission: 50,
                gasConsumption: 10f,
                oilConsumption: 5f,
                maxElectricity: 0f
            );
            Source s = new Source
            {
                Id = 1,
                TimeFrom = new DateTime(2018, 8, 11, 00, 00, 00),
                TimeTo = new DateTime(2018, 8, 11, 01, 00, 00),
                HeatDemand = 53f,
                ElectricityPrice = 245f
            };
            await _sourceService.AddSource(s.Id, s.TimeFrom, s.TimeTo, s.HeatDemand, s.ElectricityPrice);
            float correct = 0;
            float received = await _optimizerService.CalculateNetProductionCost(1, s.TimeFrom);
            Assert.Equal(correct, received);
        }
    }
}
