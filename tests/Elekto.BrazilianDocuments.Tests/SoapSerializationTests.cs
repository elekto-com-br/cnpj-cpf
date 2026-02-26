using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Elekto.BrazilianDocuments.Tests;

public class SoapSerializationTests
{
    private static string Serialize<T>(T obj)
    {
        var serializer = new DataContractSerializer(typeof(T));
        using var stream = new MemoryStream();
        using var writer = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8);
        serializer.WriteObject(writer, obj);
        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static T Deserialize<T>(string xml)
    {
        var serializer = new DataContractSerializer(typeof(T));
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(xml));
        return (T)serializer.ReadObject(stream)!;
    }

    [Test]
    public void Cpf_Can_RoundTrip()
    {
        var cpf = Cpf.Create(123456789);
        var xml = Serialize(cpf);

        Assert.That(xml, Does.Contain("xmlns=\"https://elekto.com.br/types\""));

        var deserialized = Deserialize<Cpf>(xml);
        Assert.That(deserialized, Is.EqualTo(cpf));
    }

    [Test]
    public void Cnpj_Can_RoundTrip()
    {
        var cnpj = Cnpj.Create("ELEKTO", "0042");
        var xml = Serialize(cnpj);

        Assert.That(xml, Does.Contain("xmlns=\"https://elekto.com.br/types\""));

        var deserialized = Deserialize<Cnpj>(xml);
        Assert.That(deserialized, Is.EqualTo(cnpj));
    }

    [Test]
    public void Cpf_Malicious_Deserialization_Should_Throw()
    {
        const long invalidCpfValue = 12345678900L; 
        
        // Ensure it is invalid first
        Assert.That(Cpf.IsValid(invalidCpfValue), Is.False, "Test setup failed: expected invalid CPF");

        // Use custom XML
        var xml = $"""<Cpf xmlns="https://elekto.com.br/types"><Value>{invalidCpfValue}</Value></Cpf>""";
        
        Assert.Throws<BadDocumentException>(() => Deserialize<Cpf>(xml));
    }

    [Test]
    public void Cnpj_Malicious_Deserialization_InvalidChecksum_Should_Throw()
    {
        // 00.000.000/0000-01 (invalid checksum)
        // Normalized: 00000000000001
        
        var invalidCnpjValue = "00000000000001";

        // Ensure invalid
        Assert.That(Cnpj.IsValid(invalidCnpjValue), Is.False);

        var xml = $"""<Cnpj xmlns="https://elekto.com.br/types"><Value>{invalidCnpjValue}</Value></Cnpj>""";
        
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));
    }

    [Test]
    public void Cnpj_Malicious_Deserialization_InvalidFormat_Should_Throw()
    {
        // Malformed string (too short)
        var xml = """<Cnpj xmlns="https://elekto.com.br/types"><Value>123</Value></Cnpj>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Let's force punctuation
        xml = """<Cnpj xmlns="https://elekto.com.br/types"><Value>00.000.000/0000-00</Value></Cnpj>"""; // 18 chars, invalid length for backing field
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Let's force exactley 14 spaces
        xml = """<Cnpj xmlns="https://elekto.com.br/types"><Value>              </Value></Cnpj>"""; // Full Empty
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Null
        xml = """<Cnpj xmlns="https://elekto.com.br/types"><Value></Value></Cnpj>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

    }
}
