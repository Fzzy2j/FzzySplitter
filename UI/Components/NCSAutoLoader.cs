using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;

namespace FzzyTools.UI.Components
{
    class NCSAutoLoader
    {
        private bool allowB3Load = false;

        private long gauntletTimestamp = 0;
        private long arkTimestamp = 0;

        private long btSpeak1Timestamp;
        private long btSpeak2Timestamp;

        private string queuedCommand = "load fastany1";
        private long queuedKeyDelay = 0;

        private long previousTimestamp;

        private FzzyComponent fzzy;

        public NCSAutoLoader(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        public void Tick()
        {
            if (fzzy.isLoading)
            {
                queuedKeyDelay = 0;
            }

            if (queuedKeyDelay > 0)
            {
                queuedKeyDelay -= DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - previousTimestamp;
                if (queuedKeyDelay <= 0)
                {
                    FzzyComponent.RunGameCommand(queuedCommand);
                }
            }

            previousTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (fzzy.values["gauntletDialogue"].Current > fzzy.values["gauntletDialogue"].Old)
            {
                gauntletTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            if (fzzy.values["lastLevel"].Current == "sp_training" &&
                fzzy.values["radioSpeaking"].Current > fzzy.values["radioSpeaking"].Old &&
                fzzy.values["dialogue"].Current > fzzy.values["dialogue"].Old &&
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - gauntletTimestamp < 500)
            {
                FzzyComponent.RunGameCommand("load fastany1");

                Log.Info("Load into BT");
            }

            if (fzzy.values["lastLevel"].Current == "sp_sewers1" &&
                fzzy.values["inCutscene"].Current == 1 && fzzy.values["pitch"].Current > 70 &&
                fzzy.values["rodeo"].Current == -1 &&
                DistanceSquared(-5472, -6726, 3202) < 4000 * 4000)
            {
                QueueCommand("load fastany2", 5300);

                Log.Info("Delayed load into ITA1");
            }

            if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" &&
                fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
                DistanceSquared(-1108, 6017, -10596) < 1000 * 1000)
            {
                QueueCommand("load fastany3", 6700);
                Log.Info("Delayed load into B1");
            }

            if (fzzy.values["btSpeak2"].Current == 1 && fzzy.values["btSpeak2"].Old != 1)
            {
                btSpeak2Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            if (fzzy.values["btSpeak1"].Current > fzzy.values["btSpeak1"].Old)
            {
                btSpeak1Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            if (fzzy.values["lastLevel"].Current == "sp_beacon" &&
                Math.Abs(btSpeak1Timestamp - btSpeak2Timestamp) < 100 &&
                DistanceSquared(12432, -2463) < 1000 * 1000 &&
                fzzy.values["x"].Current > 11700 &&
                fzzy.values["isB1"].Current == 0)
            {
                FzzyComponent.RunGameCommand("load fastany4");
                Log.Info("Load into B2");
            }

            if (fzzy.values["lastLevel"].Current == "sp_beacon_spoke0")
            {
                if (fzzy.values["y"].Current > 3000) allowB3Load = true;

                if (allowB3Load &&
                    fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                    fzzy.values["y"].Current < -500)
                {
                    FzzyComponent.RunGameCommand("load fastany5");
                    Log.Info("Load into B3");
                    allowB3Load = false;
                }
            }

            if (fzzy.values["clFrames"].Current <= 0)
            {
                allowB3Load = false;
            }

            if (fzzy.values["lastLevel"].Current == "sp_beacon" &&
                fzzy.values["b3Door"].Current - 8 == fzzy.values["b3Door"].Old &&
                DistanceSquared(11671, -2462) < Math.Pow(4000, 2) &&
                fzzy.values["x"].Current < 11700 &&
                fzzy.values["isB1"].Current != 0)
            {
                FzzyComponent.RunGameCommand("load fastany6");
                Log.Info("Load into TBF");
            }

            if (fzzy.values["lastLevel"].Current == "sp_tday" &&
                fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                DistanceSquared(4903, 13589, 2518) < 4000 * 4000)
            {
                FzzyComponent.RunGameCommand("load fastany7");
                Log.Info("Load into Ark");
            }

            if (fzzy.values["arkDialogue"].Current > fzzy.values["arkDialogue"].Old)
            {
                arkTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }

            if (fzzy.values["lastLevel"].Current == "sp_s2s" &&
                fzzy.values["radioSpeaking"].Current > fzzy.values["radioSpeaking"].Old &&
                fzzy.values["dialogue"].Current > fzzy.values["dialogue"].Old &&
                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - arkTimestamp < 500 &&
                DistanceSquared(-2635, 7157, 7444) < 5000 * 5000)
            {
                FzzyComponent.RunGameCommand("load fastany8");
                Log.Info("Load into Fold Weapon");
            }

            if (fzzy.values["lastLevel"].Current == "sp_skyway_v1" &&
                fzzy.values["inCutscene"].Current == 1 &&
                fzzy.values["x"].Current != fzzy.values["x"].Old &&
                DistanceSquared(7271, 13878, 5642) < 25)
            {
                FzzyComponent.RunGameCommand("load fastany9");
                Log.Info("Load into Escape");
            }
        }

        public void QueueCommand(string command, int delay)
        {
            if (queuedKeyDelay > 0 || fzzy.isLoading) return;
            queuedCommand = command;
            queuedKeyDelay = delay;
        }

        private float DistanceSquared(float x, float y, float z)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2) +
                      Math.Pow(z - fzzy.values["z"].Current, 2);
            return (float) dis;
        }

        private float DistanceSquared(float x, float y)
        {
            var dis = Math.Pow(x - fzzy.values["x"].Current, 2) + Math.Pow(y - fzzy.values["y"].Current, 2);
            return (float) dis;
        }
    }
}