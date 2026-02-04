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
        : base($"Invalid CPF: '{SanitizeForMessage(invalidCpf)}'.")
    {
        InvalidCpf = invalidCpf;
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
    /// Initializes a new instance of the <see cref="BadCpfException"/> class with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BadCpfException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
