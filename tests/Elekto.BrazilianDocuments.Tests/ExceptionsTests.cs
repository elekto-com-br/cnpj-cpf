using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

[TestFixture]
public class ExceptionsTests
{
    [Test]
    public void BadDocumentException_DefaultConstructor_ShouldHaveDefaultMessage()
    {
        var ex = new BadDocumentException();
        Assert.That(ex.Message, Is.EqualTo("Invalid document."));
        Assert.That(ex.InvalidDocument, Is.Null);
        Assert.That(ex.SourceType, Is.EqualTo(DocumentType.Unknown));
    }

    [Test]
    public void BadDocumentException_MessageAndInnerException_ShouldSetProperties()
    {
        var inner = new Exception("Inner error");
        var ex = new BadDocumentException("Custom message", inner, DocumentType.Cpf);
        Assert.That(ex.Message, Is.EqualTo("Custom message"));
        Assert.That(ex.InnerException, Is.SameAs(inner));
        Assert.That(ex.InvalidDocument, Is.Null);
        Assert.That(ex.SourceType, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void BadDocumentException_WithCpfString_ShouldSanitizeMessage()
    {
        var ex = new BadDocumentException("123\n456", DocumentType.Cpf);
        Assert.That(ex.Message, Does.Contain("123456"));
        Assert.That(ex.Message, Does.Not.Contain("\n"));
        Assert.That(ex.Message, Does.Contain("Invalid CPF:"));
        Assert.That(ex.InvalidDocument, Is.EqualTo("123\n456"));
        Assert.That(ex.SourceType, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void BadDocumentException_WithLongString_ShouldTruncateMessage()
    {
        var longString = new string('1', 50);
        var ex = new BadDocumentException(longString, DocumentType.Cpf);
        
        Assert.That(ex.Message, Does.EndWith("...'.")); // Ends with ellipse and quote and dot.
        Assert.That(ex.Message.Length, Is.LessThan(50));
        Assert.That(ex.InvalidDocument, Is.EqualTo(longString));
        Assert.That(ex.SourceType, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void BadDocumentException_WithCpfNullOrEmptyOrWhitespace_ShouldUseEmptySanitizedValue()
    {
        var nullEx = new BadDocumentException(null, DocumentType.Cpf);
        var emptyEx = new BadDocumentException(string.Empty, DocumentType.Cpf);
        var whitespaceEx = new BadDocumentException("   ", DocumentType.Cpf);

        Assert.That(nullEx.Message, Is.EqualTo("Invalid CPF: ''."));
        Assert.That(emptyEx.Message, Is.EqualTo("Invalid CPF: ''."));
        Assert.That(whitespaceEx.Message, Is.EqualTo("Invalid CPF: ''."));
        Assert.That(nullEx.SourceType, Is.EqualTo(DocumentType.Cpf));
        Assert.That(emptyEx.SourceType, Is.EqualTo(DocumentType.Cpf));
        Assert.That(whitespaceEx.SourceType, Is.EqualTo(DocumentType.Cpf));
    }

    [Test]
    public void BadDocumentException_WithCnpjNullOrEmptyOrWhitespace_ShouldUseEmptySanitizedValue()
    {
        var nullEx = new BadDocumentException(null, DocumentType.Cnpj);
        var emptyEx = new BadDocumentException(string.Empty, DocumentType.Cnpj);
        var whitespaceEx = new BadDocumentException("   ", DocumentType.Cnpj);

        Assert.That(nullEx.Message, Is.EqualTo("Invalid CNPJ: ''."));
        Assert.That(emptyEx.Message, Is.EqualTo("Invalid CNPJ: ''."));
        Assert.That(whitespaceEx.Message, Is.EqualTo("Invalid CNPJ: ''."));
        Assert.That(nullEx.SourceType, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(emptyEx.SourceType, Is.EqualTo(DocumentType.Cnpj));
        Assert.That(whitespaceEx.SourceType, Is.EqualTo(DocumentType.Cnpj));
    }

    [Test]
    public void BadDocumentException_WithUnknownSource_ShouldUseDocumentLabel()
    {
        var ex = new BadDocumentException("ABC", DocumentType.Unknown);
        Assert.That(ex.Message, Is.EqualTo("Invalid document: 'ABC'."));
        Assert.That(ex.InvalidDocument, Is.EqualTo("ABC"));
        Assert.That(ex.SourceType, Is.EqualTo(DocumentType.Unknown));
    }

    [Test]
    public void BadDocumentException_WithCnpjString_ShouldSanitizeMessage()
    {
        var ex = new BadDocumentException("123\r456", DocumentType.Cnpj);
        Assert.That(ex.Message, Does.Contain("123456"));
        Assert.That(ex.Message, Does.Not.Contain("\r"));
        Assert.That(ex.Message, Does.Contain("Invalid CNPJ:"));
        Assert.That(ex.InvalidDocument, Is.EqualTo("123\r456"));
        Assert.That(ex.SourceType, Is.EqualTo(DocumentType.Cnpj));
    }
}
