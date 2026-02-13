using HarmonyLib;
using Shared.TrackSelection;
using System.Collections.Generic;
using System;
using System.IO;
using Shared.DLC;
using MonoMod.Utils;
using Shared.TrackData;
using Shared.PlayerData;
using Shared;
using RiftArchipelago.Helpers;

namespace RiftArchipelago.Patches{
    [HarmonyPatch(typeof(SongDatabase), "InitializeDictionary")]
    public static class APDatabase {
        [HarmonyPostfix]
        public static void PostFix(ref Dictionary<string, SongDatabaseData> ____songDatabaseDict) {
            ItemHandler.songDatabaseDict = ____songDatabaseDict;
        }
    }
    
    [HarmonyPatch(typeof(TrackSelectionSceneController), "GetTrackMetadataFromDatabase")]
    public static class GetDLCTracks {
        [HarmonyPrefix]
        public static void Prefix(ref Dictionary<string, ITrackMetadata> ____dynamicMetadataMap) {
            // DLC Data Dump (Probably add some button/var that can toggle this instead of manually commenting this between releases lol)
            // string path = Directory.GetCurrentDirectory();
            // int i = 90;
            // foreach(ITrackMetadata song in ____dynamicMetadataMap.Values) {
            //     using(StreamWriter output = new StreamWriter(Path.Combine(path, "dlcdata.txt"), true)) {
            //         output.WriteLine($"{{\"{song.TrackName}\", \"{song.LevelId}\"}},");
            //     }

            //     using(StreamWriter output = new StreamWriter(Path.Combine(path, "dlcsongdata.txt"), true)) {
            //         output.WriteLine($"\"{song.TrackName}\": SongData({i}, \"{song.TrackName}\", \"{song.Counterpart}\", {song.GetDifficulty(Difficulty.Easy).Intensity}, {song.GetDifficulty(Difficulty.Medium).Intensity}, {song.GetDifficulty(Difficulty.Hard).Intensity}, {song.GetDifficulty(Difficulty.Impossible).Intensity}, False),");
            //     }
            //     i++;
            // }

            if (!ArchipelagoClient.isAuthenticated) return;

            foreach(LocalTrackMetadata song in ____dynamicMetadataMap.Values) {
                foreach(LocalTrackDifficulty diff in song.DifficultyInformation) {
                    diff.UnlockCriteria = new TrackUnlockCriteria();
                    diff.UnlockCriteria.Main = new UnlockCriteria();
                    if(!ItemHandler.dlcSongUnlocked.Contains(song.TrackName)) {
                        diff.UnlockCriteria.Main.Type = UnlockCriteriaType.AlwaysLocked;
                    }

                    diff.UnlockCriteria.Remix = new UnlockCriteria();
                    // RiftAP._log.LogInfo($"{song.TrackName}: {!ItemHandler.dlcSongUnlocked.Contains(song.TrackName)}, {!ArchipelagoClient.slotData.remix}, {!ItemHandler.dlcRemixUnlocked.Contains(song.TrackName)}");
                    if((!ItemHandler.dlcSongUnlocked.Contains(song.TrackName) && !ArchipelagoClient.slotData.remix) || !ItemHandler.dlcRemixUnlocked.Contains(song.TrackName)) {
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
            if (!ArchipelagoClient.isAuthenticated) return;

            for(int i = 0; i < __result.Length; i++) {
                MGTrackMetaData song = __result[i];
                RiftAP._log.LogInfo($"MG GetTrackMetaDatas: {song.LevelId}");

                if(song.TrackDifficulty == Difficulty.Medium && !ItemHandler.extraMedium.Contains(song.LevelId)) {
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }

                else if (song.TrackDifficulty == Difficulty.Hard && !ItemHandler.extraHard.Contains(song.LevelId)){
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }
            }
        }
    }

    [HarmonyPatch(typeof(BBTrackDatabase), "GetTrackMetaDatas")]
    public static class BBDatabase {
        [HarmonyPostfix]
        public static void PostFix(ref BBTrackMetaData[] __result) {
            if (!ArchipelagoClient.isAuthenticated) return;

            for(int i = 0; i < __result.Length; i++) {
                BBTrackMetaData song = __result[i];
                RiftAP._log.LogInfo($"BB GetTrackMetaDatas: {song.LevelId}");

                if(song.TrackDifficulty == Difficulty.Medium && !ItemHandler.extraMedium.Contains(song.LevelId)) {
                    __result[i].UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }

                else if (song.TrackDifficulty == Difficulty.Hard && !ItemHandler.extraHard.Contains(song.LevelId)){
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
            if (!ArchipelagoClient.isAuthenticated) return;

            // ____customTrackMetadatas.Clear();
            // foreach(LocalTrackMetadata song in ____customTrackMetadatas) {
            //     RiftAP._log.LogInfo(song.LevelId);
            //     foreach(LocalTrackDifficulty diff in song.DifficultyInformation) {
            //         diff.UnlockCriteria = new TrackUnlockCriteria();
            //         diff.UnlockCriteria.Main = new UnlockCriteria();
            //         if(true) {
            //             diff.UnlockCriteria.Main.Type = UnlockCriteriaType.AlwaysLocked;
            //         }
            //     }
            // }
        }
    }
}