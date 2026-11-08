using HarmonyLib;
using Shared.TrackSelection;
using System.Collections.Generic;
using Shared.TrackData;
using Shared.PlayerData;
using Shared;
using RiftArchipelago.Helpers;
using RiftOfTheNecroManager;

namespace RiftArchipelago.Patches{
    [HarmonyPatch(typeof(TrackSelectionSceneController), "GetTrackMetadataFromDatabase")]
    public static class GetDLCTracks {
        [HarmonyPrefix]
        public static void Prefix(ref List<ITrackMetadata> ____dynamicMetadataList) {
            // DLC Data Dump (Probably add some button/var that can toggle this instead of manually commenting this between releases lol)
            RiftAP.dynamicMetadataList = ____dynamicMetadataList;
            Log.Info("AAAAAAAAAA" + RiftAP.dynamicMetadataList);

            if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) return;

            foreach(LocalTrackMetadata song in ____dynamicMetadataList) {
                foreach(LocalTrackDifficulty diff in song.DifficultyInformation) {
                    diff.UnlockCriteria = new TrackUnlockCriteria {
                        Main = new UnlockCriteria()
                    };
                    if (!RiftAP.apClient.itemHandler.dlcSongUnlocked.Contains(song.TrackName)) {
                        diff.UnlockCriteria.Main.Type = UnlockCriteriaType.AlwaysLocked;
                    }

                    diff.UnlockCriteria.Remix = new UnlockCriteria();
                    // Log.Info($"{song.TrackName}: {!ItemHandler.dlcSongUnlocked.Contains(song.TrackName)}, {!ArchipelagoClient.slotData.remix}, {!ItemHandler.dlcRemixUnlocked.Contains(song.TrackName)}");
                    if((!RiftAP.apClient.itemHandler.dlcSongUnlocked.Contains(song.TrackName) && !RiftAP.apClient.slotData.remix) || !RiftAP.apClient.itemHandler.dlcRemixUnlocked.Contains(song.TrackName)) {
                        diff.UnlockCriteria.Remix.Type = UnlockCriteriaType.AlwaysLocked;
                    }
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(MGTrackDatabase), "GetTrackMetaDatas")]
    public static class MGDatabase {
        [HarmonyPostfix]
        public static void PostFix(ref MGTrackMetaData[] __result) {
            if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) return;

            for(int i = 0; i < __result.Length; i++) {
                MGTrackMetaData song = __result[i];
                // Log.Info($"MG GetTrackMetaDatas: {song.LevelId}");

                if(song.TrackDifficulty == Difficulty.Medium && !RiftAP.apClient.itemHandler.extraMedium.Contains(song.LevelId)) {
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }

                else if (song.TrackDifficulty == Difficulty.Hard && !RiftAP.apClient.itemHandler.extraHard.Contains(song.LevelId)){
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }
            }
        }
    }

    [HarmonyPatch(typeof(BBTrackDatabase), "GetTrackMetaDatas")]
    public static class BBDatabase {
        [HarmonyPostfix]
        public static void PostFix(ref BBTrackMetaData[] __result) {
            if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) return;

            for(int i = 0; i < __result.Length; i++) {
                BBTrackMetaData song = __result[i];
                // Log.Info($"BB GetTrackMetaDatas: {song.LevelId}");

                if(song.TrackDifficulty == Difficulty.Medium && !RiftAP.apClient.itemHandler.extraMedium.Contains(song.LevelId)) {
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }

                else if (song.TrackDifficulty == Difficulty.Hard && !RiftAP.apClient.itemHandler.extraHard.Contains(song.LevelId)){
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CustomTracksSelectionSceneController), "HandleTrackMetadataReSort")]
    public static class GetCustomTracks {
        [HarmonyPrefix]
        public static void Prefix(ref List<ITrackMetadata> ____customTrackMetadatas) {
            // TODO Make this not run every single time this method is called
            CustomSongHelpers.saveCustomData(____customTrackMetadatas);
            if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) return;
            List<ITrackMetadata> refTrackList = new List<ITrackMetadata>(____customTrackMetadatas);

            foreach(LocalTrackMetadata song in refTrackList) {
                string songName = $"{song.TrackName} [{song.LevelId}]"; 

                Log.Info($"{song.TrackName} in List: {!RiftAP.apClient.itemHandler.customUnlocked.Contains(songName.Replace("\'", ""))}");
                if(!RiftAP.apClient.itemHandler.customUnlocked.Contains(songName.Replace("\'", ""))) {
                    Log.Info($"Removing {song.TrackName}");
                    ____customTrackMetadatas.Remove(song);
                }
            }
        }
    }
}