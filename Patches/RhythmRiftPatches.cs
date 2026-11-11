using System;
using BossBattles;
using HarmonyLib;
using Minigames;
using RhythmRift;
using Shared.SceneLoading.Payloads;

namespace RiftArchipelago.Patches{
    internal static class DeathlinkHelper {
        internal static bool skipDeath = false;
        internal static DateTime deathTimer = DateTime.Now;
        public static bool sendDeath = false;

        internal static string[] deathLinkMessages = [
            "missed a beat on ",
        ];

        internal static void recieveDeath(RRStageController controller) {
            DeathLinkType state = Config.Settings.Deathlink;

            if(state == DeathLinkType.On) {
                controller._codaErrantDamage = 10000000;
                controller.HandleCodaErrantDamage();
                skipDeath = true;
            }

            else if(state == DeathLinkType.NonLethal && deathTimer > DateTime.Now) {
                controller._codaErrantDamage = controller.PlayerHealth - 1;
                controller.HandleCodaErrantDamage();
                deathTimer = DateTime.Now.AddMinutes(1);
            }
        }

        internal static string getDeathMsg(string msg) {
            Random rnd = new Random();
            return deathLinkMessages[rnd.Next(0, deathLinkMessages.Length)] + msg;
        }
    }
    

    [HarmonyPatch(typeof(RRStageController), "HandlePlayerDefeat")]
    public static class SendRRDeathLink {
        [HarmonyPostfix]
        public static void PostFix(RRStageController __instance) {
            if(Config.Settings.Deathlink != DeathLinkType.Off && !DeathlinkHelper.skipDeath) {
                RiftAP.apClient.SendDeathLink(__instance._trackNameText.text);
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

    [HarmonyPatch(typeof(RRStageController), "Update")]
    public static class RecieveDeathLink {
        [HarmonyPostfix]
        public static void PostFix(RRStageController __instance) {
            // Deathlink
            if(DeathlinkHelper.sendDeath) {
                DeathlinkHelper.recieveDeath (__instance);
            }
        }
    } 
}