// Copyright (c) 2013-2026 Elekto Produtos Financeiros. Licensed under the MIT License.

namespace Elekto.BrazilianDocuments;

/// <summary>
/// Helper for fields that accept either a <see cref="Cpf"/> or a <see cref="Cnpj"/>
/// — e.g. a single "document" column in a database or a generic form field.
/// </summary>
/// <remarks>
/// <para>
/// The optional <c>hint</c> parameter controls which type is <em>attempted first</em>
/// (not which type is enforced).  If the preferred type does not match, the other is tried.
/// </para>
/// <list type="table">
/// <listheader><term>hint</term><description>Behaviour</description></listheader>
/// <item>
///   <term><see cref="DocumentType.Cpf"/></term>
///   <description>Try as CPF first; if invalid, try as CNPJ.</description>
/// </item>
/// <item>
///   <term><see cref="DocumentType.Cnpj"/></term>
///   <description>Try as CNPJ first; if invalid, try as CPF.</description>
/// </item>
/// <item>
///   <term><see cref="DocumentType.Unknown"/> (default)</term>
///   <description>
///     Try both without preference.  Returns failure when the input is valid
///     for <em>both</em> types (ambiguous) — the caller must supply a hint
///     to resolve the ambiguity.
///   </description>
/// </item>
/// </list>
/// </remarks>
public static class BrazilianDocument
{
    /// <summary>
    /// Determines whether <paramref name="input"/> is a valid CPF or CNPJ and reports which type it is.
    /// </summary>
    /// <param name="input">The raw document string (supported punctuation is tolerated).</param>
    /// <param name="hint">Which type to attempt first; see class remarks.</param>
    /// <returns>
    /// <c>(true, <see cref="DocumentType.Cpf"/>)</c> or
    /// <c>(true, <see cref="DocumentType.Cnpj"/>)</c> on success.
    /// <c>(false, <see cref="DocumentType.Unknown"/>)</c> when the input is invalid or ambiguous.
    /// </returns>
    public static (bool IsValid, DocumentType Type) IsValid(
        string?      input,
        DocumentType hint = DocumentType.Unknown)
    {
        switch (hint)
        {
            case DocumentType.Cpf:
                if (Cpf.IsValid(input))  return (true, DocumentType.Cpf);
                if (Cnpj.IsValid(input)) return (true, DocumentType.Cnpj);
                return (false, DocumentType.Unknown);

            case DocumentType.Cnpj:
                if (Cnpj.IsValid(input)) return (true, DocumentType.Cnpj);
                if (Cpf.IsValid(input))  return (true, DocumentType.Cpf);
                return (false, DocumentType.Unknown);

            default:
                var asCpf  = Cpf.IsValid(input);
                var asCnpj = Cnpj.IsValid(input);

                if (asCpf && !asCnpj)  return (true, DocumentType.Cpf);
                if (asCnpj && !asCpf)  return (true, DocumentType.Cnpj);

                // Both valid (ambiguous) or neither valid (invalid)
                return (false, DocumentType.Unknown);
        }
    }

    /// <inheritdoc cref="IsValid(string?,DocumentType)"/>
    public static (bool IsValid, DocumentType Type) IsValid(
        ReadOnlySpan<char> input,
        DocumentType       hint = DocumentType.Unknown)
    {
        switch (hint)
        {
            case DocumentType.Cpf:
                if (Cpf.IsValid(input))  return (true, DocumentType.Cpf);
                if (Cnpj.IsValid(input)) return (true, DocumentType.Cnpj);
                return (false, DocumentType.Unknown);

            case DocumentType.Cnpj:
                if (Cnpj.IsValid(input)) return (true, DocumentType.Cnpj);
                if (Cpf.IsValid(input))  return (true, DocumentType.Cpf);
                return (false, DocumentType.Unknown);

            default:
                var asCpf  = Cpf.IsValid(input);
                var asCnpj = Cnpj.IsValid(input);

                if (asCpf && !asCnpj)  return (true, DocumentType.Cpf);
                if (asCnpj && !asCpf)  return (true, DocumentType.Cnpj);

                return (false, DocumentType.Unknown);
        }
    }

