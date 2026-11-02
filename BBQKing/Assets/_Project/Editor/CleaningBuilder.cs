using BBQKing.Cleaning;
using BBQKing.Core;
using BBQKing.Restoration;
using BBQKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
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
        const string ConfigPath = DataFolder + "CleaningConfig.asset";
        const string KettleGrillDataPath = DataFolder + "KettleGrill.asset";
        const string WireBrushPath = "Assets/_Project/Data/Tools/WireBrush.asset";
        const string KettleGrillPrefabPath = "Assets/_Project/Prefabs/KettleGrill.prefab";
        const string CloseUpScenePath = "Assets/_Project/Scenes/CloseUp_Cleaning.unity";

        // The close-up is built far from the backyard (which spans 0 to 40 tiles), so the two
        // scenes can be open together without either camera seeing the other's objects.
        static readonly Vector3 CloseUpOrigin = new Vector3(1000f, 1000f, 0f);

        /// <summary>How many times bigger close-up art is drawn than backyard art (design.md decision log, 2 Nov 2026).</summary>
        const int CloseUpScale = 3;

        static readonly Color32 CloseUpBackground = new Color32(0x3a, 0x2a, 0x20, 0xff);

        /// <summary>The assets the cleaning parts of the scene share.</summary>
        public struct Assets
        {
            public CloseUpSession session;
            public CleaningConfig config;
            public CleanableData kettleGrill;
            public ToolData wireBrush;
        }

        /// <summary>Loads or creates the cleaning assets. Call after the scene has been created.</summary>
        public static Assets LoadAssets()
        {
            var a = new Assets
            {
                session = PlaceholderAssetWriter.LoadOrCreateAsset<CloseUpSession>(SessionPath),
                config = PlaceholderAssetWriter.LoadOrCreateAsset<CleaningConfig>(ConfigPath),
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

        /// <summary>The object in the backyard that opens and closes the close-up scene.</summary>
        public static void AddCloseUpLoader(Assets a, InputReader input, JobBook jobBook, Canvas backyardUi)
        {
            var loader = new GameObject("Cleaning").AddComponent<CloseUpLoader>();
            SetRef(loader, "input", input);
            SetRef(loader, "session", a.session);
            SetRef(loader, "jobBook", jobBook);
            SetRefs(loader, "hideWhileOpen", new Object[] { backyardUi });
        }

        // ------------------------------------------------------------------ close-up scene

        /// <summary>
        /// Builds CloseUp_Cleaning.unity: a camera of its own, the object and its grime drawn
        /// three times bigger, and its own UI. Replaces the scene file, like the backyard builder.
        /// Returns the scene's path.
        /// </summary>
        public static string BuildCloseUpScene()
        {
            // As in the backyard builder: create the scene first, then load assets, because
            // creating a scene unloads assets nothing refers to yet.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var input = AssetDatabase.LoadAssetAtPath<InputReader>(BackyardSceneBuilder.InputReaderPath);
            var display = AssetDatabase.LoadAssetAtPath<DisplayConfig>(BackyardSceneBuilder.DisplayConfigPath);
            var a = LoadAssets();

            var root = new GameObject("Close-up");
            root.transform.position = CloseUpOrigin;

            var cam = BuildCloseUpCamera(display);

            // The object and its grime. The sprites shown here are the kettle grill's, so the scene
            // looks right while editing; the game sets them from the close-up session.
            var objectRenderer = CloseUpSprite(root.transform, "Object", a.kettleGrill.closeUpSprite, 0);
            var grimeRenderer = CloseUpSprite(root.transform, "Grime", a.kettleGrill.grimeSprite, 1);

            var ui = BuildCloseUpUi(display, a);

            var closeUp = root.AddComponent<CleaningCloseUp>();
            SetRef(closeUp, "input", input);
            SetRef(closeUp, "session", a.session);
            SetRef(closeUp, "config", a.config);
            SetRef(closeUp, "closeUpCamera", cam);
            SetRef(closeUp, "objectRenderer", objectRenderer);
            SetRef(closeUp, "grimeRenderer", grimeRenderer);
            SetRef(closeUp, "titleLabel", ui.title);
            SetRef(closeUp, "toolIcon", ui.toolIcon);
            SetRef(closeUp, "toolLabel", ui.toolLabel);
            SetRef(closeUp, "meterFill", ui.meterFill);
            SetRef(closeUp, "meterLabel", ui.meterLabel);

            PlaceholderAssetWriter.EnsureFolder(CloseUpScenePath);
            EditorSceneManager.SaveScene(scene, CloseUpScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log("BBQ King: built " + CloseUpScenePath);
            return CloseUpScenePath;
        }

        static Camera BuildCloseUpCamera(DisplayConfig display)
        {
            // Not tagged MainCamera: the backyard camera stays the main one. This camera has a
            // higher priority (depth), so it draws after the backyard camera and covers it.
            var camGo = new GameObject("Close-up Camera");
            camGo.transform.position = CloseUpOrigin + new Vector3(0f, 0f, -10f);
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = CloseUpBackground;
            cam.depth = 10;

            var ppc = camGo.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = PixelArtImportSettings.PixelsPerUnit;
            ppc.refResolutionX = display.Width;
            ppc.refResolutionY = display.Height;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;
            SetRef(camGo.AddComponent<PixelCameraSetup>(), "displayConfig", display);
            return cam;
        }

        static SpriteRenderer CloseUpSprite(Transform parent, string name, Sprite sprite, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            // A whole-number scale keeps every art pixel a crisp square.
            go.transform.localScale = new Vector3(CloseUpScale, CloseUpScale, 1f);
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.sortingOrder = order;
            return r;
        }

        struct CloseUpUi
        {
            public Text title, toolLabel, meterLabel;
            public Image toolIcon;
            public RectTransform meterFill;
        }

        static readonly Color32 MeterGreen = new Color32(0x6a, 0xa8, 0x4a, 0xff);

        static CloseUpUi BuildCloseUpUi(DisplayConfig display, Assets a)
        {
            var canvasGo = new GameObject("Close-up UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            canvas.sortingOrder = 10; // over the backyard's UI, in case any of it is showing
            SetRef(canvasGo.AddComponent<PixelCanvasScaler>(), "displayConfig", display);

            var ui = new CloseUpUi();
            ui.title = UiFactory.Text(canvasGo.transform, "Title", a.kettleGrill.cleanPrompt, 16, TextAnchor.UpperCenter, UiFactory.LightText);
            ui.title.fontStyle = FontStyle.Bold;
            UiFactory.Anchor(ui.title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(400f, 22f));
            ui.title.gameObject.AddComponent<Outline>().effectColor = UiFactory.InkText;

            // The progress meter under the title: a dark frame with a green bar that grows from
            // the left, and "37% clean" below it.
            const float meterWidth = 200f;
            var frame = UiFactory.Image(canvasGo.transform, "Meter", PlaceholderArt.UiPanelDark);
            UiFactory.Anchor(frame.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -32f), new Vector2(meterWidth + 4f, 12f));
            var fill = new GameObject("Fill", typeof(RectTransform)).AddComponent<Image>();
            fill.transform.SetParent(frame.transform, false);
            fill.color = MeterGreen; // a plain coloured rectangle: no sprite needed
            fill.raycastTarget = false;
            UiFactory.Anchor(fill.rectTransform, new Vector2(0f, 0.5f), new Vector2(2f, 0f), new Vector2(meterWidth, 8f));
            ui.meterFill = fill.rectTransform;
            ui.meterLabel = UiFactory.Text(canvasGo.transform, "Meter Label", "0% clean", 10, TextAnchor.UpperCenter, UiFactory.LightText);
            UiFactory.Anchor(ui.meterLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(200f, 14f));
            ui.meterLabel.gameObject.AddComponent<Outline>().effectColor = UiFactory.InkText;

            // The tool in hand, bottom left: one slot like the backyard toolbar, with its name.
            var slot = UiFactory.Image(canvasGo.transform, "Tool Slot", PlaceholderArt.UiSlotSelected);
            UiFactory.Anchor(slot.rectTransform, Vector2.zero, new Vector2(12f, 12f), new Vector2(36f, 36f));
            ui.toolIcon = UiFactory.Icon(slot.transform, "Icon", a.wireBrush.icon, new Vector2(32f, 32f));
            ui.toolLabel = UiFactory.Text(canvasGo.transform, "Tool Name", a.wireBrush.displayName, 10, TextAnchor.MiddleLeft, UiFactory.LightText);
            UiFactory.Anchor(ui.toolLabel.rectTransform, Vector2.zero, new Vector2(54f, 22f), new Vector2(120f, 16f));

            var help = UiFactory.Text(canvasGo.transform, "Help", "Hold the left mouse button to scrub    [Esc] Leave", 10, TextAnchor.LowerCenter, UiFactory.LightText);
            UiFactory.Anchor(help.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(400f, 14f));
            return ui;
        }
    }
}
