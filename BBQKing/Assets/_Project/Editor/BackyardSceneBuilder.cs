using BBQKing.Core;
using BBQKing.Player;
using BBQKing.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Builds the Backyard scene from code: placeholder art, tiles, data assets, camera, map and UI.
    /// Run it from the menu bar: BBQ King > Build Backyard Scene.
    ///
    /// Why build a scene from code? Every connection between objects is written down here where
    /// you can read it, and the scene can be recreated exactly if it gets into a muddle. Once
    /// built, it is a normal Unity scene that you can edit by hand. Running the builder again
    /// replaces the scene (it asks first), but never overwrites art files or data assets, so
    /// replaced art and tuned values are safe.
    /// </summary>
    public static class BackyardSceneBuilder
    {
        const string ScenePath = "Assets/_Project/Scenes/Backyard.unity";
        const string TileFolder = "Assets/_Project/Art/Tiles/";
        const string RendererPath = "Assets/Settings/Renderer2D.asset";

        // Data assets (ScriptableObjects). Created with default values if missing.
        const string InputReaderPath = "Assets/_Project/Data/Input/InputReader.asset";
        const string DisplayConfigPath = "Assets/_Project/Data/Settings/DisplayConfig.asset";
        const string PlayerConfigPath = "Assets/_Project/Data/Player/PlayerConfig.asset";
        const string PlayerSpritesPath = "Assets/_Project/Data/Characters/PlayerSprites.asset";
        const string PromptChannelPath = "Assets/_Project/Data/Events/InteractionPromptChannel.asset";
        const string MessageChannelPath = "Assets/_Project/Data/Events/ShowMessageChannel.asset";
        const string LetterMessagePath = "Assets/_Project/Data/Messages/RelativeLetter.asset";
        const string BackDoorMessagePath = "Assets/_Project/Data/Messages/BackDoor.asset";

        // Prefabs for interactable objects (created if missing, so your edits to them are kept).
        const string LetterPrefabPath = "Assets/_Project/Prefabs/Letter.prefab";
        const string BackDoorPrefabPath = "Assets/_Project/Prefabs/BackDoor.prefab";

        // Sorting orders: flat ground layers draw first, then everything that stands up is
        // Y-sorted together at order 0 (see ConfigureRenderer).
        const int GroundOrder = -100;
        const int OvergrowthOrder = -90;
        const int GroundItemOrder = -80;  // flat things lying on the ground, such as the letter
        const int StandingOrder = 0;

        static readonly Color32 SkyColour = new Color32(0x24, 0x1c, 0x16, 0xff); // shown outside the map

        [MenuItem("BBQ King/Build Backyard Scene")]
        public static void BuildFromMenu()
        {
            if (System.IO.File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Rebuild the Backyard scene?",
                    "This replaces Backyard.unity with a freshly built copy. Art files and data assets are kept.",
                    "Rebuild", "Cancel"))
                return;

            Build();
        }

        /// <summary>Entry point for the menu and for batch mode (the command line).</summary>
        public static void Build()
        {
            PlaceholderArt.GenerateAll();
            ConfigureRenderer();

            // Create the empty scene BEFORE loading tiles and data assets: opening a new scene
            // unloads any assets nothing refers to yet, which would leave us holding empty references.
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var tiles = CreateTiles();
            var input = PlaceholderAssetWriter.LoadOrCreateAsset<InputReader>(InputReaderPath);
            var display = PlaceholderAssetWriter.LoadOrCreateAsset<DisplayConfig>(DisplayConfigPath);
            var playerConfig = PlaceholderAssetWriter.LoadOrCreateAsset<PlayerConfig>(PlayerConfigPath);
            var playerSprites = LoadPlayerSprites();
            var promptChannel = PlaceholderAssetWriter.LoadOrCreateAsset<StringEventChannel>(PromptChannelPath);
            var messageChannel = PlaceholderAssetWriter.LoadOrCreateAsset<MessageEventChannel>(MessageChannelPath);
            var letterMessage = LoadOrCreateMessage(LetterMessagePath, LetterTitle, LetterBody);
            var doorMessage = LoadOrCreateMessage(BackDoorMessagePath, BackDoorTitle, BackDoorBody);

            // ---------- Game: scene-wide helpers
            var gameGo = new GameObject("Game");
            SetRef(gameGo.AddComponent<InputStartup>(), "input", input);

            // ---------- Lighting: one global 2D light so sprites show at full brightness.
            // Later, the time of day (golden hour, evening) can tint this light.
            var light = new GameObject("Global Light 2D").AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.intensity = 1f;

            // ---------- Map
            var map = BuildMap(tiles);

            // ---------- Camera
            var cam = BuildCamera(display, map.ground);

            // ---------- UI
            BuildUI(display, input, promptChannel, messageChannel);

            // ---------- Player
            var player = BuildPlayer(input, playerConfig, playerSprites);
            var interactor = player.gameObject.AddComponent<PlayerInteractor>();
            SetRef(interactor, "input", input);
            SetRef(interactor, "movement", player);
            SetRef(interactor, "config", playerConfig);
            SetRef(interactor, "promptChannel", promptChannel);

            // ---------- Interactable objects (prefabs placed in the scene)
            var letterPrefab = LoadOrCreateMessagePrefab(LetterPrefabPath, "Letter", PlaceholderArt.Letter,
                GroundItemOrder, "Read the letter", letterMessage, messageChannel,
                colliderSize: new Vector2(0.6f, 0.4f), colliderOffset: Vector2.zero);
            PlacePrefab(letterPrefab, BackyardLayout.LetterSpot);

            // The door's trigger sits along its bottom edge, where the player stands to use it.
            var doorPrefab = LoadOrCreateMessagePrefab(BackDoorPrefabPath, "Back Door", PlaceholderArt.BackDoor,
                StandingOrder + 1, "Open the back door", doorMessage, messageChannel,
                colliderSize: new Vector2(0.8f, 0.4f), colliderOffset: new Vector2(0f, -0.4f));
            PlacePrefab(doorPrefab, (Vector2)BackyardLayout.BackDoor + new Vector2(0.5f, 0.5f));
            SetRef(cam.GetComponent<CameraFollow>(), "target", player.transform);
            cam.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10f);

            // ---------- Save and add to the build
            PlaceholderAssetWriter.EnsureFolder(ScenePath);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("BBQ King: built " + ScenePath);
        }

        // ------------------------------------------------------------------ camera

        static Camera BuildCamera(DisplayConfig display, Tilemap bounds)
        {
            var camGo = new GameObject("Main Camera") { tag = "MainCamera" };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = SkyColour;
            camGo.AddComponent<AudioListener>();

            // The Pixel Perfect Camera draws the game at the reference resolution, then scales it
            // up by a whole number (x2 on 1080p for 960x540), so every art pixel stays crisp.
            var ppc = camGo.AddComponent<PixelPerfectCamera>();
            ppc.assetsPPU = PixelArtImportSettings.PixelsPerUnit;
            ppc.refResolutionX = display.Width;
            ppc.refResolutionY = display.Height;
            ppc.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
            ppc.cropFrame = PixelPerfectCamera.CropFrame.None;

            SetRef(camGo.AddComponent<PixelCameraSetup>(), "displayConfig", display);

            var follow = camGo.AddComponent<CameraFollow>();
            SetRef(follow, "boundsTilemap", bounds);
            return cam;
        }

        // ------------------------------------------------------------------ player

        static PlayerMovement BuildPlayer(InputReader input, PlayerConfig config, CharacterSprites sprites)
        {
            var playerGo = new GameObject("Player");
            playerGo.transform.position = BackyardLayout.PlayerStart;

            var body = playerGo.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate; // smooth movement between physics steps
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Only the feet collide, so the head can overlap things behind the player (three-quarter view).
            var feet = playerGo.AddComponent<BoxCollider2D>();
            feet.size = new Vector2(0.45f, 0.25f);
            feet.offset = new Vector2(0f, 0.12f);

            var movement = playerGo.AddComponent<PlayerMovement>();
            SetRef(movement, "input", input);
            SetRef(movement, "config", config);

            // The sprite sits on a child object so effects (bobbing, flashing) never move the collider.
            var visual = new GameObject("Visual");
            visual.transform.SetParent(playerGo.transform, false);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites.FramesFor(Facing.Down)[0];
            renderer.sortingOrder = StandingOrder;
            // Sort by the feet (the pivot) rather than the middle of the sprite, so the player
            // goes behind a fence only when actually standing behind it.
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;

            var animator = playerGo.AddComponent<CharacterAnimator>();
            SetRef(animator, "movement", movement);
            SetRef(animator, "spriteRenderer", renderer);
            SetRef(animator, "sprites", sprites);
            SetRef(animator, "config", config);
            return movement;
        }

        /// <summary>Creates the player's sprite set asset, filling in the placeholder frames if it is new or empty.</summary>
        static CharacterSprites LoadPlayerSprites()
        {
            var sprites = PlaceholderAssetWriter.LoadOrCreateAsset<CharacterSprites>(PlayerSpritesPath);
            if (sprites.down.Length == 0) sprites.down = LoadFrames(Facing.Down);
            if (sprites.up.Length == 0) sprites.up = LoadFrames(Facing.Up);
            if (sprites.left.Length == 0) sprites.left = LoadFrames(Facing.Left);
            if (sprites.right.Length == 0) sprites.right = LoadFrames(Facing.Right);
            EditorUtility.SetDirty(sprites);
            return sprites;
        }

        static Sprite[] LoadFrames(Facing facing)
        {
            var frames = new Sprite[PlaceholderArt.WalkFrames];
            for (int i = 0; i < frames.Length; i++) frames[i] = LoadSprite(PlaceholderArt.PlayerFrame(facing, i));
            return frames;
        }

        // ------------------------------------------------------------------ interactables

        // Placeholder text. The relative's name is still an open question (design.md section 11),
        // so it is marked clearly for replacing. Edit the text in Data/Messages, not here: this is
        // only used when the asset is first created.
        const string LetterTitle = "A letter for you";
        const string LetterBody =
            "[Placeholder: the relative's name and the final wording are still to be decided.]\n\n" +
            "If you are reading this, the house is yours now. I am sorry I let the backyard get away from me. " +
            "The old grill on the patio still has a few good cookouts left in it, and so do you.\n\n" +
            "Start with the patio. Everything else follows.\n\n" +
            "With love,\n[Name]";

        const string BackDoorTitle = "The back door";
        const string BackDoorBody =
            "[Placeholder] The kitchen is through here. Going inside to prep food arrives in a later milestone.";

        static MessageData LoadOrCreateMessage(string path, string title, string body)
        {
            bool isNew = AssetDatabase.LoadAssetAtPath<MessageData>(path) == null;
            var message = PlaceholderAssetWriter.LoadOrCreateAsset<MessageData>(path);
            if (isNew)
            {
                message.title = title;
                message.body = body;
                EditorUtility.SetDirty(message);
            }
            return message;
        }

        /// <summary>
        /// Unity concept: a prefab is a saved, reusable object. Every copy in a scene links back to
        /// the prefab file, so changing the prefab changes all of them. Interactable objects are
        /// prefabs (CLAUDE.md) so they can be dropped into any scene.
        /// </summary>
        static GameObject LoadOrCreateMessagePrefab(string path, string name, string spritePath, int sortingOrder,
            string prompt, MessageData message, MessageEventChannel channel, Vector2 colliderSize, Vector2 colliderOffset)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing;

            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = LoadSprite(spritePath);
            renderer.sortingOrder = sortingOrder;

            // A trigger collider: the interactor can find it, but the player can walk over it.
            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = colliderSize;
            trigger.offset = colliderOffset;

            var interactable = go.AddComponent<MessageInteractable>();
            SetString(interactable, "prompt", prompt);
            SetRef(interactable, "message", message);
            SetRef(interactable, "showMessageChannel", channel);

            PlaceholderAssetWriter.EnsureFolder(path);
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static void PlacePrefab(GameObject prefab, Vector2 position)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = position;
        }

        // ------------------------------------------------------------------ map

        struct TileSet
        {
            public Tile grassLong, grassCut, patioClean, patioWeedy, dirt, path, fenceIntact, fenceBroken, houseWall;
        }

        struct MapRefs { public Tilemap ground, overgrowth, walls; }

        static MapRefs BuildMap(TileSet tiles)
        {
            // Unity concept: a Grid lays out cells (here 1 x 1 world unit = 32 pixels), and each
            // Tilemap under it is a layer of tiles painted onto those cells.
            var gridGo = new GameObject("Backyard Grid");
            gridGo.AddComponent<Grid>().cellSize = Vector3.one;

            var ground = CreateTilemap(gridGo, "Ground", GroundOrder, TilemapRenderer.Mode.Chunk);
            // Overgrowth sits on top of the ground: weeds, long grass and rubbish to clear in M2.
            var overgrowth = CreateTilemap(gridGo, "Overgrowth", OvergrowthOrder, TilemapRenderer.Mode.Chunk);
            // Walls and fences stand up, so each tile is Y-sorted individually with the player:
            // you can stand in front of a fence, and the fence hides your feet if you are behind it.
            var walls = CreateTilemap(gridGo, "Walls and Fences", StandingOrder, TilemapRenderer.Mode.Individual);

            var map = BackyardLayout.Map;
            for (int y = map.yMin; y < map.yMax; y++)
                for (int x = map.xMin; x < map.xMax; x++)
                {
                    var cell = new Vector2Int(x, y);
                    ground.SetTile((Vector3Int)cell, GroundTileAt(cell, tiles));

                    if (BackyardLayout.House.Contains(cell))
                        walls.SetTile((Vector3Int)cell, tiles.houseWall);
                    else if (BackyardLayout.IsFence(x, y))
                        walls.SetTile((Vector3Int)cell, IsBrokenFence(cell) ? tiles.fenceBroken : tiles.fenceIntact);
                }

            // Walls and fences block the player. The collider shape is built from the tiles
            // automatically, and the composite merges neighbouring tiles into one smooth outline
            // so the player does not snag on the seams.
            var body = walls.gameObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            var tileCollider = walls.gameObject.AddComponent<TilemapCollider2D>();
            tileCollider.compositeOperation = Collider2D.CompositeOperation.Merge;
            walls.gameObject.AddComponent<CompositeCollider2D>();

            return new MapRefs { ground = ground, overgrowth = overgrowth, walls = walls };
        }

        static TileBase GroundTileAt(Vector2Int cell, TileSet tiles)
        {
            if (BackyardLayout.House.Contains(cell)) return tiles.dirt; // hidden behind the wall
            if (BackyardLayout.Patio.Contains(cell))
                return Random01(cell) < BackyardLayout.PatioWeedShare ? tiles.patioWeedy : tiles.patioClean;
            foreach (var path in BackyardLayout.Paths)
                if (path.Contains(cell)) return tiles.path;
            if (BackyardLayout.SmokerCorner.Contains(cell) || BackyardLayout.ShedYard.Contains(cell)) return tiles.dirt;
            // A strip of cut grass just below the house, the rest long and overgrown.
            if (cell.y >= BackyardLayout.House.yMin - 1 && cell.y < BackyardLayout.House.yMin) return tiles.grassCut;
            return tiles.grassLong;
        }

        static bool IsBrokenFence(Vector2Int cell) => System.Array.IndexOf(BackyardLayout.BrokenFence, cell) >= 0;

        /// <summary>A repeatable value between 0 and 1 for a cell, so the map is the same every build.</summary>
        static float Random01(Vector2Int cell) => (PlaceholderHash.Mix(cell.x, cell.y, 1) & 0xffff) / 65535f;

        static TileSet CreateTiles()
        {
            return new TileSet
            {
                grassLong = MakeTile("ground_grass_long", PlaceholderArt.GrassLong, Tile.ColliderType.None),
                grassCut = MakeTile("ground_grass_cut", PlaceholderArt.GrassCut, Tile.ColliderType.None),
                patioClean = MakeTile("ground_patio_clean", PlaceholderArt.PatioClean, Tile.ColliderType.None),
                patioWeedy = MakeTile("ground_patio_weedy", PlaceholderArt.PatioWeedy, Tile.ColliderType.None),
                dirt = MakeTile("ground_dirt", PlaceholderArt.Dirt, Tile.ColliderType.None),
                path = MakeTile("ground_path", PlaceholderArt.Path, Tile.ColliderType.None),
                // "Grid" makes each tile a solid square, ignoring the gaps between pickets.
                fenceIntact = MakeTile("fence_intact", PlaceholderArt.FenceIntact, Tile.ColliderType.Grid),
                // Broken panels still block the player in M1, so nobody wanders off the map.
                fenceBroken = MakeTile("fence_broken", PlaceholderArt.FenceBroken, Tile.ColliderType.Grid),
                houseWall = MakeTile("wall_house", PlaceholderArt.HouseWall, Tile.ColliderType.Grid),
            };
        }

        /// <summary>A tile asset just points at a sprite. Replacing the PNG updates the tile automatically.</summary>
        static Tile MakeTile(string name, string spritePath, Tile.ColliderType collider)
        {
            string path = TileFolder + name + ".asset";
            var tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
            if (tile == null)
            {
                PlaceholderAssetWriter.EnsureFolder(path);
                tile = ScriptableObject.CreateInstance<Tile>();
                tile.sprite = LoadSprite(spritePath);
                tile.colliderType = collider;
                AssetDatabase.CreateAsset(tile, path);
            }
            return tile;
        }

        static Tilemap CreateTilemap(GameObject grid, string name, int order, TilemapRenderer.Mode mode)
        {
            var go = new GameObject(name);
            go.transform.SetParent(grid.transform, false);
            var map = go.AddComponent<Tilemap>();
            var r = go.AddComponent<TilemapRenderer>();
            r.sortingOrder = order;
            r.mode = mode;
            return map;
        }

        /// <summary>
        /// Makes things lower on screen draw in front of things higher up ("Y-sorting"), so the
        /// player can walk behind and in front of fences and objects. Stored in the 2D Renderer asset.
        /// </summary>
        static void ConfigureRenderer()
        {
            var data = AssetDatabase.LoadAssetAtPath<ScriptableObject>(RendererPath);
            if (data == null) { Debug.LogWarning("BBQ King: 2D Renderer asset not found at " + RendererPath); return; }

            var so = new SerializedObject(data);
            var mode = so.FindProperty("m_TransparencySortMode");
            var axis = so.FindProperty("m_TransparencySortAxis");
            if (mode != null) mode.intValue = (int)TransparencySortMode.CustomAxis;
            if (axis != null) axis.vector3Value = new Vector3(0f, 1f, 0f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(data);
        }

        // ------------------------------------------------------------------ UI

        static readonly Color32 LightText = new Color32(0xf6, 0xec, 0xd6, 0xff);
        static readonly Color32 InkText = new Color32(0x3a, 0x2a, 0x20, 0xff);

        // UI sizes are in art pixels at the reference resolution; the canvas scaler multiplies them up.
        static Canvas BuildUI(DisplayConfig display, InputReader input, StringEventChannel promptChannel,
            MessageEventChannel messageChannel)
        {
            var canvasGo = new GameObject("UI");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = true;
            SetRef(canvasGo.AddComponent<PixelCanvasScaler>(), "displayConfig", display);

            // --- Interaction prompt, bottom centre. The controller sits on a parent that stays
            // active, because a disabled object stops listening for events.
            var promptRoot = UiRoot(canvasGo.transform, "Interaction Prompt");
            var promptPanel = UiImage(promptRoot, "Panel", PlaceholderArt.UiPanelDark);
            Anchor(promptPanel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(180f, 22f));
            var promptText = UiText(promptPanel.transform, "Label", "[E] Read the letter", 12, TextAnchor.MiddleCenter, LightText);
            Stretch(promptText.rectTransform, 4, 2, 4, 2);
            var prompt = promptRoot.gameObject.AddComponent<InteractionPromptUI>();
            SetRef(prompt, "promptChannel", promptChannel);
            SetRef(prompt, "panel", promptPanel.gameObject);
            SetRef(prompt, "label", promptText);
            promptPanel.gameObject.SetActive(false);

            // --- Message panel (letters and notes), centred
            var messageRoot = UiRoot(canvasGo.transform, "Message Panel");
            var paper = UiImage(messageRoot, "Paper", PlaceholderArt.UiPanelPaper);
            Anchor(paper.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(380f, 240f));
            var title = UiText(paper.transform, "Title", "Title", 16, TextAnchor.UpperLeft, InkText);
            title.fontStyle = FontStyle.Bold;
            Stretch(title.rectTransform, 16, 200, 16, 14);
            var body = UiText(paper.transform, "Body", "Body", 12, TextAnchor.UpperLeft, InkText);
            body.horizontalOverflow = HorizontalWrapMode.Wrap;
            body.lineSpacing = 1.1f;
            Stretch(body.rectTransform, 16, 30, 16, 44);
            var hint = UiText(paper.transform, "Close Hint", "[E] or [Esc] Close", 10, TextAnchor.LowerRight, InkText);
            Stretch(hint.rectTransform, 16, 10, 16, 210);
            var panel = messageRoot.gameObject.AddComponent<MessagePanelUI>();
            SetRef(panel, "input", input);
            SetRef(panel, "showMessageChannel", messageChannel);
            SetRef(panel, "panel", paper.gameObject);
            SetRef(panel, "titleLabel", title);
            SetRef(panel, "bodyLabel", body);
            paper.gameObject.SetActive(false);

            return canvas;
        }

        static RectTransform UiRoot(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt, 0, 0, 0, 0);
            return rt;
        }

        static Font UiFont => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        static Image UiImage(Transform parent, string name, string spritePath)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = LoadSprite(spritePath);
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return img;
        }

        static Text UiText(Transform parent, string name, string text, int size, TextAnchor align, Color32 colour)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = UiFont;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = colour;
            t.raycastTarget = false;
            return t;
        }

        static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        // ------------------------------------------------------------------ helpers

        static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) Debug.LogError("BBQ King: missing sprite " + path);
            return s;
        }

        static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Sets a private [SerializeField] field, exactly as dragging something into the Inspector would.
        static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogError($"BBQ King: no field '{field}' on {target.GetType().Name}"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
