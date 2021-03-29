using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using MineAssist.Framework;
using MineAssist.Config;
using System.Collections;
using System;
using Microsoft.Xna.Framework;

namespace MineAssist {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {
        public static ModEntry m_instance;

        private AdjustedInputEvent m_inputEvent;
        private SequentialCommandExecutor m_executor;

        private ModConfig m_config;
        private string m_curMode = "Default";

        private SButton m_curModifyButton = SButton.None;
        private bool m_curModifyTriggered = false;
        private CmdCfg m_cachedCmdCfg = null;

        private Dictionary<string, CmdCfg> m_keyMap = null;
        private CmdCfg m_cmdcfg = null;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            //load config
            m_config = helper.ReadConfig<ModConfig>();
            if (m_config == null || m_config.modes == null) {
                this.Monitor.Log("Read configure file error.", LogLevel.Error);
                return;
            }
            //mod disabled, quit quickly
            if (!m_config.isEnable) {
                this.Monitor.Log("The mod is configreued disabled.", LogLevel.Info);
                return;
            }
            //no mode available
            if (m_config.modes.Count <= 0) {
                this.Monitor.Log("No modes found in configure file.", LogLevel.Info);
                return;
            }

            m_instance = this;
            m_executor = new SequentialCommandExecutor(helper);
            //determine default mode
            if (!m_config.modes.ContainsKey(m_curMode)) {
                IEnumerator enumerator = m_config.modes.Keys.GetEnumerator();
                enumerator.MoveNext();
                m_curMode = (string)enumerator.Current;
                this.Monitor.Log($"Default mode does not exists, use {m_curMode} Mode instead.", LogLevel.Error);
            }
            m_keyMap = m_config.getModeDict(m_curMode);

            //bind event handler
            m_inputEvent = new AdjustedInputEvent(helper, m_config.overrideTrigger, m_config.triggerClickedThreshold);
            m_inputEvent.ButtonPressed = buttonPressed;
            m_inputEvent.ButtonReleased = buttonReleased;
            helper.Events.GameLoop.UpdateTicked += onUpdateTick;
        }

        /*********
        ** Events
        *********/
        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void onUpdateTick(object sender, UpdateTickedEventArgs e) {
            tryExecuteCachedCommand();
        }

        /// <summary>Called when a button is really released.</summary>
        /// <param name="button">The button.</param>
        /// <returns>If a the release of buttion is processed</returns>
        protected bool buttonReleased(SButton button) {
#if DEBUG
            this.Monitor.Log($"Released {button}.", LogLevel.Debug);
#endif
            //for release event, we may not wait until play ready? unless, we need to trigger action in special case
            //if (!StardewWrap.isPlayerReady()) {
            //return false;
            //}

            //modify key tracking finished.
            if (m_curModifyButton == button) {
                m_curModifyButton = SButton.None;
                //special case when modify key is not used and it in binding map, execute it
                if (!m_curModifyTriggered) {
                    CmdCfg cfg = findCmdCfg(button);
                    if (!(cfg is null) && m_executor.canTriggerNew()) {
                        m_cmdcfg = cfg;
                        m_executor.triggerNew(cfg);
                        return true;
                    }
                }
                m_curModifyTriggered = false;
            }
            //if the released key is used in cached cmd, remove the cmd
            if (m_cachedCmdCfg != null && (m_cachedCmdCfg.modifyKey == button || m_cachedCmdCfg.key == button)) {
                m_cachedCmdCfg = null;
            }
            //if the released key is used in current cmd, stop the cmd
            if (m_cmdcfg != null && (button == m_cmdcfg.modifyKey || button == m_cmdcfg.key)) {
                m_executor.tryStop();
                m_cmdcfg = null;
                return true;
            }
            return false;
        }

        /// <summary>Called when a button is pressed.</summary>
        /// <param name="button">The button.</param>
        /// <returns>If a the release of buttion is processed</returns>
        protected bool buttonPressed(SButton button) {
#if DEBUG
            this.Monitor.Log($"Pressed {button}.", LogLevel.Debug);
#endif
            //process modify key
            if (isModifyKey(button)) {
                //allow only one modify key in key conbination so previous pressed modify key will be overriden
                m_curModifyButton = button;
                m_curModifyTriggered = false;
                return true;
            }

            //process action key, look up binding
            CmdCfg cfg = findCmdCfg(button);
            if (cfg is null) {
                return false;
            }
            //exist in binding, check if it is ready to create new command
            if (!m_executor.canTriggerNew()) {
                m_cachedCmdCfg = cfg; //record the key to see if we can invoke it later. MAY HAVE ISSUE WITH THIS MECHANISM
                return true;
            }
            //exectued the command
            m_cmdcfg = cfg;
            m_executor.triggerNew(cfg);
            m_curModifyTriggered = true;
            return true;
        }

        /*********
        ** Binding logic
        *********/
        /// <summary>Switch mode by name</summary>
        /// <param name="modeName">The name of desired mode.</param>
        public void switchMode(string modeName) {
            if (!m_config.modes.ContainsKey(modeName)) {
                StardewWrap.inGameMessage($"No {modeName} mode!!");
                return;
            }
            m_curMode = modeName;
            m_keyMap = m_config.getModeDict(m_curMode);
            StardewWrap.inGameMessage($"{modeName} mode ON!!");
        }

        /// <summary>Try to see if we can invoke last cached command</summary>
        protected void tryExecuteCachedCommand() {
            if (m_cachedCmdCfg is null || !m_executor.canTriggerNew()) {
                return;
            }
            m_cmdcfg = m_cachedCmdCfg;
            m_cachedCmdCfg = null;
            m_executor.triggerNew(m_cmdcfg);
        }

        /// <summary>Try to get command config from combined keys.</summary>
        /// <param name="key">The action key.</param>
        /// <returns>Is new command created</returns>
        private CmdCfg findCmdCfg(SButton key) {
            string combinedKey = ModeCfg.constructCmdKey(m_curModifyButton, key);
            if (m_keyMap == null || !m_keyMap.ContainsKey(combinedKey)) {
                return null;
            }
            return m_keyMap[combinedKey];
        }

        /// <summary>Detrmie if a key is a modify key.</summary>
        /// <param name="key">The key.</param>
        /// <returns>If the key is a modify key</returns>
        private bool isModifyKey(SButton key) {
            if (m_config == null || m_config.modes == null || !m_config.modes.ContainsKey(m_curMode)) {
                return false;
            }
            return (m_config.modes[m_curMode].modifyKeys.Contains(key));
        }
    }
}