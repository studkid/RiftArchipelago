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
            // DLC Data Dump
            RiftAP.dynamicMetadataList = ____dynamicMetadataList;

            foreach(LocalTrackMetadata song in ____dynamicMetadataList) {
                foreach(LocalTrackDifficulty diff in song.DifficultyInformation) {
                    diff.UnlockCriteria = new TrackUnlockCriteria {
                        Main = new UnlockCriteria(),
                        Remix = new UnlockCriteria()
                    };
                    if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) continue;

                    if (!RiftAP.apClient.itemHandler.dlcSongUnlocked.Contains(song.TrackName)) {
                        diff.UnlockCriteria.Main.Type = UnlockCriteriaType.AlwaysLocked;
                    }

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
            CustomSongHelpers.customSongs = ____customTrackMetadatas;
            if (!RiftAP.apClient.isAuthenticated || RiftAP.apClient.freePlay) return;
            List<ITrackMetadata> refTrackList = new List<ITrackMetadata>(____customTrackMetadatas);

            foreach(LocalTrackMetadata song in refTrackList) {
                string songName = $"{song.TrackName} [{song.LevelId}]"; 

                if(!RiftAP.apClient.itemHandler.customUnlocked.Contains(songName.Replace("\'", ""))) {
                    ____customTrackMetadatas.Remove(song);
                }
            }
        }
    }
}