/* 
 The MIT License (MIT)

Copyright (c) 2013 Elekto Produtos Financeiros

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 
 */

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Elekto.Organizations
{
    /// <summary>
    ///     Um Cpf, sempre válido
    /// </summary>
    [Serializable]
    [ComVisible(true)]
    public struct Cpf : IComparable<Cpf>, IComparable, IEquatable<Cpf>, IXmlSerializable, ISerializable
    {
        /// <summary>
        ///     Um Cpf zerado, porem válido
        /// </summary>
        public static readonly Cpf Empty = new Cpf(0);

        private long _cpf;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cpf" /> struct.
        /// </summary>
        /// <param name="cpf">The CPF.</param>
        public Cpf(string cpf)
        {
            if (!IsValid(cpf))
            {
                throw new ArgumentException("Cpf inválido", "cpf");
            }

            if (!TryConvertToNumber(cpf, out _cpf))
            {
                throw new ArgumentException("Cpf inválido", "cpf");
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Cpf" /> struct.
        /// </summary>
        /// <param name="cpf">The CPF.</param>
        public Cpf(long cpf)
        {
            if (!IsValid(cpf))
            {
                throw new ArgumentException("Cpf inválido", "cpf");
            }
            _cpf = cpf;
        }

        /// <summary>
        ///     Prevents a default instance of the <see cref="Cpf" /> struct from being created.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        private Cpf(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            var s = (string) info.GetValue("Cpf", typeof (string));
            if (string.IsNullOrEmpty(s))
            {
                throw new InvalidOperationException("A representação deserializada está nula");
            }
            if (!IsValid(s))
            {
                throw new InvalidOperationException("A representação deserializada não é valida");
            }

            if (!TryConvertToNumber(s, out _cpf))
            {
                throw new InvalidOperationException("A representação deserializada não é valida");
            }
        }

        #region IComparable Members

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates whether
        ///     the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        ///     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these
        ///     meanings:
        ///     Value
        ///     Meaning
        ///     Less than zero
        ///     This instance is less than <paramref name="obj" />.
        ///     Zero
        ///     This instance is equal to <paramref name="obj" />.
        ///     Greater than zero
        ///     This instance is greater than <paramref name="obj" />.
        /// </returns>
        /// <param name="obj">
        ///     An object to compare with this instance.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="obj" /> is not the same type as this instance.
        /// </exception>
        /// <filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (!(obj is Cpf))
            {
                throw new ArgumentException("O argumento deve ser um Cpf", "obj");
            }

            return CompareTo((Cpf) obj);
        }

        #endregion

        #region IComparable<Cpf> Members

        /// <summary>
        ///     Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>
        ///     A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the
        ///     following meanings:
        ///     Value
        ///     Meaning
        ///     Less than zero
        ///     This object is less than the <paramref name="other" /> parameter.
        ///     Zero
        ///     This object is equal to <paramref name="other" />.
        ///     Greater than zero
        ///     This object is greater than <paramref name="other" />.
        /// </returns>
        /// <param name="other">
        ///     An object to compare with this object.
        /// </param>
        public int CompareTo(Cpf other)
        {
            return _cpf.CompareTo(other._cpf);
        }

        #endregion

        #region IEquatable<Cpf> Members

        /// <summary>
        ///     Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        ///     true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">
        ///     An object to compare with this object.
        /// </param>
        public bool Equals(Cpf other)
        {
            return CompareTo(other) == 0;
        }

        #endregion

        #region ISerializable Members

        /// <summary>
        ///     Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with the data needed to serialize the
        ///     target object.
        /// </summary>
        /// <param name="info">
        ///     The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> to populate with data.
        /// </param>
        /// <param name="context">
        ///     The destination (see <see cref="T:System.Runtime.Serialization.StreamingContext" />) for this serialization.
        /// </param>
        /// <exception cref="T:System.Security.SecurityException">
        ///     The caller does not have the required permission.
        /// </exception>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("Cpf", ToString("S"));
        }

        #endregion

        #region IXmlSerializable Members

        /// <summary>
        ///     This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return
        ///     null (Nothing in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the
        ///     <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
        /// </summary>
        /// <returns>
        ///     An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is
        ///     produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method
        ///     and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" />
        ///     method.
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        ///     Generates an object from its XML representation.
        /// </summary>
        /// <param name="reader">
        ///     The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized.
        /// </param>
        public void ReadXml(XmlReader reader)
        {
            string s = reader.ReadElementString();
            if (string.IsNullOrEmpty(s))
            {
                throw new InvalidOperationException("O elemento serializado é nulo ou vazio");
            }
            if (!IsValid(s))
            {
                throw new InvalidOperationException("O elemento serializado é invalido");
            }

            if (!TryConvertToNumber(s, out _cpf))
            {
                throw new InvalidOperationException("O elemento serializado é invalido");
            }
        }

        /// <summary>
        ///     Converts an object into its XML representation.
        /// </summary>
        /// <param name="writer">
        ///     The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized.
        /// </param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteString(ToString("S"));
        }

        #endregion

        /// <summary>
        ///     Determines whether the specified CPF is valid.
        /// </summary>
        /// <param name="cpf">The CPF.</param>
        /// <returns>
        ///     <c>true</c> if the specified CPF is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(string cpf)
        {
            long number;
            if (!TryConvertToNumber(cpf, out number))
            {
                return false;
            }
            return IsValid(number);
        }

        /// <summary>
        ///     Determines whether the specified CPF is valid.
        /// </summary>
        /// <param name="cpf">The CPF.</param>
        /// <returns>
        ///     <c>true</c> if the specified CPF is valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(long cpf)
        {
            if (cpf < 0) return false;
            if (cpf > 99999999999) return false;
            long inputCheck;
            long initial = Split(cpf, out inputCheck);
            return inputCheck == GetDigits(initial);
        }

        /// <summary>
        ///     Cria um novo CPF a partir dos 9 digitos inicias (sem os digitos de checagem)
        /// </summary>
        /// <param name="initialDigits"></param>
        /// <returns>Um Cpf</returns>
        /// <exception cref="ArgumentException" />
        public static Cpf NewCpf(string initialDigits)
        {
            long number;
            if (!TryConvertToNumber(initialDigits, out number))
            {
                throw new ArgumentException("Digitos iniciais inválidos.", "initialDigits");
            }

            return NewCpf(number);
        }

        /// <summary>
        ///     Cria um novo CPF a partir dos 9 digitos inicias (sem os digitos de checagem)
        /// </summary>
        /// <param name="initialDigits"></param>
        /// <returns>Um Cpf</returns>
        /// <exception cref="ArgumentException" />
        public static Cpf NewCpf(long initialDigits)
        {
            if (initialDigits < 0)
                throw new ArgumentOutOfRangeException("initialDigits", initialDigits, "Deve ser maior ou igual a zero.");
            if (initialDigits > 999999999)
                throw new ArgumentOutOfRangeException("initialDigits", initialDigits,
                    "Deve ser menor ou igual a 999999999.");

            long check = GetDigits(initialDigits);
            return new Cpf(initialDigits*100 + check);
        }

        /// <summary>
        ///     Gera um CPF a partir de uma string
        /// </summary>
        /// <param name="input">uma string representando um Cpf</param>
        /// <returns>Um Cpf</returns>
        /// <exception cref="ArgumentException" />
        public static Cpf Parse(string input)
        {
            Cpf cpf;
            if (!TryParse(input, out cpf))
            {
                throw new ArgumentException("O input não é um CPF válido.", "input");
            }
            return cpf;
        }

        /// <summary>
        ///     Tenta gerar um CPF a partir de uma string
        /// </summary>
        /// <param name="input">uma string representando um Cpf</param>
        /// <param name="cpf">O Cpf gerado</param>
        /// <returns>True se bem sucedido</returns>
        public static bool TryParse(string input, out Cpf cpf)
        {
            long number;
            if (!TryConvertToNumber(input, out number))
            {
                cpf = Empty;
                return false;
            }

            return TryParse(number, out cpf);
        }

        /// <summary>
        ///     Tenta gerar um CPF a partir de uma string
        /// </summary>
        /// <param name="input">uma string representando um Cpf</param>
        /// <returns>Um Cpf se bem sucedido, nulo caso contrário</returns>
        public static Cpf? TryParse(string input)
        {
            Cpf cpf;
            if (TryParse(input, out cpf)) return cpf;
            return null;
        }

        /// <summary>
        ///     Gera um CPF a partir de um long
        /// </summary>
        /// <param name="input">um long representando um Cpf</param>
        /// <returns>Um Cpf</returns>
        /// <exception cref="ArgumentException" />
        public static Cpf Parse(long input)
        {
            Cpf cpf;
            if (!TryParse(input, out cpf))
            {
                throw new ArgumentException("O input não é um CPF válido.", "input");
            }
            return cpf;
        }

        /// <summary>
        ///     Tenta gerar um CPF a partir de um long
        /// </summary>
        /// <param name="input">um long representando um Cpf</param>
        /// <param name="cpf">O Cpf gerado</param>
        /// <returns>True se bem sucedido</returns>
        public static bool TryParse(long input, out Cpf cpf)
        {
            if (!IsValid(input))
            {
                cpf = Empty;
                return false;
            }
            cpf = new Cpf(input);
            return true;
        }

        private static long GetDigits(long initialDigits)
        {
            // calculo do 1o digito            
            var weight = new long[] {10, 9, 8, 7, 6, 5, 4, 3, 2};
            long copy = initialDigits;
            long sum = 0;
            for (int i = 8; i >= 0; --i)
            {
                long digit = copy%10;
                long term = digit*weight[i];
                sum += term;
                copy /= 10;
            }
            long check1 = sum%11;
            if (check1 < 2)
            {
                check1 = 0;
            }
            else
            {
                check1 = 11 - check1;
            }

            // calculo do 2o digito
            weight = new long[] {11, 10, 9, 8, 7, 6, 5, 4, 3, 2};
            copy = initialDigits*10 + check1;
            sum = 0;
            for (int i = 9; i >= 0; --i)
            {
                long digit = copy%10;
                long term = digit*weight[i];
                sum += term;
                copy /= 10;
            }
            long check2 = sum%11;
            if (check2 < 2)
            {
                check2 = 0;
            }
            else
            {
                check2 = 11 - check2;
            }

            return check1*10 + check2;
        }

        private static bool TryConvertToNumber(string input, out long cpf)
        {
            cpf = 0;
            if (string.IsNullOrEmpty(input)) return false;

            input = input.Replace(".", "");
            input = input.Replace("-", "");

            return long.TryParse(input, out cpf);
        }

        private static long Split(long fullCnpj, out long checkDigits)
        {
            checkDigits = fullCnpj%100;
            return fullCnpj/100;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        ///     A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            return _cpf.GetHashCode();
        }

        /// <summary>
        ///     Converte para string
        /// </summary>
        /// <param name="format">
        ///     O Cpf 094.305.885-67 pode ser formatado como
        ///     "S": (short) 9430588567; ou
        ///     "B": (bare) 09430588567; ou
        ///     "G": (General) 094.305.885-67
        /// </param>
        /// <returns>Uma string formatada</returns>
        public string ToString(string format)
        {
            format = format.ToUpperInvariant();
            switch (format)
            {
                case "S":
                    return _cpf.ToString(CultureInfo.InvariantCulture);
                case "B":
                    return _cpf.ToString("00000000000");
                case "G":
                    string s = ToString("B");
                    return string.Format("{0}.{1}.{2}-{3}", s.Substring(0, 3), s.Substring(3, 3), s.Substring(6, 3),
                        s.Substring(9, 2));
                default:
                    throw new ArgumentOutOfRangeException("format", "O formato deve ser S, B ou G");
            }
        }

        /// <summary>
        ///     Converte o Cpf para um long
        /// </summary>
        /// <returns></returns>
        public long ToLong()
        {
            return _cpf;
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="o">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///     <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object o)
        {
            if (o is Cpf)
                return CompareTo((Cpf) o) == 0;
            return false;
        }

        /// <summary>
        ///     Implements the operator ==.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator ==(Cpf a, Cpf b)
        {
            return a.Equals(b);
        }

        /// <summary>
        ///     Implements the operator !=.
        /// </summary>
        /// <param name="a">A.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        ///     The result of the operator.
        /// </returns>
        public static bool operator !=(Cpf a, Cpf b)
        {
            return !(a.Equals(b));
        }

        /// <summary>
        ///     Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        ///     A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString("G");
        }
    }
}