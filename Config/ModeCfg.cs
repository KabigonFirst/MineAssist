﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;

namespace MineAssist.Config {
    public class ModeCfg {
        public HashSet<SButton> modifyKeys { get; set; }
        public List<CmdCfg> cmds { get; set; }

        public ModeCfg(HashSet<SButton> mKeys, List<CmdCfg> cmds) {
            this.modifyKeys = mKeys;
            this.cmds = cmds;
        }

        public static string constructCmdKey(SButton modifyKey, SButton key) {
            return modifyKey + "+" + key;
        }

        public Dictionary<string, CmdCfg> getCmdDict() {
            Dictionary<string, CmdCfg> dict = new Dictionary<string, CmdCfg>();
            foreach (CmdCfg c in cmds) {
                dict[constructCmdKey(c.modifyKey, c.key)] = c;
            }
            return dict;
        }
    }
}
