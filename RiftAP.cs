using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using RiftOfTheNecroManager;

namespace RiftArchipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("RiftOfTheNecroDancer.exe")]
public class RiftAP : RiftPlugin {
    public enum LogLevel {
        INFO = 0,
        WARN = 1,
        ERR = 2,
    }

    public static void LogMsg(LogLevel logLevel, string msg) {
        if(logLevel == LogLevel.INFO) {
            Log.Info("[Archipelago] " + msg);
        }
        if(logLevel == LogLevel.WARN) {
            Log.Info("[Archipelago] " + msg);
        }
        if(logLevel == LogLevel.ERR) {
            Log.Info("[Archipelago] " + msg);
        }
    }
}
