using NetSDRClient.App.Services;

var networkService = new NetSDRNetworkService("localhost");
var client = new NetSDRClientApp.NetSDRClient(networkService, "samples.bin");

await client.ConnectAsync();
await client.SetFrequencyAsync(15_010_000);
await client.StartAsync();

await Task.Delay(10000);

await client.StopAsync();
await client.DisconnectAsync();
