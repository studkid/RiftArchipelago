using BepInEx;
using RiftOfTheNecroManager;
using Shared.TrackSelection;
using System.Collections.Generic;
using UnityEngine;
using Shared.PlayerData;
using HarmonyLib;

namespace RiftArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class RiftAP : RiftPlugin {
    public static ArchipelagoClient apClient;
    public static ArchipelagoUI apUI;
    public static Dictionary<string, SongDatabaseData> songDatabaseDict;

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

        CreateClient();
        CreateUI();

        songDatabaseDict = SongDatabase._songDatabaseDict;
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
}
