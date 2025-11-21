// using HarmonyLib;
// using RhythmRift;

// namespace RiftArchipelago.Patches{
//     [HarmonyPatch(typeof(RRStageController), "HandlePlayerDefeat")]
//     public static class SendDeathLink {
//         [HarmonyPostfix]
//         public static void PostFix() {
//             if(ArchipelagoClient.deathLink != ArchipelagoClient.DeathLinkState.Off) {
//                 ArchipelagoClient.SendDeathLink();
//             }
//         }
//     } 

//     [HarmonyPatch(typeof(RRStageController), "Start")]
//     public static class RecieveDeathLink {
//         [HarmonyPostfix]
//         public static void PostFix(RRStageController __instance) {
//             // ArchipelagoClient.stageController = __instance;
//         }
//     } 
// }