using System.Text;

namespace Elekto.BrazilianDocuments;

/// <summary>
/// Utility class for alphanumeric CNPJ validation and creation.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses zero memory allocation for validation and performs
/// check digit calculation in a single loop.
/// </para>
/// <para>
/// Credits:
/// <list type="bullet">
/// <item>Fernando Cerqueira - <see href="https://github.com/FRACerqueira/CnpjAlfaNumerico"/></item>
/// <item>Elemar Junior - <see href="https://elemarjr.com/arquivo/validando-cnpj-respeitando-o-garbage-collector/"/></item>
/// </list>
/// Modified to handle leading zero omission and lowercase letters.
/// </para>
/// </remarks>
public static class CnpjHelper
{
    // Multipliers for check digit calculation
    private static readonly int[] Multiplier1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] Multiplier2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    /// <summary>
    /// Validates an alphanumeric CNPJ.
    /// </summary>
    /// <param name="cnpj">
    /// The string representing the alphanumeric CNPJ, for example, 00.ELE.KTO/0001-40 or ELEKTO000140.
    /// Leading zeros in the root can be omitted.
    /// </param>
    /// <returns><c>true</c> if the CNPJ is valid; otherwise, <c>false</c>.</returns>
    public static bool Validate(string? cnpj)
    {
        // Empty, too long or too short ("1/0001-36" is the shortest valid CNPJ)
        if (string.IsNullOrWhiteSpace(cnpj) || cnpj!.Length > 18 || cnpj.Length < 7)
            return false;

        // Count valid characters
        var validChars = 0;
        foreach (var c in cnpj)
        {
            if (IsValidInput(c))
            {
                validChars++;
            }
        }

        // Excluding punctuation, is it valid?
        if (validChars is < 7 or > 14)
        {
            return false;
        }

        var position = 0;
        var totalDigit1 = 0;
        var totalDigit2 = 0;

        if (validChars < 14)
        {
            // Leading zeros were omitted, so the first valid character corresponds to a position further ahead...
            position = 14 - validChars;
        }

        // Iterate through the string without extra memory allocation (except internal iteration)
        foreach (var c in cnpj)
        {
            // Consider only valid characters
            if (!IsValidInput(c)) continue;

            // In our validation, we convert the character to a digit using (c - '0').
            // For letters, this value does not correspond to the Base36 value, as A = 17, B = 18, etc...
            // but it preserves the original validation logic.
            var digit = c - '0';
            if (digit > 42)
            {
                // Adjustment to support lowercase letters
                digit -= 32;
            }

            switch (position)
            {
                case < 12:
                    totalDigit1 += digit * Multiplier1[position];
                    totalDigit2 += digit * Multiplier2[position];
                    break;
                case 12:
                {
                    var dv1 = totalDigit1 % 11;
                    dv1 = dv1 < 2 ? 0 : 11 - dv1;
                    if (digit != dv1)
                        return false;
                    totalDigit2 += dv1 * Multiplier2[12];
                    break;
                }
                case 13:
                {
                    var dv2 = totalDigit2 % 11;
                    dv2 = dv2 < 2 ? 0 : 11 - dv2;
                    if (digit != dv2)
                        return false;
                    break;
                }
            }

            position++;

            // In case there's weird punctuation at the end...
            if (position == 14)
                break;
        }

        return position == 14;
    }

    /// <summary>
    /// Creates an alphanumeric CNPJ from root and order, calculating the check digits.
    /// </summary>
    /// <param name="rootAndOrder">The first 12 characters of the CNPJ.</param>
    /// <returns>A tuple with the complete CNPJ (without punctuation) and the check digits.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootAndOrder"/> is null or whitespace.</exception>
    public static (string Cnpj, byte Digits) Create(string rootAndOrder)
    {
        if (string.IsNullOrWhiteSpace(rootAndOrder))
            throw new ArgumentNullException(nameof(rootAndOrder));

        rootAndOrder = rootAndOrder.ToUpperInvariant();
        rootAndOrder = new string(rootAndOrder.Where(char.IsLetterOrDigit).Take(12).ToArray());
        rootAndOrder = rootAndOrder.PadLeft(12, '0');

        return Create(rootAndOrder.Substring(0, 8), rootAndOrder.Substring(8, 4));
    }

    /// <summary>
    /// Creates an alphanumeric CNPJ from root and order, calculating the check digits.
    /// </summary>
    /// <param name="root">The CNPJ root (up to 8 characters, only [0-9A-Za-z]); if shorter, padded with leading zeros.</param>
    /// <param name="order">The CNPJ order/branch (up to 4 characters, only [0-9A-Za-z]); if shorter, padded with leading zeros.</param>
    /// <returns>
    /// A tuple with:
    /// <list type="bullet">
    /// <item><c>Cnpj</c>: Complete 14-character string (12 input characters + 2 check digits)</item>
    /// <item><c>Digits</c>: The check digits packed as a byte (dv1 * 10 + dv2)</item>
    /// </list>
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="root"/> or <paramref name="order"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="root"/> or <paramref name="order"/> has invalid length or characters.</exception>
    public static (string Cnpj, byte Digits) Create(string root, string order)
    {
        if (string.IsNullOrWhiteSpace(root))
            throw new ArgumentNullException(nameof(root));
        if (string.IsNullOrWhiteSpace(order))
            throw new ArgumentNullException(nameof(order));
        if (root.Length > 8)
            throw new ArgumentException("Root must have at most 8 characters.", nameof(root));
        if (order.Length > 4)
            throw new ArgumentException("Order must have at most 4 characters.", nameof(order));

        // Validate characters (only digits and letters)
        foreach (var c in root)
        {
            if (!IsValidInput(c))
                throw new ArgumentException("Root must contain only digits and letters.", nameof(root));
        }

        foreach (var c in order)
        {
            if (!IsValidInput(c))
                throw new ArgumentException("Order must contain only digits and letters.", nameof(order));
        }

        // Pad with leading zeros and convert to uppercase
        root = NormalizeInput(root, 8);
        order = NormalizeInput(order, 4);

        // Concatenate the 12 input characters (8 from root + 4 from order)
        var baseCnpj = root + order;

        var total1 = 0;
        var total2 = 0;
        // Single loop to calculate totals using multipliers for the first 12 positions
        for (var pos = 0; pos < 12; pos++)
        {
            // Simple conversion: (c - '0'), since valid characters are [0-9A-Z] and normalization is done
            var digit = baseCnpj[pos] - '0';
            total1 += digit * Multiplier1[pos];
            total2 += digit * Multiplier2[pos];
        }

        var dv1 = total1 % 11;
        dv1 = dv1 < 2 ? 0 : 11 - dv1;
        total2 += dv1 * Multiplier2[12];
        var dv2 = total2 % 11;
        dv2 = dv2 < 2 ? 0 : 11 - dv2;

        // Build the complete CNPJ: 12 characters + dv1 + dv2
        var sb = new StringBuilder(14);
        sb.Append(baseCnpj);
        sb.Append(dv1);
        sb.Append(dv2);
        var cnpjComplete = sb.ToString();

        // Pack check digits into a byte
        var packedDigits = (byte)(dv1 * 10 + dv2);
        return (cnpjComplete, packedDigits);
    }

    private static string NormalizeInput(string input, int length)
    {
        // If already correct
        if (input.Length == length && !ContainsLowerCase(input))
            return input;

        var result = new char[length];

        // Pad with leading zeros
        var padCount = length - input.Length;
        for (var i = 0; i < padCount; i++)
            result[i] = '0';

        // Copy and convert to uppercase
        for (var i = 0; i < input.Length; i++)
        {
            var c = input[i];
            if (c is >= 'a' and <= 'z')
                c = (char)(c - 32); // Faster than ToUpper()

            result[padCount + i] = c;
        }

        return new string(result);
    }

    private static bool ContainsLowerCase(string s)
    {
        foreach (var t in s)
        {
            if (t is >= 'a' and <= 'z')
                return true;
        }

        return false;
    }

    private static bool IsValidInput(char c)
    {
        return c switch
        {
            // Most common
            >= '0' and <= '9' => true,

            // Shortcut for common punctuation
            '.' or '/' or '-' => false,

            _ => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z'
        };
    }
}
