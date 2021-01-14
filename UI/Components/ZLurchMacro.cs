using FzzyTools.UI.Components;
using LiveSplit.UI.Components;

namespace TitanfallAutosplitter.UI.Components
{
    class ZLurchMacro
    {

        private FzzyComponent fzzy;
        private bool _pressingW;

        public ZLurchMacro(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        public void Tick()
        {
            if (fzzy.values["timescale"].Current >= 1 || fzzy.aimbot.AimbotRunning) return;
            if (_pressingW)
            {
                fzzy.board.Unpress(Keyboard.ScanCodeShort.KEY_W);
                _pressingW = false;
            }
            else if (fzzy.values["holdingZ"].Current && 
                !fzzy.values["holdingD"].Current && 
                !fzzy.values["holdingW"].Current && 
                !fzzy.values["holdingS"].Current && 
                !fzzy.values["holdingA"].Current)
            {
                fzzy.board.Press(Keyboard.ScanCodeShort.KEY_W);
                _pressingW = true;
            }
        }

    }
}
