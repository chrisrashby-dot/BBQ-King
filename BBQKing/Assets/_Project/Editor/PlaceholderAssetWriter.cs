using System.IO;
using UnityEditor;
using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Editor-only helpers for writing generated placeholder art to disk as normal PNG files, and
    /// for creating data assets. Because the art is real files in the Assets folder you can open
    /// them in any art program, or simply overwrite them with your own versions later.
    /// </summary>
    public static class PlaceholderAssetWriter
    {
        /// <summary>
        /// Draws and saves a PNG only if the file does not exist yet, so running the builder again
        /// never overwrites art you have replaced. Returns true if a new file was written.
        /// </summary>
        public static bool SavePngIfMissing(string assetPath, PixelCanvas canvas)
        {
            if (File.Exists(assetPath)) return false;

            var tex = new Texture2D(canvas.Width, canvas.Height, TextureFormat.RGBA32, false);
            tex.SetPixels32(canvas.Pixels);
            tex.Apply();
            EnsureFolder(assetPath);
            File.WriteAllBytes(assetPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            return true;
        }

        /// <summary>
        /// Sets a sprite's pivot (the point that sits on its position) and, for UI frames, its
        /// 9-slice border. Only called for newly written files, so your own settings are kept.
        /// </summary>
        public static void ConfigureSprite(string assetPath, SpriteAlignment pivot, Vector4 border = default)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;

            PixelArtImportSettings.Apply(importer);
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)pivot;
            settings.spriteBorder = border; // left, bottom, right, top in pixels
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        /// <summary>
        /// Lets scripts read the image's pixels while the game runs (Read/Write in the import
        /// settings). Only needed for images the game changes, such as a grime layer.
        /// </summary>
        public static void MakeReadable(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;
            importer.isReadable = true;
            importer.SaveAndReimport();
        }

        /// <summary>Loads a ScriptableObject asset, creating it with default values if it is missing.</summary>
        public static T LoadOrCreateAsset<T>(string assetPath) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null) return asset;

            EnsureFolder(assetPath);
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        public static void EnsureFolder(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }
    }
}
