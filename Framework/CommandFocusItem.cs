using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineAssist.Framework {
    class CommandFocusItem : Command {
        public static string name = "FocusItem";
        public new enum Paramter {
            Offset,
            Index
        };

        public override void exec(Dictionary<string, string> par) {
            if (par.ContainsKey(Paramter.Index.ToString())) {
                int index = Convert.ToInt32(par[Paramter.Index.ToString()]);
                StardewWrap.setCurItemIndex(index);
            }
            if (par.ContainsKey(Paramter.Offset.ToString())) {
                int offset = Convert.ToInt32(par[Paramter.Offset.ToString()]);
                StardewWrap.swapCurrentItemOffset(offset);
            }
            isFinish = true;
        }
    }
}
