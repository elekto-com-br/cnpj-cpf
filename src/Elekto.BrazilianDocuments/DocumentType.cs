namespace Elekto.BrazilianDocuments;

/// <summary>
/// Indicates the type of Brazilian tax document.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Unknown or not specified document type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// CPF (Cadastro de Pessoas Físicas) — the Brazilian individual taxpayer identification number (11 numeric digits).
    /// </summary>
    Cpf = 1,

    /// <summary>
    /// CNPJ (Cadastro Nacional da Pessoa Jurídica) — the Brazilian company registration number (14 alphanumeric characters).
    /// </summary>
    Cnpj = 2,
}
