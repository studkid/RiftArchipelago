using Shared.PlayerData;
using Shared.TrackSelection;
using System.Collections.Generic;
using RiftOfTheNecroManager;

namespace RiftArchipelago{
    public class ItemHandler {
        private static Dictionary<string, string> songMapping = new Dictionary<string, string>() {
            {"Disco Disaster", "RRDiscoDisaster"} ,
            {"Elusional", "RRElusional"},

            {"Matriarch", "RRMatriarch"},
            {"Under the Thunder", "RRThunder"},
            {"Eldritch House", "RREldritchHouse"},
            {"RAVEVENGE (feat. Aram Zero)", "RRRavevenge"},
            {"Rift Within", "RRRiftWithin"},
            {"Suzu's Quest", "RRSuzusQuest"},
            {"Necropolis", "RRNecropolis"},
            {"Baboosh", "RRBaboosh"},
            {"Necro Sonatica", "RRNecroSonatica"},
            {"She Banned", "RRHarmonie"},
            {"King's Ruse", "RRDeepBlues"},
            {"What's in the Box", "RRMatron"},
            {"Brave the Harvester", "RRReaper"},
            {"Final Fugue", "RRFinalFugue"},
            {"Twombtorial", "RRTwombtorial"},
            {"Portamello", "RRPortamello"},
            {"Slugger's Refrain", "DLCApricot01"},
            {"Got Danged", "DLCApricot02"},
            {"Bootus Bleez", "DLCApricot03"},
            {"Resurrections (dannyBstyle Remix)", "DLCBanana01"},
            {"Scattered and Lost", "DLCBanana02"},
            {"Reach for the Summit", "DLCBanana03"},
            {"Confronting Myself", "DLCBanana04"},
            {"Resurrections", "DLCBanana05"},
        };

        public static Dictionary<string, string> extraMapping = new Dictionary<string, string>() {
            {"A Bit of a Stretch", "MGYoga"},
            {"Lunch Rush", "MGBurger"},
            {"Voguelike", "MGPhoto"},
            {"Show Time!", "MGShow"},
            {"Take a Breather", "MGBreathing"},

            {"Harmonie", "BBHarmoniePhase2"},
            {"Deep Blues", "BBDeepBlues"},
            {"Matron", "BBMatron"},
            {"Reaper", "BBReaper"},
            {"The NecroDancer", "BBNecrodancer"},
        };

        public List<string> baseSongUnlocked = [];
        public List<string> baseRemixUnlocked = [];
        public List<string> dlcSongUnlocked = [];
        public List<string> dlcRemixUnlocked = [];
        public List<string> customUnlocked = [];
        public List<string> extraMedium = [];
        public List<string> extraHard = [];
        public bool databaseInit = false;
        public bool dlcDatabaseInit = false;
        public int diamondCount {get; private set;}

        public void Setup() {
            diamondCount = 0;
            Log.Info($"IH Setup: Setting up song Dict");

            foreach(SongDatabaseData song in RiftAP.songDatabaseDict.Values) {
                foreach(DifficultyInformation diff in song.DifficultyInformation) {
                    diff.UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                    diff.RemixUnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }
            }
        }

        public void ResetLocked(bool freePlay) {
            if(freePlay) {
                foreach(SongDatabaseData song in RiftAP.songDatabaseDict.Values) {
                    foreach(DifficultyInformation diff in song.DifficultyInformation) {
                        diff.UnlockCriteria.Type = UnlockCriteriaType.TotalDiamonds;
                        diff.RemixUnlockCriteria.Type = UnlockCriteriaType.TotalDiamonds;
                    }
                }
            }
            else{
                foreach(SongDatabaseData song in RiftAP.songDatabaseDict.Values) {
                    foreach(DifficultyInformation diff in song.DifficultyInformation) {
                        diff.UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                        diff.RemixUnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                    }
                }

                foreach(string song in baseSongUnlocked) {
                    UnlockSong(song);
                }

                foreach(string song in baseRemixUnlocked) {
                    UnlockRemix(song);
                }
            }
        }

