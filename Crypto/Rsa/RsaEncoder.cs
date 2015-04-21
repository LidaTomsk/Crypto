using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Crypto.Des;

namespace Crypto.Rsa
{
    class RsaEncoder
    {
        private readonly RandomNumberGenerator _rand = RandomNumberGenerator.Create();
        private readonly Random _simpleRand = new Random();

        public void GeneratePublicKey(byte sizeInBytes, string fileNamePrefix)
        {
            var p = BigInteger.Zero;
            var q = BigInteger.Zero;
            Parallel.Invoke(() => p = GeneratePrimeNumber(sizeInBytes), () => q = GeneratePrimeNumber(sizeInBytes));

            var n = p * q;
            var fi = (p - 1) * (q - 1);
            var eRange = new[] { 17, 257, 65537 };
            var e = new BigInteger(eRange[_simpleRand.Next(eRange.Length)]);

            var d = ModInverse(e, fi);
            
            var nBytes = n.ToByteArray();
            using (var stream = new FileStream(fileNamePrefix + "_public.rsakey", FileMode.Create))
            using (var bw = new BinaryWriter(stream))
            {
                // {e, n}
                var eBytes = e.ToByteArray();
                bw.Write(eBytes.Length);
                bw.Write(eBytes);

                bw.Write(nBytes.Length);
                bw.Write(nBytes);
            }

            using (var stream = new FileStream(fileNamePrefix + "_private.rsakey", FileMode.Create))
            using (var bw = new BinaryWriter(stream))
            {
                // {d, n}
                var dBytes = d.ToByteArray();
                bw.Write(dBytes.Length);
                bw.Write(dBytes);

                bw.Write(nBytes.Length);
                bw.Write(nBytes);
            }
        }

        private Tuple<BigInteger, BigInteger> ReadRsaKey(string keyFilePath)
        {
            BigInteger firstNumber, secondNumber;
            using (var stream = new FileStream(keyFilePath, FileMode.Open))
            using (var reader = new BinaryReader(stream))
            {
                var firstSize = reader.ReadInt32();
                var firstResBytes = new byte[firstSize + 1];
                reader.ReadBytes(firstSize).CopyTo(firstResBytes, 0);
                firstNumber = new BigInteger(firstResBytes);

                var secondSize = reader.ReadInt32();
                var secondResBytes = new byte[secondSize + 1];
                reader.ReadBytes(secondSize).CopyTo(secondResBytes, 0);
                secondNumber = new BigInteger(secondResBytes);
            }

            return Tuple.Create(firstNumber, secondNumber);
        }

        public void EncryptFile(string fileToEncodePath, string fileResultPath, string recieverPublicKeyFilePath)
        {
            var recieverPublicKey = ReadRsaKey(recieverPublicKeyFilePath);

            // генерим ключ для DES
            var desKeyBytes = new byte[9];
            _rand.GetBytes(desKeyBytes);
            desKeyBytes[desKeyBytes.Length - 1] = 0; // всегда неотрицательное
            var desKey = new BigInteger(desKeyBytes);

            // шифруем файл
            var bytesToEncrypt = File.ReadAllBytes(fileToEncodePath);
            var encryptedData = DesFileEncoder.EncryptBytes(bytesToEncrypt, desKey.ToByteArray());
            
            // шифруем ключ DES
            var desKeyEncoded = BigInteger.ModPow(desKey, recieverPublicKey.Item1, recieverPublicKey.Item2);

            // пишем в файл
            using (var fs = new FileStream(fileResultPath, FileMode.Create))
            using (var bw = new BinaryWriter(fs))
            {
                var desKeyEncodedBytes = desKeyEncoded.ToByteArray();
                bw.Write(desKeyEncodedBytes.Length);
                bw.Write(desKeyEncodedBytes);

                bw.Write(encryptedData.Length);
                bw.Write(encryptedData);
            }
        }

