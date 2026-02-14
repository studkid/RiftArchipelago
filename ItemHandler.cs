using Shared.PlayerData;
using Shared.TrackSelection;
using System.Collections.Generic;

namespace RiftArchipelago{
    public static class ItemHandler {
        private static Dictionary<string, string> songMapping = new Dictionary<string, string>() {
            {"Disco Disaster", "RRDiscoDisaster"} ,
            {"Elusional", "RRElusional"},
            {"Visualize Yourself", "RRVisualizeYourself"},
            {"Spookhouse Pop", "RRSpookhousePop"},
            {"Om and On", "RROmandOn"},
            {"Morning Dove", "RRMorningDove"},
            {"Heph's Mess", "RRHephsMess"},
            {"Amalgamaniac", "RRAmalgamaniac"},
            {"Hang Ten Heph", "RRHangTenHeph"},
            {"Count Funkula", "RRCountFunkula"},
            {"Overthinker", "RROverthinker"},
            {"Cryp2que", "RRCryp2que"},
            {"Nocturning", "RRNocturning"},
            {"Glass Cages (feat. Sarah Hubbard)", "RRGlassCages"},
            {"Hallow Queen", "RRHallowQueen"},
            {"Progenitor", "RRProgenitor"},
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

        private static Dictionary<string, string> extraMapping = new Dictionary<string, string>() {
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

        public static Dictionary<string, SongDatabaseData> songDatabaseDict;
        public static List<string> dlcSongUnlocked = [];
        public static List<string> dlcRemixUnlocked = [];
        public static List<string> customUnlocked = [];
        public static List<string> extraMedium = [];
        public static List<string> extraHard = [];
        public static bool databaseInit = false;
        public static bool dlcDatabaseInit = false;
        public static int diamondCount {get; private set;}

        public static void Setup() {
            diamondCount = 0;
            RiftAP._log.LogInfo($"IH Setup: Setting up song Dict");

            foreach(SongDatabaseData song in songDatabaseDict.Values) {
                foreach(DifficultyInformation diff in song.DifficultyInformation) {
                    diff.UnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                    diff.RemixUnlockCriteria.Type = UnlockCriteriaType.AlwaysLocked;
                }
            }
        }

        public static void AddDiamond() {
            diamondCount += 1;
            RiftAP._log.LogInfo($"AddDiamond: Adding Diamond | New Total: {diamondCount}");

            if(diamondCount >= ArchipelagoClient.slotData.diamondGoal) {
                string goalSong = ArchipelagoClient.slotData.goalSong;
                string goalType = ArchipelagoClient.slotData.goalType;

                if(goalType == "Minigame" || goalType == "Boss") {
                    UnlockExtra(goalSong);
                }

                else if(goalType == "Remix") {
                    UnlockRemix(goalSong.Substring(0, goalSong.Length - 8));
                }

                else {
                    UnlockSong(goalSong);
                }
            }
        }

        public static void UnlockSong(string songName) {
            if(songMapping.TryGetValue(songName, out var levelName)) {
                if(songDatabaseDict.TryGetValue(levelName, out var value)) {
                    RiftAP._log.LogInfo($"UnlockSong: Unlocking \"{songName}\"");
            
                    foreach(DifficultyInformation diff in value.DifficultyInformation) {
                        diff.UnlockCriteria.Type = UnlockCriteriaType.None;
                        if(!ArchipelagoClient.slotData.remix) {
                            diff.RemixUnlockCriteria.Type = UnlockCriteriaType.None;
                        }
                    }
                }
            }
            
            else {
                RiftAP._log.LogInfo($"UnlockSong: Unlocking \"{songName}\" (Post Anniversary DLC Song)");
                dlcSongUnlocked.Add(songName);
            }
        }

        public static void UnlockRemix(string songName) {
            if(songMapping.TryGetValue(songName, out var levelName)) {
                if(songDatabaseDict.TryGetValue(levelName, out var value)) {
                    RiftAP._log.LogInfo($"UnlockRemix: Unlocking \"{songName} (Remix)\"");
            
                    foreach(DifficultyInformation diff in value.DifficultyInformation) {
                        diff.RemixUnlockCriteria.Type = UnlockCriteriaType.None;
                    }
                }
            }
            
            else {
                RiftAP._log.LogInfo($"UnlockRemix: Unlocking \"{songName} (Remix)\" (Post Anniversary DLC Song)");
                dlcRemixUnlocked.Add(songName);
            }
        }

        public static void UnlockExtra(string songName) {
            if(songName.Contains("(Medium)")) {
                RiftAP._log.LogInfo($"UnlockExtra: Unlocking \"{songName}\"");
                extraMapping.TryGetValue(songName.Substring(0, songName.Length - 9), out var value);
                extraMedium.Add(value);
            }

            else if(songName.Contains("(Hard)")) {
                RiftAP._log.LogInfo($"UnlockExtra: Unlocking \"{songName}\"");
                extraMapping.TryGetValue(songName.Substring(0, songName.Length - 7), out var value);
                extraHard.Add(value);
            }

            else {
                RiftAP._log.LogInfo($"UnlockExtra: Unlocking \"{songName}\"");
                extraMapping.TryGetValue(songName, out var value);
                extraMedium.Add(value);
                extraHard.Add(value);
            }
        }

        public static void UnlockCustom(string songName) {
            RiftAP._log.LogInfo($"UnlockCustom: Unlocking \"{songName}\"");
            customUnlocked.Add(songName);
        }
    }
}