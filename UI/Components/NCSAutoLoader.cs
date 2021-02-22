using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;
using System;

namespace FzzyTools.UI.Components
{
    class NCSAutoLoader
    {

        private bool allowB3Load = false;

        private int _gauntletTimestamp = 0;
        private int _arkTimestamp = 0;

        private int _btSpeak1Timestamp;
        private int _btSpeak2Timestamp;

        private Keyboard.ScanCodeShort _queuedKey = Keyboard.ScanCodeShort.F1;
        private int _queuedKeyDelay = 0;

        private int _previousTimestamp;

        private FzzyComponent fzzy;

        public NCSAutoLoader(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        public void Tick()
        {
            if (_queuedKeyDelay > 0)
            {
                _queuedKeyDelay -= Environment.TickCount - _previousTimestamp;
                if (_queuedKeyDelay <= 0)
                {
                    fzzy.board.Send(_queuedKey);
                }
            }
            _previousTimestamp = Environment.TickCount;

            if (fzzy.values["gauntletDialogue"].Current > fzzy.values["gauntletDialogue"].Old)
            {
                _gauntletTimestamp = Environment.TickCount;
            }

            if (fzzy.values["lastLevel"].Current == "sp_training" &&
                fzzy.values["radioSpeaking"].Current > fzzy.values["radioSpeaking"].Old &&
                fzzy.values["dialogue"].Current > fzzy.values["dialogue"].Old &&
                Environment.TickCount - _gauntletTimestamp < 500)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F1);
                Log.Info("Load into BT");
            }

            if (fzzy.values["lastLevel"].Current == "sp_sewers1" &&
                fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
                fzzy.values["rodeo"].Current == fzzy.values["rodeo"].Old &&
                DistanceSquared(-5472, -6726, 3202) < 4000 * 4000 &&
                DistanceSquared(-9425, -6770, 2589) > 400 * 400)
            {
                QueueKeypress(Keyboard.ScanCodeShort.F2, 5500);
                Log.Info("Delayed load into ITA1");
            }

            if (fzzy.values["lastLevel"].Current == "sp_hub_timeshift" &&
                fzzy.values["inCutscene"].Current == 1 && fzzy.values["inCutscene"].Old == 0 &&
                DistanceSquared(-1108, 6017, -10596) < 1000 * 1000)
            {
                QueueKeypress(Keyboard.ScanCodeShort.F3, 6700);
                Log.Info("Delayed load into B1");
            }

            if (fzzy.values["btSpeak2"].Current == 1 && fzzy.values["btSpeak2"].Old != 1)
            {
                _btSpeak2Timestamp = Environment.TickCount;
            }
            if (fzzy.values["btSpeak1"].Current > fzzy.values["btSpeak1"].Old)
            {
                _btSpeak1Timestamp = Environment.TickCount;
            }
            if (fzzy.values["lastLevel"].Current == "sp_beacon" &&
                Math.Abs(_btSpeak1Timestamp - _btSpeak2Timestamp) < 100 &&
                DistanceSquared(12432, -2463) < 1000 * 1000 &&
                fzzy.values["x"].Current > 11700 &&
                fzzy.values["isB1"].Current == 0)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F4);
                Log.Info("Load into B2");
            }

            if (fzzy.values["lastLevel"].Current == "sp_beacon_spoke0")
            {
                if (fzzy.values["y"].Current > 3000) allowB3Load = true;

                if (allowB3Load &&
                    fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                    fzzy.values["y"].Current < -500)
                {
                    fzzy.board.Send(Keyboard.ScanCodeShort.F5);
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
                fzzy.board.Send(Keyboard.ScanCodeShort.F6);
                Log.Info("Load into TBF");
            }

            if (fzzy.values["lastLevel"].Current == "sp_tday" &&
                fzzy.values["clFrames"].Current <= 0 && fzzy.values["clFrames"].Old > 0 &&
                DistanceSquared(4903, 13589, 2518) < 4000 * 4000)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F7);
                Log.Info("Load into Ark");
            }

            if (fzzy.values["arkDialogue"].Current > fzzy.values["arkDialogue"].Old)
            {
                _arkTimestamp = Environment.TickCount;
            }

            if (fzzy.values["lastLevel"].Current == "sp_s2s" &&
                fzzy.values["radioSpeaking"].Current > fzzy.values["radioSpeaking"].Old &&
                fzzy.values["dialogue"].Current > fzzy.values["dialogue"].Old &&
                Environment.TickCount - _arkTimestamp < 500 &&
                DistanceSquared(-2635, 7157, 7444) < 5000 * 5000)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F8);
                Log.Info("Load into Fold Weapon");
            }

            if (fzzy.values["lastLevel"].Current == "sp_skyway_v1" &&
                fzzy.values["inCutscene"].Current == 1 &&
                fzzy.values["x"].Current != fzzy.values["x"].Old &&
                DistanceSquared(7271, 13878, 5642) < 25)
            {
                fzzy.board.Send(Keyboard.ScanCodeShort.F9);
                Log.Info("Load into Escape");
            }
        }

        private void QueueKeypress(Keyboard.ScanCodeShort key, int delay)
        {
            _queuedKey = key;
            _queuedKeyDelay = delay;
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

    }
}
