using UnityEngine;
using HarmonyLib;
using Shared.Title;
using Shared.MenuOptions;
using Shared.RhythmEngine;

namespace RiftArchipelago.Patches{
    [HarmonyPatch(typeof(MainMenuManager), "Awake")]
    public static class APUIPatch {
        [HarmonyPostfix]
        public static void PostFix() {
            CreateUI();
        }

        private static void CreateUI() {
            if(ArchipelagoClient.apUI) return;
            
            var guiGameObject = new GameObject("AP");
            ArchipelagoClient.apUI = guiGameObject.AddComponent<ArchipelagoUI>();
            Object.DontDestroyOnLoad(guiGameObject);
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), "Update")]
    public static class APUIUpdatePatch {
        private static readonly AccessTools.FieldRef<MainMenuManager, OptionsScreenInputController> _stageInputRecordRef =
            AccessTools.FieldRefAccess<MainMenuManager, OptionsScreenInputController>("_inputController");
        private static OptionsScreenInputController _inputController = null;
        private static bool _wasFocusedLastFrame = false;

        [HarmonyPrefix]
        public static void PreFix(MainMenuManager __instance) {
            // stageInputRecord: preferred via FieldRef, fallback to Traverse
            try { _inputController = _stageInputRecordRef(__instance); } catch { }
            if (_inputController == null)
            {
                try { _inputController = Traverse.Create(__instance).Field("_inputController").GetValue<OptionsScreenInputController>(); } catch { }
            }
            if (_inputController == null)
            {
                RiftAP._log.LogWarning("RRStageEnd PostFix: _inputController == null");
                return;
            }

            if (ArchipelagoUI.AnyTextFieldFocused) {
                _inputController.IsInputDisabled = true;
                _wasFocusedLastFrame = true;
            } else if (_wasFocusedLastFrame) {
                _inputController.IsInputDisabled = false;
                _wasFocusedLastFrame = false;
            }
        }
    }
}