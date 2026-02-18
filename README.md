# Elekto.BrazilianDocuments

[![NuGet](https://img.shields.io/nuget/v/Elekto.BrazilianDocuments.svg)](https://www.nuget.org/packages/Elekto.BrazilianDocuments)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

High-performance, zero-allocation validation library for Brazilian documents (CNPJ and CPF).

## Features

- **Alphanumeric CNPJ Support**: Full support for the new alphanumeric CNPJ format introduced by the Brazilian Federal Revenue Service (Receita Federal)
- **Zero-Allocation Validation**: Validation algorithms designed to minimize memory allocation and GC pressure
- **Always-Valid Types**: `Cnpj` and `Cpf` structs that guarantee validity at construction time
- **Multiple Formats**: Parse and format documents in various representations
- **JSON Serialization**: Built-in `System.Text.Json` support with custom converters
- **Multi-Target**: Supports .NET 8, .NET 9 .NET 10, and .NET Standard 2.0

## Background: CPF and CNPJ

If you're not familiar with Brazilian tax identifiers, here's a brief overview.

### CPF (Cadastro de Pessoas Físicas)

The **CPF** is Brazil's individual taxpayer identification number, issued by the [Receita Federal](https://www.gov.br/receitafederal/pt-br/assuntos/meu-cpf) (Federal Revenue Service). It is similar to the U.S. Social Security Number (SSN), India's PAN, or the EU's Tax Identification Number (TIN). Every individual in Brazil — citizen or resident — must have a CPF for banking, employment, property transactions, and many other activities.

A CPF has **11 numeric digits** in the format `NNN.NNN.NNN-DD`:
- The first **9 digits** identify the taxpayer
- The last **2 digits** (`DD`) are **check digits**, calculated using a weighted modulo-11 algorithm

### CNPJ (Cadastro Nacional da Pessoa Jurídica)

The **CNPJ** is Brazil's company registration number, also issued by the [Receita Federal](https://www.gov.br/receitafederal/pt-br/assuntos/orientacao-tributaria/cadastros/cnpj). It is similar to the U.S. EIN (Employer Identification Number) or the EU's VAT number. Every legal entity operating in Brazil must have a CNPJ.

A CNPJ has **14 characters** in the format `RR.RRR.RRR/BBBB-DD`:
- The first **8 characters** (`R`) are the **root**, identifying the company
- The next **4 characters** (`B`) are the **branch/order number** (0001 = headquarters)
- The last **2 digits** (`DD`) are **check digits**, calculated using a weighted modulo-11 algorithm

**Starting in July 2026**, the Receita Federal will issue [alphanumeric CNPJs](https://www.gov.br/receitafederal/pt-br/acesso-a-informacao/acoes-e-programas/programas-e-atividades/cnpj-alfanumerico) (letters A–Z in addition to digits 0–9), as defined by [Instrução Normativa RFB nº 2.229/2024](https://www.in.gov.br/en/web/dou/-/instrucao-normativa-rfb-n-2.229-de-18-de-outubro-de-2024-591062981). This library fully supports this new format.

### Check Digit Algorithm

Both CPF and CNPJ use a **weighted modulo-11** algorithm for check digit validation:

1. Each character in the base number is assigned a positional weight
2. Each character value is multiplied by its weight (for alphanumeric CNPJ, A=10, B=11, ..., Z=35)
3. The products are summed
4. The remainder of dividing the sum by 11 determines the check digit:
   - If the remainder is less than 2, the check digit is **0**
   - Otherwise, the check digit is **11 − remainder**
5. The second check digit is calculated the same way, but including the first check digit in the input

This is a well-known error-detection scheme that catches most single-digit errors and all transposition errors.

## Installation

```bash
dotnet add package Elekto.BrazilianDocuments
```

## Quick Start

### CNPJ (Company Registration Number)

```csharp
using Elekto.BrazilianDocuments;

// Parse from string (various formats accepted)
var cnpj = Cnpj.Parse("12.ABC.345/01DE-35");

// Validate without throwing
if (Cnpj.IsValid("00.ELE.KTO/0001-40"))
{
    Console.WriteLine("Valid!");
}

// Safe parsing
if (Cnpj.TryParse("ELEKTO000140", out var parsed))
{
    Console.WriteLine(parsed.ToString("G")); // 00.ELE.KTO/0001-40
}

// Create with automatic check digit calculation
var newCnpj = Cnpj.Create("ELEKTO", "0001");
Console.WriteLine(newCnpj.ToString("B")); // 00ELEKTO000140

// Create from 12-character base (root + branch)
var cnpjFromBase = Cnpj.Create("ELEKTO0001");

// Access parts
Console.WriteLine(cnpj.Root);        // First 8 characters
Console.WriteLine(cnpj.Branch);      // Characters 9-12 (branch/order)
Console.WriteLine(cnpj.CheckDigits); // Last 2 characters
```

### CPF (Individual Registration Number)

```csharp
using Elekto.BrazilianDocuments;

// Parse from string
var cpf = Cpf.Parse("123.456.789-09");

// Parse from number
var cpf2 = Cpf.Parse(12345678909L);

// Validate
if (Cpf.IsValid("123.456.789-09"))
{
    Console.WriteLine("Valid!");
}

// Create with automatic check digit calculation
var newCpf = Cpf.Create(123456789L);
Console.WriteLine(newCpf.ToString("G")); // 123.456.789-09

// Convert to numeric value
long value = cpf.ToLong(); // 12345678909
```

## Error Handling

When parsing or constructing invalid documents, the library throws a unified exception:

- BadDocumentException: for both CPF and CNPJ invalid inputs
- DocumentType: indicates which document triggered the exception (Cpf, Cnpj, Unknown)

```csharp
try
{
    var cpf = Cpf.Parse("invalid");
}
catch (BadDocumentException ex) when (ex.SourceType == DocumentType.Cpf)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.InvalidDocument);
}

try
{
    var cnpj = Cnpj.Parse("invalid");
}
catch (BadDocumentException ex) when (ex.SourceType == DocumentType.Cnpj)
{
    Console.WriteLine(ex.Message);
}
```

## Format Specifiers

### CNPJ Formats

| Format | Description | Example |
|--------|-------------|---------|
| `"G"` | General (with punctuation) | `09.358.105/0001-91` |
| `"B"` | Bare (14 chars, no punctuation) | `09358105000191` |
| `"S"` | Short (no leading zeros) | `9358105000191` |
| `"BS"` | Bare Small (root only) | `09358105` |

### CPF Formats

| Format | Description | Example |
|--------|-------------|---------|
| `"G"` | General (with punctuation) | `123.456.789-09` |
| `"B"` | Bare (11 digits) | `12345678909` |
| `"S"` | Short (no leading zeros) | `12345678909` |

## Alphanumeric CNPJ

Starting in 2026, the Brazilian Federal Revenue Service will issue alphanumeric CNPJs. This library fully supports the new format:

```csharp
// Traditional numeric CNPJ
var numeric = Cnpj.Parse("09.358.105/0001-91");

// New alphanumeric CNPJ
var alpha = Cnpj.Parse("12.ABC.345/01DE-35");

// "Fun" CNPJs are valid!
var elekto = Cnpj.Create("ELEKTO", "0001");  // 00.ELE.KTO/0001-40
var error = Cnpj.Create("ERRADO", "ERRO");   // 00.ERR.ADO/ERRO-51 (Portuguese for "ERROR")
```

The validation algorithm correctly handles:
- Mixed alphanumeric characters (A-Z, 0-9)
- Case-insensitive input
- Leading zero omission (e.g., `1/0001-36` is valid)
- Various punctuation formats

## Parsing Helpers

Use `Parse` for throwing behavior and `TryParse` for non-throwing behavior:

```csharp
var cpf = Cpf.Parse("123.456.789-09");
var cnpj = Cnpj.Parse("09.358.105/0001-91");

Cpf? maybeCpf = Cpf.TryParse("123.456.789-09");
Cnpj? maybeCnpj = Cnpj.TryParse("09.358.105/0001-91");
```

## JSON Serialization

Both types include built-in JSON converters:

```csharp
var company = new Company
{
    Name = "Elekto",
    Cnpj = Cnpj.Parse("09.358.105/0001-91")
};

var json = JsonSerializer.Serialize(company);
// {"Name":"Elekto","Cnpj":"09.358.105/0001-91"}

var deserialized = JsonSerializer.Deserialize<Company>(json);
```

## Performance

The validation algorithms are optimized for minimal memory allocation:

- **CNPJ validation**: Single-pass algorithm with zero heap allocation during validation
- **CPF validation**: Uses numeric operations without string manipulation
- **Check digit calculation**: Performed in a single loop for both digits

Typical performance on modern hardware:
- CNPJ validation: ~7,000,000+ validations/second
- CPF validation: ~40,000,000+ validations/second

## Credits

The zero-allocation CNPJ validation algorithm is based on work by:
- [Fernando Cerqueira](https://github.com/FRACerqueira/CnpjAlfaNumerico)
- [Elemar Júnior](https://elemarjr.com/arquivo/validando-cnpj-respeitando-o-garbage-collector/)

## License

MIT License - Copyright (c) 2013-2026 Elekto Produtos Financeiros

See [LICENSE](License.txt) for details.
