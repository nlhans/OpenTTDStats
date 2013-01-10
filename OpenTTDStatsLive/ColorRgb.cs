using System.Drawing;

namespace OpenTTDStatsLive
{
    public struct ColorRgb
    {
        public byte R;
        public byte G;
        public byte B;
        public ColorRgb(Color value)
        {
            this.R = value.R;
            this.G = value.G;
            this.B = value.B;
        }
        public static implicit operator Color(ColorRgb rgb)
        {
            Color c = Color.FromArgb(rgb.R, rgb.G, rgb.B);
            return c;
        }
        public static explicit operator ColorRgb(Color c)
        {
            return new ColorRgb(c);
        }
    }
}