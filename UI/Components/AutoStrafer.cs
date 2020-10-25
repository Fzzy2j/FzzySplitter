using LiveSplit.ComponentUtil;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TitanfallAutosplitter.UI.Components
{
    class AutoStrafer
    {

        private FzzyComponent fzzy;

        public AutoStrafer(FzzyComponent fzzy)
        {
            this.fzzy = fzzy;
        }

        public void Tick()
        {
            if (fzzy.values["timescale"].Current >= 1) return;
            float velocity = (float)Math.Sqrt(Math.Pow(fzzy.values["velX"].Current, 2) + Math.Pow(fzzy.values["velY"].Current, 2));

            var movementOverTime = 50 - (500 * (fzzy.updateTimer.Interval / 1000f) * fzzy.values["timescale"].Current);
            if (fzzy.values["holdingD"].Current && 
                !fzzy.values["holdingA"].Current && 
                !fzzy.values["holdingW"].Current && 
                !fzzy.values["holdingS"].Current && velocity > 0.1f && fzzy.values["velZ"].Current != 0)
            {
                var x = fzzy.values["velX"].Current;
                var y = fzzy.values["velY"].Current;

                float offset = (float)((180 / Math.PI) * Math.Abs(Math.Asin(movementOverTime / velocity)));

                var yaw = (float)((180 / Math.PI) * -Math.Atan2(x, y)) + 90 + offset;
                fzzy.values["yaw"].Current = yaw;
            }

            if (fzzy.values["holdingA"].Current && 
                !fzzy.values["holdingD"].Current && 
                !fzzy.values["holdingW"].Current && 
                !fzzy.values["holdingS"].Current && velocity > 0.1f && fzzy.values["velZ"].Current != 0)
            {
                var x = fzzy.values["velX"].Current;
                var y = fzzy.values["velY"].Current;

                float offset = (float)((180 / Math.PI) * Math.Abs(Math.Asin(movementOverTime / velocity)));

                var yaw = (float)((180 / Math.PI) * -Math.Atan2(x, y)) + 90 - offset;
                fzzy.values["yaw"].Current = yaw;
            }
        }
    }
}
