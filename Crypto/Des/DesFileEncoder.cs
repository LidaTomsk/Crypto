using System;
using System.IO;
using System.Linq;

namespace Crypto.Des
{
    static class DesFileEncoder
    {
        public static byte[] EncryptBytes(byte[] bytesToEncode, byte[] key)
        {
            res = bytesToEncode;
            var encoder = new DesEncoder();
            var encodedData = encoder.Encode(bytesToEncode, key);
            var bytesToRemove = (byte)((8 - bytesToEncode.Length % 8) % 8);

            var result = new byte[encodedData.Length + 1];
            result[0] = bytesToRemove;
            encodedData.CopyTo(result, 1);
            return result;
        }

        private static byte[] res;

        public static byte[] DecryptBytes(byte[] bytesToDecrypt, byte[] key)
        {
            var encoder = new DesEncoder();
            int skipCount = bytesToDecrypt[0];
            var dataToDecrypt = new byte[bytesToDecrypt.Length - 1];
            Array.Copy(bytesToDecrypt, 1, dataToDecrypt, 0, dataToDecrypt.Length);

            var decodedData = encoder.Decode(dataToDecrypt, key);
            var result = new byte[decodedData.Length - skipCount];
            Array.Copy(decodedData, 0, result, 0, result.Length);

            return result;
        }

        public static void EncryptFile(string pathToFile, string pathToKey, string pathToResultFile)
        {
            var key = File.ReadAllBytes(pathToKey);
            var bytesToEncode = File.ReadAllBytes(pathToFile);
            var result = EncryptBytes(bytesToEncode, key);
            File.WriteAllBytes(pathToResultFile, result);
        }

        public static void DecryptFile(string pathToFile, string pathToKey, string pathToResultFile)
        {
            var key = File.ReadAllBytes(pathToKey);
            var encryptedData = File.ReadAllBytes(pathToFile);
            var decryptedData = DecryptBytes(encryptedData, key);
            File.WriteAllBytes(pathToResultFile, decryptedData);
        }
    }
}
