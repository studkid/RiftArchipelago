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
            List<customSongData> workshopSongDatas = new List<customSongData>();
            int localSongs = 1;

            foreach(ITrackMetadata song in metadata) {
                if(song.LevelId.Substring(0, 2) == "ws") {
                    workshopSongDatas.Add(new customSongData {                      
                        name = song.TrackName,
                        apId = song.LevelId.Substring(2),
                        levelId = song.LevelId,
                        type = "Workshop",
                        easy = song.GetDifficulty(Difficulty.Easy)?.Intensity,
                        medium = song.GetDifficulty(Difficulty.Medium)?.Intensity,
                        hard = song.GetDifficulty(Difficulty.Hard)?.Intensity,
                        impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity,
                    });
                }
                else {
                    workshopSongDatas.Add(new customSongData {
                        name = song.TrackName,
                        apId = Convert.ToString(1 + localSongs),
                        levelId = song.LevelId,
                        type = "Local",
                        easy = song.GetDifficulty(Difficulty.Easy)?.Intensity,
                        medium = song.GetDifficulty(Difficulty.Medium)?.Intensity,
                        hard = song.GetDifficulty(Difficulty.Hard)?.Intensity,
                        impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity,
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
        public string name;
        public string apId;
        public string levelId;
        public string type;
        public float? easy;
        public float? medium;
        public float? hard;
        public float? impossible;
    }
}