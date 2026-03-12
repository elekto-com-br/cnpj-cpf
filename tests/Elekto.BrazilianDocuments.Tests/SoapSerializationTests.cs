using NUnit.Framework;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

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

        Console.WriteLine(xml);

        var deserialized = Deserialize<Cpf>(xml);
        Assert.That(deserialized, Is.EqualTo(cpf));
    }

    [Test]
    public void Cnpj_Can_RoundTrip()
    {
        var cnpj = Cnpj.Create("ELEKTO", "0042");
        var xml = Serialize(cnpj);

        Console.WriteLine(xml);

        var deserialized = Deserialize<Cnpj>(xml);
        Assert.That(deserialized, Is.EqualTo(cnpj));
    }

    [Test]
    public void Cpf_Malicious_Deserialization_Should_Throw()
    {
        const long invalidCpfValue = 12345678900L;

        // Ensure it is invalid first
        Assert.That(Cpf.IsValid(invalidCpfValue), Is.False, "Test setup failed: expected invalid CPF");

        // Use custom XML — since schema now maps to xs:long, the root element is <long>
        var xml = $"""<long xmlns="">{invalidCpfValue}</long>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cpf>(xml));

        // Let's force punctuation (not a valid long)
        xml = """<long xmlns="">123.456.789-09</long>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cpf>(xml));

        // Empty
        xml = """<long xmlns=""></long>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cpf>(xml));

        // Empty self-closing
        xml = """<long xmlns="" />""";
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

        var xml = $"""<string xmlns="">{invalidCnpjValue}</string>""";

        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));
    }

    [Test]
    public void Cnpj_Malicious_Deserialization_InvalidFormat_Should_Throw()
    {
        // Malformed string (too short)
        var xml = """<string xmlns="">123</string>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Let's force punctuation
        xml = """<string xmlns="">00.000.000/0000-00</string>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Let's force exactly 14 spaces
        xml = """<string xmlns="">              </string>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Null
        xml = """<string xmlns=""></string>""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

        // Empty self-closing
        xml = """<string xmlns="" />""";
        Assert.Throws<BadDocumentException>(() => Deserialize<Cnpj>(xml));

    }

    [Test]
    public void XmlSerialization_ShouldPreserveCnpjValue()
    {
        var original = Cnpj.Create("ELEKTO", "0042");
        var serializer = new XmlSerializer(typeof(Cnpj));

        using var writer = new StringWriter();
        Assert.That(() => { serializer.Serialize(writer, original); },
            Throws.Nothing,
            "XML Serialization should not throw.");
        var xml = writer.ToString();

        Console.WriteLine(xml);

        Cnpj deserialized;
        using (var reader = new StringReader(xml))
        {
            var o = serializer.Deserialize(reader);
            Assert.That(o, Is.Not.Null, "XML deserialization should return a non-null object.");
            deserialized = (Cnpj)o;
        }

        Assert.That(deserialized,
            Is.EqualTo(original),
            "The deserialized CNPJ should be equal to the original.");
    }

    [Test]
    public void XmlSerialization_ShouldPreserveCpfValue()
    {
        var original = new Cpf("123.456.789-09");
        var serializer = new XmlSerializer(typeof(Cpf));

        using var writer = new StringWriter();
        Assert.That(() => { serializer.Serialize(writer, original); },
            Throws.Nothing,
            "XML Serialization should not throw.");
        var xml = writer.ToString();

        Console.WriteLine(xml);

        Cpf deserialized;
        using (var reader = new StringReader(xml))
        {
            var o = serializer.Deserialize(reader);
            Assert.That(o, Is.Not.Null, "XML deserialization should return a non-null object.");
            deserialized = (Cpf)o;
        }

        Assert.That(deserialized,
            Is.EqualTo(original),
            "The deserialized CPF should be equal to the original.");
    }

    [Test]
    public void Cpf_GetSchema_Returns_Null()
    {
        IXmlSerializable serializable = new Cpf("123.456.789-09");
        Assert.That(serializable.GetSchema(), Is.Null);
    }

    [Test]
    public void Cnpj_GetSchema_Returns_Null()
    {
        IXmlSerializable serializable = Cnpj.Create("ELEKTO", "0042");
        Assert.That(serializable.GetSchema(), Is.Null);
    }
}
