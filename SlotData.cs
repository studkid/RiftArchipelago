using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using RiftOfTheNecroManager;

namespace RiftArchipelago {

    public class SlotData {
        public int diamondGoal {get; private set;}
        public int songClearCount {get; private set;}
        public int goalCondition {get; private set;}
        public string goalSong {get; private set;}
        public string goalType {get; private set;}
        public int startingHealth {get; private set;}
        public Grade gradeNeeded {get; private set;}
        public bool remix {get; private set;}
        public int mgMode {get; private set;}
        public int bbMode {get; private set;}
        public bool fullComboNeeded {get; private set;}
        public JArray worldVersion {get; private set;}

        public SlotData(Dictionary<string, object> slotData) {
            if(slotData.TryGetValue("diamondWinCount", out var diamond_goal)) {
                try {
                    diamondGoal = ParseInt(diamond_goal);
                }
                catch {
                    Log.Error("Failed to get diamondGoal from Slot Data.  Something went very wrong");
                    diamondGoal = 0;
                }
            }
            if(slotData.TryGetValue("songClearCount", out var song_clear_count)) {
                try {
                    songClearCount = ParseInt(song_clear_count);
                }
                catch {
                    Log.Error("Failed to get songClearCount from Slot Data.  Something went very wrong.");
                    songClearCount = 0;
                }
            }
            if(slotData.TryGetValue("goalCondition", out var goal_condition)) {
                try {
                    goalCondition = ParseInt(goal_condition);
                }
                catch {
                    Log.Error("Failed to get goalCondition from Slot Data.  Something went very wrong.");
                    goalCondition = 0;
                }
            }
            else { // Backwards Compat
                goalCondition = 0;
            }
            if(slotData.TryGetValue("victoryLocation", out var victory)) {
                try {
                    goalSong = (string) victory;
                }
                catch {
                    Log.Error("Failed to get victoryLocation from Slot Data.  Something went very wrong");
                    goalSong = "Goal Song Invalid";
                }
            }
            if(slotData.TryGetValue("victoryType", out var victory_type)) {
                try {
                    goalType = (string) victory_type;
                }
                catch {
                    Log.Error("Failed to get victoryType from Slot Data.  Something went very wrong");
                    goalType = "Rift";
                }
            }
            if(slotData.TryGetValue("startingHealth", out var starting_health)) {
                try {
                    startingHealth = ParseInt(starting_health);
                }
                catch {
                    Log.Error("Failed to get startingHealth from Slot Data.  Something went very wrong.");
                    startingHealth = 10;
                }
            }
            if(slotData.TryGetValue("remixes", out var remixes)) {
                try {
                remix = Convert.ToBoolean(remixes);
                }
                catch {
                    Log.Error("Failed to get remixes from Slot Data.  Something went very wrong");
                    remix = false;
                }
            }
            if(slotData.TryGetValue("minigameMode", out var mg_mode)) {
                try {
                    mgMode = ParseInt(mg_mode);
                }
                catch {
                    Log.Error("Failed to get minigameMode from Slot Data.  Something went very wrong");
                    mgMode = 0;
                }
            }
            if(slotData.TryGetValue("bossMode", out var boss_mode)) {
                try{
                    bbMode = ParseInt(boss_mode);
                }
                catch {
                    Log.Error("Failed to get bossMode from Slot Data.  Something went very wrong");
                    bbMode = 0;
                }
            }
            if (slotData.TryGetValue("gradeNeeded", out var grade_needed)) {
                try{
                    gradeNeeded = MapObjectToGrade(grade_needed);
                }
                catch {
                    Log.Error("Failed to get gradeNeeded from Slot Data.  Something went very wrong");
                    gradeNeeded = Grade.Any;
                }
            }
            if (slotData.TryGetValue("fullComboNeeded", out var fc_needed)) {
                try {
                    fullComboNeeded = Convert.ToBoolean(fc_needed);
                }
                catch {
                    Log.Error("Failed to get fullComboNeeded from Slot Data.  Something went very wrong");
                    fullComboNeeded = false;
                }
            }
            if (slotData.TryGetValue("worldVersion", out var world_version)) {
                try {
                    worldVersion = (JArray) world_version;
                }
                catch {
                    Log.Error("Failed to get world_version from Slot Data.  Something went very wrong.");
                    worldVersion = new JArray() {0,0,0};
                }
            }
            else {
                Log.Error("Failed to get world_version from Slot Data.  Server is likely on an outdated world.  Server compatibility not guarenteed!");
                worldVersion = new JArray() {0,0,0};
            }
        }

        private int ParseInt(object i) {
            return int.TryParse(i.ToString(), out var result) ? result : -1;
        }
          
        public static Grade MapObjectToGrade(object g) {
            if (g == null) return Grade.Any;
            var s = g.ToString().Trim().ToUpper();

            // If it gets sent as an enum value
            if (int.TryParse(s, out var n)) {
                return n switch {
                    0 => Grade.Any,
                    1 => Grade.C,
                    2 => Grade.B,
                    3 => Grade.A,
                    4 => Grade.S,
                    5 => Grade.SS,
                    _ => Grade.Any,
                };
            }

            // If it is sent as a string
            var up = s.ToUpperInvariant();
            if (up == "S_PLUS") return Grade.SS;
            else if (up == "SS") return Grade.SS;
            else if (up == "S") return Grade.S;
            else if (up == "A") return Grade.A;
            else if (up == "B") return Grade.B;
            else if (up == "C") return Grade.C;
            else return Grade.Any;
        }

        public enum Grade {
            Any = 0,
            C = 1,
            B = 2,
            A = 3,
            S = 4,
            SS = 5
        }
    }
}
