using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Triton.Memory;

namespace OpenTTDStatsLive
{
    public partial class TTDStats : Form
    {
        private Thread _mSampleThread;
        private Thread _mTTDThread;

        private MyMemoryReader _mTTD;
        private TTDMap mapControl;

        public int MapSizeX;
        public int MapSizeY;

        private long addr_base;

        public Bitmap bMap;

        private int SampleSpeed = 25;
        private int SamplePeriod = 30;

        public readonly List<TTDSample> Samples = new List<TTDSample>();
        public bool syncCamera = false;

        public int TileCameraX = 0;
        public int TileCameraY = 0;
        public int TileCameraW = 0;
        public int TileCameraH = 0;
        public bool drawSpeed;


        public TTDStats()
        {
            InitializeComponent();

            _mSampleThread = new Thread(Sampler);
            _mTTDThread = new Thread(TTDSeek);
            _mSampleThread.IsBackground = true;
            _mTTDThread.IsBackground = true;

            _mSampleThread.Start();
            _mTTDThread.Start();
            tb_sampleSpeed.Value = 25;
            tb_samplePeriod.Value = 30;

            cb_draw.Checked = true;

            split.SplitterDistance = 50;

            mapControl = new TTDMap(this);
            mapControl.Dock = DockStyle.Fill;
            split.Panel2.Controls.Add(mapControl);
        }

        private void tb_sampleSpeed_ValueChanged(object sender, System.EventArgs e)
        {
            lbl_SampleSpeed.Text = "Sample Speed: " + tb_sampleSpeed.Value + "Hz";
            SampleSpeed = tb_sampleSpeed.Value;
        }

