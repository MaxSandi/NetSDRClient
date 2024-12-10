﻿namespace NetSDRClientApp.Services
{
    public interface INetSDRNetworkService
    {
        Task<bool> ConnectAsync();
        Task<bool> DisconnectAsync();

        Task<bool> SendCommandAsync(byte[] command);

        Task<byte[]> ReceiveDataAsync();
    }
}