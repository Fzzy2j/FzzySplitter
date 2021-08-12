﻿using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace FzzyTools.UI.Components
{
    class Speedmod
    {

        private FzzyComponent fzzy;

        private long _previousTickCount;

        private bool _allowGauntletLoad = false;

        public Speedmod(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        private string _delayedLoadSave;
        private float _delayedLoadMillis;

        private void Load(string save, long delay = 0)
        {
            if (delay == 0)
            {
                Log.Info("Load into " + save);
                if (save == "speedmod9")
                {
                    fzzy.values["sp_unlocks_level_8"].Current = 0;
                }
                fzzy.RunGameCommand("load " + save + "; set_loading_progress_detente #INTROSCREEN_HINT_PC #INTROSCREEN_HINT_CONSOLE");
            }
            else
            {
                if (_delayedLoadMillis > 0) return;
                _delayedLoadSave = save;
                _delayedLoadMillis = delay;
            }
        }

        private string lastNonLoadLevel = "";
        private bool levelLoadedFromMenu = false;

        private long _previousTimestamp;
        public void Tick()
        {
            var timeElapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _previousTimestamp;
            if (_delayedLoadMillis > 0)
            {
                _delayedLoadMillis -= timeElapsed;
                if (_delayedLoadMillis <= 0)
                {
                    Load(_delayedLoadSave);
                }
            }
            _previousTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (!fzzy.Settings.SpeedmodEnabled)
            {
                if (IsSpeedmodEnabled())
                {
                    DisableSpeedmod();
                }
            }
            else
            {
                if (!IsSpeedmodEnabled())
                {
                    EnableSpeedmod();
                }
                if (!fzzy.isLoading)
                {
                    fzzy.values["airAcceleration"].Current = 10000f;
                    fzzy.values["airSpeed"].Current = 40f;
                    fzzy.values["lurchMax"].Current = 0f;
                    fzzy.values["slideStepVelocityReduction"].Current = 0f;
                    fzzy.values["slideBoostCooldown"].Current = 0f;
                }

                if (fzzy.values["inLoadingScreen"].Current && !fzzy.values["inLoadingScreen"].Old)
                {
                    levelLoadedFromMenu = lastNonLoadLevel == "";
                }
                if (!fzzy.values["inLoadingScreen"].Current) lastNonLoadLevel = fzzy.values["currentLevel"].Current;

                if (fzzy.values["currentLevel"].Current != fzzy.values["currentLevel"].Old && levelLoadedFromMenu)
                {
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_training")) Load("speedmod1");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_crashsite")) Load("speedmod2");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_sewers1")) Load("speedmod3");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_boomtown_start")) Load("speedmod4");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_boomtown_end")) Load("speedmod5");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_timeshift_spoke02")) Load("speedmod7");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_beacon_spoke0")) Load("speedmod8");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_beacon") && !fzzy.values["currentLevel"].Current.StartsWith("sp_beacon_spoke0")) Load("speedmod9");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_tday")) Load("speedmod10");
                    if (fzzy.values["currentLevel"].Current.StartsWith("sp_skyway_v1")) Load("speedmod11");
                }

                if (fzzy.values["clFrames"].Current <= 0)
                {
                    _allowGauntletLoad = false;
                }
                if (fzzy.values["lastLevel"].Current == "sp_training" && !fzzy.isLoading)
                {
                    if (DistanceSquared(880, 6770, 466) < 1000 * 1000) _allowGauntletLoad = true;

                    float projection = 0.866049f * fzzy.values["x"].Current + 0.499959f * fzzy.values["y"].Current;

                    if ((Math.Abs(projection + 3888.9) < 1 || Math.Abs(projection - 6622) < 1) && _allowGauntletLoad)
                    {
                        Load("speedmod2");
                        _allowGauntletLoad = false;
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_crashsite" && !fzzy.isLoading)
                {
                    if (DistanceSquared(-3435, 4201, 2357) < 180 * 180)
                    {
                        Load("speedmod3");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_sewers1" && !fzzy.isLoading)
                {
                    if (DistanceSquared(-9138, -6732, 2605) < 500 * 500 && fzzy.values["inCutscene"].Current == 1)
                    {
                        Load("speedmod4");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_boomtown" && !fzzy.isLoading)
                {
                    /*var helmet1 = new MemoryValue("float", new DeepPointer("server.dll", 0x00C70748, new int[] { 0x10, 0x6D8, 0x250, 0x10, 0x698, 0x248, 0x5D0, 0x5A4 }));
                    if (helmet1.Current == -8871.5)
                    {
                        MoveHelmet(-2214.9f, 11966.58f, 2460.8f, "server.dll", 0x00C70748, new int[] { 0x10, 0x6D8, 0x250, 0x10, 0x698, 0x248, 0x5D0, 0x5A4 });
                    }

                    IntPtr ptr = MemoryValue.SigScan("43 00 00 00 40 87 F7 45 C0 70 AB 45 00 D4 47 45 00 00 00 00");
                    if (ptr != IntPtr.Zero)
                    {
                        var helmetVisualX = new MemoryValue("float", new DeepPointer(ptr + 0x04));
                        var helmetVisualY = new MemoryValue("float", new DeepPointer(ptr + 0x04 + 0x04));
                        var helmetVisualZ = new MemoryValue("float", new DeepPointer(ptr + 0x04 + 0x08));

                        var helmetHitboxX = new MemoryValue("float", new DeepPointer(ptr - 0x110));
                        var helmetHitboxY = new MemoryValue("float", new DeepPointer(ptr - 0x110 + 0x04));
                        var helmetHitboxZ = new MemoryValue("float", new DeepPointer(ptr - 0x110 + 0x08));

                        Log.Info("x: " + helmetVisualX.Current);
                        if (helmetVisualX.Current == 7920.90625f)
                        {
                            Log.Info("move");
                            helmetVisualX.Current = 8351.46f;
                            helmetVisualY.Current = 7640.13f;
                            helmetVisualZ.Current = 2220f;

                            helmetHitboxX.Current = 8351.46f;
                            helmetHitboxY.Current = 7640.13f;
                            helmetHitboxZ.Current = 2220f;
                        }
                    }*/
                    float xDistance = fzzy.values["x"].Current - 8167;
                    float yDistance = fzzy.values["y"].Current + 3583;
                    double distance = Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
                    if (distance < 76)
                    {
                        Load("speedmod5");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_boomtown_end" && !fzzy.isLoading)
                {
                    if (DistanceSquared(8644, 1097, -2621) < 7000 * 7000 && fzzy.values["inCutscene"].Current == 1 && fzzy.values["rodeo"].Current == fzzy.values["rodeo"].Old)
                    {
                        Load("speedmod7");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" && !fzzy.isLoading)
                {
                    if (Math.Abs(fzzy.values["x"].Current - 1112.845) < 1 && Math.Abs(fzzy.values["y"].Current + 2741) < 100 && Math.Abs(fzzy.values["z"].Current + 859) < 1000)
                    {
                        Load("speedmod7");
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" && !fzzy.isLoading &&
                   fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
                   DistanceSquared(-1108, 6017, -10596) < 1000 * 1000)
                {
                    Load("speedmod8");
                }

                if (fzzy.values["lastLevel"].Old == "sp_beacon_spoke0" && fzzy.values["lastLevel"].Current == "sp_beacon")
                {
                    Load("speedmod9");
                }

                if (fzzy.values["lastLevel"].Current == "sp_beacon" && !fzzy.isLoading)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        var baseAdr = 0x01064698;
                        var offset = 0x8 * i;
                        var health = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x4D4 }).Deref<int>(FzzyComponent.process);
                        if (health == 0) continue;
                        var isAlive = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x3B0 }).Deref<int>(FzzyComponent.process) == 16;

                        var type = new MemoryValue("string30", new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x70, 0x0 }));

                        if (type.Current == "npc_super_spectre")
                        {
                            var x = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x490 }).Deref<float>(FzzyComponent.process);
                            var y = new DeepPointer("server.dll", baseAdr, new int[] { offset, 0x494 }).Deref<float>(FzzyComponent.process);

                            var compareX = 2125;
                            var compareY = -2114;
                            var dis = Math.Sqrt(Math.Pow(x - compareX, 2) + Math.Pow(y - compareY, 2));

                            if (dis < 2500 && !isAlive && health != 3000)
                            {
                                Load("speedmod10", 500);
                            }
                        }
                    }
                    if (fzzy.values["sp_unlocks_level_8"].Current == 511)
                    {
                        Load("speedmod10", 500);
                    }
                }

                if (fzzy.values["lastLevel"].Current == "sp_tday" && !fzzy.isLoading &&
                    DistanceSquared(6738, 12395, 2573) < 1000 * 1000 &&
                    fzzy.values["inCutscene"].Current == 1)
                {
                    Load("speedmod11");
                }

                if (fzzy.values["lastLevel"].Current == "sp_skyway_v1" && !fzzy.isLoading &&
                    DistanceSquared(9023, 12180, 5693) < 1000 * 1000 &&
                    fzzy.values["inCutscene"].Current == 1)
                {
                    Load("speedmod12");
                }
            }

            _previousTickCount = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void MoveHelmet(float newX, float newY, float newZ, string module, int base_, params int[] offsets)
        {
            offsets[offsets.Length - 1] = 0x490;
            new MemoryValue("float", new DeepPointer(module, base_, offsets)).Current = -2214.9f;
            offsets[offsets.Length - 1] = 0x494;
            new MemoryValue("float", new DeepPointer(module, base_, offsets)).Current = 11966.58f;
            offsets[offsets.Length - 1] = 0x498;
            new MemoryValue("float", new DeepPointer(module, base_, offsets)).Current = 2460.8f;

            offsets[offsets.Length - 1] = 0x5A4;
            new MemoryValue("float", new DeepPointer(module, base_, offsets)).Current = -2214.9f;
            offsets[offsets.Length - 1] = 0x5A8;
            new MemoryValue("float", new DeepPointer(module, base_, offsets)).Current = 11966.58f;
            offsets[offsets.Length - 1] = 0x5AC;
            new MemoryValue("float", new DeepPointer(module, base_, offsets)).Current = 2460.8f;
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
            if (IsSpeedmodEnabled()) return;
            InstallSpeedmod(fzzy.Settings);
            try
            {
                RemoveWallFriction();
                MakeAlliesInvincible();
            }
            catch (Exception)
            {
            }
        }

        private static string speedmodSavesInstaller = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Respawn\\Titanfall2\\profile\\savegames\\installspeedmodsaves.exe");
        public static void InstallSpeedmod(FzzySettings settings)
        {
            if (FzzySettings.AreSpeedmodSavesInstalled()) return;
            if (downloadInProgress) return;
            using (WebClient webClient = new WebClient())
            {
                webClient.DownloadFileAsync(new Uri(FzzyComponent.SPEEDMOD_SAVES_INSTALLER_LINK), speedmodSavesInstaller);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(SpeedmodSavesDownloadCompleted);
                downloadInProgress = true;
            }
        }
        private static bool downloadInProgress = false;

        private static void SpeedmodSavesDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            downloadInProgress = false;
            var startInfo = new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetParent(speedmodSavesInstaller).FullName,
                FileName = speedmodSavesInstaller
            };
            Process.Start(startInfo);
        }

        public void DisableSpeedmod()
        {
            if (FzzyComponent.process == null) return;
            if (!IsSpeedmodEnabled()) return;
            try
            {
                fzzy.values["airAcceleration"].Current = 500f;
                fzzy.values["airSpeed"].Current = 60f;
                fzzy.values["lurchMax"].Current = 0.7f;
                fzzy.values["slideStepVelocityReduction"].Current = 10f;
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
            var code = new byte[] { 0x83, 0xBB, 0x10, 0x01, 0x00, 0x00, 0x03, 0x74, 0x02, 0x89, 0x3B, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3 };
            Write(new DeepPointer("server.dll", 0x43373A), code);
            var jmp = new byte[] { 0x74, 0x1E };
            Write(new DeepPointer("server.dll", 0x433725), jmp);
        }

        private void MakeAlliesKillable()
        {
            var code = new byte[] { 0x89, 0x3B, 0x48, 0x8B, 0x5C, 0x24, 0x30, 0x48, 0x83, 0xC4, 0x20, 0x5F, 0xC3, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC, 0xCC };
            Write(new DeepPointer("server.dll", 0x43373A), code);
            var jmp = new byte[] { 0x74, 0x15 };
            Write(new DeepPointer("server.dll", 0x433725), jmp);
        }

        public bool IsSpeedmodEnabled()
        {
            var speedmodCode = new DeepPointer("server.dll", 0x43373A, new int[] { }).Deref<byte>(FzzyComponent.process);
            return speedmodCode == 0x83;
        }

        private void RemoveWallFriction()
        {
            byte[] code = new byte[] {
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
                0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90, 0x90,
            };

            Write(new DeepPointer("client.dll", 0x20D6E5), code);
            Write(new DeepPointer("server.dll", 0x185D36), code);
        }

        private void RestoreWallFriction()
        {
            byte[] code = new byte[] {
                0xF3, 0x0F, 0x11, 0x81, 0x8C, 0x00, 0x00, 0x00, // movss [rcx+8C],xmm0
                0xF3, 0x0F, 0x59, 0x89, 0x90, 0x00, 0x00, 0x00, // mulss xmm1,[rcx+90]
                0xF3, 0x0F, 0x11, 0x89, 0x90, 0x00, 0x00, 0x00, // movss [rcx+90],xmm1
            };

            Write(new DeepPointer("client.dll", 0x20D6E5), code);
            Write(new DeepPointer("server.dll", 0x185D36), code);
        }

    }
}
