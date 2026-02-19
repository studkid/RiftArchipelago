using HarmonyLib;

using Shared;
using System.Threading.Tasks;

using Shared.RhythmEngine;
using Shared.Leaderboard;
using RhythmRift;
using System.Collections;
using System.Collections.Generic;
using Shared.SceneLoading.Payloads;
using System.Reflection;
using Minigames;
using BossBattles;
using System.Linq;

namespace RiftArchipelago.Patches {
    [HarmonyPatch(typeof(RRStageController), "CompleteStageAfterAllEnemiesHaveDiedRoutine")]
    public static class RRCompleteStage {
        //  fast access to private fields on RRStageController
        private static LetterGradeDefinitions _letterGradeDefinitions = null;
        private static readonly AccessTools.FieldRef<RRStageController, StageInputRecord> _stageInputRecordRef =
            AccessTools.FieldRefAccess<RRStageController, StageInputRecord>("_stageInputRecord");
        private static StageInputRecord _stageInputRecord = null;

        private static readonly AccessTools.FieldRef<RRStageController, StageScenePayload> _stageScenePayloadRef =
            AccessTools.FieldRefAccess<RRStageController, StageScenePayload>("_stageScenePayload");
        private static StageScenePayload _stageScenePayload = null;

        private static readonly AccessTools.FieldRef<RRStageController, StageFlowUiController> _stageFlowUiControllerRef =
            AccessTools.FieldRefAccess<RRStageController, StageFlowUiController>("_stageFlowUiController");
        private static StageFlowUiController _stageFlowUiController = null;

        private static StageFlowUiController.StageContextInfo _stageContextInfo;

        private static readonly AccessTools.FieldRef<RRStageController, bool> _isMicroRiftRef =
            AccessTools.FieldRefAccess<RRStageController, bool>("_isMicroRift");
        private static readonly AccessTools.FieldRef<RRStageController, bool> _wereCheatsUsedRef =
            AccessTools.FieldRefAccess<RRStageController, bool>("_wereCheatsUsed");
        private static bool _isMicroRift = false;
        private static bool _isTutorial = false;
        private static bool _isPracticeMode = false;
        private static bool _wereCheatsUsed = false;
        private static bool _isRemixMode = false;
        private static bool _isDailyChallenge = false;

        // Runs at the end of the method that handles successfully beating a stage
        [HarmonyPostfix]
        public static void PostFix(RRStageController __instance, ref string ____customTrackAudioFilePath, ref IEnumerator __result) {
            // Invalid if not connected to Archipelago
            if (!ArchipelagoClient.isAuthenticated || ArchipelagoClient.freePlay) return;


            var original = __result;
            __result = Wrapper(__instance, ____customTrackAudioFilePath);

            IEnumerator Wrapper(RRStageController __instance, string ____customTrackAudioFilePath) {
                yield return original;

                TrySendLocation(__instance, ____customTrackAudioFilePath);
            }
        }

