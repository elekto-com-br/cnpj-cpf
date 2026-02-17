using System.Text.Json;
using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

[TestFixture]
public class CpfTests
{
    // Valid CPFs for testing
    private static readonly string[] ValidCpfs =
    [
        "123.456.789-09",
        "123,456,,78909",   // Using different punctuation
        "12345678909",
        "1,23456,7890,9",    // Using different punctuation
        "1234.56,7890.9",    // Using different punctuation
        "987.654.321-00",
        "98765432100",
        "111.222.333-96",
        "11122233396",
        "999.888.777-14",
        "000.000.000-00",
        "00000000000",
    ];

    private static readonly string[] InvalidCpfs =
    [
        "123.456.789-00",      // Wrong check digit
        "123,456.789-00",      // Wrong check digit with different punctuation
        "12345678900",         // Wrong check digit
        "123456789012",        // Too many digits (12)
        "",
        "   ",
        "abc.def.ghi-jk",
        "12.345.678-90a",
        "12,345,678-90a",
    ];

    [Test]
    public void Create_WithKnownValues_ShouldCalculateCorrectCheckDigits()
    {
        // Test cases from the original implementation
        Assert.That(Cpf.Create(123_456_789L).ToString("G"), Is.EqualTo("123.456.789-09"));
        Assert.That(Cpf.Create(987_654_321L).ToString("G"), Is.EqualTo("987.654.321-00"));
        Assert.That(Cpf.Create(111_222_333L).ToString("G"), Is.EqualTo("111.222.333-96"));
        Assert.That(Cpf.Create(999_888_777L).ToString("G"), Is.EqualTo("999.888.777-14"));
    }

    [Test]
    public void Create_FromString_ShouldWork()
    {
        var cpf = Cpf.Create("123456789");
        Assert.That(cpf.ToString("G"), Is.EqualTo("123.456.789-09"));
    }

    [Test]
    public void Constructor_WithValidCpfStrings_ShouldSucceed()
    {
        foreach (var cpfStr in ValidCpfs)
        {
            Assert.That(() => new Cpf(cpfStr),
                Throws.Nothing,
                $"CPF '{cpfStr}' should be valid.");
        }
    }

    [Test]
    public void Constructor_WithInvalidCpfString_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cpf("12345678900"),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void Constructor_WithLong_ShouldSucceed()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void Constructor_WithInvalidLong_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cpf(12345678900L),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void IsValid_String_WithValidCpfs_ShouldReturnTrue()
    {
        foreach (var cpfStr in ValidCpfs)
        {
            Assert.That(Cpf.IsValid(cpfStr), Is.True,
                $"CPF '{cpfStr}' should be valid.");
        }
    }

    [Test]
    public void IsValid_String_WithInvalidCpfs_ShouldReturnFalse()
    {
        foreach (var cpfStr in InvalidCpfs)
        {
            Assert.That(Cpf.IsValid(cpfStr), Is.False,
                $"CPF '{cpfStr}' should be invalid.");
        }
    }

    [Test]
    public void IsValid_Long_WithValidCpf_ShouldReturnTrue()
    {
        Assert.That(Cpf.IsValid(12345678909L), Is.True);
    }

    [Test]
    public void IsValid_Long_WithInvalidCpf_ShouldReturnFalse()
    {
        Assert.That(Cpf.IsValid(12345678900L), Is.False);
        Assert.That(Cpf.IsValid(-1L), Is.False);
        Assert.That(Cpf.IsValid(100_000_000_000L), Is.False);
    }

    [Test]
    public void IsValid_WithNull_ShouldReturnFalse()
    {
        Assert.That(Cpf.IsValid(null), Is.False);
    }

