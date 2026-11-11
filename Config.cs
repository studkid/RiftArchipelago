using RiftOfTheNecroManager;

namespace RiftArchipelago;

public enum DeathLinkType {
    Off,
    NonLethal,
    On,
}

public static class Config {
    //TODO Add config toggles
    // Wating on button support
    // public static class Connection { 
    //     const string GROUP = "Connection Info";
    //     public static Setting<string> Address { get; } = new(GROUP, "Server Address", "", "Input archipelago server ip and port.");
    //     public static Setting<string> Slot { get; } = new(GROUP, "Slot Name", "", "Input your slot name.");
    //     public static Setting<string> Password { get; } = new(GROUP, "Password", "", "Input archipelago server password.  Leave empty if none.");
    // }

    public static class Settings {
        const string GROUP = "Settings";
        public static Setting<DeathLinkType> Deathlink { get; } = new(GROUP, "Deathlink", DeathLinkType.Off, "Enable Deathlink.\nNonLethal will take you down to 1 life on recieving a deathlink.");
        public static Setting<bool> Traplink { get; } = new(GROUP, "Traplink", false, "Allows you to send and recieve traps from other traplink players.");
        // public static Setting<bool> EnergyLink { get; } = new(GROUP, "Energylink", false, "Sends score at song completion to the energylink buffer.");
        public static Setting<bool> Debug { get; } = new(GROUP, "Debug Mode", false, "When enabled, enables certain debug buttons.");
    }
}