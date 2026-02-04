using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

[TestFixture]
public class ExceptionsTests
{
    [Test]
    public void BadCpfException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        var ex = new BadCpfException();
        Assert.That(ex.Message, Is.EqualTo("Invalid CPF."));
        Assert.That(ex.InvalidCpf, Is.Null);
    }

    [Test]
    public void BadCpfException_MessageAndInnerException_ShouldSetProperties()
    {
        var inner = new Exception("Inner error");
        var ex = new BadCpfException("Custom message", inner);
        Assert.That(ex.Message, Is.EqualTo("Custom message"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
        Assert.That(ex.InvalidCpf, Is.Null);
    }

    [Test]
    public void BadCpfException_WithString_ShouldSanitizeMessage()
    {
        var ex = new BadCpfException("123\n456");
        Assert.That(ex.Message, Does.Contain("123456"));
        Assert.That(ex.Message, Does.Not.Contain("\n"));
        Assert.That(ex.InvalidCpf, Is.EqualTo("123\n456"));
    }

    [Test]
    public void BadCpfException_WithLongString_ShouldTruncateMessage()
    {
        var longString = new string('1', 50);
        var ex = new BadCpfException(longString);
        
        Assert.That(ex.Message, Does.EndWith("...'.")); // Ends with ellipse and quote and dot.
        Assert.That(ex.Message.Length, Is.LessThan(50));
        Assert.That(ex.InvalidCpf, Is.EqualTo(longString));
    }

    [Test]
    public void BadCpfException_WithNullOrEmptyOrWhitespace_ShouldUseEmptySanitizedValue()
    {
        var nullEx = new BadCpfException(null);
        var emptyEx = new BadCpfException(string.Empty);
        var whitespaceEx = new BadCpfException("   ");

        Assert.That(nullEx.Message, Is.EqualTo("Invalid CPF: ''."));
        Assert.That(emptyEx.Message, Is.EqualTo("Invalid CPF: ''."));
        Assert.That(whitespaceEx.Message, Is.EqualTo("Invalid CPF: ''."));
    }

    [Test]
    public void BadCnpjException_WithNullOrEmptyOrWhitespace_ShouldUseEmptySanitizedValue()
    {
        var nullEx = new BadCnpjException(null);
        var emptyEx = new BadCnpjException(string.Empty);
        var whitespaceEx = new BadCnpjException("   ");

        Assert.That(nullEx.Message, Is.EqualTo("Invalid CNPJ: ''."));
        Assert.That(emptyEx.Message, Is.EqualTo("Invalid CNPJ: ''."));
        Assert.That(whitespaceEx.Message, Is.EqualTo("Invalid CNPJ: ''."));
    }

    [Test]
    public void BadCnpjException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        var ex = new BadCnpjException();
        Assert.That(ex.Message, Is.EqualTo("Invalid CNPJ."));
        Assert.That(ex.InvalidCnpj, Is.Null);
    }

    [Test]
    public void BadCnpjException_MessageAndInnerException_ShouldSetProperties()
    {
        var inner = new Exception("Inner error");
        var ex = new BadCnpjException("Custom message", inner);
        Assert.That(ex.Message, Is.EqualTo("Custom message"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
        Assert.That(ex.InvalidCnpj, Is.Null);
    }

    [Test]
    public void BadCnpjException_WithString_ShouldSanitizeMessage()
    {
        var ex = new BadCnpjException("123\r456");
        Assert.That(ex.Message, Does.Contain("123456"));
        Assert.That(ex.Message, Does.Not.Contain("\r"));
        Assert.That(ex.InvalidCnpj, Is.EqualTo("123\r456"));
    }
}
