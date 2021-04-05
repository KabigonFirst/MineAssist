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
        private Dictionary<string, Command> m_commandCache = new Dictionary<string, Command>();

        public SequentialCommandExecutor(IModHelper helper) {
            Helper = helper;
            helper.Events.GameLoop.UpdateTicked += onUpdateTick;
            helper.Events.Display.Rendered += onRendered;
            IEnumerable<Command> objs = ReflectiveObjects.GetEachObjectsOfSubtype<Command>();
            foreach (var obj in objs) {
                try {
                    dynamic changedObj = Convert.ChangeType(obj, obj.GetType());
                    var nameMember = obj.GetType().GetField(nameof(Command.name));
                    string name = (string)nameMember.GetValue(null);
                    m_commandCache[name] = obj;
                } catch(Exception) {
                    ModEntry.m_instance.Monitor.Log($"Command Class {obj.GetType().Name} does not have a name member for config, use class name instead.", LogLevel.Debug);
                    m_commandCache[obj.GetType().Name] = obj;
                }
            }
            m_commandCache.ContainsKey("Craft");
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
        public bool tryTriggerNew(CmdCfg cfg) {
            if (!canTriggerNew()) {
                return false;
            }
            if (cfg is null) {
                ModEntry.m_instance.Monitor.Log("NULL cfg when trigger new command", LogLevel.Error);
                return false;
            }
            if (!m_commandCache.ContainsKey(cfg.cmd)) {
                ModEntry.m_instance.Monitor.Log("Unable to create command: " + cfg.cmd, LogLevel.Error);
                return false;
            }
            m_curCmd = m_commandCache[cfg.cmd]; //Command.create(cfg.cmd);
            //command is there so just overide and execute it
            m_curCmd.exec(cfg.par);
            return true;
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