        public static void TrySendLocation(RRStageController __instance, string ____customTrackAudioFilePath) {
            try {
                    if (__instance == null) {
                        RiftAP._log.LogError("RRStageEnd PostFix: __instance == null");
                        return;
                    }

                    // Get private fields
                    try {
                        GetPrivateFields(__instance);
                    }
                    catch (System.Exception ex) {
                        RiftAP._log.LogError($"Error getting private fields in RRStageEnd PostFix: {ex}");
                        return;
                    }

                    // Double check to ensure no null values
                    if (_letterGradeDefinitions == null || _stageInputRecord == null || _stageScenePayload == null) {
                        RiftAP._log.LogWarning("RRStageEnd PostFix: One or more private fields are null");
                        return;
                    }

                    // Copy letter grade calculation from the original files
                    float percentage = (float)_stageInputRecord.TotalScore / _stageInputRecord.BaseStageScore * 100f;
                    string letter = _letterGradeDefinitions.GetLetterGradeForPercentage(percentage);
                    if (_isMicroRift && letter != LetterGradeDefinitions.LetterGrades.SS.ToString()) {
                        List<float> thresholds = _letterGradeDefinitions.GetLetterGradePercentageThresholdsInOrder();
                        if (thresholds != null && thresholds.Count > 0) {
                            float num = thresholds[thresholds.Count - 1];
                            int extraScore = (int)((float)_stageInputRecord.BaseStageScore * num) + 1 - _stageInputRecord.TotalScore;
                            _stageInputRecord.AddMicroRiftCompletionBonus(extraScore);
                            letter = LetterGradeDefinitions.LetterGrades.SS.ToString();
                        }
                    }

                    string levelId = _stageScenePayload.GetLevelId();
                    string stageDisplayName = _stageContextInfo.StageDisplayName;
                    bool wasFullCombo = _stageInputRecord.TotalMisses == 0 && _stageInputRecord.TotalErrants == 0;
                    bool isPlayingCustomTrack = !string.IsNullOrWhiteSpace(____customTrackAudioFilePath);

                    try {
                        if (VerifyCompletionRequirements(stageDisplayName, levelId, _stageScenePayload.GetLevelDifficulty(), SlotData.MapObjectToGrade(letter), wasFullCombo, _wereCheatsUsed)) {
                            RiftAP._log.LogInfo("Archipelago location verification validated");
                            if(isPlayingCustomTrack) {
                                string songName = $"{stageDisplayName} [{levelId}]";
                                AP_RRLocationSend(songName.Replace("\'", ""), levelId, _stageScenePayload.GetLevelDifficulty(), _isRemixMode);
                            }
                            AP_RRLocationSend(stageDisplayName, levelId, _stageScenePayload.GetLevelDifficulty(), _isRemixMode);
                        }
                        else {
                            RiftAP._log.LogInfo("Archipelago location verification failed. Not sending check.");
                        }
                    }
                    catch (System.Exception ex) {
                        RiftAP._log.LogError($"Error in APLocationSend: {ex}");
                    }
                }
                catch (System.Exception ex) {
                    RiftAP._log.LogError($"Error in RRStageEnd PostFix: {ex}");
                }
        }

        public static bool VerifyCompletionRequirements(string stageDisplayName, string levelId, Difficulty difficulty,
                                            SlotData.Grade letterGrade, bool isFullCombo, bool cheatsDetected) {
            // Valid if golden lute used
            if (cheatsDetected) return true;

            // Invalid if any illegal modes were detected
            if (_isTutorial || _isPracticeMode || _isDailyChallenge) {
                RiftAP._log.LogInfo("Tutorial, Practice, or Challenge mode detected.");
                return false;
            }

            // Print out for debugging purposes
            RiftAP._log.LogInfo($"Song Completed! Stage: {stageDisplayName} | Level ID: {levelId} | Difficulty: {_stageScenePayload.GetLevelDifficulty()} | Remix Mode: {_isRemixMode}");

            // Full combo check
            if (ArchipelagoClient.slotData.fullComboNeeded && !isFullCombo) {
                RiftAP._log.LogInfo("Full Combo required but not achieved");
                return false;
            }

            // Grade threshold check
            SlotData.Grade gradeNeeded = ArchipelagoClient.slotData.gradeNeeded;
            if (letterGrade < gradeNeeded) {
                RiftAP._log.LogInfo($"Grade {letterGrade} does not meet the requirement of {gradeNeeded}");
                return false;
            }

            return true;
        }

        public static void AP_RRLocationSend(string stageDisplayName, string levelId, Difficulty difficulty, bool isRemixMode) {
            long locId = -1;

            if (!ArchipelagoClient.slotData.remix || !isRemixMode) {
                if (stageDisplayName == ArchipelagoClient.slotData.goalSong)
                {
                    ArchipelagoClient.GoalGame();
                }

                locId = ArchipelagoClient.session.Locations.GetLocationIdFromName("Rift of the Necrodancer", stageDisplayName + "-0");
            }
            else {
                if (stageDisplayName + " (Remix)" == ArchipelagoClient.slotData.goalSong) {
                    ArchipelagoClient.GoalGame();
                }

                locId = ArchipelagoClient.session.Locations.GetLocationIdFromName("Rift of the Necrodancer", stageDisplayName + " (Remix)-0");
            }


            RiftAP._log.LogInfo($"Sending {stageDisplayName} {locId}");

            if (locId != -1) {
                ArchipelagoClient.session.Locations.CompleteLocationChecksAsync([locId, locId + 1]);
            }
        }

