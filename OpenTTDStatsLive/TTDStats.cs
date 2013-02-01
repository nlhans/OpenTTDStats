using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SimTelemetry.Domain.Memory;
using Triton;
using Triton.Maths;

namespace OpenTTDStatsLive
{
    public partial class TTDStats : Form
    {
        private Thread _mSampleThread;
        private Thread _mTTDThread;

        private MemoryProvider _mTTD;
        private TTDMap mapControl;

        public int MapSizeX;
        public int MapSizeY;

        private long addr_base;

        public Bitmap bMap;

        private int SampleSpeed = 25;
        private int SamplePeriod = 30;

        protected readonly List<TTDSample> Samples = new List<TTDSample>();
        public List<TTDSample> GetSamplesCopy()
        {
            List<TTDSample> samples2 = new List<TTDSample>(Samples);
            return samples2;
        }
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

            //cb_draw.Checked = true;

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

        private void RefreshCameraPool()
        {
            MemoryPool CameraPool;
            if (_mTTD.Contains("Camera"))
            {
                Dictionary<string, IMemoryObject> fields = _mTTD.Get("Camera").Fields;
                _mTTD.Remove(_mTTD.Get("Camera"));
                CameraPool = new MemoryPool("Camera", MemoryAddress.StaticAbsolute, _mTTD.Get("Map").ReadAs<int>("CameraBasePtr"), 0x100);

                foreach (var obj in fields)
                    CameraPool.Add(obj.Value);
            }
            else
            {
                CameraPool = new MemoryPool("Camera", MemoryAddress.StaticAbsolute, _mTTD.Get("Map").ReadAs<int>("CameraBasePtr"), 0x100);
                if (CameraPool.Address < 0x100000)
                    return;

                CameraPool.Add(new MemoryFieldSignature<int>("CameraY", MemoryAddress.Dynamic,  "8B78148958XX8948??85XX74", new int[0], 4));

                CameraPool.Add(new MemoryFieldSignature<int>("CameraW", MemoryAddress.Dynamic, "89XXXX8BXXXX3B46??0F", new int[0], 4));
                CameraPool.Add(new MemoryFieldSignature<int>("CameraH", MemoryAddress.Dynamic, "33DB3B5E??XXXX8B", new int[0], 4));

                CameraPool.Add(new MemoryFieldSignature<int>("CameraOffset", MemoryAddress.Dynamic, "8BXXXX8BXX8B46??2945XX", new int[0], 4));
            }
            _mTTD.Add(CameraPool);

        }

        private ManualResetEvent _mInitWait = new ManualResetEvent(false);

        public void TTDSeek()
        {
            while(_mTTDThread.IsAlive)
            {
                if (_mTTD == null)
                {
                    var prs = Process.GetProcessesByName("openttd");
                    if (prs.Length >= 1)
                    {
                        var reader = new MemoryReader();
                        reader.Open(prs[0]);
                        reader.Process.Exited += ReadProcess_Exited;
                        _mTTD = new MemoryProvider(reader);


                        addr_base = (long)prs[0].MainModule.BaseAddress;

                        _mTTD.Scanner.Enable();

                        // Initialize memorypool
                        MemoryPool MapData = new MemoryPool("Map", MemoryAddress.Static, 0, 0, 0);
                        //MapData.Add(new MemoryFieldSignature<int>("MapX", MemoryAddress.StaticAbsolute, "FFXXXXFF35XXXXXXXXFF35????????E8XXXXXXXX", new int[0], 4));
                        //MapData.Add(new MemoryFieldSignature<int>("MapY", MemoryAddress.StaticAbsolute, "FFXXXXFF35????????FF35XXXXXXXXE8XXXXXXXX", new int[0], 4));
                        MapData.Add(new MemoryFieldSignature<int>("MapY", MemoryAddress.StaticAbsolute, "A1????????8D50FF23D6D3EE", new int[0], 4));
                        MapData.Add(new MemoryFieldSignature<int>("MapX", MemoryAddress.StaticAbsolute, "8B0D????????2BC2", new int[0], 4));

                        //MapData.Add(new MemoryFieldSignature<int>("CameraBase", MemoryAddress.StaticAbsolute, "8BXXXXXXXXXX89XX????????85F674XX", new int[0], 4));
                        MapData.Add(new MemoryFieldSignature<int>("CameraBase", MemoryAddress.StaticAbsolute, "E8XXXXXXXX8B0D????????8BF085C9", new int[0], 4));
                        MapData.Add(new MemoryFieldFunc<int>("CameraBasePtr", (pool) => _mTTD.Reader.ReadInt32(MapData.ReadAs<int>("CameraBase") + 0x3C)));

                        MapData.Add(new MemoryFieldSignature<int>("TileBase", MemoryAddress.StaticAbsolute, "A1????????8AXXXX24XX3CXX", new int[0], 4));
                        MapData.Add(new MemoryFieldSignature<int>("VehicleBase", MemoryAddress.StaticAbsolute, "73XXA1????????8BXXXX3BXX0F84", new int[0], 4));
                        MapData.Add(new MemoryFieldSignature<int>("VehicleCount", MemoryAddress.StaticAbsolute, "85C075E5FF74240CE8XXXXXXXXCC8B15XXXXXXXX8B0DXXXXXXXX53568B35????????57EB0B833CB100", new int[0], 4));

                        _mTTD.Add(MapData);
                        _mTTD.Refresh();
                        RefreshCameraPool();
                        _mInitWait.Set();
                    }
                }
                Thread.Sleep(100);
            }
        }

