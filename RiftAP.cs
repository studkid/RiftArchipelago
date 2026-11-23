using BepInEx;
using RiftOfTheNecroManager;
using Shared.TrackSelection;
using System.Collections.Generic;
using UnityEngine;
using Shared.PlayerData;
using HarmonyLib;
using Shared.TrackData;
using System.IO;
using Shared;
using System;
using RiftArchipelago.Helpers;
using System.Collections.Concurrent;

namespace RiftArchipelago;

public enum APState {
    Menu,
    InGame,
    SongMenu,
    CustomMenu,
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, VERSION)]
[NecroManagerInfo(menuNameOverride: MENU_NAME, isBeta: true)]
public class RiftAP : RiftPlugin {
    public const string VERSION = "1.0.0";
    public const string MENU_NAME = "Rift Archipelago";
    public static ArchipelagoClient apClient;
    public static ArchipelagoUI apUI;
    public static Dictionary<string, SongDatabaseData> songDatabaseDict;
    public static APState state;
    public static List<ITrackMetadata> dynamicMetadataList;
    public static Texture2D[] apIcons;
    public static ConcurrentQueue<long> scoutQueue = new();

    public static void ResetEverything() {
        foreach(SongDatabaseData song in songDatabaseDict.Values) {
            foreach(DifficultyInformation diff in song.DifficultyInformation) {
                diff.UnlockCriteria.Type = UnlockCriteriaType.TotalDiamonds;
                diff.RemixUnlockCriteria.Type = UnlockCriteriaType.TotalDiamonds;
            }
        }

        CreateClient();
    }

    protected override void OnInit() {
        base.OnInit();

        songDatabaseDict = SongDatabase._songDatabaseDict;
        state = APState.Menu;

        apIcons = [
            AssetHelpers.LoadTexture("RiftArchipelago.Assets.filler-icon.png"),
            AssetHelpers.LoadTexture("RiftArchipelago.Assets.progression-icon.png"),
            AssetHelpers.LoadTexture("RiftArchipelago.Assets.useful-icon.png"),
            AssetHelpers.LoadTexture("RiftArchipelago.Assets.trap-icon.png")
        ];

        CreateClient();
        CreateUI();
    }

    private static void CreateClient() {
        if(apClient) {
            Destroy(apClient.gameObject);
        }

        var clientGameObject = new GameObject("AP Client");
        apClient = clientGameObject.AddComponent<ArchipelagoClient>();
        DontDestroyOnLoad(apClient);
    }

    private static void CreateUI() {
        if(apUI) return;
        
        var guiGameObject = new GameObject("AP UI");
        apUI = guiGameObject.AddComponent<ArchipelagoUI>();
        DontDestroyOnLoad(guiGameObject);
    }

    public static void DumpSongData() {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Output");
        int i = 90;
        foreach(ITrackMetadata song in dynamicMetadataList) {
            using(StreamWriter output = new StreamWriter(Path.Combine(path, "dlcsongdata.txt"), true)) {
                output.WriteLine($"\"{song.TrackName}\": SongData({i}, \"{song.TrackName}\", \"{song.Counterpart}\", {song.GetDifficulty(Difficulty.Easy).Intensity}, {song.GetDifficulty(Difficulty.Medium).Intensity}, {song.GetDifficulty(Difficulty.Hard).Intensity}, {song.GetDifficulty(Difficulty.Impossible).Intensity}),");
            }
            i++;
        }
    }
}
