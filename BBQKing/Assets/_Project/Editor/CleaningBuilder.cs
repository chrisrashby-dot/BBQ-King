using BBQKing.Cleaning;
using BBQKing.Core;
using BBQKing.Restoration;
using UnityEditor;
using UnityEngine;
using static BBQKing.EditorTools.BuilderUtil;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// The M3 Cleaning parts: the kettle grill in the backyard and the data it uses. Called by
    /// <see cref="BackyardSceneBuilder"/>; kept in its own file so each builder stays readable.
    ///
    /// Like the rest of the builder, data assets and prefabs are only created if missing, so values
    /// you tune in the Inspector are kept when the scene is rebuilt.
    /// </summary>
    public static class CleaningBuilder
    {
        const string DataFolder = "Assets/_Project/Data/Cleaning/";
        const string SessionPath = DataFolder + "CloseUpSession.asset";
        const string KettleGrillDataPath = DataFolder + "KettleGrill.asset";
        const string WireBrushPath = "Assets/_Project/Data/Tools/WireBrush.asset";
        const string KettleGrillPrefabPath = "Assets/_Project/Prefabs/KettleGrill.prefab";

        /// <summary>The assets the cleaning parts of the scene share.</summary>
        public struct Assets
        {
            public CloseUpSession session;
            public CleanableData kettleGrill;
            public ToolData wireBrush;
        }

        /// <summary>Loads or creates the cleaning assets. Call after the scene has been created.</summary>
        public static Assets LoadAssets()
        {
            var a = new Assets
            {
                session = PlaceholderAssetWriter.LoadOrCreateAsset<CloseUpSession>(SessionPath),
                wireBrush = LoadOrCreateWireBrush(),
            };

            bool isNew = AssetDatabase.LoadAssetAtPath<CleanableData>(KettleGrillDataPath) == null;
            a.kettleGrill = PlaceholderAssetWriter.LoadOrCreateAsset<CleanableData>(KettleGrillDataPath);
            if (isNew)
            {
                var grill = a.kettleGrill;
                grill.job = AssetDatabase.LoadAssetAtPath<JobData>(RestorationBuilder.Job2Path);
                grill.tool = a.wireBrush; // design.md decision log, 2 Nov 2026: Job 2 uses the wire brush only
                grill.closeUpSprite = LoadSprite(PlaceholderArt.CloseUpKettleGrate);
                grill.grimeSprite = LoadSprite(PlaceholderArt.CloseUpKettleGrateGrime);
                grill.cleanPrompt = "Clean the kettle grill";
                grill.lookPrompt = "Look at the kettle grill";
                grill.notYetHint = "It needs a good scrub, but the patio comes first.";
                EditorUtility.SetDirty(grill);
            }
            return a;
        }

        static ToolData LoadOrCreateWireBrush()
        {
            bool isNew = AssetDatabase.LoadAssetAtPath<ToolData>(WireBrushPath) == null;
            var tool = PlaceholderAssetWriter.LoadOrCreateAsset<ToolData>(WireBrushPath);
            if (isNew)
            {
                tool.displayName = "Wire brush";
                tool.icon = LoadSprite(PlaceholderArt.ToolWireBrush);
                EditorUtility.SetDirty(tool);
            }
            return tool;
        }

        /// <summary>
        /// Puts the relative's kettle grill on the patio. It stands up like the player, so it is
        /// Y-sorted with them, and a small solid box at its feet stops the player walking through it.
        /// </summary>
        public static void AddKettleGrill(Assets a, JobBook jobBook, StringEventChannel hintChannel)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(KettleGrillPrefabPath);
            if (prefab == null)
            {
                var go = new GameObject("Kettle Grill");
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = LoadSprite(PlaceholderArt.KettleGrillDirty);
                renderer.sortingOrder = BackyardSceneBuilder.StandingOrder;
                renderer.spriteSortPoint = SpriteSortPoint.Pivot;

                // Solid, so the player bumps into it. The interactor also finds solid colliders.
                var box = go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(0.75f, 0.35f);
                box.offset = new Vector2(0f, 0.2f);

                var cleanable = go.AddComponent<CleanableObject>();
                SetRef(cleanable, "data", a.kettleGrill);
                SetRef(cleanable, "jobBook", jobBook);
                SetRef(cleanable, "session", a.session);
                SetRef(cleanable, "hintChannel", hintChannel);
                SetRef(cleanable, "spriteRenderer", renderer);
                SetRef(cleanable, "dirtySprite", LoadSprite(PlaceholderArt.KettleGrillDirty));
                SetRef(cleanable, "cleanSprite", LoadSprite(PlaceholderArt.KettleGrillClean));

                PlaceholderAssetWriter.EnsureFolder(KettleGrillPrefabPath);
                prefab = PrefabUtility.SaveAsPrefabAsset(go, KettleGrillPrefabPath);
                Object.DestroyImmediate(go);
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = BackyardLayout.KettleGrill;
        }
    }
}
