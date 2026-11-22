using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using RhythmRift;
using UnityEngine;
using RiftOfTheNecroManager;
using Helpers.RiftArchipelago;
using Archipelago.MultiClient.Net.Converters;
using System.Linq;

namespace RiftArchipelago;

public class ArchipelagoClient : MonoBehaviour {
    public int[] AP_VERSION = {0, 6, 7};
    public const string GAME_NAME = "Rift of the Necrodancer";

    public ItemHandler itemHandler = new();
    private ConcurrentQueue<ItemInfo> _itemQueue = new();
    public bool isAuthenticated = false;
    public ArchipelagoInfo apInfo = new ArchipelagoInfo();
    public int slotID;
    public ArchipelagoSession session;
    public SlotData slotData;
    public bool isInGame = false;
    public RRStageController rrStageController;
    public bool freePlay = false;
    public List<string> unplayedSongs = [];
    public Dictionary<long, ScoutedItemInfo> locScouts = new Dictionary<long, ScoutedItemInfo>() {};
    public static Dictionary<string, int> trapCount = new Dictionary<string, int>() {
            {"Mystery Trap", 0},
            {"Gloom Trap", 0},
            {"Nightmare Trap", 0},
            {"Coda Trap", 0},
            {"Glass Trap", 0},
            {"Upscroll Trap", 0},
        };

    public bool Connect() {
        if (isAuthenticated) {
            return true;
        }

        if (apInfo.address is null || apInfo.address.Length == 0) {
            return false;
        }

        session = ArchipelagoSessionFactory.CreateSession(apInfo.address);

        List<string> tags = new List<string>();

        if(Config.Settings.Deathlink != DeathLinkType.Off) {
            tags.Add("DeathLink" + Config.Settings.DeathLinkGroup);
        }

        if(Config.Settings.Traplink) {
            tags.Add("TrapLink");
        }
        
        LoginResult loginResult = session.TryConnectAndLogin(
            GAME_NAME,
            apInfo.slot,
            ItemsHandlingFlags.AllItems,
            new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
            tags.ToArray(),
            "",
            apInfo.password
        );

        if (loginResult is LoginSuccessful loginSuccess) {
            isAuthenticated = true;
            slotData = new SlotData(loginSuccess.SlotData);

            session.Items.ItemReceived += Session_ItemReceived;
            session.Socket.PacketReceived += Socket_PacketRecived;

            foreach(long location in session.Locations.AllMissingLocations) {
                string locName = session.Locations.GetLocationNameFromId(location);
                unplayedSongs.Add(slotData.goalSong);
                unplayedSongs.Add(locName.Substring(0, locName.Length - 2));
            }

            ProcessTrapData();
            GetLocationScouts();
            
            return true;
        }

        return false;
    }

    private void ProcessTrapData() {
        string[] keys = TrapHelper.processedTrapCount.Keys.ToArray();
        foreach(string trap in keys) {
            session.DataStorage[Scope.Slot, trap].Initialize(0);
            int count = session.DataStorage[Scope.Slot, trap];
            TrapHelper.processedTrapCount[trap] = count;
            Log.Info($"ProcessTrapData: {trap} {count}");
        }
    }

    private async void GetLocationScouts() {
        long[] missingLocations = session.Locations.AllMissingLocations.ToArray();
        locScouts = await session.Locations.ScoutLocationsAsync(HintCreationPolicy.None, missingLocations);
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

            if(item.ItemId == 1) { // Diamonds
                itemHandler.AddDiamond();
            }
            else if(item.ItemId >= 20 && item.ItemId < 50) {
                trapCount[item.ItemName]++;
                if(trapCount[item.ItemName] > TrapHelper.processedTrapCount[item.ItemName]) {
                    AddTrapToStorage(item.ItemName);
                    TrapHelper.AddTrap(item.ItemName);
                    if(Config.Settings.Traplink) {
                        SendTrapLink(item.ItemName);
                    }
                }
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
        Log.Info("SendDeathLink: " + msg);
        BouncedPacket packet = new BouncedPacket() {
            Tags = ["DeathLink" + Config.Settings.DeathLinkGroup],
            Data = new() {
                { "time", DateTime.UtcNow.ToUnixTimeStamp() },
                { "source", session.Players.GetPlayerName(session.ConnectionInfo.Slot) },
                { "cause", msg },
            }
        };
        session.Socket.SendPacket(packet);
    }

    private void AddTrapToStorage(string trapName) {
        session.DataStorage[Scope.Slot, trapName] += 1;
        int count = session.DataStorage[Scope.Slot, trapName];
        Log.Info($"AddTrapToStorage: {count}");
    }

    private void SendTrapLink(string trapName) {
        BouncedPacket packet = new BouncedPacket() {
            Tags = ["TrapLink"],
            Data = new() {
                { "time", DateTime.UtcNow.ToUnixTimeStamp() },
                { "source", session.Players.GetPlayerName(session.ConnectionInfo.Slot) },
                { "trap_name", trapName },
            }
        };
        session.Socket.SendPacket(packet);
    }
    
    private void ParseTrapLinkAlias(string trapName) {
        System.Random rand = new System.Random();
        int index;
        List<string> traps;
        if(trapName == "Confuse Trap") {
            TrapHelper.AddTrap("Mystery Trap");
        }
        else if(trapName == "Blue Balls Curse" || trapName == "One hit KO" || trapName == "One Hit Trap") {
            TrapHelper.AddTrap("Glass Trap");
        }
        else if(trapName == "Flip Trap" || trapName == "Flip Vertical Trap" || trapName == "Reversal Trap" || trapName == "Reverse Trap") {
            TrapHelper.AddTrap("Upscroll Trap");
        }
        else if(trapName == "Hidden") {
            TrapHelper.AddTrap("Gloom Trap");
        }
        else if(trapName == "Chart Modifier Trap") {
            traps = new List<string>{"Mystery Trap", "Gloom Trap", "Nightmare Trap"};
            index = rand.Next(traps.Count);
            TrapHelper.AddTrap(traps[index]);
        }
        else if(Config.Settings.TraplinkConvert) {
            traps = new List<string>{"Mystery Trap", "Gloom Trap", "Nightmare Trap", "Coda Trap", "Glass Trap", "Upscroll Trap"};
            index = rand.Next(traps.Count);
            TrapHelper.AddTrap(traps[index]);
        }

        TrapHelper.AddTrap(trapName);
    }

    private void Socket_PacketRecived(ArchipelagoPacketBase packet) {
        try {
            switch(packet) {
                case BouncePacket bouncedPacket when bouncedPacket.Tags.Contains("TrapLink"):
                    var data = bouncedPacket.Data;
                    var uuid = (string)data["source"];
                    if(uuid == session.Players.GetPlayerName(session.ConnectionInfo.Slot)) return;

                    var source = (string)data["source"];
                    var trapName = (string)data["trap_name"];
                    Log.Info($"Received {trapName} from {source}");
                    ParseTrapLinkAlias(trapName);
                    break;
                case BouncedPacket bouncedPacket when bouncedPacket.Tags.Contains("DeathLink"):
                    data = bouncedPacket.Data;
                    uuid = (string)data["source"];
                    if(uuid == session.Players.GetPlayerName(session.ConnectionInfo.Slot)) return;

                    source = (string)data["source"];
                    var cause = (string)data["cause"];
                    Log.Info($"{source} {cause}");
                    DeathlinkHelper.sendDeath = true;
                    break;
            }
        }
        catch { }
    }
}