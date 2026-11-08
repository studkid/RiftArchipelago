using HarmonyLib;
using Shared.TrackSelection;
using TMPro;
using Shared.TrackData;
using System.Linq;
using RiftOfTheNecroManager;

namespace RiftArchipelago.Patches{
    [HarmonyPatch(typeof(TrackSelectionSceneController), "Update")]
    public static class UpdateDiamondText {
        [HarmonyPostfix]
        public static void PostFix(ref TMP_Text ____totalDiamondsText) {
            if(!RiftAP.apClient.isAuthenticated) return;
            ____totalDiamondsText.text = $"x{RiftAP.apClient.itemHandler.diamondCount} / {RiftAP.apClient.slotData.diamondGoal}";
        }
    }

    [HarmonyPatch(typeof(TrackSelectMetadataProcessor), "FilterMetadata")]
    public static class UpdateFilter {
        [HarmonyPostfix]
        public static void PostFix(in ITrackMetadata[] metadataToFilter, TrackFilterOption filterOption, ref ITrackMetadata[] __result) {
            if(!RiftAP.apClient.isAuthenticated) return;
            
            if(filterOption == TrackFilterOption.UnplayedSongs) {
                Log.Info("Sorting!");
                __result = metadataToFilter.Where(delegate (ITrackMetadata d) {
                    if(d.Category != TrackCategory.Tutorial) {
                        return RiftAP.apClient.unplayedSongs.Contains(d.TrackName) && RiftAP.apClient.itemHandler.IsUnlocked(d.TrackName);
                    }
                    return false;
                }).ToArray();
            }
        }
    }
}