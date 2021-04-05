using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MineAssist.Framework {
    class CommandMove : Command {
        public new static string name = "Move";
        public new enum Paramter {
            Direction1,
            Direction2
        };
        StardewWrap.SDirection m_direction1;
        StardewWrap.SDirection m_direction2;
        StardewWrap.SDirection m_faceDirection;
        bool m_hasSecond;

        public override void exec(Dictionary<string, string> par) {
            StardewWrap.SDirection para;
            if (par.ContainsKey(Paramter.Direction1.ToString())) {
                Enum.TryParse<StardewWrap.SDirection>(par[Paramter.Direction1.ToString()], out m_direction1);
            } else {
                m_direction1 = StardewWrap.SDirection.UP;
            }
            if (par.ContainsKey(Paramter.Direction2.ToString())) {
                m_hasSecond = Enum.TryParse<StardewWrap.SDirection>(par[Paramter.Direction2.ToString()], out para);
                m_direction2 = para;
            } else {
                m_hasSecond = false;
            }
            if(m_direction1 == StardewWrap.SDirection.RIGHT || m_direction1 == StardewWrap.SDirection.LEFT) {
                m_faceDirection = m_direction1;
            } else if (m_hasSecond && (m_direction2 == StardewWrap.SDirection.RIGHT || m_direction2 == StardewWrap.SDirection.LEFT)) {
                m_faceDirection = m_direction2;
            } else {
                m_faceDirection = m_direction1;
            }
            StardewWrap.setMove(m_faceDirection, true);
            isFinish = false;
        }
        public override void update() {
            if (!isFinish) {
                StardewWrap.updateMove(m_direction1);
                if (m_hasSecond) {
                    StardewWrap.updateMove(m_direction2);
                }
            }
        }
        public override void end() {
            StardewWrap.setMove(m_faceDirection, false);
            base.end();
        }
    }
}
