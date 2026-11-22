using System.IO;
using System.Reflection;
using RiftOfTheNecroManager;
using UnityEngine;

namespace RiftArchipelago.Helpers;

public static class AssetHelpers {
    // Copied from DeamonHunter's Muse Dash Client https://github.com/DeamonHunter/ArchipelagoMuseDash/blob/7f60c5a74419ff164b8b80fc8b74aa174cf4062f/ArchipelagoMuseDash/Helpers/AssetHelpers.cs#L52
    public static Texture2D LoadTexture(string path) {
        using(var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)) {
            if(stream == null) {
                Log.Error($"LoadTexture: Failed to load asset as {path}");
                return null;
            }

            using(var memStream = new MemoryStream()) {
                stream.CopyTo(memStream);

                var iconTexture = new Texture2D(1, 1);
                iconTexture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
                iconTexture.wrapMode = TextureWrapMode.Clamp;
                ImageConversion.LoadImage(iconTexture, memStream.ToArray());
                iconTexture.name = path;
                return iconTexture;
            }
        }
    }
}