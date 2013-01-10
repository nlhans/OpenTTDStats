namespace OpenTTDStatsLive
{
    public struct TrainStat
    {
        public long tile;
        public int speed;

        public TrainStat(long tile, int speed)
        {
            this.tile = tile;
            this.speed = speed;
        }
    }
}