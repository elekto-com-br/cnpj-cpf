using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

[TestFixture]
public class CnpjStaticApiTests
{
    // Valid CNPJs for testing
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

    [Test]
    public void Performance_Validation_ShouldBeEfficient()
    {
        const double probAlpha = 0.2;
        const double probTrimStart = 0.5;
        const double probError = 0.1;

        var rand = new Random(69);
        const int numToCreate = 1_000_000;

        var all = new (string cnpj, bool isValid, string validCnpj)[numToCreate];

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

            all[i] = (cnpj, !isError, validCnpj);
        }

        Console.WriteLine($"Creation total: {numToCreate:N0} in {sw.Elapsed:g}");
        Console.WriteLine($"Creation: {(numToCreate / sw.Elapsed.TotalSeconds):N0} c/s");

        sw.Reset();

        var before2 = GC.CollectionCount(2);
        var before1 = GC.CollectionCount(1);
        var before0 = GC.CollectionCount(0);

        // Validate all
        for (var i = 0; i < numToCreate; i++)
        {
            sw.Start();
            var isValid = Cnpj.IsValid(all[i].cnpj);
            sw.Stop();

            Assert.That(isValid, Is.EqualTo(all[i].isValid),
                $"CNPJ {i:N0} '{all[i].cnpj}' (original {all[i].validCnpj}) should be {(all[i].isValid ? "valid" : "invalid")}.");
        }

        Console.WriteLine($"GC Gen #2  : {GC.CollectionCount(2) - before2}");
        Console.WriteLine($"GC Gen #1  : {GC.CollectionCount(1) - before1}");
        Console.WriteLine($"GC Gen #0  : {GC.CollectionCount(0) - before0}");

        Console.WriteLine($"Validation total: {numToCreate:N0} in {sw.Elapsed:g}");
        Console.WriteLine($"Validation: {(numToCreate / sw.Elapsed.TotalSeconds):N0} v/s");
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
}
