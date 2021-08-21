using LiveSplit.ASL;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FzzyTools.UI.Components
{
    class FzzySplitter
    {

        private FzzyComponent fzzy;

        public FzzySplitter(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }
        private bool bnrIlPause;
        private bool enc3IlPause;

        private float X
        {
            get
            {
                return fzzy.values["x"].Current;
            }
        }
        private float Y
        {
            get
            {
                return fzzy.values["y"].Current;
            }
        }
        private float Z
        {
            get
            {
                return fzzy.values["z"].Current;
            }
        }
        private bool ResetMessageShown;

        public void Tick()
        {
            if (fzzy.tasTools.tasValues["timescale"].Current != 1) return;

            var settings = fzzy.Settings.aslsettings.Reader;
            Update(settings);
            if (!fzzy.state.IsGameTimeInitialized) fzzy.timer.InitializeGameTime();
            if (fzzy.state.CurrentPhase == LiveSplit.Model.TimerPhase.Running || fzzy.state.CurrentPhase == LiveSplit.Model.TimerPhase.Paused)
            {
                fzzy.state.IsGameTimePaused = IsLoading(settings);
                if (Reset(settings))
                {
                    if (!ResetMessageShown)
                    {
                        var result = DialogResult.Yes;
                        if (fzzy.state.Settings.WarnOnReset)
                        {
                            ResetMessageShown = true;
                            result = WarnAboutResetting();
                        }
                        if (result == DialogResult.Yes)
                            fzzy.timer.Reset();
                        else if (result == DialogResult.No)
                            fzzy.timer.Reset(false);
                        ResetMessageShown = false;
                    }
                }
                Split(settings);
            }
            if (fzzy.state.CurrentPhase == LiveSplit.Model.TimerPhase.NotRunning)
            {
                if (finishedSplits.Count > 0)
                {
                    finishedSplits.Clear();
                }
                if (Start(settings))
                {
                    fzzy.timer.Start();
                }
            }
        }

        private List<string> finishedSplits = new List<string>();

        private void DoSingleSplit(string key, int delay = 0)
        {
            if (finishedSplits.Contains(key)) return;
            if (delay <= 0)
            {
                fzzy.timer.Split();
            }
            else
            {
                splitTimer = delay;
            }
            finishedSplits.Add(key);
        }

        private bool Reset(ASLSettingsReader settings)
        {
            if (fzzy.values["currentLevel"].Current.StartsWith("sp_training") && fzzy.values["inPressSpaceToContinue"].Current > 0 && fzzy.values["inPressSpaceToContinue"].Old <= 0)
            {
                return true;
            }
            if (settings["loadReset"] && fzzy.values["inPressSpaceToContinue"].Current > 0 && fzzy.values["inPressSpaceToContinue"].Old <= 0)
            {
                return true;
            }
            return false;
        }

        private DialogResult WarnAboutResetting()
        {
            var warnUser = false;
            for (var index = 0; index < fzzy.state.Run.Count; index++)
            {
                if (LiveSplitStateHelper.CheckBestSegment(fzzy.state, index, fzzy.state.CurrentTimingMethod))
                {
                    warnUser = true;
                    break;
                }
            }
            if (!warnUser && (fzzy.state.Run.Last().SplitTime[fzzy.state.CurrentTimingMethod] != null && fzzy.state.Run.Last().PersonalBestSplitTime[fzzy.state.CurrentTimingMethod] == null) || fzzy.state.Run.Last().SplitTime[fzzy.state.CurrentTimingMethod] < fzzy.state.Run.Last().PersonalBestSplitTime[fzzy.state.CurrentTimingMethod])
                warnUser = true;
            if (warnUser)
            {
                var result = MessageBox.Show("You have beaten some of your best times.\r\nDo you want to update them?", "Update Times?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                return result;
            }
            return DialogResult.Yes;
        }

        private bool IsLoading(ASLSettingsReader settings)
        {
            if (settings["flagSplit"])
            {
                if (fzzy.values["flag"].Current == 0)
                    return true;
                else
                    return false;
            }
            if (fzzy.isLoading)
            {
                bnrIlPause = false;
                enc3IlPause = false;
            }
            if (settings["BnRpause"])
            {
                if (fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1 &&
                    fzzy.values["lastLevel"].Current == "sp_sewers1" && fzzy.values["x"].Current > -9000)
                    bnrIlPause = true;
                if (bnrIlPause) return true;
            }
            if (settings["enc3pause"])
            {
                if (fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1 &&
                    fzzy.values["lastLevel"].Current == "sp_hub_timeshift" && fzzy.values["y"].Current > 4000) enc3IlPause = true;
                if (enc3IlPause) return true;
            }
            return fzzy.isLoading;
        }

        private void RemoveAltTabPause()
        {
            var deepClient = new DeepPointer("engine.dll", 0x1A1B04, new int[] { });
            deepClient.DerefOffsets(FzzyComponent.process, out IntPtr pointerClient);
            FzzyComponent.process.WriteBytes(pointerClient, new byte[] { 0x88, 0xA1 });

            var deepServer = new DeepPointer("engine.dll", 0x1C8C02, new int[] { });
            deepServer.DerefOffsets(FzzyComponent.process, out IntPtr pointerServer);
            FzzyComponent.process.WriteBytes(pointerServer, new byte[] { 0xEB });
        }

        private void AddAltTabPause()
        {
            var deepClient = new DeepPointer("engine.dll", 0x1A1B04, new int[] { });
            deepClient.DerefOffsets(FzzyComponent.process, out IntPtr pointerClient);
            FzzyComponent.process.WriteBytes(pointerClient, new byte[] { 0x88, 0x81 });

            var deepServer = new DeepPointer("engine.dll", 0x1C8C02, new int[] { });
            deepServer.DerefOffsets(FzzyComponent.process, out IntPtr pointerServer);
            FzzyComponent.process.WriteBytes(pointerServer, new byte[] { 0x75 });
        }

        private string lastNonLoadLevel = "";
        private bool levelLoadedFromMenu = false;

        private void Update(ASLSettingsReader settings)
        {
            fzzy.values["lastLevel"].Update();
            if (fzzy.values["inLoadingScreen"].Current && !fzzy.values["inLoadingScreen"].Old)
            {
                levelLoadedFromMenu = lastNonLoadLevel == "";
            }
            if (!fzzy.values["inLoadingScreen"].Current) lastNonLoadLevel = fzzy.values["currentLevel"].Current;
            if (fzzy.isLoading)
            {
                bnrIlPause = false;
                enc3IlPause = false;
            }
        }

        private long splitTimerTimestamp;
        private long splitTimer;

        private long btSaveDelay;
        private long previousTimestamp;

        private long lastLoadingTimestamp;

        private void Split(ASLSettingsReader settings)
        {
            if (settings["flagSplit"] && fzzy.values["flag"].Old == 1 && fzzy.values["flag"].Current == 0) fzzy.timer.Split();

            if (settings["helmetSplit"] && (fzzy.values["menuText"].Current.StartsWith("Found ") || fzzy.values["menuText"].Current.StartsWith("尋獲 ")) &&
                fzzy.values["menuText"].Current != fzzy.values["menuText"].Old) fzzy.timer.Split();

            if (fzzy.isLoading) lastLoadingTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var timePassed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - splitTimerTimestamp;
            splitTimerTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (splitTimer > 0)
            {
                var adjustment = splitTimer - timePassed;
                if (adjustment <= 0)
                {
                    splitTimer = 0;
                    fzzy.timer.Split();
                }
                else
                {
                    splitTimer = adjustment;
                }
            }

            if (btSaveDelay > 0)
            {
                btSaveDelay -= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - previousTimestamp;
                if (btSaveDelay <= 0)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F1);
                    fzzy.state.AdjustedStartTime -= new TimeSpan(0, 0, 3, 22, 217);
                }
                if (fzzy.isLoading) btSaveDelay = 0;
            }
            previousTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (fzzy.Settings.AutoLoad18HourSave && fzzy.values["lastLevel"].Current == "sp_crashsite" && DistanceSquared(68, -21, -12216) < 500 * 500)
            {
                if (fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0)
                {
                    btSaveDelay = 5050;
                }
            }

            // End of game
            if (fzzy.values["lastLevel"].Current == "sp_skyway_v1" && X < -10000 && Y > 0 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1 && settings["endSplit"])
            {
                DoSingleSplit("runEnd");
            }

            // Level change
            if (fzzy.values["lastLevel"].Current.Length > 0 && fzzy.values["lastLevel"].Current != fzzy.values["lastLevel"].Old && settings["levelChangeSplit"] && fzzy.values["lastLevel"].Current != "sp_training")
            {
                var level = fzzy.values["lastLevel"].Current;
                if (level != "sp_training")
                {
                    if (level == "sp_beacon" || level == "sp_hub_timeshift")
                    {
                        fzzy.timer.Split();
                    }
                    else
                    {
                        DoSingleSplit(level);
                    }
                }
            }

            // BT-7274
            if (fzzy.values["lastLevel"].Current == "sp_crashsite" && settings["btSplits"])
            {

                // Battery 1
                if (settings["btBattery1"])
                {
                    if (DistanceSquared(-4568, -3669) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        DoSingleSplit("btBattery1");
                    }
                }

                // Battery 2
                if (settings["btBattery2"])
                {
                    if (DistanceSquared(-4111, 4583) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        DoSingleSplit("btBattery2");
                    }
                }
            }

            // Blood and Rust
            if (fzzy.values["lastLevel"].Current == "sp_sewers1" && settings["bnrSplits"])
            {

                // Button 1
                if (settings["bnrButton1"])
                {
                    if (fzzy.values["bnrbutton1"].Old == 0 && fzzy.values["bnrbutton1"].Current > 0 && !fzzy.isLoading)
                    {
                        DoSingleSplit("bnrButton1");
                    }
                }

                // Door trigger
                if (settings["bnrDoor"])
                {
                    if (Y <= -226 && X <= -827 && Z > 450 && !fzzy.isLoading)
                    {
                        DoSingleSplit("bnrTrigger");
                    }
                }

                // Button 2
                if (settings["bnrButton2"])
                {
                    if (fzzy.values["bnrbutton2"].Old + 8 == fzzy.values["bnrbutton2"].Current && !fzzy.isLoading)
                    {
                        DoSingleSplit("bnrButton2");
                    }
                }

                // BT embark
                if (settings["bnrEmbark"])
                {
                    if (fzzy.values["embarkCount"].Old == 0 && fzzy.values["embarkCount"].Current == 1)
                    {
                        DoSingleSplit("bnrEmbark");
                    }
                }
            }

            // Embark on ITA3
            if (fzzy.values["lastLevel"].Current == "sp_boomtown_end" && settings["ita3Splits"])
            {
                if (fzzy.values["embarkCount"].Old == 0 && fzzy.values["embarkCount"].Current == 1)
                {
                    DoSingleSplit("ita3Embark");
                }
            }

            // Helmet on E&C1
            if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" && settings["enc1Splits"])
            {
                if (DistanceSquared(997, -2718) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    DoSingleSplit("enc1Helmet", 1800);
                }
            }

            // E&C 2
            if (fzzy.values["lastLevel"].Current == "sp_timeshift_spoke02" && settings["enc2Splits"])
            {

                if (settings["enc2Dialogue"])
                {
                    if (X > 8755 && X < 9655 && Y < -4528 && Z > 5000)
                    {
                        DoSingleSplit("enc2Dialogue", 3000);
                    }
                }

                // Button 1
                if (settings["enc2Button1"])
                {
                    if (DistanceSquared(2805, -3363) < Math.Pow(200, 2) && fzzy.values["enc2button1"].Old + 8 == fzzy.values["enc2button1"].Current && !fzzy.isLoading)
                    {
                        DoSingleSplit("enc2Button1");
                    }
                }

                // Button 2
                if (settings["enc2Button2"])
                {
                    if (DistanceSquared(6271, -3552) < Math.Pow(200, 2) && fzzy.values["enc2button2"].Old + 8 == fzzy.values["enc2button2"].Current && !fzzy.isLoading)
                    {
                        DoSingleSplit("enc2Button2");
                    }
                }

                // Hellroom
                if (settings["enc2Hellroom"])
                {
                    if (DistanceSquared(10708, -2263) < 15000 && !fzzy.isLoading)
                    {
                        DoSingleSplit("enc2Hellroom");
                    }
                }

                // Vents
                if (settings["enc2Vent"])
                {
                    if (fzzy.values["z"].Current < -1200 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        DoSingleSplit("enc2Vent");
                    }
                }
            }

            // Beacon 2
            if (fzzy.values["lastLevel"].Current == "sp_beacon_spoke0" && settings["b2Splits"])
            {

                // Death warp
                if (settings["b2Warp"])
                {
                    var warpX = fzzy.values["x"].Old - X;
                    var warpY = fzzy.values["y"].Old - Y;
                    var warpDistanceSquared = warpX * warpX + warpY * warpY;
                    if (DistanceSquared(4019, 4233) < 500 && warpDistanceSquared > 20000 && !fzzy.wasLoading)
                    {
                        DoSingleSplit("b2Warp");
                    }
                }

                // Button 1
                if (settings["b2Button1"])
                {
                    if (fzzy.values["b2button"].Current != fzzy.values["b2button"].Old)
                    {
                        if (DistanceSquared(2690, 10366) < Math.Pow(200, 2))
                        {
                            DoSingleSplit("b2Button1");
                        }
                    }
                }

                // Heatsink trigger
                if (settings["b2Trigger"])
                {
                    if (fzzy.values["x"].Old > -2113 && X <= -2113 && Y < 11800 && Y > 10100)
                    {
                        DoSingleSplit("b2Trigger");
                    }
                }
            }

            // Beacon 3
            if (fzzy.values["lastLevel"].Current == "sp_beacon" && settings["b3Splits"])
            {

                // Module retrieve
                if (settings["b3Module1"])
                {
                    if (DistanceSquared(-10670, 9523) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        DoSingleSplit("b3Module1", 1900);
                    }
                }

                // Module 2
                if (settings["b3Module2"])
                {
                    if (DistanceSquared(3797, -1905) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        DoSingleSplit("b3Module2", 1850);
                    }
                }

                // Secure beacon objective
                if (settings["b3SecureBeacon"])
                {
                    if (fzzy.values["b3SecureBeaconObjective"].Current != fzzy.values["b3SecureBeaconObjective"].Old && DistanceSquared(-3007, -1177) < 1000 * 1000)
                    {
                        DoSingleSplit("b3SecureBeacon");
                    }
                }
            }

            // TBF Elevator
            if (fzzy.values["lastLevel"].Current == "sp_tday" && settings["tbfSplits"])
            {
                if (DistanceSquared(-7867, 2758) < Math.Pow(600, 2) && fzzy.values["tbfElevator"].Current - 8 == fzzy.values["tbfElevator"].Old)
                {
                    DoSingleSplit("tbfElevator");
                }
            }

            // The Ark
            if (fzzy.values["lastLevel"].Current == "sp_s2s" && settings["arkSplits"])
            {

                // Elevator
                if (settings["arkElevator"] && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - lastLoadingTimestamp > 5000)
                {
                    if (fzzy.values["arkElevator"].Old > 0 && fzzy.values["arkElevator"].Current == 0)
                    {
                        DoSingleSplit("arkElevator", 1600);
                    }
                }

                // Knife
                if (settings["arkKnife"])
                {
                    if (DistanceSquared(-8021, -4567) < Math.Pow(1500, 2))
                    {
                        if (fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                        {
                            DoSingleSplit("arkKnife");
                        }
                    }
                }
            }

            // The Fold Weapon
            if (fzzy.values["lastLevel"].Current == "sp_skyway_v1" && settings["foldSplits"])
            {

                // Datacore
                if (settings["foldDataCore"])
                {
                    if (DistanceSquared(5252, -5776) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        splitTimer = 7950;
                    }
                }

                // Escape land
                if (settings["foldEscape"])
                {
                    if (DistanceSquared(535, 6549) < 25000 && fzzy.values["inCutscene"].Current == 1)
                    {
                        DoSingleSplit("escape");
                    }
                }
            }
        }

        private bool Start(ASLSettingsReader settings)
        {
            var last = fzzy.values["lastCommand"].Current;
            var nonull = new byte[last.Length];
            for (int i = 0; i < last.Length; i++)
            {
                nonull[i] = last[i];
                if (last[i] == 0) nonull[i] = 0x20;
            }
            var commandHistory = Encoding.UTF8.GetString(nonull);
            if (settings["flagSplit"] && fzzy.values["flag"].Current == 1 && fzzy.values["flag"].Old == 0) return true;
            if (fzzy.wasLoading && !fzzy.isLoading && commandHistory.Contains("#INTROSCREEN_HINT_PC"))
            {
                return true;
            }
            return false;
        }


        private float DistanceSquared(float x, float y, float z)
        {
            var dis = Math.Pow(x - X, 2) + Math.Pow(y - Y, 2) + Math.Pow(z - Z, 2);
            return (float)dis;
        }

        private float DistanceSquared(float x, float y)
        {
            var dis = Math.Pow(x - X, 2) + Math.Pow(y - Y, 2);
            return (float)dis;
        }


    }
}
