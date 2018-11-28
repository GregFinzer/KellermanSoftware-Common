using System;
using System.Text;

namespace KellermanSoftware.Common
{
    /// <summary>
    /// RC4 Encryption Algorithm
    /// </summary>
    public class RC4Encryption
    {
        private const int _n = 256;

        #region Public Methods
        /// <summary>
        /// Encrypt the passed string using a password
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Encrypt(string input, string password)
        {
            return StringToHexString(EncryptDecryptRC4(input, password));
        }

        /// <summary>
        /// Decrypt the passed string using a password
        /// </summary>
        /// <param name="input"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string Decrypt(string input, string password)
        {
            return EncryptDecryptRC4(HexStringToString(input), password);
        }
        #endregion

        #region Private Methods
        private int[] InitializeRC4(string password)
        {
            int[] sbox = new int[_n];
            int[] key = new int[_n];
            int keyLength = password.Length;
            for (int i = 0; i < _n; i++)
            {
                key[i] = (int)password[i % keyLength];
                sbox[i] = i;
            }

            int b = 0;
            for (int a = 0; a < _n; a++)
            {
                b = (b + sbox[a] + key[a]) % _n;
                int temp = sbox[a];
                sbox[a] = sbox[b];
                sbox[b] = temp;
            }

            return sbox;
        }

        private string EncryptDecryptRC4(string input, string password)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            int[] sbox = InitializeRC4(password);

            int i = 0;
            int j = 0;
            int k = 0;
            StringBuilder cipher = new StringBuilder();
            for (int a = 0; a < input.Length; a++)
            {
                i = (i + 1) % _n;
                j = (j + sbox[i]) % _n;
                int temp = sbox[i];
                sbox[i] = sbox[j];
                sbox[j] = temp;

                k = sbox[(sbox[i] + sbox[j]) % _n];
                int cipherBy = ((int)input[a]) ^ k;  // xor operation
                cipher.Append(Convert.ToChar(cipherBy));
            }
            return cipher.ToString();
        }

        private static string StringToHexString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                int number = Convert.ToInt32(input[i]);
                builder.Append(string.Format("{0:X2}", number));
            }
            return builder.ToString();
        }

        private static string HexStringToString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < input.Length; i += 2)
            {
                int number = Convert.ToInt32(input.Substring(i, 2), 16);
                builder.Append(Convert.ToChar(number));
            }
            return builder.ToString();
        }
        #endregion
    }
}
