using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using MineAssist.Framework;

namespace MineAssist {
    internal class AdjustedInputEvent {
        private List<SButton> m_trackingButtons = new List<SButton>();

        protected bool m_enableTriggerOverride = false;
        protected double m_threshold = 0.1;
        protected double m_lastLeft = 0;
        protected double m_lastRight = 0;
        protected bool m_leftTriggerOn = false;
        protected bool m_rightTriggerOn = false;
        protected IModHelper Helper = null;

        public Func<SButton, bool> ButtonPressed = null;
        public Func<SButton, bool> ButtonReleased = null;

        public AdjustedInputEvent(IModHelper helper, bool enable, double threshold) {
            Helper = helper;
            helper.Events.Input.ButtonPressed += onButtonPressed;
            helper.Events.Input.ButtonReleased += onButtonReleased;
            helper.Events.GameLoop.UpdateTicking += onUpdateTicking;
            m_enableTriggerOverride = enable;
            m_threshold = threshold;
        }

        /// <summary>Suppress the button and add to tracking list</summary>
        /// <param name="button">The button to suppress and track</param>
        protected void Suppress(SButton button) {
            this.Helper.Input.Suppress(button);
            m_trackingButtons.Add(button);
        }

        /// <summary>Check if tracking buttons are released by IsSuppressed status and invoke button release function</summary>
        private void trackButtonRelease() {
            for (int i = m_trackingButtons.Count - 1; i >= 0; --i) {
                SButton button = m_trackingButtons[i];
                //Suppressed buttons will be not be suppressed when it is released. Both IsDown and GetState doesn't work now.
                if (!this.Helper.Input.IsSuppressed(button)) {
                    buttonReleased(button);
                    m_trackingButtons.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// SMAPI raise trigger when its value is above about 0.2 but Stardew process trigger 0~0.2 as swap item. if trigger need to be binded, we need our way to track its events.
        /// But this way the game will not receive any trigger event in "normal" state.
        /// </summary>
        private void overrideTrigger() {
            var padState = StardewWrap.GetRealGamePadState();

            if (padState.Triggers.Left > 0) {
                if (StardewWrap.isPlayerReady()) {
                    //this.Monitor.Log($"left:{padState.Triggers.Left}, {Helper.Input.IsSuppressed(SButton.LeftTrigger)}", LogLevel.Debug);
                    if (padState.Triggers.Left > m_threshold && !m_leftTriggerOn) {
                        m_leftTriggerOn = true;
                        buttonPressed(SButton.LeftTrigger);
                    }
                    Helper.Input.Suppress(SButton.LeftTrigger);
                }
            } else if (m_lastLeft != 0) {
                //this.Monitor.Log($"left:{padState.Triggers.Left}, {Helper.Input.IsSuppressed(SButton.LeftTrigger)}", LogLevel.Debug);
                m_leftTriggerOn = false;
                Helper.Input.Suppress(SButton.LeftTrigger);
            }
            m_lastLeft = padState.Triggers.Left;

            if (padState.Triggers.Right > 0) {
                if (StardewWrap.isPlayerReady()) {
                    if (padState.Triggers.Right > 0.1 && !m_rightTriggerOn) {
                        m_rightTriggerOn = true;
                        buttonPressed(SButton.RightTrigger);
                    }
                    Helper.Input.Suppress(SButton.RightTrigger);
                }
            } else if (m_lastRight != 0) {
                //this.Monitor.Log($"left:{padState.Triggers.Left}, {Helper.Input.IsSuppressed(SButton.LeftTrigger)}", LogLevel.Debug);
                m_rightTriggerOn = false;
                Helper.Input.Suppress(SButton.RightTrigger);
            }
            m_lastRight = padState.Triggers.Right;
        }

        private void onUpdateTicking(object sender, UpdateTickingEventArgs e) {
            if (m_enableTriggerOverride) {
                overrideTrigger();
            }
            //Since SMAPI onButtonReleased doesn't work as expected after supress, we need to track their release here
            trackButtonRelease();
        }

        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void onButtonPressed(object sender, ButtonPressedEventArgs e) {
            //only when player is ready shall we start process button event
            if (!StardewWrap.isPlayerReady()) {
                return;
            }
            buttonPressed(e.Button);
        }

        /// <summary>Raised after the player releases a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void onButtonReleased(object sender, ButtonReleasedEventArgs e) {
            //released signal during suppress is a side effect of Suppress(), should be ignored
            if (this.Helper.Input.IsSuppressed(e.Button)) {
                return;
            }
            //now, the signal is real
            buttonReleased(e.Button);
        }

        private void buttonPressed(SButton button) {
            if (ButtonPressed?.Invoke(button) ?? false) {
                Suppress(button);
            }
        }

        private void buttonReleased(SButton button) {
            if (ButtonReleased?.Invoke(button) ?? false) {
                this.Helper.Input.Suppress(button);
            }
        }
    }
}
