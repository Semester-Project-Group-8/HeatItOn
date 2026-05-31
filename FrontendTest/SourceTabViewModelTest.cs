using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frontend.Data;
using Frontend.Models;
using Frontend.ViewModels;

namespace FrontendTest;

public class SourceTabViewModelTests
{
    [Fact]
    public async Task UpdateSource_SendsPut_ToUpdateEndpoint()
    {
        var initialSources = CreateSources();
        var recorder = new SourceHttpRecorder(initialSources);
        var vm = CreateViewModel(recorder);
        SeedViewModel(vm, initialSources);
        vm.SelectedFile = "source.csv";

        var source = new Source
        {
            Id = 1,
            FileName = "source.csv",
            TimeFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            TimeTo = new DateTime(2026, 1, 1, 1, 0, 0, DateTimeKind.Utc),
            HeatDemand = 10,
            ElectricityPrice = 20
        };
        source.HeatDemand = 99f;
        source.ElectricityPrice = 44f;

        vm.UpdateSource(source);

        await WaitForAsync(() => recorder.PutCalls.Count == 1);

        var putCall = recorder.PutCalls.Single();
        Assert.Equal($"/Source/Update/{source.Id}", putCall.Path);
        Assert.Contains("\"heatDemand\":99", putCall.Body);
        Assert.Contains("\"electricityPrice\":44", putCall.Body);
    }

    [Fact]
    public async Task DeleteSelected_RemovesRows_AndCallsDeletePerId()
    {
        var initialSources = CreateSources();
        var recorder = new SourceHttpRecorder(initialSources);
        var vm = CreateViewModel(recorder);

        SeedViewModel(vm, initialSources);
        vm.SelectedFile = "source.csv";

        var selected = new ArrayList
        {
            vm.Sources.First(s => s.Id == 1),
            vm.Sources.First(s => s.Id == 2)
        };

        vm.DeleteSelected(selected);

        await WaitForAsync(() => recorder.DeleteCalls.Count == 2);
        await WaitForAsync(() => vm.Sources.Count == 1);

        Assert.Equal(new[] { 1, 2 }, recorder.DeleteCalls.OrderBy(x => x).ToArray());
        Assert.Single(vm.Sources);
        Assert.Equal(3, vm.Sources.Single().Id);
    }

    private static SourceTabViewModel CreateViewModel(SourceHttpRecorder recorder)
    {
        var httpClient = new HttpClient(recorder)
        {
            BaseAddress = new Uri("http://localhost")
        };

        return new SourceTabViewModel(new SourceClient(httpClient, new PopupHub()), new PopupHub());
    }

    private static List<Source> CreateSources()
    {
        var start = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return new List<Source>
        {
            new()
            {
                Id = 1,
                FileName = "source.csv",
                TimeFrom = start,
                TimeTo = start.AddHours(1),
                HeatDemand = 10,
                ElectricityPrice = 20
            },
            new()
            {
                Id = 2,
                FileName = "source.csv",
                TimeFrom = start.AddHours(1),
                TimeTo = start.AddHours(2),
                HeatDemand = 11,
                ElectricityPrice = 21
            },
            new()
            {
                Id = 3,
                FileName = "source.csv",
                TimeFrom = start.AddHours(2),
                TimeTo = start.AddHours(3),
                HeatDemand = 12,
                ElectricityPrice = 22
            }
        };
    }

    private static async Task WaitForAsync(Func<bool> condition)
    {
        var timeout = TimeSpan.FromSeconds(5);
        var start = DateTime.UtcNow;
        while (!condition())
        {
            if (DateTime.UtcNow - start > timeout)
                throw new TimeoutException("Condition was not reached in time.");

            await Task.Delay(25);
        }
    }

    private static void SeedViewModel(SourceTabViewModel vm, List<Source> sources)
    {
        var allSourcesField = typeof(SourceTabViewModel).GetField("_allSources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var filesProperty = typeof(SourceTabViewModel).GetProperty("Files");

        var allSources = (System.Collections.ObjectModel.ObservableCollection<Source>?)allSourcesField?.GetValue(vm);
        var files = (System.Collections.ObjectModel.ObservableCollection<string>?)filesProperty?.GetValue(vm);

        allSources?.Clear();
        files?.Clear();

        foreach (var source in sources)
        {
            allSources?.Add(source);
            if (source.FileName is not null && files is not null && !files.Contains(source.FileName))
                files.Add(source.FileName);
        }
    }

    private sealed class SourceHttpRecorder : HttpMessageHandler
    {
        private readonly List<Source> _sources;

        public List<(string Path, string Body)> PutCalls { get; } = new();
        public List<int> DeleteCalls { get; } = new();

        public SourceHttpRecorder(List<Source> sources)
        {
            _sources = sources;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;

            if (request.Method == HttpMethod.Get && path.Equals("/Source/", StringComparison.OrdinalIgnoreCase))
            {
                var json = JsonSerializer.Serialize(_sources);
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
            }

            if (request.Method == HttpMethod.Put && path.StartsWith("/Source/Update/", StringComparison.OrdinalIgnoreCase))
            {
                var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
                PutCalls.Add((path, body));
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                };
            }

            if (request.Method == HttpMethod.Delete && path.StartsWith("/Source/Delete/", StringComparison.OrdinalIgnoreCase))
            {
                var idRaw = path.Split('/').Last();
                if (int.TryParse(idRaw, out var id))
                    DeleteCalls.Add(id);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}
