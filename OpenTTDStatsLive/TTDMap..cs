using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OpenTTDStatsLive
{
    public partial class TTDMap : UserControl
    {
        private int Width;
        private int Height;

        private TTDStats _mStats;
        public TTDMap
        (TTDStats stats)
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);

            _mStats = stats;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //
            var rect = e.ClipRectangle;

            Width = rect.Width;
            Height = rect.Height;

            Graphics g = e.Graphics;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.CompositingMode = CompositingMode.SourceCopy;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            if (_mStats.bMap == null)
                g.FillRectangle(Brushes.Black, rect);
            else
                lock (_mStats.bMap)
                {
                    if (_mStats.syncCamera)
                        g.DrawImage(_mStats.bMap, rect, _mStats.TileCameraX, _mStats.TileCameraY, _mStats.TileCameraW, _mStats.TileCameraH, GraphicsUnit.Pixel);
                    else
                        g.DrawImage(_mStats.bMap, rect);
                }


            try
            {
                // Render stats
                var tileStats = new TileStats[_mStats.MapSizeX*_mStats.MapSizeY];
                for (int index = 0; index < tileStats.Length; index++)
                {
                    tileStats[index].SpeedMin = 0xFFFF;
                    tileStats[index].SpeedMax = 0;
                }

                var mySamples = _mStats.GetSamplesCopy();
                foreach (var sample in mySamples)
                {
                    foreach (var train in sample.Trains)
                    {
                        if (_mStats.drawSpeed && train.speed == -1) continue;
                        if (train.tile-1 >= tileStats.Length) continue;
                        if (train.tile < 0) continue;
                        try
                        {
                            tileStats[train.tile].TrainsPassed++;
                            tileStats[train.tile].SpeedSum += train.speed;
                            tileStats[train.tile].SpeedMin = Math.Min(tileStats[train.tile].SpeedMin, train.speed);
                            tileStats[train.tile].SpeedMax = Math.Max(tileStats[train.tile].SpeedMax, train.speed);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(train.tile + " / " + tileStats.Length);
                        }
                    }
                }

                int max_value = 0;

                if (!_mStats.drawSpeed)
                {
                    max_value = mySamples.Count/2;
                }
                else
                {
                    max_value = 643;
                }
                var lastDrawX = 0;
                var lastDrawY = 0;

                for (var x = 0; x < _mStats.MapSizeX; x++)
                {
                    var drawX = x * Width/_mStats.MapSizeX;
                    if (_mStats.syncCamera)
                        drawX = (x - _mStats.TileCameraX) * Width / _mStats.TileCameraW;
                    if (drawX >= 0 && drawX <= rect.Width)
                    {
                        for (var y = 0; y < _mStats.MapSizeY; y++)
                        {
                            var tileIndex = x*_mStats.MapSizeY + y;

                            var drawY = y * Height / _mStats.MapSizeY;
                            if (_mStats.syncCamera)
                                drawY = (y - _mStats.TileCameraY) * Height / _mStats.TileCameraH;

                            if (drawY >= 0 && drawY < + Height && tileStats[tileIndex].TrainsPassed > 0)
                            {

                                var frac = 1.0;
                                //var frac = tileStats[tile] *1.0 / max_value;
                                if (_mStats.drawSpeed)
                                {
                                    frac = 1 -
                                           tileStats[tileIndex].SpeedSum/tileStats[tileIndex].TrainsPassed*1.0/max_value;
                                }
                                else
                                {
                                    frac = tileStats[tileIndex].TrainsPassed*1.0/max_value;
                                }
                                if (frac > 1) frac = 1;
                                if (frac < 0) frac = 0;
                                ColorRgb c = HSL2RGB(frac, 0.5, 0.5);

                                g.FillRectangle(new SolidBrush(Color.FromArgb(c.R, c.G, c.B)), lastDrawX, lastDrawY,
                                                drawX - lastDrawX, drawY - lastDrawY);
                            }

                            lastDrawY = drawY;
                        }
                    }
                    lastDrawX = drawX;
                }
            }catch(Exception ex)
            {
                string data = "********\r\n" + ex.Message + "\r\n" + ex.StackTrace + " \r\n\r\n";
                File.AppendAllText("error.txt", data);
            }
            base.OnPaint(e);
        }



        // Given H,S,L in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public static ColorRgb HSL2RGB(double h, double sl, double l)
        {
            h = 1 - h;
            double v;
            double r, g, b;

            r = l;   // default to gray
            g = l;
            b = l;
            v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;

                m = l + l - v;
                sv = (v - m) / v;
                h *= 4.0;
                sextant = (int)h;
                fract = h - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                }
            }
            ColorRgb rgb;
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);
            return rgb;
        }
    }

}
