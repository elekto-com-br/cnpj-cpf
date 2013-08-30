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
using System.Diagnostics;

namespace Elekto.Organizations
{
    internal class Program
    {
        private static void Main()
        {
            {
                #region Diversão com CNPJ

                // Validações com os métodos estáticos
                string goodCnpj = "53.884.727/0001-45";
                Debug.Assert(Cnpj.IsValid(goodCnpj));

                goodCnpj = "53884727000145";
                Debug.Assert(Cnpj.IsValid(goodCnpj));

                const long goodCnpjNumber = 53884727000145;
                Debug.Assert(Cnpj.IsValid(goodCnpjNumber));

                string badCnpj = "53.884.727/0001-14";
                Debug.Assert(!Cnpj.IsValid(badCnpj));

                badCnpj = "63884727000115";
                Debug.Assert(!Cnpj.IsValid(badCnpj));

                const long badCnpjNumber = 63884727000115;
                Debug.Assert(!Cnpj.IsValid(badCnpjNumber));

                // Criação do Cnpj
                var cnpj = new Cnpj(goodCnpjNumber); // ok!
                Console.WriteLine("CNPJ: {0}", cnpj.ToString("G"));

                try
                {
                    var superBad = new Cnpj(badCnpj);
                    Console.WriteLine("Nunca aqui... Bad CNPJ: {0}", superBad.ToString("G"));
                }
                catch (Exception)
                {
                    // Um Cnpj inválido não pode existir
                    Debug.Assert(true);
                }

                // Crie seu prório passando os digitos iniciais
                Cnpj myNewCnpj = Cnpj.NewCnpj(20110529);
                Console.WriteLine("Novo CNPJ: {0}", myNewCnpj.ToString("G"));

                Cnpj bigNewCnpj = Cnpj.NewCnpj("99.999.999/9999");
                Console.WriteLine("Big CNPJ: {0}", bigNewCnpj.ToString("G"));

                // Rápido?
                const int max = 100000;
                var random = new Random();
                var sw = new Stopwatch();

                int numValids = 0;
                for (int i = 0; i < max; ++i)
                {
                    long candidate = random.Next(99999999);
                    candidate *= 10000;
                    candidate += random.Next(9999);
                    candidate *= 100;
                    candidate += random.Next(99);
                    sw.Start();
                    bool isValid = Cnpj.IsValid(candidate);
                    sw.Stop();
                    if (isValid)
                    {
                        numValids++;
                    }
                }
                Console.WriteLine("Testados {0:N0} CNPJs. {1:N0} válidos. {2:N1} testes/s", max, numValids,
                    max/sw.Elapsed.TotalSeconds);

                #endregion
            }

            {
                #region Diversão com CPF

                Console.WriteLine();

                // Validações com os métodos estáticos
                string goodCpf = "716.735.161-06";
                Debug.Assert(Cpf.IsValid(goodCpf));

                goodCpf = "71673516106";
                Debug.Assert(Cpf.IsValid(goodCpf));

                const long goodCpfNumber = 71673516106;
                Debug.Assert(Cpf.IsValid(goodCpfNumber));

                string badCpf = "716.735.161-69";
                Debug.Assert(!Cpf.IsValid(badCpf));

                badCpf = "71673516169";
                Debug.Assert(!Cpf.IsValid(badCpf));

                const long badCpfNumber = 71673516169;
                Debug.Assert(!Cpf.IsValid(badCpfNumber));

                // Criação do Cnpj
                var cpf = new Cpf(goodCpfNumber); // ok!
                Console.WriteLine("CPF: {0}", cpf.ToString("G"));

                try
                {
                    var superBad = new Cpf(badCpf);
                    Console.WriteLine("Nunca aqui... Bad CPF: {0}", superBad.ToString("G"));
                }
                catch (Exception)
                {
                    // Um Cnpj inválido não pode existir
                    Debug.Assert(true);
                }

                // Crie seu prório passando os digitos iniciais
                Cpf myNewCnpj = Cpf.NewCpf(19740722);
                Console.WriteLine("Novo CPF: {0}", myNewCnpj.ToString("G"));

                Cpf bigNewCpf = Cpf.NewCpf("999.999.999");
                Console.WriteLine("Big CPF: {0}", bigNewCpf.ToString("G"));

                // Rápido?
                const int max = 100000;
                var random = new Random();
                var sw = new Stopwatch();
                int numValids = 0;
                for (int i = 0; i < max; ++i)
                {
                    long candidate = random.Next(999999999);
                    candidate *= 100;
                    candidate += random.Next(99);
                    sw.Start();
                    bool isValid = Cpf.IsValid(candidate);
                    sw.Stop();
                    if (isValid)
                    {
                        numValids++;
                    }
                }
                Console.WriteLine("Testados {0:N0} CPFs. {1:N0} válidos. {2:N1} testes/s", max, numValids,
                    max/sw.Elapsed.TotalSeconds);

                #endregion
            }            
        }
    }
}