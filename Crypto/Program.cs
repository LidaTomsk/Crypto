using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Crypto.Des;
using Crypto.Rsa;

namespace Crypto
{
    class Program
    {
        static void Main(string[] args)
        {
            //DesFileEncoder.EncryptFile("resume_chuvilyev.pdf", "key.txt", "result.txt");
            //DesFileEncoder.DecryptFile("result.txt", "key.txt", "decrypted.pdf");

            //DesFileEncoder.EncryptFile("a.mp3", "key.txt", "result.txt");
            //DesFileEncoder.DecryptFile("result.txt", "key.txt", "b.mp3");

            var rsa = new RsaEncoder();
            //rsa.GeneratePublicKey(128, "alice");
            //rsa.GeneratePublicKey(128, "bob");
            //rsa.EncryptFile("resume_chuvilyev.pdf", "result.txt", "bob_public.rsakey");
            //rsa.DecryptFile("result.txt", "decrypted.pdf", "bob_private.rsakey");
            
            //rsa.CreateDigitalSign("encode.txt", "alice_private.rsakey", "sign.rsasign");
            Console.WriteLine(rsa.CheckSign("encode.txt", "alice_public.rsakey", "sign.rsasign"));
        }
    }
}
