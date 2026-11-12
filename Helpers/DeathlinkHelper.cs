using System;
using RhythmRift;
using RiftArchipelago;

namespace Helpers.RiftArchipelago {
    internal static class DeathlinkHelper {
        internal static bool skipDeath = false;
        internal static DateTime deathTimer = DateTime.Now;
        internal static bool sendDeath = false;

        internal static string[] deathLinkMessages = [
            "missed a beat on ",
        ];

        internal static void ProcessDeath(RRStageController controller) {
            DeathLinkType state = Config.Settings.Deathlink;

            if(state == DeathLinkType.On) {
                controller._codaErrantDamage = 10000000;
                controller.HandleCodaErrantDamage();
                skipDeath = true;
                controller._codaErrantDamage = 1;
            }

            else if(state == DeathLinkType.NonLethal) {
                controller._playerController.OverrideHealth(1);
                // Log.Info("Max Health: " + controller._playerController.MaxHealth);
                controller.UpdateUI();
                // Log.Info("Max Health: " + controller._playerController.MaxHealth);
                deathTimer = DateTime.Now.AddMinutes(1);
            }

            sendDeath = false;
        }

        internal static string GetDeathMsg(string msg) {
            System.Random rnd = new System.Random();
            return deathLinkMessages[rnd.Next(0, deathLinkMessages.Length)] + msg;
        }

        public static void RecieveDeath() {
            if(RiftAP.state != APState.InGame) return;

            if(deathTimer > DateTime.Now) {
                sendDeath = true;
                deathTimer = DateTime.Now;
            }
        }
    }
}