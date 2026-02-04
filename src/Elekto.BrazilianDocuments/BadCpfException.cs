namespace Elekto.BrazilianDocuments;

/// <summary>
/// Exception thrown when an invalid CPF is provided.
/// </summary>
public class BadCpfException : ArgumentException
{
    /// <summary>
    /// Gets the invalid CPF value that caused the exception.
    /// </summary>
    public string? InvalidCpf { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadCpfException"/> class.
    /// </summary>
    public BadCpfException()
        : base("Invalid CPF.")
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadCpfException"/> class with the invalid CPF value.
    /// </summary>
    /// <param name="invalidCpf">The invalid CPF value.</param>
    public BadCpfException(string? invalidCpf)
        : base($"Invalid CPF: '{InputSanitizer.SanitizeForMessage(invalidCpf)}'.")
    {
        InvalidCpf = invalidCpf;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadCpfException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BadCpfException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
