using Shared.RhythmEngine;
using UnityEngine;

namespace RiftArchipelago{
    public class ArchipelagoUI : MonoBehaviour {
        // Minimize state persisted between sessions
        private const string MinimizedPrefKey = "RiftArchipelago_UI_Minimized";
        private const string HostPrefKey = "RiftArchipelago_UI_Host";
        private const string SlotPrefKey = "RiftArchipelago_UI_Slot";
        private string hostInputCache = "";
        private string slotInputCache = "";
        private string passwordInputCache = "";
        private bool failedLastAuthenticationAttempt = false;
        private static bool isMinimized;

        // Panel layout constants
        private const int PanelX = 8;
        private const int PanelY = 8;
        private const int PanelWidth = 320;
        private const int PanelPadding = 8;

        // Content layout constants
        private const int LeftPadding = 12;
        private const int RightPadding = 12;
        private const int LabelWidth = 120;
        private const int FieldGap = 8;
        private const int LineHeight = 22;

        private static GUIStyle toggleLabelGapStyle;

        // Control names for focus detection
        private const string CtrlHost = "AP_Ctrl_Host";
        private const string CtrlSlot = "AP_Ctrl_Slot";
        private const string CtrlPass = "AP_Ctrl_Pass";

        // Global flag: true if any AP text field is focused
        public static bool AnyTextFieldFocused;

        private void Awake() {
            isMinimized = PlayerPrefs.GetInt(MinimizedPrefKey, 0) == 1;
            hostInputCache = PlayerPrefs.GetString(HostPrefKey, "");
            slotInputCache = PlayerPrefs.GetString(SlotPrefKey, "");
        }

        private static void SetMinimized(bool value) {
            isMinimized = value;
            PlayerPrefs.SetInt(MinimizedPrefKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void OnGUI() {
            // Reset each frame
            AnyTextFieldFocused = false;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftAlt) {
                Cursor.visible = !Cursor.visible;
            }

            // If minimized, show a small maximize button in the top-left corner and bail out
            if (isMinimized) {
                if (GUI.Button(new Rect(PanelX, PanelY, 32, 22), "AP")) {
                    SetMinimized(false);
                }
                return;
            }

            // Compute panel height based on visible content
            bool inMenu = ArchipelagoClient.state == APState.Menu;
            bool hasSession = ArchipelagoClient.session != null;
            bool isAuthed = ArchipelagoClient.isAuthenticated;
            bool showLogin = (!hasSession || !isAuthed) && inMenu;
            bool showConnectedMenu = inMenu && hasSession;

            int panelHeight = 60; // base for status only
            if (showLogin) panelHeight = 140;
            else if (showConnectedMenu) panelHeight = 120;

            // Precompute column positions to avoid overflow
            int labelX = LeftPadding;
            int fieldX = LeftPadding + LabelWidth + FieldGap;
            int fieldWidth = PanelWidth - fieldX - RightPadding;
            int contentWidth = PanelWidth - (LeftPadding + RightPadding);

            // Draw panel group so the minimize button is part of the UI
            GUI.BeginGroup(new Rect(PanelX, PanelY, PanelWidth, panelHeight));
            GUI.Box(new Rect(0, 0, PanelWidth, panelHeight), "Archipelago");

            // Minimize button on the panel (top-right corner)
            if (GUI.Button(new Rect(PanelWidth - PanelPadding - 24, PanelPadding, 24, 20), "—")) {
                SetMinimized(true);
                GUI.EndGroup();
                return;
            }

            // Status
            if (ArchipelagoClient.session != null) {
                if (ArchipelagoClient.isAuthenticated) {
                    GUI.Label(new Rect(LeftPadding, 20, contentWidth, LineHeight), "Status: Connected");
                }
                else if (failedLastAuthenticationAttempt) {
                    GUI.Label(new Rect(LeftPadding, 20, contentWidth, LineHeight), "Status: Authentication failed");
                } else {
                    GUI.Label(new Rect(LeftPadding, 20, contentWidth, LineHeight), "Status: Not Connected");
                }
            }
            else {
                GUI.Label(new Rect(LeftPadding, 20, contentWidth, LineHeight), "Status: Not Connected");
            }

            if ((ArchipelagoClient.session == null || !ArchipelagoClient.isAuthenticated) && ArchipelagoClient.state == APState.Menu ) {
                // Labels
                GUI.Label(new Rect(labelX, 40, LabelWidth, LineHeight), "Host:");
                GUI.Label(new Rect(labelX, 60, LabelWidth, LineHeight), "Slot Name:");
                GUI.Label(new Rect(labelX, 80, LabelWidth, LineHeight), "Password:");

                bool submit = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;
                ArchipelagoInfo info = ArchipelagoClient.apInfo;

                // Name each field, then draw it
                GUI.SetNextControlName(CtrlHost);
                hostInputCache = GUI.TextField(new Rect(fieldX, 40, fieldWidth, LineHeight), hostInputCache);

                GUI.SetNextControlName(CtrlSlot);
                slotInputCache = GUI.TextField(new Rect(fieldX, 60, fieldWidth, LineHeight), slotInputCache);

                GUI.SetNextControlName(CtrlPass);
                passwordInputCache = GUI.PasswordField(new Rect(fieldX, 80, fieldWidth, LineHeight), passwordInputCache, '*');

                // Push current input values into info
                info.address = hostInputCache;
                info.slot = slotInputCache;
                info.password = passwordInputCache;

                // Update global focus flag
                string focused = GUI.GetNameOfFocusedControl();
                AnyTextFieldFocused = focused == CtrlHost || focused == CtrlSlot || focused == CtrlPass;

                if (submit && Event.current.type == EventType.KeyDown) {
                    // The text fields have not consumed the event, which means they were not focused.
                    submit = false;
                }

                if ((GUI.Button(new Rect(LeftPadding, 110, 100, LineHeight), "Connect") || submit) && info.Valid) {
                    bool success = ArchipelagoClient.Connect();
                    if (!success) failedLastAuthenticationAttempt = true;
                    PlayerPrefs.SetString(HostPrefKey, info.address);
                    PlayerPrefs.SetString(SlotPrefKey, info.slot);
                    hostInputCache = info.address;
                    slotInputCache = info.slot;
                    PlayerPrefs.Save();
                    if(ArchipelagoClient.isAuthenticated) {
                        ItemHandler.Setup();
                    }
                }
            }
            else if(ArchipelagoClient.state == APState.Menu && ArchipelagoClient.session != null) {

                if (toggleLabelGapStyle == null) {
                    toggleLabelGapStyle = new GUIStyle(GUI.skin.toggle);
                    toggleLabelGapStyle.padding = new RectOffset(
                        GUI.skin.toggle.padding.left + 8, // extra gap
                        GUI.skin.toggle.padding.right,
                        GUI.skin.toggle.padding.top,
                        GUI.skin.toggle.padding.bottom
                    );
                }

                GUI.Label(new Rect(LeftPadding, 40, contentWidth, LineHeight), "Goal Song: " + ArchipelagoClient.slotData.goalSong);
                
                if(GUI.Button(new Rect(LeftPadding, 60, 150, LineHeight), "Free Play: " + ArchipelagoClient.freePlay)) {
                    ArchipelagoClient.freePlay = !ArchipelagoClient.freePlay;
                    ItemHandler.ResetLocked(ArchipelagoClient.freePlay);
                }

                if(GUI.Button(new Rect(LeftPadding, 85, 100, LineHeight), "Disconnect")) {
                    ArchipelagoClient.Disconnect();
                    failedLastAuthenticationAttempt = false;
                }
            }

            GUI.EndGroup();
        }
    }
}