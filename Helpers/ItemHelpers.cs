using System.Collections.Concurrent;

namespace RiftArchipelago.Helpers;

public static class ItemHelper {
    internal static ConcurrentQueue<string> fillerQueue = new();

    public static void AddItem(string itemName) {
        fillerQueue.Enqueue(itemName);
    } 

    public static void ProcessItem() {
        
    }
}