        public static void GetPrivateFields(RRStageController __instance) {
            // BeatmapPlayer: try property first, then field
            object beatmapPlayer = null;
            try { beatmapPlayer = Traverse.Create(__instance).Property("BeatmapPlayer").GetValue(); } catch { }
            if (beatmapPlayer == null) {
                try { beatmapPlayer = Traverse.Create(__instance).Field("BeatmapPlayer").GetValue(); } catch { }
            }
            if (beatmapPlayer == null) {
                RiftAP._log.LogWarning("RRStageEnd PostFix: BeatmapPlayer == null");
                return;
            }

            // LetterGradeDefinitions: property then field
            try { _letterGradeDefinitions = Traverse.Create(beatmapPlayer).Property("LetterGradeDefinitions").GetValue<LetterGradeDefinitions>(); } catch { }
            if (_letterGradeDefinitions == null) {
                try { _letterGradeDefinitions = Traverse.Create(beatmapPlayer).Field("LetterGradeDefinitions").GetValue<LetterGradeDefinitions>(); } catch { }
            }
            if (_letterGradeDefinitions == null) {
                RiftAP._log.LogWarning("RRStageEnd PostFix: LetterGradeDefinitions == null");
                return;
            }

            // stageInputRecord: preferred via FieldRef, fallback to Traverse
            try { _stageInputRecord = _stageInputRecordRef(__instance); } catch { }
            if (_stageInputRecord == null) {
                try { _stageInputRecord = Traverse.Create(__instance).Field("_stageInputRecord").GetValue<StageInputRecord>(); } catch { }
            }
            if (_stageInputRecord == null) {
                RiftAP._log.LogWarning("RRStageEnd PostFix: _stageInputRecord == null");
                return;
            }

            // stageFlowUiController: preferred via FieldRef, fallback to Traverse
            try { _stageFlowUiController = _stageFlowUiControllerRef(__instance); } catch { }
            if (_stageFlowUiController == null) {
                try { _stageFlowUiController = Traverse.Create(__instance).Field("_stageFlowUiController").GetValue<StageFlowUiController>(); } catch { }
            }
            if (_stageFlowUiController == null) {
                RiftAP._log.LogWarning("RRStageEnd PostFix: _stageFlowUiController == null");
                return;
            }

            // Get DisplayName
            try {
                _stageContextInfo = (StageFlowUiController.StageContextInfo)typeof(StageFlowUiController).InvokeMember("_stageContextInfo", BindingFlags.GetField
                        | BindingFlags.Instance | BindingFlags.NonPublic, null, _stageFlowUiController, null);
            }
            catch (System.Exception e) {
                RiftAP._log.LogWarning(e.StackTrace + e.Message);
                return;
            }

            // stageScenePayload: preferred via FieldRef, fallback to Traverse
            try { _stageScenePayload = _stageScenePayloadRef(__instance); } catch { }
            if (_stageScenePayload == null) {
                try { _stageScenePayload = Traverse.Create(__instance).Field("_stageScenePayload").GetValue<StageScenePayload>(); } catch { }
            }
            if (_stageInputRecord == null) {
                RiftAP._log.LogWarning("RRStageEnd PostFix: _stageInputRecord == null");
                return;
            }

            try { _isMicroRift = _isMicroRiftRef(__instance); }
            catch {
                try { _isMicroRift = Traverse.Create(__instance).Field("_isMicroRift").GetValue<bool>(); } catch { }
            }

            try { _wereCheatsUsed = _wereCheatsUsedRef(__instance); }
            catch {
                try { _wereCheatsUsed = Traverse.Create(__instance).Field("_wereCheatsUsed").GetValue<bool>(); } catch { }
            }

            try { _isTutorial = _stageScenePayload.GetIsTutorial(); }
            catch { _isTutorial = false; }

            try { _isPracticeMode = _stageScenePayload.IsPracticeMode; }
            catch { _isPracticeMode = false; }

            try { _isRemixMode = _stageScenePayload.ShouldProcGen; }
            catch { _isRemixMode = false; }

            try { _isDailyChallenge = _stageScenePayload.IsDailyChallenge; }
            catch { _isDailyChallenge = false; }
        }
    }

