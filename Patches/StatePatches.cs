using HarmonyLib;
using RhythmRift;
using Shared;
using Shared.RhythmEngine;
using Shared.Title;
using Shared.TrackSelection;

namespace RiftArchipelago.Patches{

    [HarmonyPatch(typeof(MainMenuManager), "Awake")]
    public static class MenuStatePatch {
        [HarmonyPrefix]
        public static void PreFix() {
            RiftAP.state = APState.Menu;
        }
    }

    [HarmonyPatch(typeof(TrackSelectionSceneController), "Awake")]
    public static class TrackStatePatch {
        [HarmonyPrefix]
        public static void PreFix() {
            RiftAP.state = APState.SongMenu;
        }
    }

    [HarmonyPatch(typeof(MinigameSelectionSceneController), "Awake")]
    public static class MGMenuStatePatch {
        [HarmonyPrefix]
        public static void PreFix() {
            RiftAP.state = APState.Menu;
        }
    }

    [HarmonyPatch(typeof(BBSelectionSceneController), "Awake")]
    public static class BBMenuStatePatch {
        [HarmonyPrefix]
        public static void PreFix() {
            RiftAP.state = APState.Menu;
        }
    }

    [HarmonyPatch(typeof(CustomTracksSelectionSceneController), "Awake")]
    public static class CustomStatePatch {
        [HarmonyPrefix]
        public static void PreFix() {
            RiftAP.state = APState.CustomMenu;
        }
    }

    [HarmonyPatch(typeof(StageController<TimelineMapPlayer>), "Awake")]
    public static class BBStatePatch {
        [HarmonyPrefix]
        public static void PreFix(StageController<TimelineMapPlayer> __instance) {
            RiftAP.state = APState.InGame;
        }
    }
}