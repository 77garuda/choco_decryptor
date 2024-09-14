using System;
using System.Text;
using System.Security.Cryptography;

namespace choco_decryptor
{
    internal class Program
    {
        private static readonly byte[] _entropyBytes = Encoding.UTF8.GetBytes("Chocolatey");

        internal static string EncryptString(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            var decryptedByteArray = Encoding.UTF8.GetBytes(value);
            var encryptedByteArray = ProtectedData.Protect(decryptedByteArray, _entropyBytes, DataProtectionScope.LocalMachine);
            var encryptedString = Convert.ToBase64String(encryptedByteArray);
            return encryptedString;
        }

        internal static string DecryptString(string encryptedString)
        {
            var encryptedByteArray = Convert.FromBase64String(encryptedString);
            var decryptedByteArray = ProtectedData.Unprotect(encryptedByteArray, _entropyBytes, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(decryptedByteArray);
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("\tchoco_decryptor.exe -decrypt <base64_string>:");
                return;
            }
            string b64blob = args[1];
            var dec = DecryptString(b64blob);
            Console.WriteLine("Decrypted password: " + dec);
        }
    }
}