    // Minigame Handling
    [HarmonyPatch(typeof(MinigameBaseStageController<MinigameBeatmapPlayer>), "MinigameCompleteStageRoutine")]
    public static class APMGLocationSend
    {
        [HarmonyPostfix]
        public static void PostFix(ref IEnumerator __result, StageScenePayload ____stageScenePayload) {
            if (!ArchipelagoClient.isAuthenticated) return;

            var original = __result;
            __result = Wrapper();

            IEnumerator Wrapper() {
                yield return original;

                string levelName = ItemHandler.extraMapping.FirstOrDefault(x => x.Value == ____stageScenePayload.GetLevelId()).Key;
                Difficulty difficulty = ____stageScenePayload.GetLevelDifficulty();

                RiftAP._log.LogInfo("Minigame Cleared");
                APExtraLocSend.sendExtra(levelName, difficulty, ArchipelagoClient.slotData.mgMode);
        }
    }

    // Boss Battle Handling
    [HarmonyPatch(typeof(BossBattleStageController), "CompleteStageRoutine")]
    public static class APBBLocationSend
    {
        [HarmonyPostfix]
        public static void PostFix(ref IEnumerator __result, StageScenePayload ____stageScenePayload, string ____resultsBossName) {
            if (!ArchipelagoClient.isAuthenticated) return;

            var original = __result;
            __result = Wrapper();

            IEnumerator Wrapper() {
                yield return original;

                Difficulty difficulty = ____stageScenePayload.GetLevelDifficulty();

                RiftAP._log.LogInfo("Boss Battle Cleared");
                APExtraLocSend.sendExtra(____resultsBossName, difficulty, ArchipelagoClient.slotData.bbMode);
            }
        }
    }

    public static class APExtraLocSend {
        public static void sendExtra(string levelName, Difficulty difficulty, int mode) {
            long locId = -1;

            if (mode == 1) {
                    if (levelName == ArchipelagoClient.slotData.goalSong) {
                        ArchipelagoClient.GoalGame();
                    }

                    locId = ArchipelagoClient.session.Locations.GetLocationIdFromName("Rift of the Necrodancer", levelName + "-0");
                }
                else if (mode == 2) {
                    if ($"{levelName} ({difficulty})" == ArchipelagoClient.slotData.goalSong) {
                        ArchipelagoClient.GoalGame();
                    }

                    locId = ArchipelagoClient.session.Locations.GetLocationIdFromName("Rift of the Necrodancer", $"{levelName} ({difficulty})-0");
                }


                RiftAP._log.LogInfo($"Sending {levelName} {locId}");

                if (locId != -1) {
                    ArchipelagoClient.session.Locations.CompleteLocationChecksAsync([locId, locId + 1]);
                }
            }
        }
    }

    [HarmonyPatch(typeof(LeaderboardDataAccessor), "UploadScoreToLeaderboard")]
    public static class LeaderboardUploadOverride
    {
        // Prevent uploading scores to the leaderboard if connected to Archipelago
        private static bool Prefix(out Task<bool> __result) {
            RiftAP._log.LogInfo($"Uploading Score: {!ArchipelagoClient.isAuthenticated}");
            __result = Task.FromResult(false);
            return !ArchipelagoClient.isAuthenticated;
        }
    }
}