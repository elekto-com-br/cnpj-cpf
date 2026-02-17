namespace Elekto.BrazilianDocuments;

/// <summary>
/// Indicates which document type caused a <see cref="BadDocumentException"/>.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Unknown or not specified.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// CPF document.
    /// </summary>
    Cpf = 1,

    /// <summary>
    /// CNPJ document.
    /// </summary>
    Cnpj = 2,
}
