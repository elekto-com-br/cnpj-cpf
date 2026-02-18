using System.Text.Json;
using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

[TestFixture]
public class CnpjTests
{
    // Valid CNPJs for testing (various formats)
    private static readonly string[] ValidCnpjs =
    [
        "21.552.200/0001-27",
        "21,552,200/0001=27",     // Using different punctuation
        "98.503.200/0001-61",
        "48.465.264/0001-47",
        "48.465,264,0001,47",     // Using different punctuation
        "86.674.875/0001-94",
        "03.536.783/0001-10",
        "03536783-0001/10",
        "0353.6783-0001/10",      // Bad placed puntuation
        "353,67,83-0001/10",      // Multiple punctuation
        "353,67,83;0001/10",      // Multiple punctuation
        "353;67.83;0001\\10",      // Multiple punctuation
        "3.536.783/0001-10",
        "3536783000110",
        "12.ABC.345/01DE-35",      // Alphanumeric with punctuation
        "00.ELE.KTO/0001-40",      // Alphanumeric
        "ELEKTO-0001/40",          // Mixed format
        "ELEKTO/0001-40",
        "ELEKTO000140",            // Alphanumeric no punctuation
        "00.ERR.ADO/ERRO-51",      // "ERROR/ERROR" in Portuguese
        "00ERRADOERRO51",
        "ERRADOERRO51",
        "ERRADO/ERRO-51",
        "84.773.357/5047-53",
        "84.773.357/504X-53",      // Alphanumeric branch
        "10263273483639",
        "102632X3483639",          // Alphanumeric in root
        "93.774.412/9143-74",
        "1/0001-36",               // Minimal format (leading zeros omitted)
        "1/000136",
        "1000136"
    ];

    private static readonly string[] InvalidCnpjs =
    [
        "12345678901234",          // Invalid check digits
        "21.552.200/0001-28",      // Wrong check digit
        "21,552.200/0001,28",      // Wrong check digit
        "12.345.678/0001-0",       // Too few digits
        "",
        "   ",
        "abc",
        "11111111111111",          // All same digits (might be valid mathematically but typically rejected)
    ];

    [Test]
    public void Constructor_WithValidCnpjStrings_ShouldSucceed()
    {
        foreach (var cnpjStr in ValidCnpjs)
        {
            Assert.That(() => new Cnpj(cnpjStr),
                Throws.Nothing,
                $"CNPJ '{cnpjStr}' should be valid.");
        }
    }

