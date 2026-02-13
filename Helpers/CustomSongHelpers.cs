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
            List<customSongData> localSongDatas = new List<customSongData>();

            foreach(ITrackMetadata song in metadata) {
                if(song.LevelId.Substring(0, 2) == "ws") {
                    workshopSongDatas.Add(new customSongData {                      
                        name = song.TrackName,
                        apId = song.LevelId.Substring(2),
                        levelId = song.LevelId,
                        easy = song.GetDifficulty(Difficulty.Easy)?.Intensity ?? null,
                        medium = song.GetDifficulty(Difficulty.Medium)?.Intensity ?? null,
                        hard = song.GetDifficulty(Difficulty.Hard)?.Intensity ?? null,
                        impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity ?? null,
                    });
                }
                else {
                    localSongDatas.Add(new customSongData {
                        name = song.TrackName,
                        apId = Convert.ToString(1 + localSongDatas.Count),
                        levelId = song.LevelId,
                        easy = song.GetDifficulty(Difficulty.Easy)?.Intensity ?? null,
                        medium = song.GetDifficulty(Difficulty.Medium)?.Intensity ?? null,
                        hard = song.GetDifficulty(Difficulty.Hard)?.Intensity ?? null,
                        impossible = song.GetDifficulty(Difficulty.Impossible)?.Intensity ?? null,
                    });
                }
            }

            if(!Directory.Exists(savePath)) {
                Directory.CreateDirectory(savePath);
            }

            string workshopJson = JsonConvert.SerializeObject(workshopSongDatas, Formatting.None);
            string localJson = JsonConvert.SerializeObject(localSongDatas, Formatting.None);
            File.WriteAllText(Path.Combine(savePath, "workshopSongs.json"), workshopJson);
            File.WriteAllText(Path.Combine(savePath, "localSongs.json"), localJson);
        }   
    }

    public class customSongData {
        public string name;
        public string apId;
        public string levelId;
        public float? easy;
        public float? medium;
        public float? hard;
        public float? impossible;
    }
}