using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Des
{
    class DesEncoder
    {
        #region Таблицы
        private readonly byte[] _ipArray =
        {
            58, 50, 42, 34, 26, 18, 10, 2,
            60, 52, 44, 36, 28, 20, 12, 4,
            62, 54, 46, 38, 30, 22, 14, 6,
            64, 56, 48, 40, 32, 24, 16, 8,
            57, 49, 41, 33, 25, 17, 9, 1,
            59, 51, 43, 35, 27, 19, 11, 3,
            61, 53, 45, 37, 29, 21, 13, 5,
            63, 55, 47, 39, 31, 23, 15, 7
        };

        private readonly byte[] _extendArray =
        {
            32, 1, 2, 3, 4, 5,
            4, 5, 6, 7, 8, 9,
            8, 9, 10, 11, 12, 13,
            12, 13, 14, 15, 16, 17,
            16, 17, 18, 19, 20, 21,
            20, 21, 22, 23, 24, 25,
            24, 25, 26, 27, 28, 29,
            28, 29, 30, 31, 32, 1
        };

        private readonly byte[] _c0d0Array =
        {
            57, 49, 41, 33, 25, 17, 9, 1, 58, 50, 42, 34, 26, 18,
            10, 2, 59, 51, 43, 35, 27, 19, 11, 3, 60, 52, 44, 36,
            63, 55, 47, 39, 31, 23, 15, 7, 62, 54, 46, 38, 30, 22,
            14, 6, 61, 53, 45, 37, 29, 21, 13, 5, 28, 20, 12, 4
        };

        private readonly byte[] _c0d0ShuffleArray =
        {
            14, 17, 11, 24, 1, 5, 3, 28, 15, 6, 21, 10, 23, 19, 12, 4,
            26, 8, 16, 7, 27, 20, 13, 2, 41, 52, 31, 37, 47, 55, 30, 40,
            51, 45, 33, 48, 44, 49, 39, 56, 34, 53, 46, 42, 50, 36, 29, 32
        };

        private readonly byte[] _shiftArray = {1, 1, 2, 2, 2, 2, 2, 2, 1, 2, 2, 2, 2, 2, 2, 1};

        private readonly byte[][] _sArray =
        {
            new byte[]
            {
                14, 4, 13, 1, 2, 15, 11, 8, 3, 10, 6, 12, 5, 9, 0, 7,
                0, 15, 7, 4, 14, 2, 13, 1, 10, 6, 12, 11, 9, 5, 3, 8,
                4, 1, 14, 8, 13, 6, 2, 11, 15, 12, 9, 7, 3, 10, 5, 0,
                15, 12, 8, 2, 4, 9, 1, 7, 5, 11, 3, 14, 10, 0, 6, 13
            },
            new byte[]
            {
                15, 1, 8, 14, 6, 11, 3, 4, 9, 7, 2, 13, 12, 0, 5, 10,
                3, 13, 4, 7, 15, 2, 8, 14, 12, 0, 1, 10, 6, 9, 11, 5,
                0, 14, 7, 11, 10, 4, 13, 1, 5, 8, 12, 6, 9, 3, 2, 15,
                13, 8, 10, 1, 3, 15, 4, 2, 11, 6, 7, 12, 0, 5, 14, 9
            },
            new byte[]
            {
                10, 0, 9, 14, 6, 3, 15, 5, 1, 13, 12, 7, 11, 4, 2, 8,
                13, 7, 0, 9, 3, 4, 6, 10, 2, 8, 5, 14, 12, 11, 15, 1,
                13, 6, 4, 9, 8, 15, 3, 0, 11, 1, 2, 12, 5, 10, 14, 7,
                1, 10, 13, 0, 6, 9, 8, 7, 4, 15, 14, 3, 11, 5, 2, 12
            },
            new byte[]
            {
                7, 13, 14, 3, 0, 6, 9, 10, 1, 2, 8, 5, 11, 12, 4, 15,
                13, 8, 11, 5, 6, 15, 0, 3, 4, 7, 2, 12, 1, 10, 14, 9,
                10, 6, 9, 0, 12, 11, 7, 13, 15, 1, 3, 14, 5, 2, 8, 4,
                3, 15, 0, 6, 10, 1, 13, 8, 9, 4, 5, 11, 12, 7, 2, 14
            },
            new byte[]
            {
                2, 12, 4, 1, 7, 10, 11, 6, 8, 5, 3, 15, 13, 0, 14, 9,
                14, 11, 2, 12, 4, 7, 13, 1, 5, 0, 15, 10, 3, 9, 8, 6,
                4, 2, 1, 11, 10, 13, 7, 8, 15, 9, 12, 5, 6, 3, 0, 14,
                11, 8, 12, 7, 1, 14, 2, 13, 6, 15, 0, 9, 10, 4, 5, 3
            },
            new byte[]
            {
                12, 1, 10, 15, 9, 2, 6, 8, 0, 13, 3, 4, 14, 7, 5, 11,
                10, 15, 4, 2, 7, 12, 9, 5, 6, 1, 13, 14, 0, 11, 3, 8,
                9, 14, 15, 5, 2, 8, 12, 3, 7, 0, 4, 10, 1, 13, 11, 6,
                4, 3, 2, 12, 9, 5, 15, 10, 11, 14, 1, 7, 6, 0, 8, 13
            },
            new byte[]
            {
                4, 11, 2, 14, 15, 0, 8, 13, 3, 12, 9, 7, 5, 10, 6, 1,
                13, 0, 11, 7, 4, 9, 1, 10, 14, 3, 5, 12, 2, 15, 8, 6,
                1, 4, 11, 13, 12, 3, 7, 14, 10, 15, 6, 8, 0, 5, 9, 2,
                6, 11, 13, 8, 1, 4, 10, 7, 9, 5, 0, 15, 14, 2, 3, 12
            },
            new byte[]
            {
                13, 2, 8, 4, 6, 15, 11, 1, 10, 9, 3, 14, 5, 0, 12, 7,
                1, 15, 13, 8, 10, 3, 7, 4, 12, 5, 6, 11, 0, 14, 9, 2,
                7, 11, 4, 1, 9, 12, 14, 2, 0, 6, 10, 13, 15, 3, 5, 8,
                2, 1, 14, 7, 4, 10, 8, 13, 15, 12, 9, 0, 3, 5, 6, 11
            }
        };

        private readonly byte[] _pTable =
        {
            16, 7, 20, 21, 29, 12, 28, 17,
            1, 15, 23, 26, 5, 18, 31, 10,
            2, 8, 24, 14, 32, 27, 3, 9,
            19, 13, 30, 6, 22, 11, 4, 25
        };

        private readonly byte[] _ip1Array =
        {
            40, 8, 48, 16, 56, 24, 64, 32, 39, 7, 47, 15, 55, 23, 63, 31,
            38, 6, 46, 14, 54, 22, 62, 30, 37, 5, 45, 13, 53, 21, 61, 29,
            36, 4, 44, 12, 52, 20, 60, 28, 35, 3, 43, 11, 51, 19, 59, 27,
            34, 2, 42, 10, 50, 18, 58, 26, 33, 1, 41, 9, 49, 17, 57, 25
        };
        #endregion

        private void OutputBinary(ulong number, int precision)
        {
            var list = new List<ulong>();
            for (int i = 0; i < precision; i++)
            {
                var bit = (number >> i) & 1;
                list.Add(bit);
            }
            list.Reverse();
            var str = string.Concat(list.Select(x => x.ToString()));

            var outputStr = string.Empty;
            for (int i = 0; i < str.Length; i++)
            {
                outputStr += str[i];
                if ((i + 1)%4 == 0)
                {
                    outputStr += " ";
                }
            }

            Debug.WriteLine(outputStr);
        }

        private ulong ShuffleUsingVector(ulong number, byte[] shuffleVector)
        {
            ulong res = 0;
            for (int i = 0; i < shuffleVector.Length; i++)
            {
                var bit = (number >> i) & 1;
                if (bit == 1)
                {
                    var newPos = shuffleVector[i] - 1;
                    res |= 1ul << newPos;
                }
            }
            return res;
        }

        private ulong TransformRightBits(ulong right, ulong key)
        {
            var extendedRight = ProjectUsingVector(right, _extendArray);
            var xoredRight = extendedRight ^ key;

            // разбиваем по 6 бит
            var bArray = new byte[8];
            for (int j = 0; j < 8; j++)
            {
                bArray[j] = (byte)(xoredRight & 0x3f); // 111111b
                xoredRight >>= 6;
            }

            // трансформируем
            var transBytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                var cur = bArray[i];
                var a = ((cur >> 5) << 1) | (cur & 1);
                var b = (cur & 0x1e) >> 1; // 11110b
                transBytes[i] = _sArray[i][a * 16 + b]; // row a, col b
            }

            // соединяем
            ulong concatedVal = 0;
            for (int i = 0; i < 8; i++)
            {
                concatedVal = (concatedVal << 4) | transBytes[i];
            }

            var resRightVal = ShuffleUsingVector(concatedVal, _pTable);
            return resRightVal;
        }

        private ulong ProjectUsingVector(ulong vector, byte[] projectVector)
        {
            ulong res = 0;
            for (int i = 0; i < projectVector.Length; i++)
            {
                var offset = projectVector[i] - 1;
                var bit = (vector >> offset) & 1;
                res = (bit << i) | res;
            }
            return res;
        }

        public ulong[] GenerateKey(byte[] keyBytes)
        {
            //var res = new byte[8];
            //var rand = new Random(0);
            //rand.NextBytes(res);

            var key = BitConverter.ToUInt64(keyBytes, 0);
            OutputBinary(key, 64);
            // 1101 1000 1110 0100 0111 0101 0101 1101 0110 1111 0100 0110 0000 1100 0001 1010

            ulong modKey = 0;
            ulong modBit = 0;
            for (int i = 0; i < 64; i++)
            {
                var curBit = (key >> i) & 1;
                if ((i + 1) % 8 == 0) // каждый восьмой бит
                {
                    if (modBit == 0)
                    {
                        // добавляем единицу для нечётности
                        modKey |= 1ul << i;
                    }
                    modBit = 0;
                }
                else
                {
                    if (curBit == 1) modKey |= 1ul << i;
                    modBit ^= curBit;
                }
            }

            OutputBinary(modKey, 64);
            // 0101 1000 0110 0100 0111 0101 0101 1101 1110 1111 0100 0110 1000 1100 0001 1010 

            // переставляем
            var shuffledKey = ProjectUsingVector(modKey, _c0d0Array);

            OutputBinary(shuffledKey, 56);
            // 1011 1010 1111 1100 1001 0110 1100 0010 0001 0000 1111 1000 1000 0100

            // разбиваем на 2 части
            ulong c0 = (shuffledKey >> 28) & 0xfffffff; // 28 старших бит
            ulong d0 = shuffledKey & 0xfffffff; // низшие 28 бит

            OutputBinary(c0, 28);
            OutputBinary(d0, 28);

            var keys = new ulong[16]; // 16 ключей по 48 бит

            // генерим 16 ключей
            for (int i = 0; i < 16; i++)
            {
                var c0Old = c0;
                var d0Old = d0;

                //сдвигаем
                c0 = 0;
                d0 = 0;
                for (int j = 0; j < 28; j++)
                {
                    var shift = (j + _shiftArray[i]) % 28;

                    var bitAtC0WithShift = (int) ((c0Old >> shift) & 1);
                    if (bitAtC0WithShift == 1) c0 |= 1ul << j;

                    var bitAtD0WithShift = (int)((d0Old >> shift) & 1);
                    if (bitAtD0WithShift == 1) d0 |= 1ul << j;
                }

                OutputBinary(c0, 28);
                OutputBinary(d0, 28);

                // соединяем
                ulong curKeyConcat = (c0 << 28) | d0;
                OutputBinary(curKeyConcat, 56);

                // вычленяем 48 бит
                var curKey = ProjectUsingVector(curKeyConcat, _c0d0ShuffleArray);
                OutputBinary(curKey, 48);

                // сохраняем ключ
                keys[i] = curKey;
            }

            return keys;
        }

        public byte[] Encode(byte[] data, byte[] key)
        {
            var result = new List<byte>(data.Length);

            var keys = GenerateKey(key);
            var partsCount = data.Length / 8;
            if (data.Length % 8 != 0) partsCount++;
            for (int i = 0; i < partsCount; i++)
            {
                ulong partData;
                if (data.Length % 8 != 0 && i == partsCount-1)
                {
                    var bytesToHandle = new byte[8];
                    for (int j = i*8, k = 0; j < data.Length; j++, k++)
                    {
                        bytesToHandle[k] = data[j];
                    }
                    partData = BitConverter.ToUInt64(bytesToHandle, 0);
                }
                else
                {
                    partData = BitConverter.ToUInt64(data, i * 8);
                }
                // разбить по 64
                OutputBinary(partData, 64);
                var shuffledData = ShuffleUsingVector(partData, _ipArray);
                OutputBinary(shuffledData, 64);

                var left32Bits = shuffledData >> 32;
                var right32Bits = shuffledData & uint.MaxValue;

                OutputBinary(left32Bits, 32);
                OutputBinary(right32Bits, 32);

                // 16 циклов фейстеля
                for (int j = 0; j < 16; j++)
                {
                    var temp = left32Bits;
                    left32Bits = right32Bits;

                    var rightTransformed = TransformRightBits(right32Bits, keys[j]);
                    right32Bits = temp ^ rightTransformed;
                }

                var concatedVal = (right32Bits << 32) | left32Bits;
                var resVal = ShuffleUsingVector(concatedVal, _ip1Array);
                var resAsBytes = BitConverter.GetBytes(resVal);
                result.AddRange(resAsBytes);
            }
            
            return result.ToArray();
        }

        public byte[] Decode(byte[] data, byte[] key)
        {
            var result = new List<byte>(data.Length);
            
            var keys = GenerateKey(key);
            var partsCount = data.Length / 8;
            if (data.Length % 8 != 0) partsCount++;
            for (int i = 0; i < partsCount; i++)
            {
                // разбить по 64
                var partData = BitConverter.ToUInt64(data, i * 8);
                var shuffledData = ShuffleUsingVector(partData, _ipArray);
                OutputBinary(shuffledData, 64);

                var left32Bits = shuffledData >> 32;
                var right32Bits = shuffledData & uint.MaxValue;

                OutputBinary(left32Bits, 32);
                OutputBinary(right32Bits, 32);

                // 16 циклов фейстеля
                for (int j = 0; j < 16; j++)
                {
                    var temp = left32Bits;
                    left32Bits = right32Bits;

                    var rightTransformed = TransformRightBits(right32Bits, keys[15 - j]);
                    right32Bits = temp ^ rightTransformed;
                }

                var concatedVal = (right32Bits << 32) | left32Bits;
                var resVal = ShuffleUsingVector(concatedVal, _ip1Array);
                var resAsBytes = BitConverter.GetBytes(resVal);
                result.AddRange(resAsBytes);
            }

            return result.ToArray();
        }
    }
}
