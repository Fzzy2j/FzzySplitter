using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FzzyTools.UI.Components
{
    class SpeedmodAutoLoader
    {

        private FzzyComponent fzzy;

        private Keyboard.ScanCodeShort _queuedKey = Keyboard.ScanCodeShort.F1;
        private int _queuedKeyDelay = 0;
        private int _previousTickCount;

        private bool _allowGauntletLoad = false;
        private bool _allowB3Load = false;

        public SpeedmodAutoLoader(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        public void Tick()
        {
            if (_queuedKeyDelay > 0)
            {
                _queuedKeyDelay -= Environment.TickCount - _previousTickCount;
                if (_queuedKeyDelay <= 0)
                {
                    fzzy.board.Send(_queuedKey);
                }
            }

            fzzy.values["airAcceleration"].Current = 10000;
            fzzy.values["maxHealth"].Current = 9000;
            fzzy.values["currentHealth"].Current = 9000;
            fzzy.values["airSpeed"].Current = 40;
            RemoveWallFriction();

            if (fzzy.values["clFrames"].Current <= 0)
            {
                _allowGauntletLoad = false;
            }
            if (fzzy.values["level"].Current == "sp_training")
            {
                if (DistanceSquared(880, 6770, 466) < 1000*1000) _allowGauntletLoad = true;

                float projection = 0.866049f * fzzy.values["x"].Current + 0.499959f * fzzy.values["y"].Current;

                if ((Math.Abs(projection + 3888.9) < 1 || Math.Abs(projection - 6622) < 1) && _allowGauntletLoad)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F2);
                    Log.Info("Load into BT");
                    _allowGauntletLoad = false;
                }
            }

            if (fzzy.values["level"].Current == "sp_crashsite")
            {
                if (DistanceSquared(-445, -383, 112) < 25)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F3);
                    Log.Info("Load into bnr");
                }
            }

            if (fzzy.values["level"].Current == "sp_sewers1")
            {
                if (DistanceSquared(-9138, -6732, 2605) < 500 * 500 && fzzy.values["inCutscene"].Current == 1)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F4);
                    Log.Info("Load into ita1");
                }
            }

            if (fzzy.values["level"].Current == "sp_boomtown")
            {
                float xDistance = fzzy.values["x"].Current - 8167;
                float yDistance = fzzy.values["y"].Current + 3583;
                double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
                if (distance < 76)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F5);
                    Log.Info("Load into ita3");
                }
            }

            if (fzzy.values["level"].Current == "sp_boomtown_end")
            {
                if (DistanceSquared(8644, 1097, -2621) < 7000*7000 && fzzy.values["inCutscene"].Current == 1)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F6);
                    Log.Info("Load into enc1");
                }
            }

            if (fzzy.values["level"].Current == "sp_hub_timeshift")
            {
                if (Math.Abs(fzzy.values["x"].Current - 1112.845) < 1 && Math.Abs(fzzy.values["y"].Current + 2741) < 100 && Math.Abs(fzzy.values["z"].Current + 859) < 1000)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F7);
                    Log.Info("Load into enc1");
                }
            }

            if (fzzy.values["level"].Current == "sp_hub_timeshift" &&
               fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
               DistanceSquared(-1108, 6017, -10596) < 1000*1000)
            {
                QueueKeypress(Keyboard.ScanCodeShort.F8, 6700);
                Log.Info("Delayed load into B1");
            }

            if (fzzy.values["level"].Current == "sp_beacon_spoke0")
            {
                if (fzzy.values["y"].Current > 3000) _allowB3Load = true;

                if (_allowB3Load &&
                    fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                    fzzy.values["y"].Current < -500)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F9);
                    Log.Info("Load into B3");
                    _allowB3Load = false;
                }
            }
            if (fzzy.values["clFrames"].Current <= 0)
            {
                _allowB3Load = false;
            }

            if (fzzy.values["level"].Current == "sp_beacon" &&
                fzzy.values["b3Fight"].Current > 0 &&
                fzzy.values["inCutscene"].Current == 2)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F10);
                Log.Info("Load into TBF");
            }

            if (fzzy.values["level"].Current == "sp_tday" &&
                DistanceSquared(6738, 12395, 2573) < 1000*1000 &&
                fzzy.values["inCutscene"].Current == 1)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F11);
                Log.Info("Load into Fold Weapon");
            }

            if (fzzy.values["level"].Current == "sp_skyway_v1" &&
                DistanceSquared(9023, 12180, 5693) < 1000*1000 &&
                fzzy.values["inCutscene"].Current == 1)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F12);
                Log.Info("Load into Escape");
            }

            _previousTickCount = Environment.TickCount;
        }

        private void QueueKeypress(Keyboard.ScanCodeShort key, int delay)
        {
            _queuedKey = key;
            _queuedKeyDelay = delay;
        }

        private float DistanceSquared(float x, float y, float z)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2) + Math.Pow(z - fzzy.values["z"].Current, 2);
            return dis;
        }

        private void Write(DeepPointer pointer, byte[] b)
        {
            pointer.DerefOffsets(FzzyComponent.process, out var ptr);
            FzzyComponent.process.WriteBytes(ptr, b);
        }

        private void RemoveWallFriction()
        {
            // X Friction
            Write(new DeepPointer("client.dll", 0x1FA2CF), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("client.dll", 0x1F9705), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("client.dll", 0x20CD14), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("client.dll", 0x20D6E5), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            Write(new DeepPointer("server.dll", 0x16BE9A), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("server.dll", 0x16AD35), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("server.dll", 0x1852FB), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("server.dll", 0x185D36), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            // Y Friction
            Write(new DeepPointer("client.dll", 0x1FA2E7), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("client.dll", 0x1F971D), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("client.dll", 0x20CD28), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("client.dll", 0x20D6F5), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });

            Write(new DeepPointer("server.dll", 0x16BEB2), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("server.dll", 0x16AD4D), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("server.dll", 0x185313), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
            Write(new DeepPointer("server.dll", 0x185D46), new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90 });
        }

        private void RestoreWallFriction()
        {
            // X Friction
            Write(new DeepPointer("client.dll", 0x1FA2CF), new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("client.dll", 0x1F9705), new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("client.dll", 0x20CD14), new byte[] { 0xF3, 0x0F, 0x11, 0x82, 0x8C, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("client.dll", 0x20D6E5), new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00 });

            Write(new DeepPointer("server.dll", 0x16BE9A), new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("server.dll", 0x16AD35), new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("server.dll", 0x1852FB), new byte[] { 0xF3, 0x0F, 0x11, 0x82, 0x8C, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("server.dll", 0x185D36), new byte[] { 0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00 });

            // Y Friction
            Write(new DeepPointer("client.dll", 0x1FA2E7), new byte[] { 0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("client.dll", 0x1F971D), new byte[] { 0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("client.dll", 0x20CD28), new byte[] { 0xF3, 0x0F, 0x11, 0x8A, 0x90, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("client.dll", 0x20D6F5), new byte[] { 0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00 });

            Write(new DeepPointer("server.dll", 0x16BEB2), new byte[] { 0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("server.dll", 0x16AD4D), new byte[] { 0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("server.dll", 0x185313), new byte[] { 0xF3, 0x0F, 0x11, 0x8A, 0x90, 0x00, 0x00, 0x00 });
            Write(new DeepPointer("server.dll", 0x185D46), new byte[] { 0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00 });
        }

    }
}
