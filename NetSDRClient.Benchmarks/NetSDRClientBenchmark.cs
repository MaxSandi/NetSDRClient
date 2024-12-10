using BenchmarkDotNet.Attributes;
using NetSDRClient.Services;

namespace NetSDRClient.Benchmarks
{
    [MemoryDiagnoser]
    public class NetSDRClientBenchmark
    {
        private NetSDRClientApp.NetSDRClient _client;

        [GlobalSetup]
        public void Setup()
        {
            var mockNetworkService = new MockNetSDRNetworkService();
            _client = new(mockNetworkService, string.Empty);
        }

        [Benchmark]
        public async Task SetFrequencyOldBenchmark()
        {
            await _client.SetFrequencyOldAsync(14_010_000);
        }

        [Benchmark]
        public async Task SetFrequencyBenchmark()
        {
            await _client.SetFrequencyAsync(14_010_000);
        }
    }
}
