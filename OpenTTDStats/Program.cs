using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using Triton.Memory;

namespace OpenTTDStats
{

    public struct ColorRGB
    {
        public byte R;
        public byte G;
        public byte B;
        public ColorRGB(Color value)
        {
            this.R = value.R;
            this.G = value.G;
            this.B = value.B;
        }
        public static implicit operator Color(ColorRGB rgb)
        {
            Color c = Color.FromArgb(rgb.R, rgb.G, rgb.B);
            return c;
        }
        public static explicit operator ColorRGB(Color c)
        {
            return new ColorRGB(c);
        }
    }

    public struct TileStats
    {
        public int TrainsPassed;
        public int SpeedSum;
        public int SpeedMin;
        public int SpeedMax;
    }

    class Program
    {
        private static int MapSizeX;
        private static int MapSizeY;

        [STAThread]
        static void Main(string[] args)
        {
            int samples_max ;
            bool sample = true;
            var tileStats = new TileStats[0];

            if(sample)
            {
                // 1.3.0-beta1
                var p = Process.GetProcessesByName("openttd");
                var index = 0;

                if (p.Length == 0)
                {
                    Console.WriteLine("Waiting for OpenTTD. Press ENTER to QUIT");
                    while (p.Length == 0)
                    {
                        p = Process.GetProcessesByName("openttd");
                        Thread.Sleep(500);
                        if (Console.KeyAvailable )
                            return;
                    }
                }
                else if (p.Length > 1)
                {
                    Console.WriteLine("Choose an instance.");

                    index = -1;

                    do
                    {
                        // More than one);
                        for (int i = 0; i < p.Length; i++)
                        {
                            Console.WriteLine((i + 1) + ". OpenTTD :D");
                        }

                        try
                        {
                            var ver = Int32.Parse(Console.ReadLine());
                            index = ver;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Invalid");
                        }
                    } while (index == -1);
                }

                Console.WriteLine("How many samples to take? NOTE: depending on the number of trains and your PC speed. Aim is always 25fps");
                try
                {
                    samples_max = int.Parse(Console.ReadLine());
                }
                catch(Exception)
                {
                    Console.WriteLine("Invalid..");
                    return;
                }

                var pr = p[index];
                var reader = new MemoryWriter();
                reader.ReadProcess = pr;
                reader.OpenProcess();
                var addr_base = pr.MainModule.BaseAddress;

                MapSizeX = reader.ReadInt32(addr_base + 0x9D5730);
                MapSizeY = reader.ReadInt32(addr_base + 0x9D5724);
                tileStats = new TileStats[MapSizeX * MapSizeY];
                DateTime n = DateTime.Now;
                int samples = 0;
                do
                {
                    samples++;

                    var vehiclePtr = reader.ReadInt32(addr_base + 0xA1E828);
                    var vehicleCount = reader.ReadInt32(addr_base + 0xA1E810);

                    // what is 3FF476C8 @ vehicleListPtr?
                    // Ptr size -> 6E30 - 7010
                    var calls = 0;
                    var vehicleListCache = new int[vehicleCount*2+100]; // 50 extra for good measure.
                    var temp_array = reader.ReadBytes((IntPtr)vehiclePtr, (uint)vehicleListCache.Length * 4);
                    calls++;
                    for (int i = 0; i < vehicleCount+50; i++)
                    {
                        vehicleListCache[i] = BitConverter.ToInt32(temp_array, i*8);

                    }

                    var objects = 0;
                    var trainCnt = 0;
                    var locoCnt = 0;

                    while (trainCnt < vehicleCount)
                    {
                        try
                        {
                            int vehicleListPtr = 0;
                            if (objects < vehicleListCache.Length)
                                vehicleListPtr = vehicleListCache[objects];
                            else
                            {
                                vehicleListPtr = reader.ReadInt32(vehiclePtr + objects*8);
                                calls++;
                            }

                            if (vehicleListPtr != 0)
                            {
                                //var veh = reader.ReadInt32((IntPtr) vehicleListPtr);
                                var tile = reader.ReadInt32(vehicleListPtr + 0x58);
                                calls++;
                                if (tile >= tileStats.Length) break;
                                if (tile != 0)
                                {
                                    int spd = reader.ReadInt32(vehicleListPtr + 0xF4) >> 16;
                                    calls++;

                                    if (spd != 0)
                                    {
                                        locoCnt++;

                                        tileStats[tile].TrainsPassed++;
                                        tileStats[tile].SpeedSum += spd;
                                        tileStats[tile].SpeedMin = Math.Min(tileStats[tile].SpeedMin, spd);
                                        tileStats[tile].SpeedMax = Math.Min(tileStats[tile].SpeedMax, spd);
                                    }
                                }

                                trainCnt++;
                            }
                        }catch(Exception ex)
                        {

                        }
                        objects++;


                    }
                    Console.Clear();
                    var dt = DateTime.Now.Subtract(n);
                    var fps = samples / (dt.TotalMilliseconds / 1000.0);
                    Console.WriteLine(samples + "(" + Math.Round(fps, 2) + "fps / " + Math.Round(calls*fps) + " RPM Int32 / sec");
                    Console.WriteLine(trainCnt + " (" + objects + ") trains ingame");
                    Console.WriteLine(vehicleCount + " trains ingame");
                    if (fps> 25)
                        Thread.Sleep((int)Math.Floor(1000.0/25*2 - 1000.0/fps));
                } while (samples < samples_max);

                /*StringBuilder bl = new StringBuilder();
                for (int i = 0; i < tileStats.Length; i++)
                    bl.AppendLine(i + "," + tileStats[i].TrainsPassed);
                File.WriteAllText("test.csv", bl.ToString());*/
            }
            else
            {
                Console.WriteLine("Map X?");
                MapSizeX = int.Parse(Console.ReadLine());
                Console.WriteLine("Map Y?");
                MapSizeY = int.Parse(Console.ReadLine());

                tileStats = new TileStats[MapSizeX*MapSizeY];
                

                var data = File.ReadAllLines("test.csv");
                samples_max = 0;
                foreach(var l in data)
                {
                    var ls = l.Trim().Split(',');
                    var v = Int32.Parse(ls[1]);
                    tileStats[Int32.Parse(ls[0])] = new TileStats {TrainsPassed = v};
                    if(v < 1000)
                    samples_max = Math.Max(v, samples_max);
                }
            }

            Bitmap b = new Bitmap(MapSizeX, MapSizeY);
            Graphics g = Graphics.FromImage(b);
            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 200)), 0, 0, MapSizeX, MapSizeY);
            
            Dictionary<int, int> frequency = new Dictionary<int, int>();
            for (int i = 0; i < tileStats.Length; i++)
            {
                var j = tileStats[i];
                var p = j.TrainsPassed;
                if (frequency.ContainsKey(p))
                    frequency[p]++;
                else
                    frequency.Add(p, 1);
            }

            var max_value = 0;
            var max_freq = 0;
            StringBuilder freq_out = new StringBuilder();
            
            foreach(var kvp in frequency)
            {
                if (kvp.Key > 1000 || kvp.Value > 1000)
                continue;
                if (kvp.Value > 10 && kvp.Key > max_freq)
                {
                    max_value = kvp.Key;
                    max_freq = kvp.Value;

                }
            }

            max_value = samples_max;

            // Average speed: max_value =643
            max_value = 643;

            for (int x = 0; x < MapSizeX; x++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    int tile = x*MapSizeY + y;
                    if (tileStats[tile].TrainsPassed == 0) continue;

                    //var frac = tileStats[tile] *1.0 / max_value;
                    var frac = 1 - tileStats[tile].SpeedSum/tileStats[tile].TrainsPassed*1.0/max_value;
                    if (frac > 1) frac = 1;
                    if (frac < 0) frac = 0;
                    ColorRGB c = HSL2RGB(frac, 0.5, 0.5);

                    b.SetPixel(x, y, Color.FromArgb(c.R, c.G, c.B));

                }
            }

            var f = "heatmap.png";
            b.Save(f);

            // Legned:
            Bitmap legend = new Bitmap(max_value, 200);
            for (int i = 0; i < 643; i++)
            {
                for (int y = 0; y < 25; y++)
                {
                    var frac = 1-i*1.0/max_value;
                    ColorRGB c = HSL2RGB(frac, 0.5, 0.5);

                    legend.SetPixel(i, y, Color.FromArgb(c.R, c.G, c.B));
                }
            }

            Graphics g2 = Graphics.FromImage(legend);
            for (int i = 0; i < 650; i += 50)
            {
                var frac = i * 1.0 / max_value;
                g2.DrawLine(new Pen(Brushes.Black, 1), i, 25, i, 38);
                g2.DrawString(i.ToString(), new Font("Verdana", 10, FontStyle.Regular), Brushes.Black, i-3, 40 );
            }

                legend.Save("legend.png");
        }

        // Given H,S,L in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public static ColorRGB HSL2RGB(double h, double sl, double l)
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
            ColorRGB rgb;
            rgb.R = Convert.ToByte(r * 255.0f);
            rgb.G = Convert.ToByte(g * 255.0f);
            rgb.B = Convert.ToByte(b * 255.0f);
            return rgb;
        }
    }


}