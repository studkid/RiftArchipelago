using System.Collections.Generic;
using HarmonyLib;
using Shared.TrackData;
using Shared;
using System.IO;
using System;
using Newtonsoft.Json;
using RiftOfTheNecroManager;
using Shared.PlayerData;
using Archipelago.MultiClient.Net.Models;

namespace RiftArchipelago.Helpers {
    public class CustomSongHelpers {

        internal static string savePath = Path.Combine(Directory.GetCurrentDirectory(), "Output");
        public static List<ITrackMetadata> customSongs = new List<ITrackMetadata>();

        public static void saveCustomData() {
            Dictionary<string, customSongData> workshopSongDatas = new Dictionary<string, customSongData>();
            int localSongs = 0;
            int workshopSongs = 0;
            Log.Info("saveCustomdata: Scanning Custom Songs...");
            Log.Info(customSongs.Count);

            foreach(ITrackMetadata song in customSongs) {
                if(!PlayerDataUtil.IsLevelFavorite(song.LevelId)) continue;

                if(song.LevelId.Substring(0, 2) == "ws") {   
                    long itemId = 0;
                    try {
                        itemId = Convert.ToInt64(song.LevelId.Substring(2));
                    }
                    catch (Exception e ){
                        Log.Error($"saveCustomData: Failed to save custom song {song.TrackName} {song.LevelId}.\n{e}");
                        continue;
                    }
                    itemId = itemId - 3402139497 + 6000 % int.MaxValue;

                    if(itemId < 6000) {
                        Log.Error($"saveCustomData: {song.TrackName} {song.LevelId} returned value {itemId} which is lower than allocated location ids, skipping!\nPlease contact @studkid in the RotN ap thread, DMs or in the RiftAP github issue tracker with the workshop link if you see this message!");
                        continue;
                    }

                    string songName = $"{song.TrackName} [{song.LevelId}]";
                    workshopSongDatas.Add(songName.Replace("\'", ""), new customSongData {                      
                        code = Convert.ToString(itemId),
                        song_id = song.LevelId,
                        DLC = "Workshop",
                        diff_easy = song.GetDifficulty(Difficulty.Easy)?.Intensity,
                        diff_medium = song.GetDifficulty(Difficulty.Medium)?.Intensity,
                        diff_hard = song.GetDifficulty(Difficulty.Hard)?.Intensity,
                        diff_impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity,
                        
                    });
                    workshopSongs =+ 1;
                }
                else {
                    string songName = $"{song.TrackName} [{song.LevelId}]";
                    workshopSongDatas.Add(songName.Replace("\'", ""), new customSongData {
                        code = Convert.ToString(1 + localSongs),
                        song_id = song.LevelId,
                        DLC = "Local",
                        diff_easy = song.GetDifficulty(Difficulty.Easy)?.Intensity,
                        diff_medium = song.GetDifficulty(Difficulty.Medium)?.Intensity,
                        diff_hard = song.GetDifficulty(Difficulty.Hard)?.Intensity,
                        diff_impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity,
                    });
                    localSongs =+ 1;
                }
            }

            if(!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }

            Log.Info($"saveCustomData: Found {workshopSongs} workshop songs and {localSongs} local songs in favorites.  Dumping!");
            string workshopJson = JsonConvert.SerializeObject(workshopSongDatas, Formatting.None);
            File.WriteAllText(Path.Combine(savePath, "CustomSongs.json"), workshopJson);
        }   
    }

    public class customSongData {
        public string code;
        public string song_id;
        public string DLC;
        public float? diff_easy;
        public float? diff_medium;
        public float? diff_hard;
        public float? diff_impossible;
    }
}