        public void ResetEverything() {
            diamondCount = 0;
            dlcSongUnlocked = [];
            dlcRemixUnlocked = [];
            customUnlocked = [];
            extraMedium = [];
            extraHard = [];

            foreach(SongDatabaseData song in RiftAP.songDatabaseDict.Values) {
                foreach(DifficultyInformation diff in song.DifficultyInformation) {
                    diff.UnlockCriteria.Type = UnlockCriteriaType.TotalDiamonds;
                    diff.RemixUnlockCriteria.Type = UnlockCriteriaType.TotalDiamonds;
                }
            }
        }

        public bool IsUnlocked(string songName) {
            if(baseSongUnlocked.Contains(songName)) return true;
            if(baseRemixUnlocked.Contains(songName)) return true;
            if(dlcSongUnlocked.Contains(songName)) return true;
            if(dlcRemixUnlocked.Contains(songName)) return true;
            return false;
        }

        public void AddDiamond() {
            diamondCount += 1;
            Log.Info($"AddDiamond: Adding Diamond | New Total: {diamondCount}");

            if(diamondCount >= RiftAP.apClient.slotData.diamondGoal) {
                string goalSong = RiftAP.apClient.slotData.goalSong;
                string goalType = RiftAP.apClient.slotData.goalType;

                if(goalType == "Minigame" || goalType == "Boss") {
                    UnlockExtra(goalSong);
                }

                else if(goalType == "Remix") {
                    UnlockRemix(goalSong.Substring(0, goalSong.Length - 8));
                }

                else if(goalType == "Custom") {
                    UnlockCustom(goalSong);
                }

                else {
                    UnlockSong(goalSong);
                }
            }
        }

        public void UnlockSong(string songName) {
            if(songMapping.TryGetValue(songName, out var levelName)) {
                if(RiftAP.songDatabaseDict.TryGetValue(levelName, out var value)) {
                    Log.Info($"UnlockSong: Unlocking \"{songName}\"");
                    baseSongUnlocked.Add(songName);
            
                    foreach(DifficultyInformation diff in value.DifficultyInformation) {
                        Log.Info(RiftAP.apClient.slotData);
                        diff.UnlockCriteria.Type = UnlockCriteriaType.None;
                        if(!RiftAP.apClient.slotData.remix) {
                            diff.RemixUnlockCriteria.Type = UnlockCriteriaType.None;
                        }
                        Log.Info($"UnlockSong: Chaning Unlock State {diff.UnlockCriteria.Type}");
                    }
                }
            }
            
            else {
                Log.Info($"UnlockSong: Unlocking \"{songName}\" (Post Anniversary DLC Song)");
                dlcSongUnlocked.Add(songName);
            }
        }

        public void UnlockRemix(string songName) {
            if(songMapping.TryGetValue(songName, out var levelName)) {
                if(RiftAP.songDatabaseDict.TryGetValue(levelName, out var value)) {
                    Log.Info($"UnlockRemix: Unlocking \"{songName} (Remix)\"");
                    baseRemixUnlocked.Add(songName);
            
                    foreach(DifficultyInformation diff in value.DifficultyInformation) {
                        diff.RemixUnlockCriteria.Type = UnlockCriteriaType.None;
                        Log.Info($"UnlockRemix: Chaning Unlock State {diff.UnlockCriteria.Type}");
                    }
                }
            }
            
            else {
                Log.Info($"UnlockRemix: Unlocking \"{songName} (Remix)\" (Post Anniversary DLC Song)");
                dlcRemixUnlocked.Add(songName);
            }
        }

        public void UnlockExtra(string songName) {
            if(songName.Contains("(Medium)")) {
                Log.Info($"UnlockExtra: Unlocking \"{songName}\"");
                extraMapping.TryGetValue(songName.Substring(0, songName.Length - 9), out var value);
                extraMedium.Add(value);
            }

            else if(songName.Contains("(Hard)")) {
                Log.Info($"UnlockExtra: Unlocking \"{songName}\"");
                extraMapping.TryGetValue(songName.Substring(0, songName.Length - 7), out var value);
                extraHard.Add(value);
            }

            else {
                Log.Info($"UnlockExtra: Unlocking \"{songName}\"");
                extraMapping.TryGetValue(songName, out var value);
                extraMedium.Add(value);
                extraHard.Add(value);
            }
        }

        public void UnlockCustom(string songName) {
            Log.Info($"UnlockCustom: Unlocking \"{songName}\"");
            customUnlocked.Add(songName);
        }
    }
}