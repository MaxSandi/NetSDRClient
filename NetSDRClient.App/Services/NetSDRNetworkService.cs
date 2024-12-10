namespace NetSDRClientApp.Services
{
    using global::NetSDRClientApp.Helpers;
    using System;
    using System.Buffers;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    public class NetSDRNetworkService : INetSDRNetworkService
    {
        #region Private fields
        private readonly int _udpPort;
        private readonly IPEndPoint _tcpEndPoint;

        private TcpClient? _tcpClient;
        private NetworkStream? _tcpStream;
        private UdpClient? _udpClient;
        #endregion


        public bool IsConnected => _tcpClient is not null && _tcpClient.Connected;

        public NetSDRNetworkService(string tcpHost, int tcpPort = 50000, int udpPort = 60000)
        {
            _tcpEndPoint = new IPEndPoint(IPAddress.Parse(tcpHost), tcpPort);
            _udpPort = udpPort;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _tcpClient = new TcpClient();
                _udpClient = new UdpClient(_udpPort);

                await _tcpClient.ConnectAsync(_tcpEndPoint.Address, _tcpEndPoint.Port);
                _tcpStream = _tcpClient.GetStream();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Task<bool> DisconnectAsync()
        {
            try
            {
                _tcpStream?.Close();
                _tcpClient?.Close();
                _udpClient?.Close();
                return Task.FromResult(true);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        public async Task<bool> SendCommandAsync(byte[] command)
        {
            try
            {
                if (!IsConnected)
                    return false;

                if (_tcpStream is not null && _tcpStream.CanWrite)
                {
                    await _tcpStream.WriteAsync(command);
                    return await HandleSendCommandResponseAsync(command.Length);
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<byte[]> ReceiveDataAsync()
        {
            try
            {
                if (_udpClient is null)
                    return [];

                var result = await _udpClient.ReceiveAsync();
                return result.Buffer;
            }
            catch (Exception)
            {
                return Array.Empty<byte>();
            }
        }

        private async Task<bool> HandleSendCommandResponseAsync(int commandLength)
        {
            if (_tcpStream is null || !_tcpStream.CanRead)
                return false;

            using var header = MemoryPool<byte>.Shared.Rent(16);
            await _tcpStream.ReadAsync(header.Memory);

            // "NAK" check
            var messageLength = NetSDRHelper.ParseMessageHeader(header.Memory.Span).length;
            return messageLength != 2; //TODO: rework for return Result<byte[]> for implement other commands
        }
    }

}
