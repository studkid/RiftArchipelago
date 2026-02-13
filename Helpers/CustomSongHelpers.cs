using System.Collections.Generic;
using HarmonyLib;
using Shared.TrackData;
using Shared;
using System.IO;
using System;
using Newtonsoft.Json;

namespace RiftArchipelago.Helpers {
    public static class CustomSongHelpers {

        internal static string savePath = Path.Combine(Directory.GetCurrentDirectory(), "Output");

        public static void saveCustomData(List<ITrackMetadata> metadata) {
            Dictionary<string, customSongData> workshopSongDatas = new Dictionary<string, customSongData>();
            int localSongs = 0;

            foreach(ITrackMetadata song in metadata) {
                if(song.LevelId.Substring(0, 2) == "ws") {
                    workshopSongDatas.Add($"{song.TrackName} [{song.LevelId}]", new customSongData {                      
                        code = song.LevelId.Substring(2),
                        DLC = "Workshop",
                        diff_easy = song.GetDifficulty(Difficulty.Easy)?.Intensity,
                        diff_medium = song.GetDifficulty(Difficulty.Medium)?.Intensity,
                        diff_hard = song.GetDifficulty(Difficulty.Hard)?.Intensity,
                        diff_impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity,
                    });
                }
                else {
                    workshopSongDatas.Add($"{song.TrackName} [{song.LevelId}]", new customSongData {
                        code = Convert.ToString(1 + localSongs),
                        DLC = "Local",
                        diff_easy = song.GetDifficulty(Difficulty.Easy)?.Intensity,
                        diff_medium = song.GetDifficulty(Difficulty.Medium)?.Intensity,
                        diff_hard = song.GetDifficulty(Difficulty.Hard)?.Intensity,
                        diff_impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity,
                    });
                    localSongs++;
                }
            }

            if(!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }

            string workshopJson = JsonConvert.SerializeObject(workshopSongDatas, Formatting.None);
            File.WriteAllText(Path.Combine(savePath, "CustomSongs.json"), workshopJson);
        }   
    }

    public class customSongData {
        public string code;
        public string DLC;
        public float? diff_easy;
        public float? diff_medium;
        public float? diff_hard;
        public float? diff_impossible;
    }
}