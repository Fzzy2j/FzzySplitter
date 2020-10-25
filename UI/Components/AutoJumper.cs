using LiveSplit.ComponentUtil;
using LiveSplit.UI.Components;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TitanfallAutosplitter.UI.Components
{
    class AutoJumper
    {

        private FzzyComponent fzzy;

        private int _jumpTimestamp;
        private bool _pressingJump;

        public AutoJumper(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        public void Tick()
        {
            if (fzzy.values["timescale"].Current >= 1) return;
            float velocity = (float)Math.Sqrt(Math.Pow(fzzy.values["velX"].Current, 2) + Math.Pow(fzzy.values["velY"].Current, 2));
            var oldvelocity = Math.Sqrt(Math.Pow(fzzy.values["velX"].Old, 2) + Math.Pow(fzzy.values["velY"].Old, 2));

            if (_pressingJump)
            {
                fzzy.board.Unpress(Keyboard.ScanCodeShort.KEY_N);
                _pressingJump = false;
            }
            else
            {
                if (velocity < oldvelocity - 0.1f && velocity > 340 && !fzzy.values["holdingShift"].Current)
                {
                    if ((Math.Abs(fzzy.values["lean"].Current) > 10f && Math.Abs(fzzy.values["lean"].Current) < 30f && 
                        Environment.TickCount - _jumpTimestamp > 1000) || fzzy.values["velZ"].Current == 0)
                    {
                        _jumpTimestamp = Environment.TickCount;
                        fzzy.board.Press(Keyboard.ScanCodeShort.KEY_N);
                        _pressingJump = true;
                    }
                }
            }
        }
    }
}
