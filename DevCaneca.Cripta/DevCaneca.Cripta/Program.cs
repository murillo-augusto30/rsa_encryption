using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DevCaneca.Cripta
{
    static class Program
    {
        //[STAThread]
        static void Main(string[] args)
        {
            string mensagem = "Minha primeira mensagem criptografada com sucesso!";
            Console.WriteLine("Mensagem Original: {0}", mensagem);


            List<int> mensagemASCII = ObterASCII(mensagem).Select(item => Convert.ToInt32(item)).ToList();
            Console.WriteLine("Mensagem ASCII Original: {0}\n", String.Join(" ", mensagemASCII));


            int tamanhoChave = 2;
            Dictionary<string, int> chaves = ObterChaves(tamanhoChave);
            Console.WriteLine(
                "Chaves: {\n" +
                    $"      \"Tamanho\": {tamanhoChave}  \n" +
                    $"      \"p\": {chaves["p"]}  \n" +
                    $"      \"q\": {chaves["q"]}  \n" +
                    $"      \"E\": {chaves["E"]}  \n" +
                    $"      \"D\": {chaves["D"]}  \n" +
                    $"      \"N\": {chaves["N"]}  \n" +
                "}"
            );


            List<int> mensagemEncriptada = EncriptarMensagem(mensagemASCII, chaves);
            Console.WriteLine("Mensagem Encriptada: {0}", String.Join(" ", mensagemEncriptada));


            List<byte> mensagemDecriptadaASCII = DecriptarMensagem(mensagemEncriptada, chaves).Select(item => Convert.ToByte(item)).ToList();


            string mensagemDecriptada = ObterMensagem(mensagemDecriptadaASCII);
            Console.WriteLine("\nMensagem ASCII Decriptada: {0}", mensagemDecriptada);


            List<int> numerosPrimos = ObterFatores(chaves["N"]);
            Console.WriteLine(
                "Quebra da criptografia: {\n" +
                    $"      \"p\": {numerosPrimos.Min()}  \n" +
                    $"      \"q\": {numerosPrimos.Max()}  \n" +
                "}"
            );


            Console.ReadLine();
        }

        #region Funções Matematicas
        private static List<int> ObterNumerosPrimos(int maxRange)
        {
            int numeroAtual = 3;

            List<int> numerosPrimos = new List<int>();
            numerosPrimos.Add(2);

            while (numerosPrimos.Last() < maxRange + 1)
            {
                bool primo = true;
                for (int teste = 3; teste < numeroAtual; teste += 2)
                {
                    if (numeroAtual % teste == 0)
                    {
                        primo = false;
                        break;
                    }
                }

                if (primo)
                    numerosPrimos.Add(numeroAtual);

                numeroAtual += 2;
            }

            return numerosPrimos;
        }
        private static int ObterMDC(params int[] numeros)
        {
            int mdc = 1;
            var numerosPrimos = ObterNumerosPrimos(numeros.Max());

            while (numeros.Any(num => !num.Equals(1)))
            {
                bool divisorComum = true;
                int numPrimo = numerosPrimos.FirstOrDefault(a => numeros.Any(b => b % a == 0));

                for (int i = 0; i < numeros.Count(); i++)
                    if (numeros[i] % numPrimo == 0)
                        numeros[i] /= numPrimo;
                    else
                        divisorComum = false;

                if (divisorComum)
                    mdc *= numPrimo;
            }

            return mdc;
        }
        private static List<int> ObterFatores(int num)
        {
            List<int> fatores = new List<int>();
            var numerosPrimos = ObterNumerosPrimos(num);

            while (!num.Equals(1))
            {
                int numPrimo = numerosPrimos.FirstOrDefault(a => num % a == 0);

                if (num % numPrimo == 0)
                {
                    num /= numPrimo;
                    fatores.Add(numPrimo);
                }
            }

            return fatores;
        }
        #endregion

        #region Encriptador/Descriptador
        private static Dictionary<string, int> ObterChaves(int tamanho)
        {
            Dictionary<string, int> keys = new Dictionary<string, int>
            {
                { "p", 0 },

                { "q", 0 },

                { "E", 0 },

                { "D", 0 },

                { "N", 0 }
            };

            int tamanhoNumeros = Convert.ToInt32("1" + new String('0', tamanho));
            List<int> numerosPrimos = ObterNumerosPrimos(tamanhoNumeros);
            numerosPrimos.RemoveAll(item => !item.ToString().Length.Equals(tamanho));

            var random = new Random();
            int a = numerosPrimos.ElementAt(random.Next(0, numerosPrimos.Count));
            int b = numerosPrimos.Where(item => !item.Equals(keys["p"])).ElementAt(random.Next(0, numerosPrimos.Count - 1));

            keys["p"] = Math.Min(a, b);
            keys["q"] = Math.Max(a, b);

            keys["N"] = keys["p"] * keys["q"];

            int totiente = (keys["p"] - 1) * (keys["q"] - 1);
            List<int> numerosEncript = new List<int>();
            for (int i = 2; i < totiente; i++)
                if (ObterMDC(i, totiente).Equals(1))
                    numerosEncript.Add(i);

            keys["E"] = numerosEncript.ElementAt(random.Next(0, numerosEncript.Count));

            while (keys["D"] * keys["E"] % totiente != 1)
                keys["D"]++;

            return keys;
        }
        private static List<int> EncriptarMensagem(List<int> msgLetras, Dictionary<string, int> Keys)
        {
            var letrasEncriptadas = new List<int>();

            foreach (var codigoLetra in msgLetras)
            {
                int numeroEncriptado = Convert.ToInt32(BigInteger.ModPow(codigoLetra, Keys["E"], Keys["N"]).ToString());
                letrasEncriptadas.Add(numeroEncriptado);
            }

            return letrasEncriptadas;
        }
        private static List<int> DecriptarMensagem(List<int> msgLetras, Dictionary<string, int> Keys)
        {
            var letrasDecriptadas = new List<int>();

            foreach (var codigoLetra in msgLetras)
            {
                int numeroDecriptado = Convert.ToInt32(BigInteger.ModPow(codigoLetra, Keys["D"], Keys["N"]).ToString());
                letrasDecriptadas.Add(numeroDecriptado);
            }

            return letrasDecriptadas;
        }
        #endregion

        #region Processador de Texto
        private static string ObterMensagem(List<byte> valoresASCII)
        {
            StringBuilder textoFinal = new StringBuilder();
            valoresASCII.ForEach(letra => textoFinal.Append(Convert.ToChar(letra)));

            return textoFinal.ToString();
        }
        private static List<byte> ObterASCII(string texto)
        {
            return Encoding.ASCII.GetBytes(texto).ToList();
        }
        #endregion
    }
}
