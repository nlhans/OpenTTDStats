namespace OpenTTDStatsLive
{
    public class TileInfo
    {
        public bool IsWater;
        public byte Height;

        public TileInfo(ulong v)
        {
            Height = (byte) (v & 0xF);
            IsWater = (Height == 0) && ((v & 0xFF0) == 0x160);

        }

    }
}