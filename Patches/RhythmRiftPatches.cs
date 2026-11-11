using System;
using System.Collections.Concurrent;
using BossBattles;
using HarmonyLib;
using Minigames;
using RhythmRift;
using Shared.Pins;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace RiftArchipelago.Patches{
    internal static class DeathlinkHelper {
        internal static bool skipDeath = false;
        internal static DateTime deathTimer = DateTime.Now;
        internal static bool sendDeath = false;

        internal static string[] deathLinkMessages = [
            "missed a beat on ",
        ];

        internal static void RecieveDeath(RRStageController controller) {
            DeathLinkType state = Config.Settings.Deathlink;

            if(state == DeathLinkType.On) {
                controller._codaErrantDamage = 10000000;
                controller.HandleCodaErrantDamage();
                skipDeath = true;
                controller._codaErrantDamage = 1;
            }

            else if(state == DeathLinkType.NonLethal) {
                controller._playerController.CurrentHealth = 1;
                controller.UpdateUI();
                deathTimer = DateTime.Now.AddMinutes(1);
            }
        }

        internal static string GetDeathMsg(string msg) {
            System.Random rnd = new System.Random();
            return deathLinkMessages[rnd.Next(0, deathLinkMessages.Length)] + msg;
        }

        public static void SendDeath() {
            if(RiftAP.state != APState.InGame) return;

            if(deathTimer > DateTime.Now) {
                sendDeath = true;
                deathTimer = DateTime.Now;
            }
        }
    }

    internal static class TrapHelper {
        private static ConcurrentQueue<string> trapQueue = new ConcurrentQueue<string>();
        private static int confuseTimer = -1;
        private static int gloomTimer = -1;
        private static int nightmareTimer = -1;
        private static int codaTimer = -1;
        private static int glassTimer = -1;

        public static void AddTrap(string trapName) {
            trapQueue.Enqueue(trapName);
        }

        internal static void RunTraps(RRStageController controller) {
            if(!trapQueue.IsEmpty) {
                trapQueue.TryDequeue(out string trap);

                if(trap == "Confusion Trap") {
                    confuseTimer =+ 12;
                }

                if(trap == "Gloom Trap") {
                    gloomTimer =+ 12;
                }
            }

            if(confuseTimer > 0) {
                controller._enigmaAnimator.SetTrigger("EnigmaOn");

                confuseTimer--;
            }
            else if(confuseTimer == 0) {
                controller._enigmaAnimator.ResetTrigger("EnigmaOn");

                confuseTimer--;
            }

            if(gloomTimer > 0) {
                for (int i = 0; i < 3; i++) {
                    Animator animator = controller._obscureAnimators[i];

                    if ((bool)animator) {
                        animator.SetTrigger("GloomOn");
                    }
            }

                gloomTimer--;
            }
            else if(gloomTimer == 0) {
                for (int i = 0; i < 3; i++) {
                    Animator animator = controller._obscureAnimators[i];

                    if ((bool)animator) {
                        animator.ResetTrigger("GloomOn");
                    }
                }
                    
                gloomTimer--;
            }

            if(nightmareTimer > 0) {
                controller._nightmareTimeBeforeRowChange = controller.GetRandomNightmareDelay();
                controller._nightmareCurrentRow = controller._nightmareRandomGenerator.Next(0, controller._obscureAnimators.Count);
                Animator animator2 = controller._obscureAnimators[controller._nightmareCurrentRow];
                if ((bool)animator2) {
                    animator2.SetTrigger("GloomOn");
                }
                
                nightmareTimer--;
            }
            else if(nightmareTimer == 0) {

            }

            if(codaTimer > 0) {
                codaTimer--;
            }

            if(glassTimer > 0) {
                controller._playerController.OverrideHealth(1, 1);
                controller.UpdateUI();

                glassTimer--;
            }
            else if(glassTimer == 0) {
                controller._playerController.OverrideHealth(1, 1);
                controller.UpdateUI();

                glassTimer--;
            }
        }

        public static int getTimer(string timer) {
            if(timer == "Enigma") {
                return confuseTimer;
            }

            if(timer == "Perfectionist") {
                return codaTimer;
            }

            return -1;
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

    [HarmonyPatch(typeof(RRStageController), "HandleBeatUpdate")]
    public static class RecieveDeathLink {
        [HarmonyPostfix]
        public static void PostFix(RRStageController __instance) {
            // Deathlink
            if(DeathlinkHelper.sendDeath) {
                DeathlinkHelper.RecieveDeath (__instance);
            }

            // Traps
            TrapHelper.RunTraps(__instance);
        }
    } 

    [HarmonyPatch(typeof(PinsController), "IsPinActive")]
    public static class ModifyPins {
        public static void PostFix(object[] __args, ref bool __result) {
            string arg = "";
            try {
                Convert.ToString(__args[0]);
            }
            catch {
               return; 
            }

            if(TrapHelper.getTimer(arg) > 0) {
                __result = true;
            }
        }
    }
}