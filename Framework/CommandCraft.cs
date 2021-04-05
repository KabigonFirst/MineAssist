using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineAssist.Framework {
    class CommandCraft : Command {
        public new static string name = "Craft";
        public new enum Paramter {
            ItemName,
            ToPosition
        };

        public override void exec(Dictionary<string, string> par) {
            string itemName = "Staircase";
            int toPosition = -1;
            if (par.ContainsKey(Paramter.ItemName.ToString())) {
                itemName = par[Paramter.ItemName.ToString()];
            }
            if (par.ContainsKey(Paramter.ToPosition.ToString())) {
                toPosition = Convert.ToInt32(par[Paramter.ToPosition.ToString()]);
            }
            StardewWrap.fastCraft(itemName, toPosition);
            isFinish = true;
        }
    }
}
