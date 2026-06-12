using HarmonyLib;
using Shared.TrackSelection;
using TMPro;
using Shared.TrackData;
using System.Linq;

namespace RiftArchipelago.Patches{
    [HarmonyPatch(typeof(TrackSelectionSceneController), "Update")]
    public static class UpdateDiamondText {
        [HarmonyPostfix]
        public static void PostFix(ref TMP_Text ____totalDiamondsText) {
            if(!ArchipelagoClient.isAuthenticated) return;
            ____totalDiamondsText.text = $"x{ItemHandler.diamondCount} / {ArchipelagoClient.slotData.diamondGoal}";
        }
    }

    [HarmonyPatch(typeof(TrackSelectMetadataProcessor), "FilterMetadata")]
    public static class UpdateFilter {
        [HarmonyPostfix]
        public static void PostFix(in ITrackMetadata[] metadataToFilter, TrackFilterOption filterOption, ref ITrackMetadata[] __result) {
            if(!ArchipelagoClient.isAuthenticated) return;
            
            if(filterOption == TrackFilterOption.UnplayedSongs) {
                RiftAP._log.LogInfo("Sorting!");
                __result = metadataToFilter.Where(delegate (ITrackMetadata d) {
                    if(d.Category != TrackCategory.Tutorial) {
                        return ArchipelagoClient.unplayedSongs.Contains(d.TrackName) && ItemHandler.IsUnlocked(d.TrackName);
                    }
                    return false;
                }).ToArray();
            }
        }
    }
}