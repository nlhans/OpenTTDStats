using System;
using Triton.Memory;

namespace OpenTTDStatsLive
{
    public class MyMemoryReader : MemoryReader
    {
        public ulong ReadUInt64(int address)
        {
            byte[] d = ReadBytes(address, 8);
            return BitConverter.ToUInt64(d, 0);
        }
    }
}