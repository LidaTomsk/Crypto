using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Des
{
    class Program
    {
        static void Main(string[] args)
        {
            const bool isEncoding = true;
            var encoder = new DesEncoder();
            var key = File.ReadAllBytes("key.txt");

            byte[] result;
            if (isEncoding)
            {
                var bytesToEncode = File.ReadAllBytes("encode.txt");
                result = encoder.Encode(bytesToEncode, key);
                var bytesToRemove = (byte) ((8 - bytesToEncode.Length%8)%8);
                var res = new List<byte> {bytesToRemove};
                res.AddRange(result);
                result = res.ToArray();
            }
            else
            {
                var bytesToDecode = File.ReadAllBytes("result.txt");
                var skipCount = bytesToDecode[0];
                bytesToDecode = bytesToDecode.ToList().Skip(1).ToArray();
                result = encoder.Decode(bytesToDecode, key);
                result = result.ToList().Take(result.Length - skipCount).ToArray();
            }

            File.WriteAllBytes("result.txt", result);
        }
    }
}
