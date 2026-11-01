using BBQKing.Core;
using BBQKing.Player;
using BBQKing.Restoration;
using BBQKing.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static BBQKing.EditorTools.BuilderUtil;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// The M2 Restoration parts of the Backyard scene: tools, the toolbar and (in later steps) the
    /// clearing system and jobs. Called by <see cref="BackyardSceneBuilder"/>; kept in its own file
    /// so each builder stays readable.
    ///
    /// Like the rest of the builder, data assets are only created if missing, so values you tune in
    /// the Inspector are kept when the scene is rebuilt.
    /// </summary>
    public static class RestorationBuilder
    {
        const string ToolFolder = "Assets/_Project/Data/Tools/";
        const string ToolSetPath = ToolFolder + "BackyardTools.asset";
        const string EventFolder = "Assets/_Project/Data/Events/";
        const string SelectedSlotChannelPath = EventFolder + "SelectedToolSlotChannel.asset";
        const string ToolUseChannelPath = EventFolder + "ToolUseChannel.asset";
        const string TileClearedChannelPath = EventFolder + "TileClearedChannel.asset";
        const string HintChannelPath = EventFolder + "HintChannel.asset";
        const string RestorationFolder = "Assets/_Project/Data/Restoration/";
        const string ClearingConfigPath = RestorationFolder + "ClearingConfig.asset";
        const string ClearableFolder = RestorationFolder + "Clearables/";

        /// <summary>The assets the Restoration parts of the scene share.</summary>
        public struct Assets
        {
            public ToolSet tools;
            public ToolData gloves, trashBag;
            public IntEventChannel selectedSlotChannel;
            public ToolUseEventChannel toolUseChannel;
            public TileClearedEventChannel tileClearedChannel;
            public StringEventChannel hintChannel;
            public ClearingConfig clearing;
            public ClearableData[] weeds;   // small, medium, large
            public ClearableData[] trash;   // bag, cans, broken pot
        }

        /// <summary>Loads or creates the tool and clearing assets. Call after the scene has been created.</summary>
        public static Assets LoadAssets(BackyardSceneBuilder.TileSet groundTiles)
        {
            var a = new Assets
            {
                gloves = LoadOrCreateTool("Gloves", "Gloves", PlaceholderArt.ToolGloves),
                trashBag = LoadOrCreateTool("TrashBag", "Trash bag", PlaceholderArt.ToolTrashBag),
                selectedSlotChannel = PlaceholderAssetWriter.LoadOrCreateAsset<IntEventChannel>(SelectedSlotChannelPath),
                toolUseChannel = PlaceholderAssetWriter.LoadOrCreateAsset<ToolUseEventChannel>(ToolUseChannelPath),
                tileClearedChannel = PlaceholderAssetWriter.LoadOrCreateAsset<TileClearedEventChannel>(TileClearedChannelPath),
                hintChannel = PlaceholderAssetWriter.LoadOrCreateAsset<StringEventChannel>(HintChannelPath),
            };

            // Weeds take one to three pulls by size and leave a clean slab behind; trash is one
            // grab with the trash bag (starting values, tune them in Data/Restoration/Clearables).
            a.weeds = new[]
            {
                LoadOrCreateClearable("WeedSmall", "overgrowth_weed_small", PlaceholderArt.WeedSmall, a.gloves, 1, groundTiles.patioClean),
                LoadOrCreateClearable("WeedMedium", "overgrowth_weed_medium", PlaceholderArt.WeedMedium, a.gloves, 2, groundTiles.patioClean),
                LoadOrCreateClearable("WeedLarge", "overgrowth_weed_large", PlaceholderArt.WeedLarge, a.gloves, 3, groundTiles.patioClean),
            };
            a.trash = new[]
            {
                LoadOrCreateClearable("TrashBag", "overgrowth_rubbish_bag", PlaceholderArt.RubbishBag, a.trashBag, 1, null),
                LoadOrCreateClearable("TrashCans", "overgrowth_rubbish_cans", PlaceholderArt.RubbishCans, a.trashBag, 1, null),
                LoadOrCreateClearable("TrashPotBroken", "overgrowth_rubbish_pot_broken", PlaceholderArt.RubbishPotBroken, a.trashBag, 1, null),
            };

            bool configIsNew = AssetDatabase.LoadAssetAtPath<ClearingConfig>(ClearingConfigPath) == null;
            a.clearing = PlaceholderAssetWriter.LoadOrCreateAsset<ClearingConfig>(ClearingConfigPath);
            if (configIsNew)
            {
                a.clearing.clearables = new[] { a.weeds[0], a.weeds[1], a.weeds[2], a.trash[0], a.trash[1], a.trash[2] };
                EditorUtility.SetDirty(a.clearing);
            }

            bool isNew = AssetDatabase.LoadAssetAtPath<ToolSet>(ToolSetPath) == null;
            a.tools = PlaceholderAssetWriter.LoadOrCreateAsset<ToolSet>(ToolSetPath);
            if (isNew)
            {
                // Act 1 adds the rake, shears and mower here as later jobs need them.
                SetRefs(a.tools, "tools", new Object[] { a.gloves, a.trashBag });
                EditorUtility.SetDirty(a.tools);
            }
            return a;
        }

        static ClearableData LoadOrCreateClearable(string fileName, string tileName, string spritePath,
            ToolData tool, int actions, TileBase groundAfter)
        {
            string path = ClearableFolder + fileName + ".asset";
            bool isNew = AssetDatabase.LoadAssetAtPath<ClearableData>(path) == null;
            var clearable = PlaceholderAssetWriter.LoadOrCreateAsset<ClearableData>(path);
            if (isNew)
            {
                clearable.tile = BackyardSceneBuilder.MakeTile(tileName, spritePath, Tile.ColliderType.None);
                clearable.tool = tool;
                clearable.actionsToClear = actions;
                clearable.groundAfter = groundAfter;
                EditorUtility.SetDirty(clearable);
            }
            return clearable;
        }

        static ToolData LoadOrCreateTool(string fileName, string displayName, string iconPath)
        {
            string path = ToolFolder + fileName + ".asset";
            bool isNew = AssetDatabase.LoadAssetAtPath<ToolData>(path) == null;
            var tool = PlaceholderAssetWriter.LoadOrCreateAsset<ToolData>(path);
            if (isNew)
            {
                tool.displayName = displayName;
                tool.icon = LoadSprite(iconPath);
                EditorUtility.SetDirty(tool);
            }
            return tool;
        }

        /// <summary>
        /// Gives the player a tool belt (the tool in hand, chosen with 1 to 6 or the mouse wheel)
        /// and a tool user, which aims the tool at a tile and shows a highlight there.
        /// </summary>
        public static void AddToPlayer(PlayerMovement player, InputReader input, PlayerConfig config, Assets a)
        {
            var go = player.gameObject;
            var belt = go.AddComponent<ToolBelt>();
            SetRef(belt, "input", input);
            SetRef(belt, "tools", a.tools);
            SetRef(belt, "selectedSlotChannel", a.selectedSlotChannel);

            // The highlight is its own object rather than a child of the player, because it sits on
            // the grid and should not move with the player between tiles.
            var highlight = new GameObject("Tool Highlight").AddComponent<SpriteRenderer>();
            highlight.sprite = LoadSprite(PlaceholderArt.TileHighlight);
            highlight.sortingOrder = BackyardSceneBuilder.HighlightOrder;
            highlight.enabled = false;

            var user = go.AddComponent<ToolUser>();
            SetRef(user, "input", input);
            SetRef(user, "movement", player);
            SetRef(user, "config", config);
            SetRef(user, "toolBelt", belt);
            SetRef(user, "highlight", highlight);
            SetRef(user, "toolUseChannel", a.toolUseChannel);
        }

        // ------------------------------------------------------------------ clearing

        /// <summary>
        /// Puts weeds on every weedy patio slab (sized at random) and trash on some of the clean
        /// ones, for Job 1. The player's starting tile and the letter's tile are kept clear.
        /// </summary>
        public static void PlaceOvergrowth(BackyardSceneBuilder.MapRefs map, BackyardSceneBuilder.TileSet tiles, Assets a)
        {
            var keepClear = new[]
            {
                Vector2Int.FloorToInt(BackyardLayout.PlayerStart),
                Vector2Int.FloorToInt(BackyardLayout.LetterSpot),
            };

            var patio = BackyardLayout.Patio;
            for (int y = patio.yMin; y < patio.yMax; y++)
                for (int x = patio.xMin; x < patio.xMax; x++)
                {
                    var cell = new Vector2Int(x, y);
                    if (System.Array.IndexOf(keepClear, cell) >= 0)
                    {
                        // A weedy slab here would look like it needs clearing but have nothing to pull.
                        map.ground.SetTile((Vector3Int)cell, tiles.patioClean);
                        continue;
                    }

                    ClearableData what = null;
                    if (map.ground.GetTile((Vector3Int)cell) == tiles.patioWeedy)
                        what = a.weeds[Pick(cell, 11, a.weeds.Length)];
                    else if (Random01(cell, 12) < BackyardLayout.PatioTrashShare)
                        what = a.trash[Pick(cell, 13, a.trash.Length)];

                    if (what != null) map.overgrowth.SetTile((Vector3Int)cell, what.tile);
                }
        }

        static float Random01(Vector2Int cell, int seed) => (PlaceholderHash.Mix(cell.x, cell.y, seed) & 0xffff) / 65535f;
        static int Pick(Vector2Int cell, int seed, int count) => (int)(PlaceholderHash.Mix(cell.x, cell.y, seed) % (uint)count);

        /// <summary>The clearing system, which handles tool uses on the Overgrowth tilemap.</summary>
        public static void AddClearingSystem(BackyardSceneBuilder.MapRefs map, Assets a)
        {
            var system = new GameObject("Clearing System").AddComponent<ClearingSystem>();
            SetRef(system, "overgrowth", map.overgrowth);
            SetRef(system, "ground", map.ground);
            SetRef(system, "config", a.clearing);
            SetRef(system, "toolUseChannel", a.toolUseChannel);
            SetRef(system, "tileClearedChannel", a.tileClearedChannel);
            SetRef(system, "hintChannel", a.hintChannel);
        }

        /// <summary>Short hints such as "Try the gloves.", above the toolbar and the prompt.</summary>
        public static void AddHint(Transform canvas, Assets a, float bottom)
        {
            var root = UiFactory.Root(canvas, "Hint");
            var label = UiFactory.Text(root, "Label", "", 12, TextAnchor.LowerCenter, UiFactory.LightText);
            UiFactory.Anchor(label.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, bottom), new Vector2(300f, 16f));
            label.gameObject.AddComponent<Outline>().effectColor = UiFactory.InkText;
            var hint = root.gameObject.AddComponent<HintUI>();
            SetRef(hint, "hintChannel", a.hintChannel);
            SetRef(hint, "label", label);
        }

        // Toolbar layout in art pixels: six 36 x 36 slots with a 2 pixel gap, along the bottom.
        const float SlotSize = 36f, SlotGap = 2f, ToolbarBottom = 6f;

        /// <summary>Height taken by the toolbar and its name label, so other UI can sit above it.</summary>
        public const float ToolbarHeight = 56f;

        /// <summary>The toolbar along the bottom centre of the screen, with the tool's name above it.</summary>
        public static void AddToolbar(Transform canvas, Assets a)
        {
            var root = UiFactory.Root(canvas, "Toolbar");
            var frames = new Image[ToolSet.MaxSlots];
            var icons = new Image[ToolSet.MaxSlots];
            float width = ToolSet.MaxSlots * SlotSize + (ToolSet.MaxSlots - 1) * SlotGap;

            for (int i = 0; i < ToolSet.MaxSlots; i++)
            {
                frames[i] = UiFactory.Image(root, $"Slot {i + 1}", PlaceholderArt.UiSlot);
                float x = -width / 2f + SlotSize / 2f + i * (SlotSize + SlotGap);
                UiFactory.Anchor(frames[i].rectTransform, new Vector2(0.5f, 0f), new Vector2(x, ToolbarBottom), new Vector2(SlotSize, SlotSize));
                // Shift the pivot back to the bottom centre of the slot so slots line up along the bottom edge.
                frames[i].rectTransform.pivot = new Vector2(0.5f, 0f);

                icons[i] = UiFactory.Icon(frames[i].transform, "Icon", null, new Vector2(32f, 32f));
                var key = UiFactory.Text(frames[i].transform, "Key", (i + 1).ToString(), 8, TextAnchor.UpperLeft, UiFactory.LightText);
                UiFactory.Stretch(key.rectTransform, 3, 0, 0, 1);
            }

            var name = UiFactory.Text(root, "Tool Name", "Gloves", 10, TextAnchor.LowerCenter, UiFactory.LightText);
            UiFactory.Anchor(name.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, ToolbarBottom + SlotSize + 1f), new Vector2(width, 13f));
            name.gameObject.AddComponent<Outline>().effectColor = UiFactory.InkText; // readable over grass and slabs

            var toolbar = root.gameObject.AddComponent<ToolbarUI>();
            SetRef(toolbar, "tools", a.tools);
            SetRef(toolbar, "selectedSlotChannel", a.selectedSlotChannel);
            SetRefs(toolbar, "slotFrames", frames);
            SetRefs(toolbar, "slotIcons", icons);
            SetRef(toolbar, "normalFrame", LoadSprite(PlaceholderArt.UiSlot));
            SetRef(toolbar, "selectedFrame", LoadSprite(PlaceholderArt.UiSlotSelected));
            SetRef(toolbar, "nameLabel", name);
        }
    }
}
