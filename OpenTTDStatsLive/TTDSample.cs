using System;
using System.Collections.Generic;
using SimTelemetry.Domain.Memory;


namespace OpenTTDStatsLive
{
    public class TTDSample
    {
        public List<TrainStat> Trains = new List<TrainStat>();

        public TTDSample(MemoryProvider provider)
        {
            var vehiclePtr = provider.Get("Map").ReadAs<int>("VehicleBase");
            if (vehiclePtr == 0) return;
            var vehicleCount = provider.Get("Map").ReadAs<int>("VehicleCount");

            var vehicleListCache = new int[vehicleCount*2];
            var tempArray = provider.Reader.ReadBytes((IntPtr)vehiclePtr, (uint)vehicleListCache.Length * 4);
            for (var i = 0; i < vehicleListCache.Length; i++)
                vehicleListCache[i] = BitConverter.ToInt32(tempArray, i * 4);
            tempArray = new byte[0];

            var objects = 0;
            var trainCnt = 0;

            foreach (var vehicleListPtr in vehicleListCache)
            {
                try
                {

                    if (vehicleListPtr != 0)
                    {
                        var tile = provider.Reader.ReadInt32(new IntPtr(vehicleListPtr + 0x38)); // <<<<
                        if (tile > 0)
                        {
                            var spd = provider.Reader.ReadInt16(vehicleListPtr + 0xBE);
                            
                            if (spd > 0)
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