using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using MineAssist.Framework;
using MineAssist.Config;

namespace MineAssist {
    class SequentialCommandExecutor {
        protected IModHelper Helper = null;
        private Command m_curCmd = null;

        public SequentialCommandExecutor(IModHelper helper) {
            Helper = helper;
            helper.Events.GameLoop.UpdateTicked += onUpdateTick;
            helper.Events.Display.Rendered += onRendered;
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void onRendered(object sender, RenderedEventArgs e) {
            if (m_curCmd != null && !m_curCmd.isFinish) {
                m_curCmd.updateGraphic();
                return;
            }
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void onUpdateTick(object sender, UpdateTickedEventArgs e) {
            //if last command is not finished, update it
            if (m_curCmd != null && !m_curCmd.isFinish) {
                m_curCmd.update();
                return;
            }
        }

        /// <summary>Create new command from config and execute. Caller should ensure it is proper time. Old command will be discarded.</summary>
        /// <param name="button">The input button.</param>
        public void triggerNew(CmdCfg p_cfg) {
            //now create the command
            CmdCfg cfg = p_cfg ?? throw new ArgumentNullException("cfg");
            m_curCmd = Command.create(cfg.cmd);
            if (m_curCmd == null) {
                ModEntry.m_instance.Monitor.Log("Unable to create command: " + cfg.cmd, LogLevel.Error);
                return;
            }
            //command is there so just overide and execute it
            m_curCmd.exec(cfg.par);
            return;
        }

        /// <summary>Perform proper end action. Finished command will not be affected</summary>
        public void tryStop() {
            if (m_curCmd != null && !m_curCmd.isFinish) {
                m_curCmd.end();
                m_curCmd = null;
            }
        }

        /// <summary>Check if new command is good to go.</summary>
        public bool canTriggerNew() {
            return !((m_curCmd != null && !m_curCmd.isFinish) || StardewWrap.isPlayerBusy());
        }

    }
}