        public void DecryptFile(string fileToDecrypt, string fileResultPath, string privateKeyOfReceiver)
        {
            var recieverPrivateKey = ReadRsaKey(privateKeyOfReceiver);
            byte[] desKeyBytes, encryptedDataBytes;
            using (var fs = new FileStream(fileToDecrypt, FileMode.Open))
            using (var bw = new BinaryReader(fs))
            {
                var desKeyLength = bw.ReadInt32();
                desKeyBytes = bw.ReadBytes(desKeyLength);

                var encLength = bw.ReadInt32();
                encryptedDataBytes = bw.ReadBytes(encLength);
            }

            // получили зашифрованный ключ DES
            var desKeyBytesForBigInt = new byte[desKeyBytes.Length + 1];
            desKeyBytes.CopyTo(desKeyBytesForBigInt, 0);
            var desKeyEncrypted = new BigInteger(desKeyBytesForBigInt);

            var desKeyDecrypted = BigInteger.ModPow(desKeyEncrypted, recieverPrivateKey.Item1, recieverPrivateKey.Item2).ToByteArray();
            var decryptedData = DesFileEncoder.DecryptBytes(encryptedDataBytes, desKeyDecrypted);

            File.WriteAllBytes(fileResultPath, decryptedData);
        }

        public void CreateDigitalSign(string filePath, string privateKeyFilePath, string resultSignFilePath)
        {
            var checkSum = GetChecksum(filePath);
            var checkSumTembBytes = new byte[checkSum.Length + 1];
            checkSum.CopyTo(checkSumTembBytes, 0);
            var checkSumBigInt = new BigInteger(checkSumTembBytes);

            var privateKey = ReadRsaKey(privateKeyFilePath);
            var sign = BigInteger.ModPow(checkSumBigInt, privateKey.Item1, privateKey.Item2);
            File.WriteAllBytes(resultSignFilePath, sign.ToByteArray());
        }

        public bool CheckSign(string checkFilePath, string publicKeyFilePath, string signFilePath)
        {
            var checkSum = GetChecksum(checkFilePath);
            var checkSumTembBytes = new byte[checkSum.Length + 1];
            checkSum.CopyTo(checkSumTembBytes, 0);
            var checkSumBigInt = new BigInteger(checkSumTembBytes);

            var publicKey = ReadRsaKey(publicKeyFilePath);

            var sign = File.ReadAllBytes(signFilePath);
            var signTempBytes = new byte[sign.Length + 1];
            sign.CopyTo(signTempBytes, 0);
            var signBigInt = new BigInteger(signTempBytes);

            var decryptedCheckSum = BigInteger.ModPow(signBigInt, publicKey.Item1, publicKey.Item2);

            return decryptedCheckSum == checkSumBigInt;
        }

        /// <summary>
        /// Расширенный алгоритм эвклида
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <returns>число, мультипликативно обратное a по модулю n</returns>
        private BigInteger ModInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i/a, x = a;
                a = i%x;
                i = x;
                x = d;
                d = v - t*x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n)%n;
            return v;
        }

        private BigInteger GeneratePrimeNumber(byte byteSize)
        {
            var byteData = new byte[byteSize + 1];
            while (true)
            {
                _rand.GetBytes(byteData);
                byteData[byteData.Length - 1] = 0; // всегда неотрицательное
                var testNumber = new BigInteger(byteData);
                if (IsPrimeNumber(testNumber)) return testNumber;
            }
        }

        private bool IsPrimeNumber(BigInteger m)
        {
            // Тест Миллера-Рабина
            if (m == 2 || m == 3) return true;
            if (m < 2 || m % 2 == 0) return false;

            var s = 0;
            var t = m - 1;
            while (t % 2 == 0)
            {
                t /= 2;
                s++;
            }

            var randData = new byte[m.ToByteArray().LongLength + 1];
            var r = (int) BigInteger.Log(m, 2);
            for (int i = 0; i < r; i++)
            {
                BigInteger a;
                do
                {
                    _rand.GetBytes(randData);
                    randData[randData.Length - 1] = 0; // всегда неотрицательное
                    a = new BigInteger(randData);
                } while (a < 2 || a > m - 2);

                var x = BigInteger.ModPow(a, t, m);
                if (x == 1 || x == m - 1) continue;
                for (int j = 0; j < s - 1; j++)
                {
                    x = BigInteger.ModPow(x, 2, m);
                    if (x == 1) return false;
                    if (x == m - 1) break;
                }
                if (x != m - 1) return false;
            }

            return true;
        }

        private static byte[] GetChecksum(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                return md5.ComputeHash(stream);
            }
        }
    }
}
