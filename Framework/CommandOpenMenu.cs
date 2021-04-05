using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineAssist.Framework {
    class CommandOpenMenu: Command {
        public new static string name = "OpenMenu";

        public override void exec(Dictionary<string, string> par) {
            StardewWrap.openJournalMenu();
            isFinish = true;
        }
    }
}