    /// <summary>
    /// Tries to parse <paramref name="input"/> as a CPF or CNPJ.
    /// </summary>
    /// <param name="input">The raw document string.</param>
    /// <param name="cpf">
    /// Set to the parsed value when the return is <see cref="DocumentType.Cpf"/>;
    /// otherwise <c>default</c>.
    /// </param>
    /// <param name="cnpj">
    /// Set to the parsed value when the return is <see cref="DocumentType.Cnpj"/>;
    /// otherwise <c>default</c>.
    /// </param>
    /// <param name="hint">Which type to attempt first; see class remarks.</param>
    /// <returns>
    /// <see cref="DocumentType.Cpf"/> or <see cref="DocumentType.Cnpj"/> on success;
    /// <see cref="DocumentType.Unknown"/> when the input is invalid or ambiguous.
    /// </returns>
    public static DocumentType TryParse(
        string?      input,
        out Cpf      cpf,
        out Cnpj     cnpj,
        DocumentType hint = DocumentType.Unknown)
    {
        cpf  = default;
        cnpj = default;

        switch (hint)
        {
            case DocumentType.Cpf:
                if (Cpf.TryParse(input, out cpf))   return DocumentType.Cpf;
                if (Cnpj.TryParse(input, out cnpj)) return DocumentType.Cnpj;
                return DocumentType.Unknown;

            case DocumentType.Cnpj:
                if (Cnpj.TryParse(input, out cnpj)) return DocumentType.Cnpj;
                if (Cpf.TryParse(input, out cpf))   return DocumentType.Cpf;
                return DocumentType.Unknown;

            default:
                var isCpf  = Cpf.TryParse(input, out cpf);
                var isCnpj = Cnpj.TryParse(input, out cnpj);

                if (isCpf && !isCnpj)  return DocumentType.Cpf;
                if (isCnpj && !isCpf)  return DocumentType.Cnpj;

                // Both matched (ambiguous) or neither matched (invalid)
                cpf  = default;
                cnpj = default;
                return DocumentType.Unknown;
        }
    }

    /// <inheritdoc cref="TryParse(string?,out Cpf,out Cnpj,DocumentType)"/>
    public static DocumentType TryParse(
        ReadOnlySpan<char> input,
        out Cpf            cpf,
        out Cnpj           cnpj,
        DocumentType       hint = DocumentType.Unknown)
    {
        cpf  = default;
        cnpj = default;

        switch (hint)
        {
            case DocumentType.Cpf:
                if (Cpf.TryParse(input, out cpf))   return DocumentType.Cpf;
                if (Cnpj.TryParse(input, out cnpj)) return DocumentType.Cnpj;
                return DocumentType.Unknown;

            case DocumentType.Cnpj:
                if (Cnpj.TryParse(input, out cnpj)) return DocumentType.Cnpj;
                if (Cpf.TryParse(input, out cpf))   return DocumentType.Cpf;
                return DocumentType.Unknown;

            default:
                var isCpf  = Cpf.TryParse(input, out cpf);
                var isCnpj = Cnpj.TryParse(input, out cnpj);

                if (isCpf && !isCnpj)  return DocumentType.Cpf;
                if (isCnpj && !isCpf)  return DocumentType.Cnpj;

                cpf  = default;
                cnpj = default;
                return DocumentType.Unknown;
        }
    }

    /// <summary>
    /// Parses <paramref name="input"/> as a CPF or CNPJ.
    /// </summary>
    /// <param name="input">The raw document string.</param>
    /// <param name="cpf">
    /// Set to the parsed value when the return is <see cref="DocumentType.Cpf"/>;
    /// otherwise <c>default</c>.
    /// </param>
    /// <param name="cnpj">
    /// Set to the parsed value when the return is <see cref="DocumentType.Cnpj"/>;
    /// otherwise <c>default</c>.
    /// </param>
    /// <param name="hint">Which type to attempt first; see class remarks.</param>
    /// <returns>
    /// <see cref="DocumentType.Cpf"/> or <see cref="DocumentType.Cnpj"/>;
    /// the corresponding out parameter is set.
    /// </returns>
    /// <exception cref="BadDocumentException">
    /// Thrown when the input is invalid or ambiguous.
    /// When ambiguous, pass a <paramref name="hint"/> to resolve.
    /// </exception>
    public static DocumentType Parse(
        string       input,
        out Cpf      cpf,
        out Cnpj     cnpj,
        DocumentType hint = DocumentType.Unknown)
    {
        var found = TryParse(input, out cpf, out cnpj, hint);
        if (found == DocumentType.Unknown)
            throw new BadDocumentException(input);
        return found;
    }
}
