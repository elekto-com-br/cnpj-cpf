using BenchmarkDotNet.Attributes;
using Elekto.BrazilianDocuments;

namespace Elekto.BrazilianDocuments.Benchmarks;

/// <summary>
/// Benchmarks for <see cref="Cnpj.IsValid(string?)"/> covering numeric-only,
/// alphanumeric, leading-zero-trimmed and intentionally invalid inputs.
/// Run in Release mode: dotnet run -c Release
/// </summary>
[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class CnpjBenchmarks
{
    // ── dataset sizes ────────────────────────────────────────────────────────
    [Params(1_000, 10_000)]
    public int N { get; set; }

    // ── pre-built input arrays (populated in GlobalSetup) ────────────────────
    private string[] _numericValid = [];
    private string[] _numericInvalid = [];
    private string[] _numericTrimmed = [];      // leading zeros removed
    private string[] _alphanumericValid = [];
    private string[] _alphanumericInvalid = [];
    private string[] _mixed = [];               // mix of all the above

    // ── character pools ──────────────────────────────────────────────────────
    private const string AlphaChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string NumericChars = "0123456789";

    [GlobalSetup]
    public void Setup()
    {
        var rand = new Random(69);

        _numericValid     = BuildNumericValid(rand, N);
        _numericInvalid   = BuildNumericInvalid(rand, N);
        _numericTrimmed   = BuildNumericTrimmed(rand, N);
        _alphanumericValid   = BuildAlphanumericValid(rand, N);
        _alphanumericInvalid = BuildAlphanumericInvalid(rand, N);
        _mixed = BuildMixed(rand, N);
    }

    // ── benchmarks ───────────────────────────────────────────────────────────

    [Benchmark(Description = "IsValid – numeric, valid")]
    public bool IsValid_NumericValid()
    {
        var ok = false;
        foreach (var s in _numericValid)
            ok = Cnpj.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – numeric, invalid check digit")]
    public bool IsValid_NumericInvalid()
    {
        var ok = false;
        foreach (var s in _numericInvalid)
            ok = Cnpj.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – numeric, leading zeros trimmed")]
    public bool IsValid_NumericTrimmed()
    {
        var ok = false;
        foreach (var s in _numericTrimmed)
            ok = Cnpj.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – alphanumeric, valid")]
    public bool IsValid_AlphanumericValid()
    {
        var ok = false;
        foreach (var s in _alphanumericValid)
            ok = Cnpj.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – alphanumeric, invalid")]
    public bool IsValid_AlphanumericInvalid()
    {
        var ok = false;
        foreach (var s in _alphanumericInvalid)
            ok = Cnpj.IsValid(s);
        return ok;
    }

    [Benchmark(Description = "IsValid – mixed dataset")]
    public bool IsValid_Mixed()
    {
        var ok = false;
        foreach (var s in _mixed)
            ok = Cnpj.IsValid(s);
        return ok;
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private static string[] BuildNumericValid(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
            arr[i] = Cnpj.Create(RandomNumericRoot(rand), RandomNumericBranch(rand)).ToString("B");
        return arr;
    }

    private static string[] BuildNumericInvalid(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
        {
            var s = Cnpj.Create(RandomNumericRoot(rand), RandomNumericBranch(rand)).ToString("B");
            arr[i] = FlipLastDigit(s);
        }
        return arr;
    }

    private static string[] BuildNumericTrimmed(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
        {
            var s = Cnpj.Create(RandomNumericRoot(rand, leadingZero: true), RandomNumericBranch(rand)).ToString("B");
            arr[i] = s.TrimStart('0');
            if (arr[i].Length == 0) arr[i] = "0";
        }
        return arr;
    }

    private static string[] BuildAlphanumericValid(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
            arr[i] = Cnpj.Create(RandomAlphaRoot(rand), RandomAlphaBranch(rand)).ToString("B");
        return arr;
    }

    private static string[] BuildAlphanumericInvalid(Random rand, int n)
    {
        var arr = new string[n];
        for (var i = 0; i < n; i++)
        {
            var s = Cnpj.Create(RandomAlphaRoot(rand), RandomAlphaBranch(rand)).ToString("B");
            arr[i] = FlipLastChar(s);
        }
        return arr;
    }

    private static string[] BuildMixed(Random rand, int n)
    {
        var numericValid     = BuildNumericValid(rand, n / 6 + 1);
        var numericInvalid   = BuildNumericInvalid(rand, n / 6 + 1);
        var numericTrimmed   = BuildNumericTrimmed(rand, n / 6 + 1);
        var alphaValid       = BuildAlphanumericValid(rand, n / 6 + 1);
        var alphaInvalid     = BuildAlphanumericInvalid(rand, n / 6 + 1);

        var list = new List<string>(n);
        list.AddRange(numericValid);
        list.AddRange(numericInvalid);
        list.AddRange(numericTrimmed);
        list.AddRange(alphaValid);
        list.AddRange(alphaInvalid);

        // shuffle
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rand.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list.Take(n).ToArray();
    }

    private static string RandomNumericRoot(Random rand, bool leadingZero = false)
    {
        var chars = new char[8];
        chars[0] = leadingZero ? '0' : NumericChars[rand.Next(NumericChars.Length)];
        for (var i = 1; i < 8; i++)
            chars[i] = NumericChars[rand.Next(NumericChars.Length)];
        return new string(chars);
    }

    private static string RandomNumericBranch(Random rand)
    {
        var chars = new char[4];
        for (var i = 0; i < 4; i++)
            chars[i] = NumericChars[rand.Next(NumericChars.Length)];
        return new string(chars);
    }

    private static string RandomAlphaRoot(Random rand)
    {
        var chars = new char[8];
        for (var i = 0; i < 8; i++)
            chars[i] = AlphaChars[rand.Next(AlphaChars.Length)];
        return new string(chars);
    }

    private static string RandomAlphaBranch(Random rand)
    {
        var chars = new char[4];
        for (var i = 0; i < 4; i++)
            chars[i] = AlphaChars[rand.Next(AlphaChars.Length)];
        return new string(chars);
    }

    /// <summary>Increments the last digit to produce an invalid check digit.</summary>
    private static string FlipLastDigit(string s)
    {
        var last = s[^1];
        var flipped = last == '9' ? '0' : (char)(last + 1);
        // Avoid accidentally creating a valid "all-zeros" check pair
        return s[..^1] + flipped;
    }

    /// <summary>Increments the last character (digit or letter) to produce an invalid check digit.</summary>
    private static string FlipLastChar(string s)
    {
        var last = char.ToUpper(s[^1]);
        char flipped;
        if (last is >= '0' and <= '8') flipped = (char)(last + 1);
        else if (last == '9') flipped = 'A';
        else if (last is >= 'A' and < 'Z') flipped = (char)(last + 1);
        else flipped = '0'; // 'Z' → '0'
        return s[..^1] + flipped;
    }
}
