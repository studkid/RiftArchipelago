using System;
using System.Collections.Concurrent;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;

namespace RiftArchipelago;

public enum APState {
    Menu,
    InGame,
}

public static class ArchipelagoClient {
    public static int[] AP_VERSION = {0, 6, 1};
    public const string GAME_NAME = "Rift of the Necrodancer";

    private static ConcurrentQueue<ItemInfo> _itemQueue = new();
    public static bool isAuthenticated = false;
    public static ArchipelagoInfo apInfo = new ArchipelagoInfo();
    public static ArchipelagoUI apUI = new ArchipelagoUI();
    public static APState state;
    public static int slotID;
    public static ArchipelagoSession session;
    public static SlotData slotData;
    public static bool isInGame = false;

    public static bool Connect() {
        if (isAuthenticated) {
            return true;
        }

        if (apInfo.address is null || apInfo.address.Length == 0) {
            return false;
        }

        session = ArchipelagoSessionFactory.CreateSession(apInfo.address);
        
        LoginResult loginResult = session.TryConnectAndLogin(
            GAME_NAME,
            apInfo.slot,
            ItemsHandlingFlags.AllItems,
            new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
            null,
            "",
            apInfo.password
        );

        if (loginResult is LoginSuccessful loginSuccess) {
            isAuthenticated = true;
            state = APState.Menu;
            slotData = new SlotData(loginSuccess.SlotData);

            session.Items.ItemReceived += Session_ItemReceived;
            
            return true;
        }

        return false;
    }

    public static async void Disconnect() {
        if(session is { Socket.Connected: true }) await session.Socket.DisconnectAsync(); 
        isAuthenticated = false;
        slotData = null;
    }
    
    public static void GoalGame() {
        RiftAP._log.LogInfo("Goal Reached!");
        var statusUpdatePacket = new StatusUpdatePacket();
        statusUpdatePacket.Status = ArchipelagoClientState.ClientGoal;
        session.Socket.SendPacket(statusUpdatePacket);
    }

    private static void Session_ItemReceived(ReceivedItemsHelper helper) {
        var item = helper.DequeueItem();
        RiftAP._log.LogInfo($"Item Recieved: {item.ItemDisplayName} | ID: {item.ItemId}");

        if(item.ItemId == 1) {
            ItemHandler.AddDiamond();
        }
        else if(item.ItemId >= 50 && item.ItemId < 1000) {
            ItemHandler.UnlockSong(item.ItemName);
        }
        else if(item.ItemId >= 1000 && item.ItemId < 2000) {;
            ItemHandler.UnlockRemix(item.ItemName.Substring(0, item.ItemName.Length - 8));
        }
        else if(item.ItemId >= 2000 && item.ItemId < 3000) {
            ItemHandler.UnlockExtra(item.ItemName);
        }
    }
}