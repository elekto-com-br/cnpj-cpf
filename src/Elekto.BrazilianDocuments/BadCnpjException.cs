namespace Elekto.BrazilianDocuments;

/// <summary>
/// Exception thrown when an invalid CNPJ is provided.
/// </summary>
public class BadCnpjException : ArgumentException
{
    /// <summary>
    /// Gets the invalid CNPJ value that caused the exception.
    /// </summary>
    public string? InvalidCnpj { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadCnpjException"/> class.
    /// </summary>
    public BadCnpjException()
        : base("Invalid CNPJ.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadCnpjException"/> class with the invalid CNPJ value.
    /// </summary>
    /// <param name="invalidCnpj">The invalid CNPJ value.</param>
    public BadCnpjException(string? invalidCnpj)
        : base($"Invalid CNPJ: '{InputSanitizer.SanitizeForMessage(invalidCnpj)}'.")
    {
        InvalidCnpj = invalidCnpj;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadCnpjException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BadCnpjException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
