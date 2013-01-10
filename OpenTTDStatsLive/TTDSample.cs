using System;
using System.Collections.Generic;

namespace OpenTTDStatsLive
{
    public class TTDSample
    {
        public List<TrainStat> Trains = new List<TrainStat>();

        public TTDSample(MyMemoryReader reader, long addr_base)
        {
            var vehiclePtr = reader.ReadInt32(new IntPtr(addr_base + 0xA1E828));
            var vehicleCount = reader.ReadInt32(new IntPtr(addr_base + 0xA1E810));

            var vehicleListCache = new int[vehicleCount * 2 + 100]; // 50 extra for good measure.
            var tempArray = reader.ReadBytes((IntPtr)vehiclePtr, (uint)vehicleListCache.Length * 4);
            for (var i = 0; i < vehicleCount + 50; i++)
            {
                vehicleListCache[i] = BitConverter.ToInt32(tempArray, i * 8);

            }

            var objects = 0;
            var trainCnt = 0;

            while (trainCnt < vehicleCount)
            {
                try
                {
                    int vehicleListPtr = 0;
                    if (objects < vehicleListCache.Length)
                    {
                        vehicleListPtr = vehicleListCache[objects];
                    }
                    else
                    {
                        vehicleListPtr = reader.ReadInt32(new IntPtr(vehiclePtr + objects * 8));
                    }

                    if (vehicleListPtr != 0)
                    {
                        var tile = reader.ReadInt32(new IntPtr(vehicleListPtr + 0x58));
                        if (tile != 0)
                        {
                            var spd = reader.ReadInt32(vehicleListPtr + 0xF4) >> 16;
                            
                            if (spd != 0)
                            {
                                TrainStat stat = new TrainStat(tile, spd);
                                Trains.Add(stat);
                            }
                            else 
                            {
                                TrainStat stat = new TrainStat(tile, -1);
                                Trains.Add(stat);
                            }
                        }

                        trainCnt++;
                    }
                }
                catch (Exception ex)
                {

                }
                objects++;


            }
        }

    }
}