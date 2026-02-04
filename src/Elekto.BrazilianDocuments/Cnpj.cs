using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Elekto.BrazilianDocuments;

/// <summary>
/// Represents an always-valid alphanumeric Brazilian CNPJ (Cadastro Nacional de Pessoa Jurídica).
/// </summary>
/// <remarks>
/// <para>
/// A CNPJ is a unique identifier for companies in Brazil, consisting of 14 alphanumeric characters:
/// 8 characters for the root (company identifier), 4 characters for the branch/order number,
/// and 2 check digits.
/// </para>
/// <para>
/// This implementation supports the new alphanumeric CNPJ format introduced by the Brazilian
/// Federal Revenue Service (Receita Federal), where characters A-Z can be used in addition to digits 0-9.
/// </para>
/// <para>
/// Credits for the zero-allocation validation algorithm:
/// <list type="bullet">
/// <item>Fernando Cerqueira - <see href="https://github.com/FRACerqueira/CnpjAlfaNumerico"/></item>
/// <item>Elemar Junior - <see href="https://elemarjr.com/arquivo/validando-cnpj-respeitando-o-garbage-collector/"/></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Parse a CNPJ from string
/// var cnpj = Cnpj.Parse("12.ABC.345/01DE-35");
///
/// // Create a CNPJ with check digit calculation
/// var cnpj = Cnpj.Create("ELEKTO", "0001");
///
/// // Validate a CNPJ
/// bool isValid = Cnpj.IsValid("00.ELE.KTO/0001-40");
///
/// // Format output
/// Console.WriteLine(cnpj.ToString("G")); // 00.ELE.KTO/0001-40
/// Console.WriteLine(cnpj.ToString("B")); // 00ELEKTO000140
/// </code>
/// </example>
[JsonConverter(typeof(CnpjJsonConverter))]
public readonly struct Cnpj : IComparable<Cnpj>, IComparable, IEquatable<Cnpj>
{
    /// <summary>
    /// A valid but empty CNPJ (all zeros).
    /// </summary>
    public static readonly Cnpj Empty = new("0/0000-00");

    /// <summary>
    /// In the clean representation (without punctuation), the CNPJ has 14 characters, e.g., 00ZHMRO3VI7K43
    /// </summary>
    private readonly string _cnpj;

    /// <summary>
    /// Initializes a new instance of the <see cref="Cnpj"/> struct.
    /// </summary>
    /// <param name="cnpj">The CNPJ string to parse.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="cnpj"/> is null or whitespace.</exception>
    /// <exception cref="BadCnpjException">Thrown when <paramref name="cnpj"/> is not a valid CNPJ.</exception>
    public Cnpj(string cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
        {
            throw new ArgumentNullException(nameof(cnpj), "CNPJ cannot be empty.");
        }

        // Sample: 20.913.792/0001-01 or L3.ZHM.RO3/VI7K-43
        cnpj = cnpj.Trim();
        if (cnpj.Length > 18)
        {
            throw new BadCnpjException(cnpj);
        }

        if (!CnpjHelper.Validate(cnpj))
        {
            throw new BadCnpjException(cnpj);
        }

        _cnpj = CleanAndNormalize(cnpj);
    }

    // Private constructor for internal use
    private Cnpj(string cnpj, bool skipValidation)
    {
        _cnpj = cnpj;
    }

    /// <summary>
    /// Gets the CNPJ value without punctuation (14 characters, uppercase, zero-padded).
    /// </summary>
    public string Value => _cnpj;

    /// <summary>
    /// Gets the root part of the CNPJ (first 8 characters).
    /// </summary>
    public string Root => _cnpj.Substring(0, 8);

    /// <summary>
    /// Gets the branch/order part of the CNPJ (characters 9-12).
    /// </summary>
    public string Branch => _cnpj.Substring(8, 4);

    /// <summary>
    /// Gets the check digits of the CNPJ (last 2 characters).
    /// </summary>
    public string CheckDigits => _cnpj.Substring(12, 2);

    private static string CleanAndNormalize(string cnpj)
    {
        // Remove any character that is not 0-9 or A-Z or a-z
        cnpj = cnpj.ToUpperInvariant();
        cnpj = new string(cnpj.Where(char.IsLetterOrDigit).ToArray());
        cnpj = cnpj.PadLeft(14, '0');
        return cnpj;
    }

    #region IComparable Members

    /// <summary>
    /// Compares the current instance with another object of the same type.
    /// </summary>
    /// <param name="obj">An object to compare with this instance.</param>
    /// <returns>A value indicating the relative order of the objects being compared.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="obj"/> is not a <see cref="Cnpj"/>.</exception>
    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;
        if (obj is not Cnpj cnpj)
        {
            throw new ArgumentException("Argument must be a Cnpj", nameof(obj));
        }

        return CompareTo(cnpj);
    }

    #endregion

    #region IComparable<Cnpj> Members

    /// <summary>
    /// Compares the current object with another <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="other">A <see cref="Cnpj"/> to compare with this object.</param>
    /// <returns>A value indicating the relative order of the objects being compared.</returns>
    public int CompareTo(Cnpj other)
    {
        return string.Compare(_cnpj, other._cnpj, StringComparison.Ordinal);
    }

    #endregion

    #region IEquatable<Cnpj> Members

    /// <summary>
    /// Indicates whether the current object is equal to another <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="other">A <see cref="Cnpj"/> to compare with this object.</param>
    /// <returns><c>true</c> if the current object is equal to <paramref name="other"/>; otherwise, <c>false</c>.</returns>
    public bool Equals(Cnpj other)
    {
        return CompareTo(other) == 0;
    }

    #endregion

    #region JsonConverter

    /// <summary>
    /// JSON converter for <see cref="Cnpj"/>.
    /// </summary>
    public class CnpjJsonConverter : JsonConverter<Cnpj>
    {
        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Cnpj value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }

        /// <inheritdoc />
        public override Cnpj Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected string value for Cnpj, but found {reader.TokenType}");
            }
            return Parse(reader.GetString()!);
        }
    }

    #endregion

    /// <summary>
    /// Determines whether the specified CNPJ string is valid.
    /// </summary>
    /// <param name="cnpj">The CNPJ string to validate.</param>
    /// <returns><c>true</c> if the CNPJ is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(string? cnpj)
    {
        return CnpjHelper.Validate(cnpj);
    }

    /// <summary>
    /// Parses the specified input string into a <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A valid <see cref="Cnpj"/>.</returns>
    /// <exception cref="BadCnpjException">Thrown when <paramref name="input"/> is not a valid CNPJ.</exception>
    public static Cnpj Parse(string input)
    {
        if (!TryParse(input, out var cnpj))
        {
            throw new BadCnpjException(input);
        }

        return cnpj;
    }

    /// <summary>
    /// Tries to parse the specified input string into a <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <param name="cnpj">When this method returns, contains the parsed <see cref="Cnpj"/> if successful.</param>
    /// <returns><c>true</c> if parsing was successful; otherwise, <c>false</c>.</returns>
    public static bool TryParse(string? input, out Cnpj cnpj)
    {
        if (!IsValid(input))
        {
            cnpj = Empty;
            return false;
        }

        cnpj = new Cnpj(input!);
        return true;
    }

    /// <summary>
    /// Tries to parse the specified input string into a <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="input">The input string to parse.</param>
    /// <returns>A <see cref="Cnpj"/> if successful; otherwise, <c>null</c>.</returns>
    public static Cnpj? TryParse(string? input)
    {
        if (TryParse(input, out var cnpj)) return cnpj;
        return null;
    }

    /// <summary>
    /// Gets the check digits for the specified initial digits.
    /// </summary>
    /// <param name="initialDigits">The first 12 characters of the CNPJ.</param>
    /// <returns>The check digits as a byte (dv1 * 10 + dv2).</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="initialDigits"/> is invalid.</exception>
    public static byte GetDigits(string initialDigits)
    {
        var cnpj = CnpjHelper.Create(initialDigits);
        return cnpj.Digits;
    }

    /// <summary>
    /// Creates a CNPJ from root and branch, calculating the check digits.
    /// </summary>
    /// <param name="root">The root (up to 8 characters).</param>
    /// <param name="branch">The branch/order (up to 4 characters).</param>
    /// <returns>A valid <see cref="Cnpj"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="root"/> or <paramref name="branch"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="root"/> or <paramref name="branch"/> is invalid.</exception>
    public static Cnpj Create(string root, string branch)
    {
        var (cnpj, _) = CnpjHelper.Create(root, branch);
        return new Cnpj(cnpj, skipValidation: true);
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>A hash code suitable for use in hashing algorithms and data structures.</returns>
    public override int GetHashCode()
    {
        return _cnpj.GetHashCode();
    }

    /// <summary>
    /// Converts the CNPJ to a string representation.
    /// </summary>
    /// <param name="format">
    /// The format specifier:
    /// <list type="bullet">
    /// <item><c>"S"</c> (Short): Without leading zeros, e.g., "9358105000191"</item>
    /// <item><c>"B"</c> (Bare): 14 characters with leading zeros, no punctuation, e.g., "09358105000191"</item>
    /// <item><c>"BS"</c> (Bare Small): Root only (first 8 characters), e.g., "09358105"</item>
    /// <item><c>"G"</c> (General): Full format with punctuation, e.g., "09.358.105/0001-91"</item>
    /// </list>
    /// </param>
    /// <returns>A formatted string representation of the CNPJ.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="format"/> is not recognized.</exception>
    [Pure]
    public string ToString(string format)
    {
        format = format.ToUpperInvariant();
        return format switch
        {
            "S" => _cnpj.TrimStart('0'),
            "B" => _cnpj,
            "BS" => _cnpj.Substring(0, 8),
            "G" => $"{_cnpj.Substring(0, 2)}.{_cnpj.Substring(2, 3)}.{_cnpj.Substring(5, 3)}/{_cnpj.Substring(8, 4)}-{_cnpj.Substring(12, 2)}",
            _ => throw new ArgumentOutOfRangeException(nameof(format), "Format must be S, B, BS, or G")
        };
    }

    /// <summary>
    /// Determines whether the specified object is equal to this instance.
    /// </summary>
    /// <param name="obj">The object to compare with this instance.</param>
    /// <returns><c>true</c> if the specified object is equal to this instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is Cnpj cnpj)
        {
            return CompareTo(cnpj) == 0;
        }

        return false;
    }

    /// <summary>
    /// Determines whether two <see cref="Cnpj"/> instances are equal.
    /// </summary>
    public static bool operator ==(Cnpj a, Cnpj b)
    {
        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two <see cref="Cnpj"/> instances are not equal.
    /// </summary>
    public static bool operator !=(Cnpj a, Cnpj b)
    {
        return !a.Equals(b);
    }

    /// <summary>
    /// Implicitly converts a string to a nullable <see cref="Cnpj"/>.
    /// </summary>
    /// <param name="cnpj">The CNPJ string.</param>
    /// <returns>A <see cref="Cnpj"/> if valid; otherwise, <c>null</c> for null or empty input.</returns>
    /// <exception cref="BadCnpjException">Thrown when <paramref name="cnpj"/> is not null/empty but is invalid.</exception>
    public static implicit operator Cnpj?(string? cnpj)
    {
        if (string.IsNullOrEmpty(cnpj)) return null;
        return new Cnpj(cnpj);
    }

    /// <summary>
    /// Returns the string representation of this CNPJ in the general format (with punctuation).
    /// </summary>
    /// <returns>A string representation of the CNPJ, e.g., "09.358.105/0001-91".</returns>
    public override string ToString()
    {
        return ToString("G");
    }
}
