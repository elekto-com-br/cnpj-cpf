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
}