        private void tb_period_ValueChanged(object sender, System.EventArgs e)
        {
            lbl_average.Text = "Averaging Period: " + tb_samplePeriod.Value + " seconds";
            SamplePeriod = tb_samplePeriod.Value;
        }
        public void TTDSeek()
        {
            var timer_count = 0;
            while(_mTTDThread.IsAlive)
            {
                if (_mTTD == null)
                {
                    var prs = Process.GetProcessesByName("openttd");
                    if (prs.Length == 1)
                    {
                        _mTTD = new MyMemoryReader { ReadProcess = prs[0] };
                        lock (_mTTD)
                        {
                            _mTTD.OpenProcess();
                            _mTTD.ReadProcess.Exited += new System.EventHandler(ReadProcess_Exited);

                            addr_base = (long)prs[0].MainModule.BaseAddress;
                        }
                    }
                }
                else
                {
                    var x = _mTTD.ReadInt32(new IntPtr(addr_base + 0x9D5730));
                    var y = _mTTD.ReadInt32(new IntPtr(addr_base + 0x9D5724));

                    var camera_basePtr1 = _mTTD.ReadInt32(new IntPtr(addr_base + 0xA1BB58));
                    var camera_base = _mTTD.ReadInt32(new IntPtr(camera_basePtr1 + 0x48));

                    TileCameraW = _mTTD.ReadInt32(new IntPtr(camera_base + 0x18)) /4/ 32;
                    TileCameraH = _mTTD.ReadInt32(new IntPtr(camera_base + 0x1C)) /4/ 32*2;

                    TileCameraY = _mTTD.ReadInt32(new IntPtr(camera_base + 0x14))/128;
                    TileCameraX = TileCameraY;

                    var temp_offset = _mTTD.ReadInt32(new IntPtr(camera_base + 0x10)) / 128;
                    TileCameraX += temp_offset / 2;
                    TileCameraY -= temp_offset / 2;

                    TileCameraY -= TileCameraH / 2;

                    if (TileCameraX < 0) TileCameraX = 0;
                    if (TileCameraY < 0) TileCameraY = 0;
                    if (TileCameraX+TileCameraW  > x) TileCameraX = x-TileCameraW;
                    if (TileCameraY+TileCameraH >y) TileCameraY = y-TileCameraH;

                    if (x != MapSizeX || y != MapSizeY)
                    {
                        MapSizeX = x;
                        MapSizeY = y;

                        // Update background
                        RenderMap();
                        timer_count = 0;

                        lock (Samples)
                        {
                            Samples.Clear();
                        }
                    }

                    timer_count++;
                    if(timer_count >= 600) // 10 seconds
                    {
                        timer_count = 0;
                        RenderMap();
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void RenderMap()
        {
            bMap = new Bitmap(MapSizeX, MapSizeY);


            // Capture tile array
            var Map = new TileInfo[MapSizeX*MapSizeY];
            var tilebase = _mTTD.ReadInt32(new IntPtr(addr_base + 0xA1BA68));

            for (int x = 0; x < MapSizeX; x++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    int d = x*MapSizeY + y;
                    Map[d] = new TileInfo(_mTTD.ReadUInt64(tilebase + d*8));
                }
            }

            // Render onto bitmap
            // TODO: Take camera into account?
            var waterColor = Color.FromArgb(100, 100, 200);
            var heightColors = new Color[16];
            for (int i = 0; i < 16; i++)
                heightColors[i] = Color.FromArgb((16 - i)*3, 100 + i*7, 100);

            lock(bMap)
            {
            for (var x = 0; x < MapSizeX; x++)
            {
                for (var y = 0; y < MapSizeY; y++)
                {
                    int tile = x*MapSizeY + y;
                    var tileinfo = Map[tile];
                    if (tileinfo.IsWater)
                    {
                        bMap.SetPixel(x, y, waterColor);
                    }
                    else
                    {
                        bMap.SetPixel(x, y, heightColors[tileinfo.Height]);
                    }

                }
            }

            }

        }

        private void Sampler()
        {
            var count = 0;

            while(_mSampleThread.IsAlive)
            {
                var periodTime =1000.0/SampleSpeed;

                DateTime n = DateTime.Now;

                // Do sampling here.

                if (_mTTD != null)
                {
                    TTDSample sample = new TTDSample(_mTTD, addr_base);
                    //

                    lock (Samples)
                    {
                        while (Samples.Count >= SampleSpeed*SamplePeriod)
                            Samples.RemoveAt(0);
                        Samples.Add(sample);
                    }
                }

                count++;
                if (count >= SampleSpeed)
                {
                    count = 0;
                    try
                    {
                        this.Invoke(new EventHandler(UpdateStatus), new object[2] { null, null });
                        mapControl.Invalidate();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                var dt = DateTime.Now.Subtract(n);

                Thread.Sleep(Math.Max(1,Convert.ToInt32(periodTime-dt.TotalMilliseconds)));
            }
        }

        private void UpdateStatus(object sender, EventArgs e)
        {
            if (_mTTD == null)
                lbl_status.Text = "No OpenTTD found";
            else
            {
                var trains = 0;
                if (Samples.Count > 0)
                    trains = Samples[0].Trains.Count;

                var bufferIndex = Samples.Count*100/SampleSpeed/SamplePeriod;
                if (bufferIndex < 0) bufferIndex = 0;
                if (bufferIndex > 100) bufferIndex = 100;

                buffer.Value = bufferIndex;

                lbl_status.Text = "Found TTD; " + trains + " trains";
            }
        }

        private void ReadProcess_Exited(object sender, EventArgs e)
        {
            lock (_mTTD)
            {
                _mTTD.CloseHandle();
                _mTTD = null;
            }
            this.Invoke(new EventHandler(UpdateStatus), new object[2] { sender, e});
        }

        private void cb_Camera_CheckedChanged(object sender, EventArgs e)
        {
            syncCamera = cb_Camera.Checked;
        }

        private void cb_draw_CheckedChanged(object sender, EventArgs e)
        {
            drawSpeed = cb_draw.Checked;
        }
    }
}
