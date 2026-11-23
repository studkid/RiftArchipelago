using System.Collections.Concurrent;
using RhythmRift;

namespace RiftArchipelago.Helpers;

public static class ItemHelper {
    internal static ConcurrentQueue<string> fillerQueue = new();

    public static void AddItem(string itemName) {
        fillerQueue.Enqueue(itemName);
    } 

    public static void RunItems(RRStageController controller) {
        if(fillerQueue.IsEmpty) return;

        fillerQueue.TryDequeue(out string item);
        if(item == "Apple") {
            controller._playerController.PerformPlayerHeal(1);
        }

        else if(item == "Cheese") {
            controller._playerController.PerformPlayerHeal(2);
        }

        else if(item == "Chicken") {
            controller._playerController.PerformPlayerHeal(3);
        }

        else if(item == "Ham") {
            controller._playerController.PerformPlayerHeal(4);
        }
    }
}