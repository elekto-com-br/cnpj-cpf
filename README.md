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
- **Multi-Target**: Supports .NET 8, .NET 9, and .NET Standard 2.0

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
var newCpf = Cpf.NewCpf(123456789L);
Console.WriteLine(newCpf.ToString("G")); // 123.456.789-09

// Convert to numeric value
long value = cpf.ToLong(); // 12345678909
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
- CNPJ validation: ~1,000,000+ validations/second
- CPF validation: ~2,000,000+ validations/second

## Credits

The zero-allocation CNPJ validation algorithm is based on work by:
- [Fernando Cerqueira](https://github.com/FRACerqueira/CnpjAlfaNumerico)
- [Elemar Júnior](https://elemarjr.com/arquivo/validando-cnpj-respeitando-o-garbage-collector/)

## License

MIT License - Copyright (c) 2013-2025 Elekto Produtos Financeiros

See [LICENSE](License.txt) for details.
