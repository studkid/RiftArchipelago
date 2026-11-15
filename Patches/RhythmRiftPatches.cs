using System.Collections;
using BossBattles;
using HarmonyLib;
using Helpers.RiftArchipelago;
using Minigames;
using RhythmRift;
using Shared.Pins;
using Shared.SceneLoading.Payloads;

namespace RiftArchipelago.Patches{
    [HarmonyPatch(typeof(RRStageController), "HandlePlayerDeathRoutine")]
    public static class SendRRDeathLink {
        [HarmonyPostfix]
        public static void PostFix(RRStageController __instance, ref IEnumerator __result) {
            var original = __result;
            __result = Wrapper();

            TrapHelper.ResetTraps();

            if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) return;
            if(Config.Settings.Deathlink != DeathLinkType.Off && !DeathlinkHelper.skipDeath) {
                RiftAP.apClient.SendDeathLink(__instance._trackNameText.text);
            }
                
            IEnumerator Wrapper() {
                yield return original;
            }
        }
    } 

    [HarmonyPatch(typeof(BossBattleStageController), "HandlePlayerDefeat")]
    public static class SendBBDeathLink {
        [HarmonyPostfix]
        public static void PostFix(BossBattleStageController __instance) {
            if(Config.Settings.Deathlink != DeathLinkType.Off && !DeathlinkHelper.skipDeath) {
                RiftAP.apClient.SendDeathLink(__instance._resultsBossName);
            }
        }
    } 

    [HarmonyPatch(typeof(MinigameBaseStageController<MinigameBeatmapPlayer>), "HandlePlayerDefeat")]
    public static class SendMGDeathLink {
        [HarmonyPostfix]
        public static void PostFix(StageScenePayload ____stageScenePayload) {
            if(Config.Settings.Deathlink != DeathLinkType.Off && !DeathlinkHelper.skipDeath) {
                RiftAP.apClient.SendDeathLink(____stageScenePayload.GetLevelId());
            }
        }
    } 

    [HarmonyPatch(typeof(RRStageController), "HandleBeatUpdate")]
    public static class RecieveDeathLink {
        [HarmonyPostfix]
        public static void PostFix(RRStageController __instance) {
            // Debug
            if(Config.Settings.Debug) {
                ArchipelagoUI.curStage = __instance._trackNameText.text;
                ArchipelagoUI.remix = __instance._stageScenePayload.ShouldProcGen;
            }

            // Deathlink
            if(DeathlinkHelper.sendDeath) {
                DeathlinkHelper.ProcessDeath (__instance);
            }

            // Traps
            TrapHelper.RunTraps(__instance);
        }
    } 

    [HarmonyPatch(typeof(PinsController), "IsPinActive")]
    public static class ModifyPins {
        [HarmonyPostfix]
        public static void PostFix(string pinName, ref bool __result) {

            if(TrapHelper.getTimer(pinName) > 0) {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(RRGridView), "InitTiles")]
    public static class UpscrollTrap {
        [HarmonyPostfix]
        public static void Postfix(RRGridView __instance) {
            TrapHelper.UpscrollTrap(__instance);
        }
    }
}