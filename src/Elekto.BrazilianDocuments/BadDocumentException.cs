using System.Text;

namespace Elekto.BrazilianDocuments;

/// <summary>
/// Exception thrown when an invalid document value is provided.
/// </summary>
public class BadDocumentException : ArgumentException
{
    /// <summary>
    /// Gets the invalid document value that caused the exception.
    /// </summary>
    public string? InvalidDocument { get; }

    /// <summary>
    /// Gets the type of document that caused the exception.
    /// </summary>
    public DocumentType SourceType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadDocumentException"/> class.
    /// </summary>
    public BadDocumentException()
        : base("Invalid document.")
    {
        SourceType = DocumentType.Unknown;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadDocumentException"/> class with the invalid value and source.
    /// </summary>
    /// <param name="invalidDocument">The invalid document value.</param>
    /// <param name="sourceType">The document source (CPF, CNPJ, or unknown).</param>
    public BadDocumentException(string? invalidDocument, DocumentType sourceType = DocumentType.Unknown)
        : base($"Invalid {GetDocumentName(sourceType)}: '{SanitizeForMessage(invalidDocument)}'.")
    {
        InvalidDocument = invalidDocument;
        SourceType = sourceType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadDocumentException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="sourceType">The document source (CPF, CNPJ, or unknown).</param>
    public BadDocumentException(string message, Exception innerException, DocumentType sourceType = DocumentType.Unknown)
        : base(message, innerException)
    {
        SourceType = sourceType;
    }

    private static string GetDocumentName(DocumentType sourceType)
    {
        return sourceType switch
        {
            DocumentType.Cpf => "CPF",
            DocumentType.Cnpj => "CNPJ",
            _ => "document",
        };
    }

    private static string SanitizeForMessage(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        const int maxLength = 20;
        var length = Math.Min(input!.Length, maxLength);
        var needsTruncation = input.Length > maxLength;
        var sb = new StringBuilder(length + (needsTruncation ? 3 : 0));

        for (var i = 0; i < length; i++)
        {
            var c = input[i];
            if (!char.IsControl(c))
            {
                sb.Append(c);
            }
        }

        if (needsTruncation)
        {
            sb.Append("...");
        }

        return sb.ToString();
    }
}