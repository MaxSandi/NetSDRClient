using BenchmarkDotNet.Running;
using NetSDRClient.Benchmarks;

var summary = BenchmarkRunner.Run<NetSDRClientBenchmark>();
Console.WriteLine(summary);
