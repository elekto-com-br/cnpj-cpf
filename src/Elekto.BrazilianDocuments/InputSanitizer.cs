using System.Text;

namespace Elekto.BrazilianDocuments;

/// <summary>
/// Utility class for sanitizing user input to prevent security issues.
/// </summary>
internal static class InputSanitizer
{
    /// <summary>
    /// Sanitizes input for safe inclusion in error messages to prevent log injection.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>A sanitized string safe for logging.</returns>
    public static string SanitizeForMessage(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Limit length to prevent excessive log entries
        const int maxLength = 20;
        var length = Math.Min(input!.Length, maxLength);
        var needsTruncation = input.Length > maxLength;

        // Use StringBuilder for better performance - no LINQ allocations
        var sb = new StringBuilder(length + (needsTruncation ? 3 : 0));
        
        for (var i = 0; i < length; i++)
        {
            var c = input[i];
            // Skip control characters to prevent log injection
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
