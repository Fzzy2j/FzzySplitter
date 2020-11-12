using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FzzyTools.UI.Components
{
    class Speedmod
    {

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        private FzzyComponent fzzy;

        private int _previousTickCount;

        private bool _allowGauntletLoad = false;
        private bool _allowB3Load = false;

        private string cfg;

        public Speedmod(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
            try
            {
                this.cfg = Path.Combine(FzzyComponent.GetTitanfallInstallDirectory(), "r2\\cfg\\autosplitter.cfg");
            }
            catch (Exception)
            {
            }
        }

        private int loadTimestamp;

        private void Load(string save)
        {
            if (Environment.TickCount - loadTimestamp > 5000)
            {
                loadTimestamp = Environment.TickCount;
                Log.Info("Load into " + save);
                RunCommand("load " + save);
            }
        }

        private int loadingTimestamp = -1;

        public void Tick()
        {
            if (!fzzy.Settings.Speedmod || !fzzy.values["f12Bind"].Current.StartsWith("exec autosplitter.cfg"))
            {
                if (!fzzy.isLoading)
                {
                    if (fzzy.values["airAcceleration"].Current != 500f)
                    {
                        DisableSpeedmod();
                    }
                }
            }
            else
            {
                if (!fzzy.isLoading)
                {
                    if (fzzy.values["airAcceleration"].Current != 10000f)
                    {
                        EnableSpeedmod();
                    }
                }

                if (fzzy.values["speedmodLevel"].Current == "sp_training")
                {
                    if (!fzzy.values["speedmodLoading"].Current)
                    {
                        loadingTimestamp = -1;
                    }
                    else if (loadingTimestamp == -1)
                    {
                        loadingTimestamp = Environment.TickCount;
                    }
                    if (fzzy.values["speedmodLoading"].Current && Environment.TickCount - loadingTimestamp > 1000)
                    {
                        Load("speedmod1");
                    }
                }

                if (fzzy.values["clFrames"].Current <= 0)
                {
                    _allowGauntletLoad = false;
                }
                if (fzzy.values["level"].Current == "sp_training")
                {

                    if (DistanceSquared(880, 6770, 466) < 1000 * 1000) _allowGauntletLoad = true;

                    float projection = 0.866049f * fzzy.values["x"].Current + 0.499959f * fzzy.values["y"].Current;

                    if ((Math.Abs(projection + 3888.9) < 1 || Math.Abs(projection - 6622) < 1) && _allowGauntletLoad)
                    {
                        Load("speedmod2");
                        _allowGauntletLoad = false;
                    }
                }

                if (fzzy.values["level"].Current == "sp_crashsite")
                {
                    if (DistanceSquared(-445, -383, 112) < 25)
                    {
                        Load("speedmod3");
                    }
                }

                if (fzzy.values["level"].Current == "sp_sewers1")
                {
                    if (DistanceSquared(-9138, -6732, 2605) < 500 * 500 && fzzy.values["inCutscene"].Current == 1)
                    {
                        Load("speedmod4");
                    }
                }

                if (fzzy.values["level"].Current == "sp_boomtown")
                {
                    float xDistance = fzzy.values["x"].Current - 8167;
                    float yDistance = fzzy.values["y"].Current + 3583;
                    double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
                    if (distance < 76)
                    {
                        Load("speedmod5");
                    }
                }

                if (fzzy.values["level"].Current == "sp_boomtown_end")
                {
                    if (DistanceSquared(8644, 1097, -2621) < 7000 * 7000 && fzzy.values["inCutscene"].Current == 1)
                    {
                        Load("speedmod7");
                    }
                }

                if (fzzy.values["level"].Current == "sp_hub_timeshift")
                {
                    if (Math.Abs(fzzy.values["x"].Current - 1112.845) < 1 && Math.Abs(fzzy.values["y"].Current + 2741) < 100 && Math.Abs(fzzy.values["z"].Current + 859) < 1000)
                    {
                        Load("speedmod7");
                    }
                }

                if (fzzy.values["level"].Current == "sp_hub_timeshift" &&
                   fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
                   DistanceSquared(-1108, 6017, -10596) < 1000 * 1000)
                {
                    Load("speedmod8");
                }

                if (fzzy.values["level"].Current == "sp_beacon_spoke0")
                {
                    if (fzzy.values["y"].Current > 3000) _allowB3Load = true;

                    if (_allowB3Load &&
                        fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                        fzzy.values["y"].Current < -500)
                    {
                        Load("speedmod9");
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
                    Load("speedmod10");
                }

                if (fzzy.values["level"].Current == "sp_tday" &&
                    DistanceSquared(6738, 12395, 2573) < 1000 * 1000 &&
                    fzzy.values["inCutscene"].Current == 1)
                {
                    Load("speedmod11");
                }

                if (fzzy.values["level"].Current == "sp_skyway_v1" &&
                    DistanceSquared(9023, 12180, 5693) < 1000 * 1000 &&
                    fzzy.values["inCutscene"].Current == 1)
                {
                    Load("speedmod12");
                }
            }

            _previousTickCount = Environment.TickCount;
        }

        private float DistanceSquared(float x, float y, float z)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2) + Math.Pow(z - fzzy.values["z"].Current, 2);
            return (float)dis;
        }

        private float DistanceSquared(float x, float y)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2);
            return (float)dis;
        }

        private void Write(DeepPointer pointer, byte[] b)
        {
            pointer.DerefOffsets(FzzyComponent.process, out var ptr);
            FzzyComponent.process.WriteBytes(ptr, b);
        }

        public void EnableSpeedmod()
        {
            if (FzzyComponent.process == null) return;
            try
            {
                fzzy.values["airAcceleration"].Current = 10000f;
                fzzy.values["airSpeed"].Current = 40f;
                fzzy.values["lurchMax"].Current = 0f;
                fzzy.values["slideStepVelocityReduction"].Current = 0f;
                fzzy.values["repelEnable"].Current = false;
                fzzy.values["slideBoostCooldown"].Current = 0f;
                RemoveWallFriction();
                MakeAlliesInvincible();
            }
            catch (Exception)
            {
            }
        }

        public void DisableSpeedmod()
        {
            if (FzzyComponent.process == null) return;
            try
            {
                fzzy.values["airAcceleration"].Current = 500f;
                fzzy.values["airSpeed"].Current = 60f;
                fzzy.values["lurchMax"].Current = 0.7f;
                fzzy.values["slideStepVelocityReduction"].Current = 10f;
                fzzy.values["repelEnable"].Current = true;
                fzzy.values["slideBoostCooldown"].Current = 2f;
                RestoreWallFriction();
                MakeAlliesKillable();
            }
            catch (Exception)
            {
            }
        }

        private void MakeAlliesInvincible()
        {
            foreach (ProcessModule m in FzzyComponent.process.Modules)
            {
                if (m.ModuleName == "server.dll")
                {
                    var code = new byte[] { 0x83, 0xBB, 0x10, 0x01, 0x00, 0x00, 0x03, 0x74, 0x02, 0x89, 0x3B, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3 };
                    FzzyComponent.process.WriteBytes(m.BaseAddress + 0x43373A, code);
                    var jmp = new byte[] { 0x74, 0x1E };
                    FzzyComponent.process.WriteBytes(m.BaseAddress + 0x433725, jmp);
                    break;
                }
            }
        }

        private void RunCommand(string cmd)
        {
            File.WriteAllText(cfg, cmd);
            Log.Info("running command: " + cmd);
            fzzy.board.Send(Keyboard.ScanCodeShort.F12);
        }

        private void MakeAlliesKillable()
        {
            foreach (ProcessModule m in FzzyComponent.process.Modules)
            {
                if (m.ModuleName == "server.dll")
                {
                    var code = new byte[] { 0x89, 0x3B, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC };
                    FzzyComponent.process.WriteBytes(m.BaseAddress + 0x43373A, code);
                    var jmp = new byte[] { 0x74, 0x15 };
                    FzzyComponent.process.WriteBytes(m.BaseAddress + 0x433725, jmp);
                    break;
                }
            }
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