    [Test]
    public void Parse_WithValidCpf_ShouldSucceed()
    {
        var cpf = Cpf.Parse("123.456.789-09");
        Assert.That(cpf.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void Parse_Long_WithValidCpf_ShouldSucceed()
    {
        var cpf = Cpf.Parse(12345678909L);
        Assert.That(cpf.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void Parse_WithInvalidCpf_ShouldThrowBadDocumentException()
    {
        Assert.That(() => Cpf.Parse("invalid"),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void TryParse_String_WithValidCpf_ShouldReturnTrue()
    {
        Assert.That(Cpf.TryParse("123.456.789-09", out var cpf), Is.True);
        Assert.That(cpf.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void TryParse_String_WithInvalidCpf_ShouldReturnFalse()
    {
        Assert.That(Cpf.TryParse("invalid", out _), Is.False);
    }

    [Test]
    public void TryParse_Nullable_WithValidCpf_ShouldReturnCpf()
    {
        var result = Cpf.TryParse("123.456.789-09");
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void TryParse_Nullable_WithInvalidCpf_ShouldReturnNull()
    {
        var result = Cpf.TryParse("invalid");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryParse_Long_WithValidCpf_ShouldReturnTrue()
    {
        Assert.That(Cpf.TryParse(12345678909L, out var cpf), Is.True);
        Assert.That(cpf.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void TryParse_Long_WithInvalidCpf_ShouldReturnFalse()
    {
        Assert.That(Cpf.TryParse(12345678900L, out _), Is.False);
    }

    [Test]
    public void EqualityOperators_ShouldBehaveCorrectly()
    {
        var cpf1 = new Cpf(12345678909L);
        var cpf2 = new Cpf(12345678909L);
        var cpf3 = new Cpf(98765432100L);

        Assert.That(cpf1 == cpf2, Is.True);
        Assert.That(cpf1 != cpf2, Is.False);
        Assert.That(cpf1 == cpf3, Is.False);
        Assert.That(cpf1 != cpf3, Is.True);
    }

    [Test]
    public void Equals_WithSameCpf_ShouldReturnTrue()
    {
        var cpf1 = new Cpf(12345678909L);
        var cpf2 = new Cpf(12345678909L);

        Assert.That(cpf1.Equals(cpf2), Is.True);
        Assert.That(cpf1.Equals((object)cpf2), Is.True);
    }

    [Test]
    public void GetHashCode_SameValues_ShouldBeEqual()
    {
        var cpf1 = new Cpf(12345678909L);
        var cpf2 = new Cpf(12345678909L);

        Assert.That(cpf1.GetHashCode(), Is.EqualTo(cpf2.GetHashCode()));
    }

    [Test]
    public void CompareTo_ShouldOrderCorrectly()
    {
        var cpf1 = new Cpf(0L);
        var cpf2 = new Cpf(12345678909L);

        Assert.That(cpf1.CompareTo(cpf2), Is.LessThan(0));
        Assert.That(cpf2.CompareTo(cpf1), Is.GreaterThan(0));
        Assert.That(cpf1.CompareTo(cpf1), Is.EqualTo(0));
    }

    [Test]
    public void ToString_FormatS_ShouldReturnNumericWithoutLeadingZeros()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.ToString("S"), Is.EqualTo("12345678909"));
    }

    [Test]
    public void ToString_FormatS_WithLeadingZeros_ShouldNotPad()
    {
        var cpf = new Cpf(1234567890L);  // 01234567890
        Assert.That(cpf.ToString("S"), Is.EqualTo("1234567890"));
    }

    [Test]
    public void ToString_FormatB_ShouldReturn11DigitsWithLeadingZeros()
    {
        var cpf = new Cpf(1234567890L);
        Assert.That(cpf.ToString("B"), Is.EqualTo("01234567890"));
        Assert.That(cpf.ToString("B").Length, Is.EqualTo(11));
    }

    [Test]
    public void ToString_FormatG_ShouldReturnFormattedCpf()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.ToString("G"), Is.EqualTo("123.456.789-09"));
    }

    [Test]
    public void ToString_FormatG_WithLeadingZeros_ShouldFormat()
    {
        var cpf = new Cpf(1234567890L);
        Assert.That(cpf.ToString("G"), Is.EqualTo("012.345.678-90"));
    }

    [Test]
    public void ToString_Default_ShouldReturnFormatG()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.ToString(), Is.EqualTo("123.456.789-09"));
    }

    [Test]
    public void ToString_InvalidFormat_ShouldThrowArgumentOutOfRangeException()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(() => cpf.ToString("X"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void ToLong_ShouldReturnNumericValue()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void GetDigits_ShouldReturnCorrectCheckDigits()
    {
        Assert.That(Cpf.GetDigits(123456789L), Is.EqualTo(9)); // "09" => 9
        Assert.That(Cpf.GetDigits(987654321L), Is.EqualTo(0)); // "00" => 0
        Assert.That(Cpf.GetDigits(111222333L), Is.EqualTo(96)); // "96" => 96
    }

    [Test]
    public void Empty_ShouldBeValidAndAllZeros()
    {
        Assert.That(Cpf.Empty.ToLong(), Is.EqualTo(0L));
        Assert.That(Cpf.Empty.ToString("B"), Is.EqualTo("00000000000"));
    }

    [Test]
    public void ImplicitConversion_FromString_ShouldWork()
    {
        Cpf? cpf = "123.456.789-09";
        Assert.That(cpf, Is.Not.Null);
        Assert.That(cpf!.Value.ToLong(), Is.EqualTo(12345678909L));
    }

    [Test]
    public void ImplicitConversion_FromNullOrEmpty_ShouldReturnNull()
    {
        Cpf? cpf1 = null;
        Cpf? cpf2 = "";

        Assert.That(cpf1, Is.Null);
        Assert.That(cpf2, Is.Null);
    }

    [Test]
    public void JsonSerialization_ShouldPreserveCpfValue()
    {
        var original = new Cpf(12345678909L);
        var options = new JsonSerializerOptions { WriteIndented = true };

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Cpf>(json);

        Assert.That(deserialized, Is.EqualTo(original));
    }

    [Test]
    public void JsonSerialization_InObject_ShouldWork()
    {
        var original = new CpfContainer
        {
            MainCpf = new Cpf(12345678909L),
            Related = [new Cpf(98765432100L), new Cpf(11122233396L)]
        };
        var options = new JsonSerializerOptions { WriteIndented = true };

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<CpfContainer>(json);

        Assert.That(deserialized!.MainCpf, Is.EqualTo(original.MainCpf));
        Assert.That(deserialized.Related.Length, Is.EqualTo(original.Related.Length));
    }

    [Test]
    public void Create_WithNegativeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.That(() => Cpf.Create(-1L),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Create_WithTooLargeValue_ShouldThrowArgumentOutOfRangeException()
    {
        Assert.That(() => Cpf.Create(1_000_000_000L),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void NormalizedValues_ShouldBeEqual()
    {
        var cpf1 = new Cpf("123.456.789-09");
        var cpf2 = new Cpf("12345678909");

        Assert.That(cpf1, Is.EqualTo(cpf2));
    }

    [Test]
    public void CompareTo_Object_WithBoxedCpf_ShouldWork()
    {
        var cpf = new Cpf(12345678909L);
        object obj = new Cpf(12345678909L);
        Assert.That(cpf.CompareTo(obj), Is.EqualTo(0));
    }

    [Test]
    public void CompareTo_Object_WithCnpj_ShouldThrowArgumentException()
    {
        var cpf = new Cpf(12345678909L);
        object obj = new Cnpj("09358105000191");
        Assert.That(() => cpf.CompareTo(obj), Throws.ArgumentException);
    }
    
    [Test]
    public void Create_WithNonNumericString_ShouldThrowBadDocumentException()
    {
        Assert.That(() => Cpf.Create("no-cpf"),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void Parse_Long_WithInvalidValue_ShouldThrowBadDocumentException()
    {
        // 12345678900 is invalid checksum
        Assert.That(() => Cpf.Parse(12345678900L),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void Cpf_WithTooLongString_ShouldBeInvalid()
    {
        var longCpf = new string('1', 25);
        Assert.That(Cpf.IsValid(longCpf), Is.False);
    }

    [Test]
    public void CompareTo_WithNull_ShouldReturnPositive()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.CompareTo(null), Is.GreaterThan(0));
    }

    [Test]
    public void CompareTo_WithDifferentType_ShouldThrowArgumentException()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(() => cpf.CompareTo("not a cpf"), Throws.ArgumentException);
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.Equals(null), Is.False);
    }

    [Test]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var cpf = new Cpf(12345678909L);
        Assert.That(cpf.Equals("not a cpf"), Is.False);
    }

    [Test]
    public void ImplicitOperator_WithInvalidString_ShouldThrowBadDocumentException()
    {
        Assert.That(() =>
        {
            Cpf? c = "invalid-cpf";
        }, Throws.TypeOf<BadDocumentException>()
            .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void JsonSerialization_WithInvalidTokenType_ShouldThrowJsonException()
    {
        var json = "12345678909"; // Number instead of string
        Assert.That(() => JsonSerializer.Deserialize<Cpf>(json), Throws.TypeOf<JsonException>());
    }

    [Test]
    public void IsValid_WithOnlyPunctuation_ShouldReturnFalse()
    {
         // Covers digitCount < 1 check in slow path because "." and "-" are valid chars in loop but don't increment digitCount
         Assert.That(Cpf.IsValid(".-"), Is.False);
    }

    [Test]
    public void IsValid_With11CharsIncludingPunctuation_ShouldReturnTrue()
    {
        // Length 11, but contains punctuation -> Canonical is 012.345.678-90, that is valide
        Assert.That(Cpf.IsValid("12345.67890"), Is.True);
    }

    private class TestCpf
    {
        public string Cpf { get; set; } = null!;
        public bool IsValid { get; set; }
        public string ValidCpf { get; set; } = null!;
        public bool? IsValidResult { get; set; } = null;
    }

    [TestCase(0.0, 0.0, 100_000, true)]
    [TestCase(0.5, 1.0, 100_000, true)]
    [TestCase(0.5, 0.0, 100_000, true)]
    [TestCase(0.5, 0.1, 100_000, true)]
    public void Performance_Validation_ShouldBeEfficient(double probTrimStart, double probError, int numToCreate, bool assertGc)
    {
        var rand = new Random(42);
        var all = new TestCpf[numToCreate];

        var sw = new System.Diagnostics.Stopwatch();

        // Create test data
        for (var i = 0; i < numToCreate; i++)
        {
            var initial = rand.NextInt64(0, 999_999_999);
            var digits = Cpf.GetDigits(initial);
            var fullCpf = initial * 100 + digits;

            sw.Start();
            var cpfStr = fullCpf.ToString("00000000000");
            sw.Stop();
            var validCpf = cpfStr;

            var shouldTrimZero = cpfStr.StartsWith("0") && cpfStr.Length > 1 && rand.NextDouble() <= probTrimStart;
            if (shouldTrimZero)
            {
                cpfStr = cpfStr.TrimStart('0');
            }

            var isError = rand.NextDouble() <= probError;
            if (isError)
            {
                var posError = rand.Next(cpfStr.Length);
                var original = cpfStr[posError];
                var substitute = (char)(original == '9' ? '0' : original + 1);

                cpfStr = cpfStr.Substring(0, posError) + substitute + cpfStr.Substring(posError + 1);

                if (substitute == '0' && cpfStr.EndsWith("00"))
                {
                    cpfStr = validCpf.Substring(0, 9) + "99";
                }
            }

            all[i] = new TestCpf
            {
                Cpf = cpfStr,
                IsValid = Cpf.IsValid(cpfStr),
                ValidCpf = validCpf
            };
        }

        Console.WriteLine($"Creation total: {numToCreate:N0} in {sw.Elapsed:g}");
        Console.WriteLine($"Probability of trimming leading zero: {probTrimStart:P0}");
        Console.WriteLine($"Probability of introducing error: {probError:P0}");
        Console.WriteLine($"Creation: {numToCreate / sw.Elapsed.TotalSeconds:N0} c/s");

        sw.Reset();

        // Force GC to collect all garbage from setup phase
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);

        // Use thread-specific allocation tracking (GC.CollectionCount is process-wide
        // and captures GC triggered by other threads like NUnit adapter, test runner, etc.)
        var allocBefore = GC.GetAllocatedBytesForCurrentThread();

        // Validate all
        sw.Start();
        for (var i = 0; i < numToCreate; i++)
        {
            all[i].IsValidResult = Cpf.IsValid(all[i].Cpf);
        }
        sw.Stop();

        var allocAfter = GC.GetAllocatedBytesForCurrentThread();
        var allocatedBytes = allocAfter - allocBefore;

        Console.WriteLine($"Validation total: {numToCreate:N0} in {sw.Elapsed:g}");
        Console.WriteLine($"Validation: {numToCreate / sw.Elapsed.TotalSeconds:N0} v/s");
        Console.WriteLine($"Allocated bytes: {allocatedBytes:N0}");

        // Assert all
        for (var i = 0; i < numToCreate; i++)
        {
            Assert.That(all[i].IsValidResult, Is.EqualTo(all[i].IsValid),
                $"CPF {i:N0} '{all[i].Cpf}' (original {all[i].ValidCpf}) should be {(all[i].IsValid ? "valid" : "invalid")}.");
        }

        if (assertGc)
        {
            Assert.That(allocatedBytes, Is.EqualTo(0),
                $"Cpf.IsValid should be zero-allocation, but {allocatedBytes:N0} bytes were allocated in {numToCreate:N0} calls.");
        }
    }

    private class CpfContainer
    {
        public Cpf MainCpf { get; set; }
        public Cpf[] Related { get; set; } = [];
    }
}
