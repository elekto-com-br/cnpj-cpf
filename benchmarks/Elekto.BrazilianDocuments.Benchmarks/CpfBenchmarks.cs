using BenchmarkDotNet.Attributes;

namespace Elekto.BrazilianDocuments.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="Cpf.IsValid(string?)"/> covering valid, invalid,
/// and leading-zero-trimmed numeric inputs.
/// Run in Release mode: dotnet run -c Release
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class CpfBenchmarks
{
    // ── dataset sizes ────────────────────────────────────────────────────────
    [Params(1_000, 10_000)]
    public int N { get; set; }

    // ── pre-built input arrays (populated in GlobalSetup) ────────────────────
    private string[] _valid = [];
    private string[] _invalid = [];
    private string[] _trimmed = [];     // leading zeros removed
    private string[] _mixed = [];       // mix of all the above

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(42);

        _valid   = BuildValid(rand, N);
        _invalid = BuildInvalid(rand, N);
        _trimmed = BuildTrimmed(rand, N);
        _mixed   = BuildMixed(rand, N);
    }

    // ── benchmarks ───────────────────────────────────────────────────────────

    [Benchmark(Description = "IsValid – valid")]
    public bool IsValid_Valid()
    {
        var ok = false;
        foreach (var s in _valid)
            ok = Cpf.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – invalid check digit")]
    public bool IsValid_Invalid()
    {
        var ok = false;
        foreach (var s in _invalid)
            ok = Cpf.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – leading zeros trimmed")]
    public bool IsValid_Trimmed()
    {
        var ok = false;
        foreach (var s in _trimmed)
            ok = Cpf.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – mixed dataset")]
    public bool IsValid_Mixed()
    {
        var ok = false;
        foreach (var s in _mixed)
            ok = Cpf.IsValid(s);
        return ok;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string[] BuildValid(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
        {
            var root = rand.NextInt64(0, 999_999_999L);
            var digits = Cpf.GetDigits(root);
            arr[i] = (root * 100 + digits).ToString("00000000000");
        }
        return arr;
    }

    private static string[] BuildInvalid(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
        {
            var root = rand.NextInt64(0, 999_999_999L);
            var digits = Cpf.GetDigits(root);
            var s = (root * 100 + digits).ToString("00000000000");
            arr[i] = FlipLastDigit(s);
        }
        return arr;
    }

    private static string[] BuildTrimmed(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
        {
            // Force a leading zero by keeping the root in [0, 99_999_999]
            var root = rand.NextInt64(0, 99_999_999L);
            var digits = Cpf.GetDigits(root);
            var s = (root * 100 + digits).ToString("00000000000");
            arr[i] = s.TrimStart('0');
            if (arr[i].Length == 0) arr[i] = "0";
        }
        return arr;
    }

    private static string[] BuildMixed(Random rand, int n)
    {
        var valid   = BuildValid(rand, n / 4 + 1);
        var invalid = BuildInvalid(rand, n / 4 + 1);
        var trimmed = BuildTrimmed(rand, n / 4 + 1);

        var list = new List<string>(n);
        list.AddRange(valid);
        list.AddRange(invalid);
        list.AddRange(trimmed);

        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rand.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list.Take(n).ToArray();
    }

    /// <summary>Increments the last digit to produce an invalid check digit.</summary>
    private static string FlipLastDigit(string s)
    {
        var last = s[^1];
        var flipped = last == '9' ? '0' : (char)(last + 1);
        return s[..^1] + flipped;
    }
}
