using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elekto.BrazilianDocuments;

/// <summary>
/// Represents an always-valid Brazilian CPF (Cadastro de Pessoas Físicas).
/// </summary>
/// <remarks>
/// <para>
/// A CPF is a unique identifier for individuals in Brazil, consisting of 11 numeric digits:
/// 9 base digits and 2 check digits.
/// </para>
/// <para>
/// This implementation uses a 64-bit integer (long) for storage, providing optimal memory usage
/// and fast comparisons. Validation is performed using zero-allocation techniques.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Parse a CPF from string
/// var cpf = Cpf.Parse("123.456.789-09");
///
/// // Create a CPF with check digit calculation
/// var cpf = Cpf.NewCpf(123456789);
///
/// // Validate a CPF
/// bool isValid = Cpf.IsValid("123.456.789-09");
///
/// // Format output
/// Console.WriteLine(cpf.ToString("G")); // 123.456.789-09
/// Console.WriteLine(cpf.ToString("B")); // 12345678909
/// </code>
/// </example>
[JsonConverter(typeof(CpfJsonConverter))]
public readonly struct Cpf : IComparable<Cpf>, IComparable, IEquatable<Cpf>
{
    /// <summary>
    /// A valid but empty CPF (all zeros).
    /// </summary>
    public static readonly Cpf Empty = new(0);

    /// <summary>
    /// Maximum valid CPF value (99999999999 = 11 nines).
    /// </summary>
    private const long MaxValue = 99_999_999_999L;

    private readonly long _cpf;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cpf"/> struct from a string.
    /// </summary>
    /// <param name="cpf">The CPF string to parse.</param>
    /// <exception cref="BadCpfException">Thrown when <paramref name="cpf"/> is not a valid CPF.</exception>
    public Cpf(string cpf)
    {
        if (!TryConvertToNumber(cpf, out var number) || !IsValidNumber(number))
        {
            throw new BadCpfException(cpf);
        }

        _cpf = number;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Cpf"/> struct from a numeric value.
    /// </summary>
    /// <param name="cpf">The CPF as a numeric value.</param>
    /// <exception cref="BadCpfException">Thrown when <paramref name="cpf"/> is not a valid CPF.</exception>
    public Cpf(long cpf)
    {
        if (!IsValidNumber(cpf))
        {
            throw new BadCpfException(cpf.ToString());
        }
        _cpf = cpf;
    }

    #region IComparable Members

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A value indicating the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not a <see cref="Cpf"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;
        if (obj is not Cpf cpf)
        {
            throw new ArgumentException("Argument must be a Cpf", nameof(obj));
        }

        return CompareTo(cpf);
    }

    #endregion

    #region IComparable<Cpf> Members

    /// <summary>
    /// Compares the current object with another <see cref="Cpf"/>.
    /// </summary>
    /// <param name="other">A <see cref="Cpf"/> to compare with this object.</param>
    /// <returns>A value indicating the relative order of the objects being compared.</returns>
    public int CompareTo(Cpf other)
    {
        return _cpf.CompareTo(other._cpf);
    }

    #endregion

    #region IEquatable<Cpf> Members

    /// <summary>
    /// Indicates whether the current object is equal to another <see cref="Cpf"/>.
    /// </summary>
    /// <param name="other">A <see cref="Cpf"/> to compare with this object.</param>
    /// <returns><c>true</c> if the current object is equal to <paramref name="other"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(Cpf other)
    {
        return _cpf == other._cpf;
    }

    #endregion

    #region JsonConverter

    /// <summary>
    /// JSON converter for <see cref="Cpf"/>.
    /// </summary>
    public class CpfJsonConverter : JsonConverter<Cpf>
    {
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Cpf value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        /// <inheritdoc />
        public override Cpf Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string value for Cpf, but found {reader.TokenType}");
            }
            return Parse(reader.GetString()!);
        }
    }

    #endregion

    /// <summary>
    /// Determines whether the specified CPF string is valid.
    /// </summary>
    /// <param name="cpf">The CPF string to validate.</param>
    /// <returns><c>true</c> if the CPF is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(string? cpf)
    {
        if (!TryConvertToNumber(cpf, out var number))
        {
            return false;
        }
        return IsValidNumber(number);
    }

    /// <summary>
    /// Determines whether the specified CPF number is valid.
    /// </summary>
    /// <param name="cpf">The CPF as a numeric value.</param>
    /// <returns><c>true</c> if the CPF is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(long cpf)
    {
        return IsValidNumber(cpf);
    }

    /// <summary>
    /// Validates a CPF number with zero memory allocation.
    /// </summary>
    private static bool IsValidNumber(long cpf)
    {
        if (cpf < 0 || cpf > MaxValue)
            return false;

        // Extract check digits and initial digits
        var checkDigits = cpf % 100;
        var initial = cpf / 100;

        return checkDigits == CalculateDigits(initial);
    }

    /// <summary>
    /// Calculates check digits using zero-allocation technique.
    /// Single pass for both digits where possible.
    /// </summary>
    private static long CalculateDigits(long initialDigits)
    {
        // Weights for first digit: 10, 9, 8, 7, 6, 5, 4, 3, 2
        // Weights for second digit: 11, 10, 9, 8, 7, 6, 5, 4, 3, 2

        var sum1 = 0L;
        var sum2 = 0L;
        var copy = initialDigits;

        // Process 9 digits from right to left
        for (var i = 0; i < 9; i++)
        {
            var digit = copy % 10;
            copy /= 10;

            // Weight for sum1: 2, 3, 4, 5, 6, 7, 8, 9, 10 (i+2)
            sum1 += digit * (i + 2);
            // Weight for sum2: 3, 4, 5, 6, 7, 8, 9, 10, 11 (i+3)
            sum2 += digit * (i + 3);
        }

        // First check digit
        var dv1 = sum1 % 11;
        dv1 = dv1 < 2 ? 0 : 11 - dv1;

        // Add first check digit contribution to sum2 (weight 2)
        sum2 += dv1 * 2;

        // Second check digit
        var dv2 = sum2 % 11;
        dv2 = dv2 < 2 ? 0 : 11 - dv2;

        return dv1 * 10 + dv2;
    }

    /// <summary>
    /// Creates a new CPF from the initial 9 digits (without check digits).
    /// </summary>
    /// <param name="initialDigits">The first 9 digits as a string.</param>
    /// <returns>A valid <see cref="Cpf"/>.</returns>
    /// <exception cref="BadCpfException">Thrown when <paramref name="initialDigits"/> is invalid.</exception>
    public static Cpf NewCpf(string initialDigits)
    {
        if (!TryConvertToNumber(initialDigits, out var number))
        {
            throw new BadCpfException(initialDigits);
        }

        return NewCpf(number);
    }

    /// <summary>
    /// Creates a new CPF from the initial 9 digits (without check digits).
    /// </summary>
    /// <param name="initialDigits">The first 9 digits as a number.</param>
    /// <returns>A valid <see cref="Cpf"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialDigits"/> is out of range.</exception>
    public static Cpf NewCpf(long initialDigits)
    {
        if (initialDigits < 0)
            throw new ArgumentOutOfRangeException(nameof(initialDigits), initialDigits, "Must be greater than or equal to zero.");
        if (initialDigits > 999_999_999)
            throw new ArgumentOutOfRangeException(nameof(initialDigits), initialDigits, "Must be less than or equal to 999999999.");

        var check = CalculateDigits(initialDigits);
        return new Cpf(initialDigits * 100 + check);
    }

    /// <summary>
    /// Parses the specified input string into a <see cref="Cpf"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A valid <see cref="Cpf"/>.</returns>
    /// <exception cref="BadCpfException">Thrown when <paramref name="input"/> is not a valid CPF.</exception>
    public static Cpf Parse(string input)
    {
        if (!TryParse(input, out var cpf))
        {
            throw new BadCpfException(input);
        }
        return cpf;
    }

    /// <summary>
    /// Tries to parse the specified input string into a <see cref="Cpf"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="cpf">When this method returns, contains the parsed <see cref="Cpf"/> if successful.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? input, out Cpf cpf)
    {
        if (TryConvertToNumber(input, out var number) && IsValidNumber(number))
        {
            cpf = new Cpf(number);
            return true;
        }
        cpf = Empty;
        return false;
    }

    /// <summary>
    /// Tries to parse the specified input string into a <see cref="Cpf"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A <see cref="Cpf"/> if successful; otherwise, <c>null</c>.</returns>
    public static Cpf? TryParse(string? input)
    {
        if (TryParse(input, out var cpf)) return cpf;
        return null;
    }

    /// <summary>
    /// Parses the specified numeric value into a <see cref="Cpf"/>.
    /// </summary>
    /// <param name="input">The numeric value to parse.</param>
    /// <returns>A valid <see cref="Cpf"/>.</returns>
    /// <exception cref="BadCpfException">Thrown when <paramref name="input"/> is not a valid CPF.</exception>
    public static Cpf Parse(long input)
    {
        if (!TryParse(input, out var cpf))
        {
            throw new BadCpfException(input.ToString());
        }
        return cpf;
    }

    /// <summary>
    /// Tries to parse the specified numeric value into a <see cref="Cpf"/>.
    /// </summary>
    /// <param name="input">The numeric value to parse.</param>
    /// <param name="cpf">When this method returns, contains the parsed <see cref="Cpf"/> if successful.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
    public static bool TryParse(long input, out Cpf cpf)
    {
        if (!IsValidNumber(input))
        {
            cpf = Empty;
            return false;
        }
        cpf = new Cpf(input);
        return true;
    }

    /// <summary>
    /// Gets the check digits for the specified initial digits.
    /// </summary>
    /// <param name="initialDigits">The first 9 digits of the CPF.</param>
    /// <returns>The check digits (0-99).</returns>
    public static long GetDigits(long initialDigits)
    {
        return CalculateDigits(initialDigits);
    }

    /// <summary>
    /// Converts a CPF string to its numeric representation without allocation when possible.
    /// </summary>
    private static bool TryConvertToNumber(string? input, out long cpf)
    {
        cpf = 0;
        if (string.IsNullOrEmpty(input)) return false;

        // Security: Prevent DoS with excessively long inputs
        // Max valid CPF with formatting: "000.000.000-00" = 14 characters
        // Allow some extra room for unusual formatting, but cap at reasonable limit
        if (input!.Length > 20)
            return false;

        // Fast path: try parsing directly if no punctuation
        if (input.Length == 11 && !ContainsPunctuation(input))
        {
            return long.TryParse(input, out cpf);
        }

        // Slow path: remove punctuation
        // Count digits first to pre-check
        var digitCount = 0;
        foreach (var c in input)
        {
            if (c is >= '0' and <= '9')
                digitCount++;
            else if (c is not '.' and not '-')
                return false; // Invalid character
        }

        if (digitCount is < 1 or > 11)
            return false;

        // Build number without allocation when possible
        long result = 0;
        foreach (var c in input)
        {
            if (c is >= '0' and <= '9')
            {
                result = result * 10 + (c - '0');
            }
        }

        cpf = result;
        return true;
    }

    private static bool ContainsPunctuation(string s)
    {
        foreach (var c in s)
        {
            if (c is '.' or '-')
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code suitable for use in hashing algorithms and data structures.</returns>
    public override int GetHashCode()
    {
        return _cpf.GetHashCode();
    }

    /// <summary>
    /// Converts the CPF to a string representation.
    /// </summary>
    /// <param name="format">
    /// The format specifier:
    /// <list type="bullet">
    /// <item><c>"S"</c> (Short): Numeric value without leading zeros, e.g., "12345678909"</item>
    /// <item><c>"B"</c> (Bare): 11 digits with leading zeros, no punctuation, e.g., "01234567890"</item>
    /// <item><c>"G"</c> (General): Full format with punctuation, e.g., "012.345.678-90"</item>
    /// </list>
    /// </param>
    /// <returns>A formatted string representation of the CPF.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not recognized.</exception>
    [Pure]
    public string ToString(string format)
    {
        format = format.ToUpperInvariant();
        return format switch
        {
            "S" => _cpf.ToString(CultureInfo.InvariantCulture),
            "B" => _cpf.ToString("00000000000", CultureInfo.InvariantCulture),
            "G" => FormatGeneral(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Format must be S, B, or G")
        };
    }

    private string FormatGeneral()
    {
        var s = _cpf.ToString("00000000000", CultureInfo.InvariantCulture);
        return $"{s.Substring(0, 3)}.{s.Substring(3, 3)}.{s.Substring(6, 3)}-{s.Substring(9, 2)}";
    }

    /// <summary>
    /// Converts the CPF to its numeric (long) representation.
    /// </summary>
    /// <returns>The CPF as a long value.</returns>
    public long ToLong()
    {
        return _cpf;
    }

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Cpf cpf)
        {
            return _cpf == cpf._cpf;
        }
        return false;
    }

    /// <summary>
    /// Determines whether two <see cref="Cpf"/> instances are equal.
    /// </summary>
    public static bool operator ==(Cpf a, Cpf b)
    {
        return a._cpf == b._cpf;
    }

    /// <summary>
    /// Determines whether two <see cref="Cpf"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Cpf a, Cpf b)
    {
        return a._cpf != b._cpf;
    }

    /// <summary>
    /// Implicitly converts a string to a nullable <see cref="Cpf"/>.
    /// </summary>
    /// <param name="cpf">The CPF string.</param>
    /// <returns>A <see cref="Cpf"/> if valid; otherwise, <c>null</c> for null or empty input.</returns>
    /// <exception cref="BadCpfException">Thrown when <paramref name="cpf"/> is not null/empty but is invalid.</exception>
    public static implicit operator Cpf?(string? cpf)
    {
        if (string.IsNullOrEmpty(cpf)) return null;
        return new Cpf(cpf!);
    }

    /// <summary>
    /// Returns the string representation of this CPF in the general format (with punctuation).
    /// </summary>
    /// <returns>A string representation of the CPF, e.g., "123.456.789-09".</returns>
    public override string ToString()
    {
        return ToString("G");
    }
}
