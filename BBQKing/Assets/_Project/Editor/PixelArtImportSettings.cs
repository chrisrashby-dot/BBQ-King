using UnityEditor;
using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Automatically applies crisp pixel-art import settings to any NEW image added under
    /// Assets/_Project/Art, so you never have to set them by hand.
    ///
    /// Unity concept: an AssetPostprocessor is an Editor-only class that Unity calls while it
    /// imports files. OnPreprocessTexture runs just before an image is turned into a texture,
    /// which is the moment to change its import settings.
    ///
    /// It only acts the first time a file is imported (when it has no settings yet), so any
    /// changes you make later in the Inspector are kept. The settings are explained in the README.
    /// </summary>
    public class PixelArtImportSettings : AssetPostprocessor
    {
        public const string ArtFolder = "Assets/_Project/Art/";

        /// <summary>32 pixels = 1 world unit = 1 tile. Must match the Pixel Perfect Camera's Assets PPU.</summary>
        public const int PixelsPerUnit = 32;

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder)) return;

            var importer = (TextureImporter)assetImporter;
            if (!importer.importSettingsMissing) return; // already set up: respect existing settings

            Apply(importer);
        }

        /// <summary>The pixel-art settings. Also used by the placeholder art generator.</summary>
        public static void Apply(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;            // no smoothing between pixels
            importer.textureCompression = TextureImporterCompression.Uncompressed; // compression smears colours
            importer.mipmapEnabled = false;                    // mipmaps blur pixel art at a distance
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None; // keep odd sizes exactly as drawn

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // a simple rectangle; avoids edge artefacts
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
        }
    }
}
