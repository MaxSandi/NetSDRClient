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
        public async Task SetFrequencyAsyncBenchmark()
        {
            await _client.SetFrequencyAsync(14_010_000);
        }
    }
}