    [Test]
    public void Constructor_WithInvalidCnpjString_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cnpj("12345678901234"),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void Constructor_WithNullOrEmpty_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cnpj(null!), Throws.TypeOf<BadDocumentException>()
            .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
        Assert.That(() => new Cnpj(""), Throws.TypeOf<BadDocumentException>()
            .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
        Assert.That(() => new Cnpj("   "), Throws.TypeOf<BadDocumentException>()
            .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void IsValid_WithValidCnpjs_ShouldReturnTrue()
    {
        foreach (var cnpjStr in ValidCnpjs)
        {
            Assert.That(Cnpj.IsValid(cnpjStr), Is.True,
                $"CNPJ '{cnpjStr}' should be valid.");
        }
    }

    [Test]
    public void IsValid_WithInvalidCnpjs_ShouldReturnFalse()
    {
        foreach (var cnpjStr in InvalidCnpjs)
        {
            Assert.That(Cnpj.IsValid(cnpjStr), Is.False,
                $"CNPJ '{cnpjStr}' should be invalid.");
        }
    }

    [Test]
    public void IsValid_WithNull_ShouldReturnFalse()
    {
        Assert.That(Cnpj.IsValid(null), Is.False);
    }

    [Test]
    public void Parse_WithValidCnpj_ShouldSucceed()
    {
        var cnpj = Cnpj.Parse("09358105000191");
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void Constructor_FromSpan_WithValidCnpj_ShouldSucceed()
    {
        var cnpj = new Cnpj("09358105000191".AsSpan());
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void IsValid_ReadOnlySpan_WithValidCnpj_ShouldReturnTrue()
    {
        var cnpj = "09.358.105/0001-91".AsSpan();
        Assert.That(Cnpj.IsValid(cnpj), Is.True);
    }

    [Test]
    public void Parse_ReadOnlySpan_WithValidCnpj_ShouldSucceed()
    {
        var cnpj = Cnpj.Parse("09.358.105/0001-91".AsSpan());
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void TryParse_ReadOnlySpan_WithValidCnpj_ShouldReturnTrue()
    {
        var input = "09.358.105/0001-91".AsSpan();
        Assert.That(Cnpj.TryParse(input, out var cnpj), Is.True);
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void Parse_WithInvalidCnpj_ShouldThrowBadDocumentException()
    {
        Assert.That(() => Cnpj.Parse("invalid"),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void TryParse_WithValidCnpj_ShouldReturnTrue()
    {
        Assert.That(Cnpj.TryParse("09358105000191", out var cnpj), Is.True);
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void TryParse_WithInvalidCnpj_ShouldReturnFalse()
    {
        Assert.That(Cnpj.TryParse("invalid", out _), Is.False);
    }

    [Test]
    public void TryParse_Nullable_WithValidCnpj_ShouldReturnCnpj()
    {
        var result = Cnpj.TryParse("09358105000191");
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void TryParse_Nullable_WithInvalidCnpj_ShouldReturnNull()
    {
        var result = Cnpj.TryParse("invalid");
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ParseAndTryParse_ShouldReturnEquivalentObjects()
    {
        const string valid = "09358105000191";
        var parsed = Cnpj.Parse(valid);

        Assert.That(Cnpj.TryParse(valid, out var tryParsed), Is.True);
        Assert.That(parsed, Is.EqualTo(tryParsed));
    }

    [Test]
    public void EqualityOperators_ShouldBehaveCorrectly()
    {
        var cnpj1 = new Cnpj("09358105000191");
        var cnpj2 = new Cnpj("09358105000191");
        var cnpj3 = new Cnpj("54641030000106");

        Assert.That(cnpj1 == cnpj2, Is.True);
        Assert.That(cnpj1 != cnpj2, Is.False);
        Assert.That(cnpj1 == cnpj3, Is.False);
        Assert.That(cnpj1 != cnpj3, Is.True);
    }

    [Test]
    public void Equals_WithSameCnpj_ShouldReturnTrue()
    {
        var cnpj1 = new Cnpj("09358105000191");
        var cnpj2 = new Cnpj("09358105000191");

        Assert.That(cnpj1.Equals(cnpj2), Is.True);
        Assert.That(cnpj1.Equals((object)cnpj2), Is.True);
    }

    [Test]
    public void GetHashCode_SameValues_ShouldBeEqual()
    {
        var cnpj1 = new Cnpj("09358105000191");
        var cnpj2 = new Cnpj("09358105000191");

        Assert.That(cnpj1.GetHashCode(), Is.EqualTo(cnpj2.GetHashCode()));
    }

    [Test]
    public void CompareTo_ShouldOrderCorrectly()
    {
        var cnpj1 = new Cnpj("00000000000000");
        var cnpj2 = new Cnpj("09358105000191");

        Assert.That(cnpj1.CompareTo(cnpj2), Is.LessThan(0));
        Assert.That(cnpj2.CompareTo(cnpj1), Is.GreaterThan(0));
        Assert.That(cnpj1.CompareTo(cnpj1), Is.EqualTo(0));
    }

    [Test]
    public void ToString_FormatS_ShouldReturnWithoutLeadingZeros()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.ToString("S"), Is.EqualTo("9358105000191"));
    }

    [Test]
    public void ToString_FormatB_ShouldReturn14CharactersWithLeadingZeros()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
        Assert.That(cnpj.ToString("B").Length, Is.EqualTo(14));
    }

    [Test]
    public void ToString_FormatBS_ShouldReturnRootOnly()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.ToString("BS"), Is.EqualTo("09358105"));
    }

    [Test]
    public void ToString_FormatG_ShouldReturnFormattedCnpj()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.ToString("G"), Is.EqualTo("09.358.105/0001-91"));
    }

    [Test]
    public void ToString_Default_ShouldReturnFormatG()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.ToString(), Is.EqualTo("09.358.105/0001-91"));
    }

    [Test]
    public void StringInterpolation_ShouldUseFormatSpecifiers()
    {
        var cnpj = new Cnpj("09358105000191");
        var general = $"{cnpj}";
        var bare = $"{cnpj:B}";

        Assert.That(general, Is.EqualTo("09.358.105/0001-91"));
        Assert.That(bare, Is.EqualTo("09358105000191"));
    }

    [Test]
    public void ToString_InvalidFormat_ShouldThrowArgumentOutOfRangeException()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(() => cnpj.ToString("X"), Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public void Value_ShouldReturnCleanCnpj()
    {
        var cnpj = new Cnpj("09.358.105/0001-91");
        Assert.That(cnpj.Value, Is.EqualTo("09358105000191"));
    }

    [Test]
    public void CompareTo_Object_WithBoxedCnpj_ShouldWork()
    {
        var cnpj = new Cnpj("09358105000191");
        object obj = new Cnpj("09358105000191");
        Assert.That(cnpj.CompareTo(obj), Is.EqualTo(0));
    }

    [Test]
    public void Root_ShouldReturnFirst8Characters()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.Root, Is.EqualTo("09358105"));
    }

    [Test]
    public void Branch_ShouldReturnCharacters9To12()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.Branch, Is.EqualTo("0001"));
    }

    [Test]
    public void CheckDigits_ShouldReturnLast2Characters()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.CheckDigits, Is.EqualTo("91"));
    }

    [Test]
    public void Create_WithRootAndBranch_ShouldCalculateCheckDigits()
    {
        var cnpj = Cnpj.Create("09358105", "0001");
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void Create_WithAlphanumericRootAndBranch_ShouldWork()
    {
        var cnpj = Cnpj.Create("ELEKTO", "0001");
        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
        Assert.That(cnpj.ToString("B"), Is.EqualTo("00ELEKTO000140"));
    }

    [Test]
    public void GetDigits_ShouldReturnCorrectCheckDigits()
    {
        var digits = Cnpj.GetDigits("093581050001");
        Assert.That(digits, Is.EqualTo(91)); // dv1=9, dv2=1 => 9*10+1=91
    }

    [Test]
    public void Empty_ShouldBeValidAndAllZeros()
    {
        Assert.That(Cnpj.Empty.ToString("B"), Is.EqualTo("00000000000000"));
    }

    [Test]
    public void Parse_FromString_ShouldWork()
    {
        var cnpj = Cnpj.Parse("09358105000191");
        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
    }

    [Test]
    public void TryParse_FromNullOrEmpty_ShouldReturnNull()
    {
        Cnpj? cnpj1 = Cnpj.TryParse(null);
        Cnpj? cnpj2 = Cnpj.TryParse(string.Empty);

        Assert.That(cnpj1, Is.Null);
        Assert.That(cnpj2, Is.Null);
    }

    [Test]
    public void JsonSerialization_ShouldPreserveCnpjValue()
    {
        var original = new Cnpj("09358105000191");
        var options = new JsonSerializerOptions { WriteIndented = true };

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<Cnpj>(json);

        Assert.That(deserialized, Is.EqualTo(original));
    }

    [Test]
    public void JsonSerialization_InObject_ShouldWork()
    {
        var original = new CnpjContainer
        {
            MainCnpj = new Cnpj("09358105000191"),
            Related = ValidCnpjs.Take(5).Select(c => new Cnpj(c)).ToArray()
        };
        var options = new JsonSerializerOptions { WriteIndented = true };

        var json = JsonSerializer.Serialize(original, options);
        var deserialized = JsonSerializer.Deserialize<CnpjContainer>(json);

        Assert.That(deserialized!.MainCnpj, Is.EqualTo(original.MainCnpj));
        Assert.That(deserialized.Related.Length, Is.EqualTo(original.Related.Length));
    }

    [Test]
    public void AlphanumericCnpj_Elekto_ShouldBeValid()
    {
        var cnpj = new Cnpj("ELEKTO000140");
        Assert.That(cnpj.ToString("B"), Is.EqualTo("00ELEKTO000140"));
        Assert.That(cnpj.Root, Is.EqualTo("00ELEKTO"));
    }

    [Test]
    public void AlphanumericCnpj_WithMinimalFormat_ShouldBeValid()
    {
        // "1/0001-36" is the smallest valid CNPJ
        var cnpj = new Cnpj("1/0001-36");
        Assert.That(cnpj.ToString("B"), Is.EqualTo("00000001000136"));
    }

    [Test]
    public void NormalizedValues_ShouldBeEqual()
    {
        var cnpj1 = new Cnpj("09.358.105/0001-91");
        var cnpj2 = new Cnpj("09358105000191");
        var cnpj3 = new Cnpj("9358105000191");

        Assert.That(cnpj1, Is.EqualTo(cnpj2));
        Assert.That(cnpj2, Is.EqualTo(cnpj3));
    }

    [Test]
    public void Constructor_WithInputTooLong_ShouldThrowBadDocumentException()
    {
        var longString = new string('1', 20);
        Assert.That(() => new Cnpj(longString), Throws.TypeOf<BadDocumentException>()
            .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void CompareTo_WithNull_ShouldReturnPositive()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.CompareTo(null), Is.GreaterThan(0));
    }

    [Test]
    public void CompareTo_WithDifferentType_ShouldThrowArgumentException()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(() => cnpj.CompareTo("not a cnpj"), Throws.ArgumentException);
    }

    [Test]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var cnpj = new Cnpj("09358105000191");
        Assert.That(cnpj.Equals(null), Is.False);
    }

    [Test]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        var cnpj = new Cnpj("09358105000191");
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.That(cnpj.Equals("not a cnpj"), Is.False);
    }

    [Test]
    public void Parse_WithInvalidString_ShouldThrowBadDocumentException()
    {
        Assert.That(() => Cnpj.Parse("invalid-cnpj"), Throws.TypeOf<BadDocumentException>()
            .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void JsonSerialization_WithInvalidTokenType_ShouldThrowJsonException()
    {
        var json = "12345678901234"; // Number instead of string
        Assert.That(() => JsonSerializer.Deserialize<Cnpj>(json), Throws.TypeOf<JsonException>());
    }

    [Test]
    public void CaseInsensitive_ShouldBeEqual()
    {
        var cnpj1 = new Cnpj("elekto000140");
        var cnpj2 = new Cnpj("ELEKTO000140");

        Assert.That(cnpj1, Is.EqualTo(cnpj2));
    }

    private class CnpjContainer
    {
        public Cnpj MainCnpj { get; init; }
        public Cnpj[] Related { get; init; } = [];
    }

    [Test]
    public void Validate_WithValidCnpjs_ShouldReturnTrue()
    {
        foreach (var cnpj in ValidCnpjs)
        {
            Assert.That(Cnpj.IsValid(cnpj), Is.True,
                $"CNPJ '{cnpj}' should be valid.");
        }
    }

    [Test]
    public void Validate_WithInvalidCnpj_ShouldReturnFalse()
    {
        // Too few valid digits
        Assert.That(Cnpj.IsValid("12.345.678/0001-0"), Is.False,
            "CNPJ with fewer than 14 valid digits should be invalid.");

        // Incorrect check digit
        Assert.That(Cnpj.IsValid("21.552.200/0001-28"), Is.False,
            "CNPJ with incorrect check digit should be invalid.");
    }

    [Test]
    public void Validate_WithNullOrEmpty_ShouldReturnFalse()
    {
        Assert.That(Cnpj.IsValid(null), Is.False, "Null input should be invalid.");
        Assert.That(Cnpj.IsValid(string.Empty), Is.False, "Empty input should be invalid.");
        Assert.That(Cnpj.IsValid("         "), Is.False, "Whitespace input should be invalid.");
    }

    [Test]
    public void Validate_WithTooShortInput_ShouldReturnFalse()
    {
        Assert.That(Cnpj.IsValid("123456"), Is.False, "Input shorter than 7 characters should be invalid.");
    }

    [Test]
    public void Validate_WithTooLongInput_ShouldReturnFalse()
    {
        Assert.That(Cnpj.IsValid("12345678901234567890"), Is.False,
            "Input longer than 18 characters should be invalid.");
    }

    [Test]
    public void Validade_WithEnoughLength_ButFewValidChars_ShouldReturnFalse()
    {
        // Length is 8 (ok), but only 4 valid chars (digits)
        Assert.That(Cnpj.IsValid("1.2.3.4."), Is.False);
    }

    [Test]
    public void Validade_WithEnoughLength_ButTooManyValidChars_ShouldReturnFalse()
    {
        // Length is 15 (ok), but 15 valid chars (digits) -> Max is 14
        Assert.That(Cnpj.IsValid("123456789012345"), Is.False);
    }

    [Test]
    public void Create_FromRootAndOrder_ShouldReturnValidCnpj()
    {
        var cnpj = Cnpj.Create("09358105", "0001");
        var digits = Cnpj.GetDigits("093581050001");

        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
        Assert.That(digits, Is.EqualTo(91)); // dv1=9, dv2=1 => 9*10+1=91
        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
    }

    [Test]
    public void Create_FromRootAndOrder_WithPadding_ShouldWork()
    {
        var cnpj = Cnpj.Create("1", "1");

        Assert.That(cnpj.ToString("B"), Is.EqualTo("00000001000136"));
        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
    }

    [Test]
    public void Create_ElektoCnpj_ShouldWork()
    {
        var cnpj = Cnpj.Create("ELEKTO", "0001");

        Assert.That(cnpj.ToString("B"), Is.EqualTo("00ELEKTO000140"));
        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
    }

    [Test]
    public void Create_ErrorCnpj_ShouldWork()
    {
        var cnpj = Cnpj.Create("ERRADO", "ERRO");

        Assert.That(cnpj.ToString("B"), Is.EqualTo("00ERRADOERRO51"));
        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
    }

    [Test]
    public void Create_FromRootAndOrder_ShouldValidate()
    {
        var cnpj = Cnpj.Create("ELEKTO", "2011");

        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
    }

    [Test]
    public void Create_FromSingleString_ShouldWork()
    {
        var cnpj = Cnpj.Create("093581050001");

        Assert.That(cnpj.ToString("B"), Is.EqualTo("09358105000191"));
        Assert.That(Cnpj.IsValid(cnpj.ToString("B")), Is.True);
    }

    [Test]
    public void Create_FromSingleString_WithNull_ShouldThrowArgumentNullException()
    {
        Assert.That(() => Cnpj.Create(null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Create_FromSingleString_WithEmpty_ShouldThrowArgumentNullException()
    {
        Assert.That(() => Cnpj.Create(string.Empty),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Create_FromSingleString_WithWhitespace_ShouldThrowArgumentNullException()
    {
        Assert.That(() => Cnpj.Create("   "),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Create_WithNullRoot_ShouldThrowArgumentNullException()
    {
        Assert.That(() => Cnpj.Create(null!, "0001"),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Create_WithNullBranch_ShouldThrowArgumentNullException()
    {
        Assert.That(() => Cnpj.Create("12345678", null!),
            Throws.TypeOf<ArgumentNullException>());
    }

    [Test]
    public void Create_WithTooLongRoot_ShouldThrowArgumentException()
    {
        Assert.That(() => Cnpj.Create("123456789", "0001"),
            Throws.TypeOf<ArgumentException>()
                .With.Property("ParamName").EqualTo("root"));
    }

    [Test]
    public void Create_WithTooLongBranch_ShouldThrowArgumentException()
    {
        Assert.That(() => Cnpj.Create("12345678", "00011"),
            Throws.TypeOf<ArgumentException>()
                .With.Property("ParamName").EqualTo("branch"));
    }

    [Test]
    public void Create_WithInvalidCharactersInRoot_ShouldThrowArgumentException()
    {
        Assert.That(() => Cnpj.Create("1234$678", "0001"),
            Throws.TypeOf<ArgumentException>()
                .With.Property("ParamName").EqualTo("root"));
    }

    [Test]
    public void Create_WithInvalidCharactersInBranch_ShouldThrowArgumentException()
    {
        Assert.That(() => Cnpj.Create("12345678", "00#1"),
            Throws.TypeOf<ArgumentException>()
                .With.Property("ParamName").EqualTo("branch"));
    }

    [Test]
    public void Create_WithLowercaseInput_ShouldNormalizeToUppercase()
    {
        var cnpj1 = Cnpj.Create("elekto", "0001");
        var cnpj2 = Cnpj.Create("ELEKTO", "0001");

        Assert.That(cnpj1.ToString("B"), Is.EqualTo(cnpj2.ToString("B")));
    }

    [Test]
    public void Created_RandomCnpjs_ShouldAllBeValid()
    {
        var rand = new Random(69);
        const int numToCreate = 10_000;

        for (var i = 0; i < numToCreate; i++)
        {
            var useAlpha = rand.NextDouble() <= 0.2;
            var (root, branch) = CreateRandom(rand, useAlpha);
            var cnpj = Cnpj.Create(root, branch).ToString("B");

            Assert.That(Cnpj.IsValid(cnpj), Is.True,
                $"Created CNPJ '{cnpj}' should be valid.");
        }
    }

    [Test]
    public void Validate_WithLeadingZerosOmitted_ShouldWork()
    {
        // Create a CNPJ that starts with zeros
        var fullCnpj = Cnpj.Create("1", "1").ToString("B");
        Assert.That(fullCnpj, Is.EqualTo("00000001000136"));

        // Validate with leading zeros omitted
        Assert.That(Cnpj.IsValid("1000136"), Is.True);
        Assert.That(Cnpj.IsValid("1/0001-36"), Is.True);
    }

    [Test]
    public void Validate_WithTrailingPunctuation_ShouldBeValid()
    {
        // Punctuation is ignored, so trailing dot is accepted if numeric part is valid
        Assert.That(Cnpj.IsValid("09358105000191."), Is.True);
    }

    [Test]
    public void Create_RootWithPunctuation_ShouldThrow()
    {
        // Punctuation is not allowed in Create root parameter
        Assert.That(() => Cnpj.Create("12.3", "0001"), Throws.ArgumentException);
    }

    private class TestCnpj
    {
        public string Cnpj { get; init; } = null!;
        public bool IsValid { get; init; }
        public string ValidCnpj { get; init; } = null!;
        public bool? IsValidResult { get; set; }

    }

    [TestCase(0.2, 0.0, 0.0, 100_000, true)]
    [TestCase(0.2, 0.5, 1.0, 100_000, true)]
    [TestCase(0.2, 0.5, 0.0, 100_000, true)]
    [TestCase(0.2, 0.5, 0.1, 100_000, true)]
    public void Performance_Validation_ShouldBeEfficient(double probAlpha, double probTrimStart, double probError, int numToCreate, bool assertGc)
    {

        var rand = new Random(69);
        var all = new TestCnpj[numToCreate];

        var sw = new System.Diagnostics.Stopwatch();

        // Create test data
        for (var i = 0; i < numToCreate; i++)
        {
            var useAlpha = rand.NextDouble() <= probAlpha;
            var (root, branch) = CreateRandom(rand, useAlpha);

            sw.Start();
            var cnpj = Cnpj.Create(root, branch).ToString("B");
            sw.Stop();
            var validCnpj = cnpj;

            var shouldTrimZero = root.StartsWith("0") && root.Length > 1 && rand.NextDouble() <= probTrimStart;
            if (shouldTrimZero)
            {
                cnpj = cnpj.TrimStart('0');
            }

            var isError = rand.NextDouble() <= probError;
            if (isError)
            {
                var posError = rand.Next(cnpj.Length);
                var original = char.ToUpper(cnpj[posError]);
                var substitute = (char)(original + 1);
                if (substitute > 'Z')
                    substitute = '0';
                if (substitute is > '9' and < 'A')
                    substitute = 'A';

                cnpj = cnpj.Substring(0, posError) + substitute + cnpj.Substring(posError + 1);

                if (substitute == '0' && cnpj.EndsWith("00"))
                {
                    cnpj = validCnpj.Substring(0, 12) + "99";
                }
            }

            all[i] = new TestCnpj
            {
                Cnpj = cnpj,
                IsValid = Cnpj.IsValid(cnpj),
                ValidCnpj = validCnpj
            };
        }

        Console.WriteLine($"Creation total: {numToCreate:N0} in {sw.Elapsed:g}");
        Console.WriteLine($"Probability of alpha chars: {probAlpha:P0}");
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
            all[i].IsValidResult = Cnpj.IsValid(all[i].Cnpj);
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
                $"CNPJ {i:N0} '{all[i].Cnpj}' (original {all[i].ValidCnpj}) should be {(all[i].IsValid ? "valid" : "invalid")}.");
        }

        if (assertGc)
        {
            Assert.That(allocatedBytes, Is.EqualTo(0),
                $"Cnpj.IsValid should be zero-allocation, but {allocatedBytes:N0} bytes were allocated in {numToCreate:N0} calls.");
        }


    }

    private static (string root, string branch) CreateRandom(Random rand, bool useAlpha)
    {
        const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        var max = useAlpha ? chars.Length : 10;

        var rootChars = new char[8];
        var branchChars = new char[4];

        for (var i = 0; i < 8; i++)
        {
            rootChars[i] = chars[rand.Next(max)];
        }

        for (var j = 0; j < 4; j++)
        {
            branchChars[j] = chars[rand.Next(max)];
        }

        return (new string(rootChars), new string(branchChars));
    }

    [Test]
    public void Constructor_FromSpan_WithTooLongInput_ShouldThrowBadDocumentException()
    {
        // input.Length > MaxInputLength – short-circuits the || so !Validate is not evaluated
        var longString = new string('1', 20);
        Assert.That(() => new Cnpj(longString.AsSpan()),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void Constructor_FromSpan_WithTooLongButValidCharCount_ShouldThrowBadDocumentException()
    {
        // input.Length <= MaxInputLength but !Validate – covers the second branch of the || on line 106
        Assert.That(() => new Cnpj("12345678901234".AsSpan()),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    // ── Span overload – bad-path coverage ────────────────────────────────────

    [Test]
    public void Constructor_FromSpan_WithEmptySpan_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cnpj(ReadOnlySpan<char>.Empty),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void Constructor_FromSpan_WithWhitespaceSpan_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cnpj("   ".AsSpan()),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void Constructor_FromSpan_WithInvalidCnpj_ShouldThrowBadDocumentException()
    {
        Assert.That(() => new Cnpj("12345678901234".AsSpan()),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void IsValid_ReadOnlySpan_WithAllWhitespace_ShouldReturnFalse()
    {
        Assert.That(Cnpj.IsValid("         ".AsSpan()), Is.False);
    }

    [Test]
    public void IsValid_ReadOnlySpan_WithInvalidCnpj_ShouldReturnFalse()
    {
        Assert.That(Cnpj.IsValid("12345678901234".AsSpan()), Is.False);
    }

    [Test]
    public void Parse_ReadOnlySpan_WithInvalidCnpj_ShouldThrowBadDocumentException()
    {
        Assert.That(() => Cnpj.Parse("12345678901234".AsSpan()),
            Throws.TypeOf<BadDocumentException>()
                .With.Property(nameof(BadDocumentException.SourceType)).EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void TryParse_ReadOnlySpan_WithInvalidCnpj_ShouldReturnFalse()
    {
        Assert.That(Cnpj.TryParse("12345678901234".AsSpan(), out _), Is.False);
    }

    [Test]
    public void TryParse_NullableSpan_WithInvalidCnpj_ShouldReturnNull()
    {
        Cnpj? result = Cnpj.TryParse("12345678901234".AsSpan());
        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryParse_NullableSpan_WithValidCnpj_ShouldReturnCnpj()
    {
        Cnpj? result = Cnpj.TryParse("09358105000191".AsSpan());
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Value.ToString("B"), Is.EqualTo("09358105000191"));
    }
}
