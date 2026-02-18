using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

/// <summary>
/// Tests for <see cref="BrazilianDocument"/> — the helper for fields that accept
/// either a CPF or a CNPJ.
/// </summary>
[TestFixture]
public class BrazilianDocumentTests
{
    // ── known unambiguous inputs ─────────────────────────────────────────────

    private const string ValidCpfOnly  = "123.456.789-09";   // 12345678909  — valid CPF, not valid CNPJ
    private const string ValidCnpjOnly = "09.358.105/0001-91"; // — valid CNPJ, not valid CPF
    private const string InvalidBoth   = "00000000001";       // neither

    // Strings that are simultaneously valid as CPF and as an 11-char CNPJ (leading zeros implied).
    // Confirmed by running both Cpf.IsValid and Cnpj.IsValid.
    private const string Ambiguous1 = "00970938900";
    private const string Ambiguous2 = "00305265318";

    // ── IsValid ──────────────────────────────────────────────────────────────

    [Test]
    public void IsValid_UnambiguousCpf_WithNoHint_ReturnsTrue_AndCpf()
    {
        var (isValid, type) = BrazilianDocument.IsValid(ValidCpfOnly);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void IsValid_UnambiguousCnpj_WithNoHint_ReturnsTrue_AndCnpj()
    {
        var (isValid, type) = BrazilianDocument.IsValid(ValidCnpjOnly);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void IsValid_InvalidInput_WithNoHint_ReturnsFalse_AndUnknown()
    {
        var (isValid, type) = BrazilianDocument.IsValid(InvalidBoth);
        Assert.That(isValid, Is.False);
        Assert.That(type, Is.EqualTo(DocumentType.Unknown));
    }

    [Test]
    public void IsValid_NullInput_WithNoHint_ReturnsFalse_AndUnknown()
    {
        var (isValid, type) = BrazilianDocument.IsValid(null);
        Assert.That(isValid, Is.False);
        Assert.That(type, Is.EqualTo(DocumentType.Unknown));
    }

    [Test]
    public void IsValid_AmbiguousInput_WithNoHint_ReturnsFalse_AndUnknown()
    {
        var (isValid, type) = BrazilianDocument.IsValid(Ambiguous1);
        Assert.That(isValid, Is.False);
        Assert.That(type, Is.EqualTo(DocumentType.Unknown));
    }

    // ── IsValid with hint ────────────────────────────────────────────────────

    [Test]
    public void IsValid_AmbiguousInput_WithCpfHint_ReturnsTrue_AndCpf()
    {
        var (isValid, type) = BrazilianDocument.IsValid(Ambiguous1, DocumentType.Cpf);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void IsValid_AmbiguousInput_WithCnpjHint_ReturnsTrue_AndCnpj()
    {
        var (isValid, type) = BrazilianDocument.IsValid(Ambiguous1, DocumentType.Cnpj);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void IsValid_CpfInput_WithCnpjHint_FallsBackToCpf_ReturnsTrue()
    {
        // ValidCpfOnly is not a valid CNPJ, so hint=Cnpj falls back to CPF
        var (isValid, type) = BrazilianDocument.IsValid(ValidCpfOnly, DocumentType.Cnpj);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void IsValid_CnpjInput_WithCpfHint_FallsBackToCnpj_ReturnsTrue()
    {
        // ValidCnpjOnly is not a valid CPF, so hint=Cpf falls back to CNPJ
        var (isValid, type) = BrazilianDocument.IsValid(ValidCnpjOnly, DocumentType.Cpf);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void IsValid_InvalidInput_WithHint_ReturnsFalse_AndUnknown()
    {
        var (v1, t1) = BrazilianDocument.IsValid(InvalidBoth, DocumentType.Cpf);
        var (v2, t2) = BrazilianDocument.IsValid(InvalidBoth, DocumentType.Cnpj);

        Assert.That(v1, Is.False);
        Assert.That(t1, Is.EqualTo(DocumentType.Unknown));
        Assert.That(v2, Is.False);
        Assert.That(t2, Is.EqualTo(DocumentType.Unknown));
    }

    // ── IsValid ReadOnlySpan overload ────────────────────────────────────────

    [Test]
    public void IsValid_Span_UnambiguousCpf_ReturnsTrue_AndCpf()
    {
        var (isValid, type) = BrazilianDocument.IsValid(ValidCpfOnly.AsSpan());
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void IsValid_Span_AmbiguousInput_WithCpfHint_ReturnsTrue_AndCpf()
    {
        var (isValid, type) = BrazilianDocument.IsValid(Ambiguous2.AsSpan(), DocumentType.Cpf);
        Assert.That(isValid, Is.True);
        Assert.That(type, Is.EqualTo(DocumentType.Cpf));
    }

    // ── TryParse ─────────────────────────────────────────────────────────────

    [Test]
    public void TryParse_UnambiguousCpf_WithNoHint_ReturnsCpf()
    {
        var result = BrazilianDocument.TryParse(ValidCpfOnly, out var cpf, out _);
        Assert.That(result, Is.EqualTo(DocumentType.Cpf));
        Assert.That(cpf, Is.EqualTo(Cpf.Parse(ValidCpfOnly)));
    }

    [Test]
    public void TryParse_UnambiguousCnpj_WithNoHint_ReturnsCnpj()
    {
        var result = BrazilianDocument.TryParse(ValidCnpjOnly, out _, out var cnpj);
        Assert.That(result, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(cnpj, Is.EqualTo(Cnpj.Parse(ValidCnpjOnly)));
    }

    [Test]
    public void TryParse_InvalidInput_WithNoHint_ReturnsUnknown()
    {
        var result = BrazilianDocument.TryParse(InvalidBoth, out _, out _);
        Assert.That(result, Is.EqualTo(DocumentType.Unknown));
    }

    [Test]
    public void TryParse_AmbiguousInput_WithNoHint_ReturnsUnknown()
    {
        var result = BrazilianDocument.TryParse(Ambiguous1, out _, out _);
        Assert.That(result, Is.EqualTo(DocumentType.Unknown));
    }

    [Test]
    public void TryParse_AmbiguousInput_WithCpfHint_ReturnsCpf()
    {
        // Without hint → ambiguous
        Assert.That(BrazilianDocument.TryParse(Ambiguous1, out _, out _), Is.EqualTo(DocumentType.Unknown));

        // With Cpf hint → CPF wins
        var result = BrazilianDocument.TryParse(Ambiguous1, out var cpf, out _, DocumentType.Cpf);
        Assert.That(result, Is.EqualTo(DocumentType.Cpf));
        Assert.That(cpf, Is.EqualTo(Cpf.Parse(Ambiguous1)));
    }

    [Test]
    public void TryParse_AmbiguousInput_WithCnpjHint_ReturnsCnpj()
    {
        var result = BrazilianDocument.TryParse(Ambiguous1, out _, out var cnpj, DocumentType.Cnpj);
        Assert.That(result, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(cnpj, Is.EqualTo(Cnpj.Parse(Ambiguous1)));
    }

    [Test]
    public void TryParse_CpfInput_WithCnpjHint_FallsBackToCpf()
    {
        // CNPJ parse fails, falls back to CPF
        var result = BrazilianDocument.TryParse(ValidCpfOnly, out var cpf, out _, DocumentType.Cnpj);
        Assert.That(result, Is.EqualTo(DocumentType.Cpf));
        Assert.That(cpf, Is.EqualTo(Cpf.Parse(ValidCpfOnly)));
    }

    [Test]
    public void TryParse_CnpjInput_WithCpfHint_FallsBackToCnpj()
    {
        // CPF parse fails, falls back to CNPJ
        var result = BrazilianDocument.TryParse(ValidCnpjOnly, out _, out var cnpj, DocumentType.Cpf);
        Assert.That(result, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(cnpj, Is.EqualTo(Cnpj.Parse(ValidCnpjOnly)));
    }

    // ── TryParse ReadOnlySpan overload ───────────────────────────────────────

    [Test]
    public void TryParse_Span_UnambiguousCpf_ReturnsCpf()
    {
        var result = BrazilianDocument.TryParse(ValidCpfOnly.AsSpan(), out var cpf, out _);
        Assert.That(result, Is.EqualTo(DocumentType.Cpf));
        Assert.That(cpf, Is.EqualTo(Cpf.Parse(ValidCpfOnly)));
    }

    [Test]
    public void TryParse_Span_AmbiguousInput_WithCnpjHint_ReturnsCnpj()
    {
        var result = BrazilianDocument.TryParse(Ambiguous2.AsSpan(), out _, out var cnpj, DocumentType.Cnpj);
        Assert.That(result, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(cnpj, Is.EqualTo(Cnpj.Parse(Ambiguous2)));
    }

    // ── Parse ────────────────────────────────────────────────────────────────

    [Test]
    public void Parse_ValidCpf_ReturnsCpfType_AndSetsCpf()
    {
        var result = BrazilianDocument.Parse(ValidCpfOnly, out var cpf, out _);
        Assert.That(result, Is.EqualTo(DocumentType.Cpf));
        Assert.That(cpf, Is.EqualTo(Cpf.Parse(ValidCpfOnly)));
    }

    [Test]
    public void Parse_ValidCnpj_ReturnsCnpjType_AndSetsCnpj()
    {
        var result = BrazilianDocument.Parse(ValidCnpjOnly, out _, out var cnpj);
        Assert.That(result, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(cnpj, Is.EqualTo(Cnpj.Parse(ValidCnpjOnly)));
    }

    [Test]
    public void Parse_InvalidInput_ThrowsBadDocumentException()
    {
        Assert.That(
            () => BrazilianDocument.Parse(InvalidBoth, out _, out _),
            Throws.TypeOf<BadDocumentException>());
    }

    [Test]
    public void Parse_AmbiguousInput_WithNoHint_ThrowsBadDocumentException()
    {
        Assert.That(
            () => BrazilianDocument.Parse(Ambiguous1, out _, out _),
            Throws.TypeOf<BadDocumentException>());
    }

    [Test]
    public void Parse_AmbiguousInput_WithCpfHint_Succeeds()
    {
        var result = BrazilianDocument.Parse(Ambiguous1, out var cpf, out _, DocumentType.Cpf);
        Assert.That(result, Is.EqualTo(DocumentType.Cpf));
        Assert.That(cpf, Is.EqualTo(Cpf.Parse(Ambiguous1)));
    }

    [Test]
    public void Parse_AmbiguousInput_WithCnpjHint_Succeeds()
    {
        var result = BrazilianDocument.Parse(Ambiguous1, out _, out var cnpj, DocumentType.Cnpj);
        Assert.That(result, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(cnpj, Is.EqualTo(Cnpj.Parse(Ambiguous1)));
    }
}
