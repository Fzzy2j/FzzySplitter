using FzzyTools.UI.Components;
using LiveSplit.ComponentUtil;
using LiveSplit.Model.Input;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FzzyTools.UI.Components
{
    class TASTools
    {

        private FzzyComponent fzzy;

        private Dictionary<Keyboard.ScanCodeShort, long> pressedKeys = new Dictionary<Keyboard.ScanCodeShort, long>();

        private bool allowKick;

        private Thread tasThread;
        private bool tasRunning;
        public Dictionary<string, MemoryValue> tasValues = new Dictionary<string, MemoryValue>();

        public TASTools(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;

            tasValues["lean"] = new MemoryValue("float", new DeepPointer("client.dll", 0x216FC68, new int[] { }));
            tasValues["timescale"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x1315A2C8, new int[] { }));
            tasValues["velX"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C2C, new int[] { }));
            tasValues["velY"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C30, new int[] { }));
            tasValues["velZ"] = new MemoryValue("float", new DeepPointer("client.dll", 0xB34C34, new int[] { }));
            tasValues["holdingW"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C7D8, new int[] { }));
            tasValues["holdingA"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C678, new int[] { }));
            tasValues["holdingS"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C798, new int[] { }));
            tasValues["holdingD"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C6A8, new int[] { }));
            tasValues["holdingZ"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C80C, new int[] { }));
            tasValues["holdingB"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396C68C, new int[] { }));
            tasValues["holdingShift"] = new MemoryValue("bool", new DeepPointer("engine.dll", 0x1396CAB8, new int[] { }));
            tasValues["yaw"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00E69EA0, new int[] { 0x1E94 }));
            tasValues["pitch"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00E69EA0, new int[] { 0x1E90 }));
            tasValues["airSpeed"] = new MemoryValue("float", new DeepPointer("engine.dll", 0x13084248, new int[] { 0xEA8, 0x1008, 0x1038, 0x390, 0x48, 0x18, 0xA30, 0x10, 0x2218 }));
            tasValues["approachingWall"] = new MemoryValue("bool", new DeepPointer("client.dll", 0x00E69EA0, new int[] { 0x830, 0x12C8, 0x38 }));
            tasValues["onGround"] = new MemoryValue("bool", new DeepPointer("client.dll", 0x11EED78, new int[] { }));
            tasValues["viewThunkVertical"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00B188C0, new int[] { 0xD8, 0x1A24 }));
            tasValues["viewThunkHorizontal"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00B188C0, new int[] { 0xD8, 0x1A28 }));
            tasValues["recoilVertical"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00B188C0, new int[] { 0xD8, 0x1A3C }));
            tasValues["recoilHorizontal"] = new MemoryValue("float", new DeepPointer("client.dll", 0x00B188C0, new int[] { 0xD8, 0x1A40 }));
        }

        public bool IsStarted { get { return tasThread != null; } }

        public void Start()
        {
            if (tasThread != null) return;
            try
            {
                tasRunning = true;
                tasThread = new Thread(TASMain);
                tasThread.Start();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        public void Stop()
        {
            if (tasThread == null) return;
            tasRunning = false;
        }

        public void PressMovement(Keyboard.ScanCodeShort key)
        {
            if (pressedKeys.ContainsKey(key)) return;
            fzzy.board.Press(key);
            pressedKeys[key] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private const int MOVEMENT_KEY_PRESS_TIME = 8;

        private long zigzagMacro;
        private const int ZIGZAG_ALTERNATE_TIME = 84;

        public void Tick()
        {
            if (FzzyComponent.process == null || FzzyComponent.process.HasExited) return;
            if (tasValues["timescale"].Current >= 1 || fzzy.aimbot.AimbotRunning) return;

            float velocity = (float)Math.Sqrt(Math.Pow(tasValues["velX"].Current, 2) + Math.Pow(tasValues["velY"].Current, 2));
            var oldvelocity = Math.Sqrt(Math.Pow(tasValues["velX"].Old, 2) + Math.Pow(tasValues["velY"].Old, 2));

            var margin = 10;
            if (tasValues["holdingD"].Current &&
                !tasValues["holdingA"].Current &&
                !tasValues["holdingW"].Current &&
                !tasValues["holdingS"].Current && velocity > 0.1f && tasValues["velZ"].Current != 0)
            {
                var x = tasValues["velX"].Current;
                var y = tasValues["velY"].Current;

                float offset = (float)((180 / Math.PI) * Math.Abs(Math.Asin((tasValues["airSpeed"].Current - margin) / velocity)));

                var yaw = (float)((180 / Math.PI) * -Math.Atan2(x, y)) + 90 + offset;
                tasValues["yaw"].Current = yaw;
            }

            if (tasValues["holdingA"].Current &&
                !tasValues["holdingD"].Current &&
                !tasValues["holdingW"].Current &&
                !tasValues["holdingS"].Current && velocity > 0.1f && tasValues["velZ"].Current != 0)
            {
                var x = tasValues["velX"].Current;
                var y = tasValues["velY"].Current;

                float offset = (float)((180 / Math.PI) * Math.Abs(Math.Asin((tasValues["airSpeed"].Current - margin) / velocity)));

                var yaw = (float)((180 / Math.PI) * -Math.Atan2(x, y)) + 90 - offset;
                tasValues["yaw"].Current = yaw;
            }

            if (fzzy.Settings.SpeedmodEnabled)
            {
                if (tasValues["holdingZ"].Current &&
                    !tasValues["holdingW"].Current &&
                    !tasValues["holdingS"].Current)
                {
                    if (zigzagMacro > ZIGZAG_ALTERNATE_TIME)
                    {
                        fzzy.board.Press(Keyboard.ScanCodeShort.KEY_D);
                        fzzy.board.Unpress(Keyboard.ScanCodeShort.KEY_A);
                    }
                    else
                    {
                        fzzy.board.Press(Keyboard.ScanCodeShort.KEY_A);
                        fzzy.board.Unpress(Keyboard.ScanCodeShort.KEY_D);
                    }
                }
                if (!tasValues["holdingZ"].Current && tasValues["holdingZ"].Old)
                {
                    fzzy.board.Unpress(Keyboard.ScanCodeShort.KEY_D);
                    fzzy.board.Unpress(Keyboard.ScanCodeShort.KEY_A);
                }
            }
            else
            {
                if (tasValues["holdingZ"].Current && !tasValues["holdingZ"].Old)
                {
                    var x = tasValues["velX"].Current;
                    var y = tasValues["velY"].Current;

                    var yaw = (float)((180 / Math.PI) * -Math.Atan2(x, y)) + 90;
                    tasValues["yaw"].Current = yaw;
                }

                if (tasValues["holdingZ"].Current &&
                    !tasValues["holdingD"].Current &&
                    !tasValues["holdingW"].Current &&
                    !tasValues["holdingS"].Current &&
                    !tasValues["holdingA"].Current)
                {
                    PressMovement(Keyboard.ScanCodeShort.KEY_W);
                }
            }
            if (tasValues["holdingB"].Current &&
                !tasValues["holdingD"].Current &&
                !tasValues["holdingW"].Current &&
                !tasValues["holdingS"].Current &&
                !tasValues["holdingA"].Current)
            {
                PressMovement(Keyboard.ScanCodeShort.KEY_N);
                PressMovement(Keyboard.ScanCodeShort.KEY_W);
            }

            if (Math.Abs(tasValues["lean"].Current) > Math.Abs(tasValues["lean"].Old) && Math.Abs(tasValues["lean"].Current) > 12)
            {
                allowKick = true;
            }
            if (Math.Abs(tasValues["lean"].Current) < Math.Abs(tasValues["lean"].Old))
            {
                allowKick = false;
            }
            if (velocity < oldvelocity - 0.1f && velocity > 300 && !tasValues["holdingShift"].Current)
            {
                if (tasValues["onGround"].Current)
                {
                    PressMovement(Keyboard.ScanCodeShort.KEY_N);
                }
                if (allowKick && tasValues["approachingWall"].Current && !fzzy.Settings.SpeedmodEnabled)
                {
                    PressMovement(Keyboard.ScanCodeShort.KEY_N);
                }
            }
        }

        private long lastTick;

        public void TASMain()
        {
            while (tasRunning)
            {
                var deltaTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastTick;
                lastTick = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                zigzagMacro -= deltaTime;
                if (zigzagMacro <= 0)
                {
                    zigzagMacro = ZIGZAG_ALTERNATE_TIME * 2;
                }
                List<Keyboard.ScanCodeShort> removals = new List<Keyboard.ScanCodeShort>();
                foreach (KeyValuePair<Keyboard.ScanCodeShort, long> entry in pressedKeys)
                {
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - entry.Value > MOVEMENT_KEY_PRESS_TIME)
                    {
                        fzzy.board.Unpress(entry.Key);
                    }
                    if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - entry.Value > MOVEMENT_KEY_PRESS_TIME * 2)
                    {
                        removals.Add(entry.Key);
                    }
                }
                foreach (var key in removals)
                {
                    pressedKeys.Remove(key);
                }

                try
                {
                    Tick();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }

                foreach (MemoryValue value in tasValues.Values)
                {
                    value.EndTick();
                }
            }
            tasThread = null;
        }

        private float startYaw;
        private float startPitch;

        public void Draw()
        {
            var bitmap = new Bitmap("C:\\Users\\Fzzy\\Desktop\\draw\\credits.bmp");
            startYaw = tasValues["yaw"].Current;
            startPitch = tasValues["pitch"].Current;

            int sampleDensity = 1;
            float displayDensity = 1f;
            for (int y = 0; y < bitmap.Height; y += sampleDensity)
            {
                for (int x = 0; x < bitmap.Width; x += sampleDensity)
                {
                    Color pixel = bitmap.GetPixel(x, y);

                    var yawOffset = ((x - (bitmap.Width / 2)) / sampleDensity) * displayDensity;
                    var pitchOffset = ((y - (bitmap.Height / 2)) / sampleDensity) * displayDensity;

                    var pixelStart = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (pixel.R < 255)
                    {
                        var padding = (int)(30f / tasValues["timescale"].Current);
                        while (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - pixelStart < padding)
                        {
                            tasValues["yaw"].Current = (startYaw - yawOffset) - tasValues["viewThunkHorizontal"].Current - tasValues["recoilHorizontal"].Current;
                            tasValues["pitch"].Current = (startPitch + pitchOffset) - tasValues["viewThunkVertical"].Current - tasValues["recoilVertical"].Current;
                            tasValues["viewThunkHorizontal"].EndTick();
                            tasValues["viewThunkVertical"].EndTick();
                            tasValues["recoilHorizontal"].EndTick();
                            tasValues["recoilVertical"].EndTick();
                        }
                        fzzy.board.Send(Keyboard.ScanCodeShort.KEY_M);
                        Thread.Sleep(padding);
                    }
                }
            }
        }
    }
}
