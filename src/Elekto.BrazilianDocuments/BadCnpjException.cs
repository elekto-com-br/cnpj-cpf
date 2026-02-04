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
        : base($"Invalid CNPJ: '{SanitizeForMessage(invalidCnpj)}'.")
    {
        InvalidCnpj = invalidCnpj;
    }

    /// <summary>
    /// Sanitizes input for safe inclusion in error messages to prevent log injection.
    /// </summary>
    private static string SanitizeForMessage(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Limit length to prevent excessive log entries
        const int maxLength = 20;
        if (input!.Length > maxLength)
            input = input.Substring(0, maxLength) + "...";

        // Remove control characters and newlines to prevent log injection
        return new string(input.Where(c => !char.IsControl(c)).ToArray());
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
