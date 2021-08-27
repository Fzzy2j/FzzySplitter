using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

[assembly: ComponentFactory(typeof(FzzyComponentFactory))]

namespace LiveSplit.UI.Components
{
    class FzzyComponentFactory : IComponentFactory
    {
        public string ComponentName => "FzzyTools";

        public string Description => "Various Titanfall 2 Tools";

        public ComponentCategory Category => ComponentCategory.Control;

        public string UpdateName => ComponentName;

        public string XMLURL => UpdateURL + "update.LiveSplit.Titanfall2.xml";

        public string UpdateURL => "https://raw.githubusercontent.com/Fzzy2j/FzzySplitter/master/";

        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public IComponent Create(LiveSplitState state)
        {
            return new FzzyComponent(state);
        }
    }
}
