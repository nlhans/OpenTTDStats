using System;
using System.Collections.Generic;
using SimTelemetry.Domain.Memory;


namespace OpenTTDStatsLive
{
    public class TTDSample
    {
        public List<TrainStat> Trains = new List<TrainStat>();

        public TTDSample(MemoryProvider provider, long addr_base)
        {
            var vehiclePtr = provider.Get("Map").ReadAs<int>("VehicleBase");
            //if (vehiclePtr == 0) return;
            //vehiclePtr = provider.Reader.ReadInt32(vehiclePtr);
            if (vehiclePtr == 0) return;
            var vehicleCount = provider.Get("Map").ReadAs<int>("VehicleCount");

            var vehicleListCache = new int[vehicleCount + 100]; // 50 extra for good measure.
            var tempArray = provider.Reader.ReadBytes((IntPtr)vehiclePtr, (uint)vehicleListCache.Length * 4);
            for (var i = 0; i < vehicleListCache.Length; i++)
            {
                vehicleListCache[i] = BitConverter.ToInt32(tempArray, i * 4);

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
                        vehicleListPtr = provider.Reader.ReadInt32(new IntPtr(vehiclePtr + objects * 8));
                    }

                    if (vehicleListPtr != 0)
                    {
                        var tile = provider.Reader.ReadInt32(new IntPtr(vehicleListPtr + 0x38)); // <<<<
                        if (tile != 0)
                        {
                            var spd = provider.Reader.ReadInt16(vehicleListPtr + 0xBE);
                            
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