        private byte[] rawTileData;

        private void RenderMap()
        {
            if (MapSizeX <= 0 || MapSizeY <= 0) return;
            bMap = new Bitmap(MapSizeX, MapSizeY);


            // Capture tile array
            var Map = new TileInfo[MapSizeX*MapSizeY];

            for (int x = 0; x < MapSizeX; x++)
            {
                for (int y = 0; y < MapSizeY; y++)
                {
                    int d = x*MapSizeY + y;
                    //Map[d] = new TileInfo(_mTTD.Reader.ReadUInt64(tilebase + d*8));
                    Map[d] = new TileInfo(MemoryDataConverter.Read<ulong>(rawTileData, d*8));
                }
            }
            rawTileData = new byte[0];

            // Render onto bitmap
            // TODO: Take camera into account?
            var waterColor = Color.FromArgb(100, 100, 200);
            var heightColors = new Color[16];
            for (int i = 0; i < 16; i++)
                heightColors[i] = Color.FromArgb((16 - i)*3, 100 + i*9, 50);

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

        private Filter AvgFps = new Filter(500);
        private Filter AvgLoad = new Filter(500);
        private void Sampler()
        {
            var count = 0;
            var timer_count = 0;

            _mInitWait.WaitOne();

            while (_mSampleThread.IsAlive)
            {
                if (_mTTD == null)
                {
                    Thread.Sleep(100);
                    continue;
                }
                _mTTD.Refresh();
                var periodTime = 1000.0/SampleSpeed;

                DateTime n = DateTime.Now;

                // Do sampling here.

                if (_mTTD != null)
                {

                    var MapData = _mTTD.Get("Map");
                    var x = MapData.ReadAs<int>("MapX");
                    var y = MapData.ReadAs<int>("MapY");

                    if (_mTTD.Contains("Camera"))
                    {
                        var CameraData = _mTTD.Get("Camera");
                        TileCameraW = CameraData.ReadAs<int>("CameraW")/4/32;
                        TileCameraH = CameraData.ReadAs<int>("CameraH")/4/32*2;
                        TileCameraY = CameraData.ReadAs<int>("CameraY")/128;
                        TileCameraX = TileCameraY;

                        var temp_offset = CameraData.ReadAs<int>("CameraOffset")/128;
                        TileCameraX += temp_offset/2;
                        TileCameraY -= temp_offset/2;

                        TileCameraY -= TileCameraH/2;

                        if (TileCameraX < 0) TileCameraX = 0;
                        if (TileCameraY < 0) TileCameraY = 0;
                        if (TileCameraX + TileCameraW > x) TileCameraX = x - TileCameraW;
                        if (TileCameraY + TileCameraH > y) TileCameraY = y - TileCameraH;
                    }
                    else
                        RefreshCameraPool();

                    if (x != MapSizeX || y != MapSizeY)
                    {
                        MapSizeX = x;
                        MapSizeY = y;

                        // Update background
                        var tilebase = MapData.ReadAs<int>("TileBase"); // _mTTD.Reader.ReadInt32(new IntPtr(addr_base + 0xA1BA68));
                        rawTileData = new byte[MapSizeX * MapSizeY * 8];
                        _mTTD.Reader.Read(tilebase, rawTileData);
                        this.Invoke(new AnonymousSignal(RenderMap));
                        timer_count = 0;
                        Samples.Clear();
                    }

                    timer_count++;
                    if (timer_count >= 10000/periodTime) // 10 seconds
                    {
                        timer_count = 0;
                        var tilebase = MapData.ReadAs<int>("TileBase"); // _mTTD.Reader.ReadInt32(new IntPtr(addr_base + 0xA1BA68));
                        rawTileData = new byte[MapSizeX * MapSizeY * 8];
                        _mTTD.Reader.Read(tilebase, rawTileData);
                        this.Invoke(new AnonymousSignal(RenderMap));
                    }

                    var sample = new TTDSample(_mTTD);
                    while (Samples.Count >= SampleSpeed*SamplePeriod)
                        Samples.RemoveAt(0);
                    Samples.Add(sample);
                }

                count++;
                if (count >= SampleSpeed)
                {
                    count = 0;
                    try
                    {
                        this.Invoke(new EventHandler(UpdateStatus), new object[2] {null, null});
                        mapControl.Invalidate();
                    }
                    catch (Exception ex)
                    {

                    }
                }
                var dt = DateTime.Now.Subtract(n);
                Thread.Sleep(Math.Max(1, Convert.ToInt32(periodTime - dt.TotalMilliseconds)));
                AvgFps.Add(DateTime.Now.Subtract(n).TotalMilliseconds);
                AvgLoad.Add(dt.TotalMilliseconds);
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

                lbl_status.Text = "Found TTD; " + trains + " trains; " + Math.Round(1000.0/AvgFps.Average,2)+"Hz/"+Math.Round((1-AvgLoad.Average/AvgFps.Average)*100.0,1)+"%";
            }
        }

        private void ReadProcess_Exited(object sender, EventArgs e)
        {
            lock (_mTTD)
            {
                _mTTD.Reader.Close();
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
