using NetSDRClientApp.Helpers;
using NetSDRClientApp.Model;
using NetSDRClientApp.Services;
using System.Collections.Concurrent;

namespace NetSDRClientApp
{
    public class NetSDRClient
    {
        #region Interfaces
        private readonly INetSDRNetworkService _networkService;
        private readonly string _filePath;
        #endregion

        #region Private fields
        internal ulong _currentFrequency = 14_010_000;
        private ConcurrentQueue<byte[]> _dataQueue = new ConcurrentQueue<byte[]>();
        internal CancellationTokenSource _cancellationTokenSource = new();
        internal Task _receivingTask = Task.CompletedTask;
        internal Task _processingTask = Task.CompletedTask;
        #endregion

        #region Constructors
        public NetSDRClient(INetSDRNetworkService networkService, string filePath)
        {
            _networkService = networkService;
            _filePath = filePath;
        }
        #endregion

        #region Public methods
        public Task<bool> ConnectAsync()
        {
            return _networkService.ConnectAsync();
        }

        public Task<bool> DisconnectAsync()
        {
            return _networkService.DisconnectAsync();
        }

        public async Task<bool> StartAsync()
        {
            var startParameters = CreateRunParameters(DataType.Complex, CaptureMode.Continuos24bit);
            var message = CreateControlItemMessage((ushort)ControlItemType.SetState, NetSDRHelper.StructToBytes(startParameters));

            var result = await _networkService.SendCommandAsync(message);
            if (!result)
                return false;

            _cancellationTokenSource = new CancellationTokenSource();
            _receivingTask = Task.Run(() => StartReceivingDataAsync(_cancellationTokenSource.Token));
            _processingTask = Task.Run(() => StartProcessingDataAsync(_cancellationTokenSource.Token));
            return true;
        }

        public async Task<bool> StopAsync()
        {
            var startParameters = CreateStopParameters();
            var message = CreateControlItemMessage((ushort)ControlItemType.SetState, NetSDRHelper.StructToBytes(startParameters));

            var result = await _networkService.SendCommandAsync(message);
            if (!result)
                return false;

            _cancellationTokenSource?.Cancel();
            await Task.WhenAll([_receivingTask, _processingTask]);
            return true;
        }

        public async Task<bool> SetFrequencyAsync(ulong frequency)
        {
            var startParameters = CreateSetFrequencyParameters(ChannelId.Channel1, frequency);
            var message = CreateControlItemMessage((ushort)ControlItemType.SetFrequency, NetSDRHelper.StructToBytes(startParameters));

            var result = await _networkService.SendCommandAsync(message);
            if (!result)
                return false;

            _currentFrequency = frequency;
            return true;
        }
        #endregion

        #region Private methods
        private byte[] CreateControlItemMessage(ushort controlItem, byte[] parameters)
        {
            byte[] message = new byte[2 + 2 + parameters.Length]; // 2 - header, 2 - controlItem

            var header = CreateHeader(0, (ushort)message.Length);
            message[0] = (byte)(header & 0xFF);
            message[1] = (byte)((header >> 8) & 0xFF);
            message[2] = (byte)(controlItem & 0xFF);
            message[3] = (byte)((controlItem >> 8) & 0xFF);
            Buffer.BlockCopy(parameters, 0, message, 4, parameters.Length);

            return message;
        }

        private ushort CreateHeader(byte type, ushort length)
        {
            if (type < 0 || type > 7)
                throw new ArgumentOutOfRangeException(nameof(type));

            if (length < 0 || length > 0x1FFF)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be from 0 to 8191.");

            ushort lengthLsb = (ushort)(length & 0xFF);
            ushort lengthMsb = (ushort)((length >> 8) & 0x1F);
            return (ushort)((lengthLsb) | (type << 13) | (lengthMsb << 8));
        }

        private StateParameters CreateRunParameters(DataType dataType, CaptureMode captureType, byte numberSamples = 0)
        {
            return new StateParameters()
            {
                StateType = (byte)StateType.Run,
                ChannelType = (byte)(dataType == DataType.Complex ? 0 | (1 << 7) : 0),
                CaptureType = (byte)captureType,
                NumberSamples = captureType == CaptureMode.FIFO16bit ? numberSamples : (byte)0
            };
        }

        private StateParameters CreateStopParameters()
        {
            return new StateParameters()
            {
                StateType = (byte)StateType.Idle,
            };
        }

        private SetFrequencyParameters CreateSetFrequencyParameters(ChannelId channel, ulong frequency)
        {
            return new SetFrequencyParameters()
            {
                ChannelId = (byte)channel,
                Frequency =
                [
                    (byte)(frequency & 0xFF),
                    (byte)((frequency >> 8) & 0xFF),
                    (byte)((frequency >> 16) & 0xFF),
                    (byte)((frequency >> 24) & 0xFF),
                    (byte)((frequency >> 32) & 0xFF),
                ]
            };
        }

        private async Task StartReceivingDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    byte[] data = await _networkService.ReceiveDataAsync();

                    if (data != null && data.Length > 0)
                        _dataQueue.Enqueue(data);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while receiving data: {ex.Message}");
            }
        }

        private async Task StartProcessingDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                using BinaryWriter fileStream = new BinaryWriter(File.Open(_filePath, FileMode.OpenOrCreate));
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        if (_dataQueue.TryDequeue(out var data))
                        {
                            var parsingData = ParsingData(data);

                            var offset = 4; // write only data samples (offset - 2 static bytes + 2 bytes sequence Number)
                            fileStream.Write(data, offset, data.Length - offset);
                            fileStream.Flush();
                        }
                        else
                        {
                            await Task.Delay(50, cancellationToken); //TODO: set correct delay
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while writing data: {ex.Message}");
            }
        }

        private byte[] ParsingData(byte[] data)
        {
            // TODO: check  Unsolicited Control Item and change 
            var messageType = NetSDRHelper.ParseMessageHeader(data.AsSpan(0, 2)).type;

            return data;

        }
        #endregion
    }
}
