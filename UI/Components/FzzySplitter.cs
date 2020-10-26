using LiveSplit.ASL;
using LiveSplit.ComponentUtil;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FzzyTools.UI.Components
{
    class FzzySplitter
    {

        private FzzyComponent fzzy;

        public FzzySplitter(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        private bool isLoading;
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

        public void Tick()
        {

            var settings = fzzy.Settings.aslsettings.Reader;
            Update(settings);
            if (fzzy.state.CurrentPhase == LiveSplit.Model.TimerPhase.Running || fzzy.state.CurrentPhase == LiveSplit.Model.TimerPhase.Paused)
            {
                fzzy.state.IsGameTimePaused = IsLoading(settings);
                if (Reset(settings))
                {
                    fzzy.timer.Reset();
                }
                if (Split(settings))
                {
                    fzzy.timer.Split();
                }
            }
            if (fzzy.state.CurrentPhase == LiveSplit.Model.TimerPhase.NotRunning)
            {
                if (Start(settings))
                {
                    fzzy.timer.Start();
                }
            }
        }

        private bool Reset(ASLSettingsReader settings)
        {
            if (settings["loadReset"] && isLoadingOld && !isLoading)
            {
                return true;
            }
            return false;
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
            if (isLoading)
            {
                bnrIlPause = false;
                enc3IlPause = false;
            }
            if (settings["BnRpause"])
            {
                if (fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1 &&
                    fzzy.values["level"].Current == "sp_sewers1" && fzzy.values["x"].Current > -9000)
                    bnrIlPause = true;
                if (bnrIlPause) return true;
            }
            if (settings["enc3pause"])
            {
                if (fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1 &&
                    fzzy.values["level"].Current == "sp_hub_timeshift" && fzzy.values["y"].Current > 4000) enc3IlPause = true;
                if (enc3IlPause) return true;
            }
            return isLoading;
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

        private void Update(ASLSettingsReader settings)
        {
            if (isLoading)
            {
                splitTimer = 0;

                bnrdoorsplit = false;
                hellroomsplit = false;

                bnrIlPause = false;
                enc3IlPause = false;

                arkElevatorSplit = false;
                arkKnifeSplit = false;
            }

            isLoadingOld = isLoading;
            isLoading = fzzy.values["clFrames"].Current <= 0 || fzzy.values["thing"].Current == 0;
        }

        private bool isLoadingOld;
        private int splitTimerTimestamp;
        private int splitTimer;
        private int b2buttonTimestamp;
        private bool hellroomsplit;
        private bool bnrdoorsplit;
        private bool enc2Dialogue;
        private bool arkElevatorSplit;
        private bool arkKnifeSplit;

        private bool Split(ASLSettingsReader settings)
        {
            if (settings["flagSplit"] && fzzy.values["flag"].Old == 1 && fzzy.values["flag"].Current == 0) return true;

            if (settings["helmetSplit"] && (fzzy.values["menuText"].Current.StartsWith("Found ") || fzzy.values["menuText"].Current.StartsWith("尋獲 ")) &&
                fzzy.values["menuText"].Current != fzzy.values["menuText"].Old) return true;

            // This is used for delaying splits
            var timePassed = Environment.TickCount - splitTimerTimestamp;
            splitTimerTimestamp = Environment.TickCount;
            if (splitTimer > 0)
            {
                var adjustment = splitTimer - timePassed;
                if (adjustment <= 0)
                {
                    splitTimer = 0;
                    return true;
                }
                else
                {
                    splitTimer = adjustment;
                }
            }

            // End of game
            if (fzzy.values["level"].Current == "sp_skyway_v1" && X < -10000 && Z > 0 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1 && settings["endSplit"])
            {
                return true;
            }

            //Level change
            if (fzzy.values["level"].Current != fzzy.values["level"].Old && settings["levelChangeSplit"])
            {
                return true;
            }

            // BT-7274
            if (fzzy.values["level"].Current == "sp_crashsite" && settings["btSplits"])
            {

                //Battery 1
                if (DistanceSquared(-4568, -3669) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    return true;
                }

                //Battery 2
                if (DistanceSquared(-4111, 4583) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    return true;
                }
            }

            // Blood and Rust
            if (fzzy.values["level"].Current == "sp_sewers1" && settings["bnrSplits"])
            {

                // Button 1
                if (fzzy.values["bnrbutton1"].Old == 0 && fzzy.values["bnrbutton1"].Current > 0 && !isLoading)
                {
                    return true;
                }

                // Door trigger
                if (Y <= -226 && X <= -827 && Z > 450 && !bnrdoorsplit && !isLoading)
                {
                    bnrdoorsplit = true;
                    return true;
                }

                // Button 2
                if (fzzy.values["bnrbutton2"].Old + 8 == fzzy.values["bnrbutton2"].Current && !isLoading)
                {
                    return true;
                }

                // BT embark
                if (fzzy.values["embarkCount"].Old == 0 && fzzy.values["embarkCount"].Current == 1)
                {
                    return true;
                }
            }

            //Embark on ITA3
            if (fzzy.values["level"].Current == "sp_boomtown_end" && settings["ita3Splits"])
            {
                if (fzzy.values["embarkCount"].Old == 0 && fzzy.values["embarkCount"].Current == 1)
                {
                    return true;
                }
            }

            //Helmet on E&C1
            if (fzzy.values["level"].Current == "sp_hub_timeshift" && settings["enc1Splits"])
            {
                if (DistanceSquared(997, -2718) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    splitTimer = 1800;
                }
            }

            //E&C 2
            if (fzzy.values["level"].Current == "sp_timeshift_spoke02" && settings["enc2Splits"])
            {

                // Dialogue
                if (X > 8755 && X < 9655 && Y < -4528 && Z > 5000)
                {
                    if (!enc2Dialogue)
                    {
                        splitTimer = 3000;
                        enc2Dialogue = true;
                    }
                }
                else if (isLoading)
                    enc2Dialogue = false;

                // Button 2
                if (DistanceSquared(2805, -3363) < Math.Pow(200, 2) && fzzy.values["enc2button1"].Old + 8 == fzzy.values["enc2button1"].Current && !isLoading)
                {
                    return true;
                }

                // Button 3
                if (DistanceSquared(6271, -3552) < Math.Pow(200, 2) && fzzy.values["enc2button2"].Old + 8 == fzzy.values["enc2button2"].Current && !isLoading)
                {
                    return true;
                }

                // Hellroom
                if (DistanceSquared(10708, -2263) < 15000 && !hellroomsplit && !isLoading)
                {
                    hellroomsplit = true;
                    return true;
                }
            }

            // Beacon 2
            if (fzzy.values["level"].Current == "sp_beacon_spoke0" && settings["b2Splits"])
            {

                // Death warp
                var warpX = fzzy.values["x"].Old - X;
                var warpY = fzzy.values["y"].Old - Y;
                var warpDistanceSquared = warpX * warpX + warpY * warpY;
                if (DistanceSquared(4019, 4233) < 500 && warpDistanceSquared > 20000 && !isLoadingOld)
                {
                    return true;
                }

                // Button 1
                if (fzzy.values["b2button"].Current != fzzy.values["b2button"].Old && Environment.TickCount - b2buttonTimestamp > 1000)
                {
                    b2buttonTimestamp = Environment.TickCount;
                    if (DistanceSquared(2690, 10366) < Math.Pow(200, 2))
                    {
                        return true;
                    }
                }

                // Heatsink trigger
                if (fzzy.values["x"].Old > -2113 && X <= -2113 && Y < 11800 && Y > 10100)
                {
                    return true;
                }
            }

            // Beacon 3
            if (fzzy.values["level"].Current == "sp_beacon" && settings["b3Splits"])
            {

                // Module retrieve
                if (DistanceSquared(-10670, 9523) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    splitTimer = 1900;
                }

                //Module 2
                if (DistanceSquared(3797, -1905) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    splitTimer = 1850;
                }
            }

            //TBF Elevator
            if (fzzy.values["level"].Current == "sp_tday" && settings["tbfSplits"])
            {
                if (DistanceSquared(-7867, 2758) < Math.Pow(600, 2) && fzzy.values["z"].Current > 450 && fzzy.values["z"].Old <= 450)
                {
                    if (fzzy.values["velZ"].Current == 0)
                    {
                        return true;
                    }
                }
            }

            // The Ark
            if (fzzy.values["level"].Current == "sp_s2s" && settings["arkSplits"])
            {

                // Elevator
                if (!arkElevatorSplit && fzzy.values["arkElevator"].Old > 0 && fzzy.values["arkElevator"].Current == 0)
                {
                    splitTimer = 1600;
                    arkElevatorSplit = true;
                }

                // Knife
                if (DistanceSquared(-8021, -4567) < Math.Pow(1500, 2))
                {
                    if (!arkKnifeSplit && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                    {
                        arkKnifeSplit = true;
                        return true;
                    }
                }
            }

            // The Fold Weapon
            if (fzzy.values["level"].Current == "sp_skyway_v1" && settings["foldSplits"])
            {

                // Datacore
                if (DistanceSquared(5252, -5776) < 25000 && fzzy.values["inCutscene"].Old == 0 && fzzy.values["inCutscene"].Current == 1)
                {
                    splitTimer = 7950;
                }

                // Escape land
                if (DistanceSquared(535, 6549) < 25000 && fzzy.values["angle"].Old == 0 && fzzy.values["angle"].Current != 0)
                {
                    return true;
                }
            }
            return false;
        }

        private bool Start(ASLSettingsReader settings)
        {
            if (settings["flagSplit"] && fzzy.values["flag"].Current == 1 && fzzy.values["flag"].Old == 0) return true;
            if (fzzy.values["clFrames"].Old <= 0 && fzzy.values["clFrames"].Current > 0)
            {
                float threshold = (float)Math.Pow(500, 2);
                //Speedmod
                if (fzzy.values["level"].Current == "sp_training" && DistanceSquared(-7573, 375) < threshold)
                    return true;
                //Pilots Gauntlet
                if (fzzy.values["level"].Current == "sp_training" && DistanceSquared(10662, -10200) < threshold)
                    return true;
                //BT-7274
                if (fzzy.values["level"].Current == "sp_crashsite" && DistanceSquared(-13568, -14336) < threshold)
                    return true;
                //Blood and Rust
                if (fzzy.values["level"].Current == "sp_sewers1" && DistanceSquared(9075, -14415) < threshold)
                    return true;
                //ITA 1
                if (fzzy.values["level"].Current == "sp_boomtown_start" && DistanceSquared(13578, -8781) < threshold)
                    return true;
                //ITA 2
                if (fzzy.values["level"].Current == "sp_boomtown" && DistanceSquared(-4087, 11155) < threshold)
                    return true;
                //ITA 3
                if (fzzy.values["level"].Current == "sp_boomtown_end" && DistanceSquared(-15120, -5284) < threshold)
                    return true;
                //E&C 1
                if (fzzy.values["level"].Current == "sp_hub_timeshift" && DistanceSquared(910, -7112) < threshold)
                    return true;
                //E&C 2
                if (fzzy.values["level"].Current == "sp_timeshift_spoke02" && DistanceSquared(-251, -3350) < threshold)
                    return true;
                //E&C 3
                if (fzzy.values["level"].Current == "sp_hub_timeshift" && DistanceSquared(1388, -2737) < threshold)
                    return true;
                //Beacon 1
                if (fzzy.values["level"].Current == "sp_beacon" && DistanceSquared(14297, -10858) < threshold)
                    return true;
                //Beacon 2
                if (fzzy.values["level"].Current == "sp_beacon_spoke0" && DistanceSquared(-1088, -336) < threshold)
                    return true;
                //Beacon 3
                if (fzzy.values["level"].Current == "sp_beacon" && DistanceSquared(12360, -1008) < threshold)
                    return true;
                //TBF
                if (fzzy.values["level"].Current == "sp_tday" && DistanceSquared(593, -15557) < threshold)
                    return true;
                //The Ark
                if (fzzy.values["level"].Current == "sp_s2s" && DistanceSquared(-25, -15189) < threshold)
                    return true;
                //The Fold Weapon
                if (fzzy.values["level"].Current == "sp_skyway_v1" && DistanceSquared(-11646, -6724) < threshold)
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
