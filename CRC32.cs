using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ROMClass.Checksum
{
    /// <summary>
    /// Calculates a 32bit Cyclic Redundancy Checksum (CRC) using the
    /// same polynomial used by Zip.
    /// </summary>
    public class CRC32
    {
        private uint[] crc32Table;
        private const int BUFFER_SIZE = 1024;

        /// <summary>
        /// Returns the CRC32 for the specified stream.
        /// </summary>
        /// <param name="stream">The stream to calculate the CRC32 for</param>
        /// <returns>An unsigned integer containing the CRC32 calculation</returns>
        public uint GetCrc32(Stream stream)
        {
            unchecked
            {
                uint crc32Result;
                crc32Result = 0xFFFFFFFF;
                byte[] buffer = new byte[BUFFER_SIZE];
                int readSize = BUFFER_SIZE;

                int count = stream.Read(buffer, 0, readSize);
                while (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        crc32Result = (crc32Result >> 8) ^ this.crc32Table[buffer[i] ^ (crc32Result & 0x000000FF)];
                    }
                    count = stream.Read(buffer, 0, readSize);
                }

                return ~crc32Result;
            }
        }

        /// <summary>
        /// Returns the CRC32 for the specified stream.
        /// </summary>
        /// <param name="data">A byte array to CRC check.</param>
        /// <returns>An unsigned integer containing the CRC32 calculation</returns>
        public uint GetCrc32(byte[] data)
        {
            unchecked
            {
                uint crc32Result;
                crc32Result = 0xFFFFFFFF;

                int count = data.Length;
                while (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        crc32Result = (crc32Result >> 8) ^ this.crc32Table[data[i] ^ (crc32Result & 0x000000FF)];
                    }
                }

                return ~crc32Result;
            }
        }

        /// <summary>
        /// Initializes a new instance of the CRC32 class. Pre-initialises the table
        /// for speed of lookup.
        /// </summary>
        public CRC32()
        {
            unchecked
            {
                // This is the official polynomial used by CRC32 in PKZip.
                // Often the polynomial is shown reversed as 0x04C11DB7.
                uint polynomial = 0xEDB88320;
                uint i, j;

                this.crc32Table = new uint[256];

                uint crc32;
                for (i = 0; i < 256; i++)
                {
                    crc32 = i;
                    for (j = 8; j > 0; j--)
                    {
                        if ((crc32 & 1) == 1)
                        {
                            crc32 = (crc32 >> 1) ^ polynomial;
                        }
                        else
                        {
                            crc32 >>= 1;
                        }
                    }
                    this.crc32Table[i] = crc32;
                }
            }
        }
    }
}