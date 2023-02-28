using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace KIOKI_2.Classes
{
    public class SDES
    {
        string FirstKey;
        string SecondKey;

        int[] RuleP10 = { 3, 5, 2, 7, 4, 10, 1, 9, 8, 6 };
        int[] RuleP8 = { 6, 3, 7, 4, 8, 5, 10, 9 };
        int[] RuleP4 = { 2, 4, 3, 1 };
        int[] InitialPermutation = { 2, 6, 3, 1, 4, 8, 5, 7 };
        int[] PartsPermutation = { 5, 6, 7, 8, 1, 2, 3, 4 };
        int[] FinalPermutation = { 4, 1, 3, 5, 7, 2, 8, 6 };
        int[] ExpansionRule = { 4, 1, 2, 3, 2, 3, 4, 1 };

        int[,] SBox1 = { { 1, 0, 3, 2},
                         { 3, 2, 1, 0},
                         { 0, 2, 1, 3},
                         { 3, 1, 3, 2} };

        int[,] SBox2 = { { 0, 1, 2, 3},
                         { 2, 0, 1, 3},
                         { 3, 0, 1, 0},
                         { 2, 1, 0, 3} };

        public SDES(string key)
        {
            KeyGen(out FirstKey, out SecondKey, key);
        }

        private void KeyGen(out string firstKey, out string secondKey, string originalKey)
        {
            string processedWhithP10 = UseRule(RuleP10, originalKey);
            string shifted = Shift(1, processedWhithP10.Substring(0, 5)) + Shift(1, processedWhithP10.Substring(5, 5));

            firstKey = UseRule(RuleP8, shifted);
            secondKey = UseRule(RuleP8, Shift(2, shifted.Substring(0, 5)) + Shift(2, shifted.Substring(5, 5)));
        }

        private string Shift(int value, string block)
        {
            return (block + block.Substring(0, value)).Substring(value);
        }
        private string UseRule(int[] rule, string block)
        {
            string tmp = string.Empty;

            for (int i = 0; i < rule.Length; i++)
            {
                tmp += block[rule[i] - 1];
            }

            return tmp;
        }

        private async Task<string> Round(string key, string block)
        {
            string leftPart = await RoundLeftPart(key, block);

            return leftPart + block.Substring(4, 4);
        }
        private async Task<string> RoundLeftPart(string key, string block)
        {
            string _block = XOR(UseRule(ExpansionRule, block.Substring(4, 4)), key);

            string left2bits = await ProcessSBox(SBox1, _block.Substring(0, 4));
            string right2bits = await ProcessSBox(SBox2, _block.Substring(4, 4));

            return XOR(block.Substring(0, 4), UseRule(RuleP4, left2bits + right2bits));
        }
        private Task<string> ProcessSBox(int[,] sbox, string block)
        {
            int row = Convert.ToInt16(block.Substring(0, 1) + block.Substring(3, 1), 2);
            int column = Convert.ToInt16(block.Substring(1, 2), 2);
            int num = sbox[row, column];
            string bits = Convert.ToString(num, 2);

            if (bits.Length < 2) return Task.FromResult("0" + bits);

            return Task.FromResult(bits);
        }

        private string XOR(string str1, string str2)
        {
            string tmp = string.Empty;

            for (int i = 0; i < str1.Length; i++)
            {
                tmp += (str1[i] ^ str2[i]).ToString();
            }

            return tmp;
        }

        public async Task<string> DESencryption(string message)
        {
            message = UseRule(InitialPermutation, message);

            message = await Round(FirstKey, message);

            message = UseRule(PartsPermutation, message);

            message = await Round(SecondKey, message);

            return UseRule(FinalPermutation, message);
        }
        public async Task<string> EncryptAsync(string message) 
        {
            string result = string.Empty;

            foreach (var sym in message)
            {
                string binary = Convert.ToString(sym, 2);
                string expanded = new string('0', 16 - binary.Length) + binary;

                result += (char)Convert.ToInt32(await DESencryption(expanded.Substring(0, 8)) + await DESencryption(expanded.Substring(8, 8)), 2);
            }

            return result;
        }

        public async Task<string> DESdecryption(string message)
        {
            message = UseRule(InitialPermutation, message);

            message = await Round(SecondKey, message);

            message = UseRule(PartsPermutation, message);

            message = await Round(FirstKey, message);

            return UseRule(FinalPermutation, message);
        }
        public async Task<string> DecryptAsync(string message) 
        {
            string result = string.Empty;
            foreach (var sym in message)
            {
                string binary = Convert.ToString(sym, 2);
                string expanded = new string('0', 16 - binary.Length) + binary;

                result += (char)Convert.ToInt32(await DESdecryption(expanded.Substring(0, 8)) + await DESdecryption(expanded.Substring(8, 8)), 2);
            }

            return result; 
        }
    }
}
