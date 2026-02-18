using BenchmarkDotNet.Running;
using Elekto.BrazilianDocuments.Benchmarks;

BenchmarkSwitcher.FromAssembly(typeof(CnpjBenchmarks).Assembly).Run(args);
