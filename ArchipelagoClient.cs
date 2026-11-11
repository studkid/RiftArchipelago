using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using RhythmRift;
using UnityEngine;
using RiftOfTheNecroManager;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using RiftArchipelago.Patches;

namespace RiftArchipelago;

public class ArchipelagoClient : MonoBehaviour {
    public int[] AP_VERSION = {0, 6, 7};
    public const string GAME_NAME = "Rift of the Necrodancer";

    public ItemHandler itemHandler = new();
    private ConcurrentQueue<ItemInfo> _itemQueue = new();
    public bool isAuthenticated = false;
    public ArchipelagoInfo apInfo = new ArchipelagoInfo();
    public ArchipelagoUI apUI = new ArchipelagoUI();
    public int slotID;
    public ArchipelagoSession session;
    public SlotData slotData;
    public bool isInGame = false;
    public DeathLinkService deathLinkService;
    public RRStageController rrStageController;
    public bool freePlay = false;
    public List<string> unplayedSongs = [];

    public bool Connect() {
        if (isAuthenticated) {
            return true;
        }

        if (apInfo.address is null || apInfo.address.Length == 0) {
            return false;
        }

        session = ArchipelagoSessionFactory.CreateSession(apInfo.address);
        deathLinkService = session.CreateDeathLinkService();

        string[] tags = [];

        if(Config.Settings.Deathlink != DeathLinkType.Off) {
            tags.AddItem("DeathLink");
        }

        if(Config.Settings.Traplink) {
            tags.AddItem("TrapLink");
        }
        
        LoginResult loginResult = session.TryConnectAndLogin(
            GAME_NAME,
            apInfo.slot,
            ItemsHandlingFlags.AllItems,
            new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
            tags,
            "",
            apInfo.password
        );

        if (loginResult is LoginSuccessful loginSuccess) {
            isAuthenticated = true;
            slotData = new SlotData(loginSuccess.SlotData);

            session.Items.ItemReceived += Session_ItemReceived;

            foreach(long location in session.Locations.AllMissingLocations) {
                string locName = session.Locations.GetLocationNameFromId(location);
                unplayedSongs.Add(slotData.goalSong);
                unplayedSongs.Add(locName.Substring(0, locName.Length - 2));
            }
            
            return true;
        }

        return false;
    }
    
    public void GoalGame() {
        Log.Info("Goal Reached!");
        var statusUpdatePacket = new StatusUpdatePacket {
            Status = ArchipelagoClientState.ClientGoal
        };
        session.Socket.SendPacket(statusUpdatePacket);
    }

    private void Session_ItemReceived(ReceivedItemsHelper helper) {
        var item = helper.DequeueItem();

        _itemQueue.Enqueue(item);
    }

    private void Update() {
        if(!_itemQueue.IsEmpty) {
            _itemQueue.TryDequeue(out var item);

            Log.Info($"Item Recieved: {item.ItemDisplayName} | ID: {item.ItemId}");
            Log.Info($"Slot Data Loaded: {slotData.remix}");

            if(item.ItemId == 1) { // Diamonds
                itemHandler.AddDiamond();
            }
            else if(item.ItemId >= 30 && item.ItemId < 50) {
                TrapHelper.AddTrap(item.ItemName);
            }
            else if(item.ItemId >= 50 && item.ItemId < 1000) { // Rhythm Rifts
                itemHandler.UnlockSong(item.ItemName);
            }
            else if(item.ItemId >= 1000 && item.ItemId < 2000) { // Remixes
                itemHandler.UnlockRemix(item.ItemName.Substring(0, item.ItemName.Length - 8));
            }
            else if(item.ItemId >= 2000 && item.ItemId < 3000) { //Minigames/Bosses
                itemHandler.UnlockExtra(item.ItemName);
            }
            else if(item.ItemId >= 5000) { // Custom Rifts
                itemHandler.UnlockCustom(item.ItemName);
            }
        }
    }

    public void SendDeathLink(string msg) {
        deathLinkService.SendDeathLink(new DeathLink(apInfo.slot, msg));
    }

    private void Session_DeathlinkRecieved(Dictionary<string, JToken> packet) {


        JToken cause = "";
        JToken source = "";

        packet.TryGetValue("source", out source);
        packet.TryGetValue("cause", out cause);


    }
}