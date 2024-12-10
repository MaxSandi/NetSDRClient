using NetSDRClientApp.Services;

namespace NetSDRClient.Services
{
    internal class MockNetSDRNetworkService : INetSDRNetworkService
    {
        public Task<bool> ConnectAsync() => Task.FromResult(true);
        public Task<bool> DisconnectAsync() => Task.FromResult(true);
        public Task<bool> SendCommandAsync(byte[] command) => Task.FromResult(true);
        public Task<bool> SendCommandAsync(Memory<byte> command) => Task.FromResult(true);
        public Task<byte[]> ReceiveDataAsync() => Task.FromResult(new byte[0]);
    }
}
