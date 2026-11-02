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
        const string JobFolder = "Assets/_Project/Data/Jobs/";
        const string JobBookPath = JobFolder + "JobBook.asset";
        /// <summary>Job 2, finished in the cleaning close-up (see <see cref="CleaningBuilder"/>).</summary>
        public const string Job2Path = JobFolder + "Job2_CleanTheKettleGrill.asset";
        const string Job1MessagePath = "Assets/_Project/Data/Messages/Job1Done.asset";
        const string Job2MessagePath = "Assets/_Project/Data/Messages/Job2Done.asset";
        const string UiConfigPath = "Assets/_Project/Data/Settings/UiConfig.asset";

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
            public JobBook jobBook;
            public UiConfig ui;            // hint and toolbar timings
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
                ui = PlaceholderAssetWriter.LoadOrCreateAsset<UiConfig>(UiConfigPath),
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
                LoadOrCreateClearable("TrashBag", "overgrowth_trash_bag", PlaceholderArt.TrashBag, a.trashBag, 1, null),
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

            a.jobBook = LoadOrCreateJobs();

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

        // ------------------------------------------------------------------ jobs

        /// <summary>
        /// The five Act 1 jobs from design.md section 4.3, each needing the one before. Only Job 1
        /// can be finished in M2; the others are listed on the jobs board so the player can see
        /// what is coming. The events between jobs arrive in M4 and later.
        /// </summary>
        static JobBook LoadOrCreateJobs()
        {
            var job1 = LoadOrCreateJob("Job1_ClearThePatio", "Clear the patio",
                "Pull the weeds growing through the slabs and pick up the trash, tile by tile. " +
                "Use the gloves for weeds and the trash bag for trash.",
                null, new[] { BackyardLayout.Patio }, LoadOrCreateJob1Message());
            var job2 = LoadOrCreateJob("Job2_CleanTheKettleGrill", "Clean the kettle grill",
                "Scrub years of grime off the old kettle grill on the patio.", job1);
            if (job2.completionMessage == null)
            {
                // Job 2 was first made in M2 without a message; M3 gives it one. A message you
                // have chosen in the Inspector is kept.
                job2.completionMessage = LoadOrCreateMessage(Job2MessagePath, "Grill cleaned!",
                    "[Placeholder text] Under all that grime, the old kettle grill still shines. " +
                    "It looks ready for its first cookout in years.\n\n" +
                    "Press Tab to see the jobs board.");
                EditorUtility.SetDirty(job2);
            }
            var job3 = LoadOrCreateJob("Job3_ClearTheLawn", "Clear the lawn and fix the fence",
                "Mow and rake the lawn and replace the broken fence panels.", job2);
            var job4 = LoadOrCreateJob("Job4_RestoreTheSmoker", "Restore the offset smoker",
                "Take off the tarp, scrape off the rust, clean the firebox and replace the door seal.", job3);
            var job5 = LoadOrCreateJob("Job5_SortTheShed", "Sort the shed and stack the woodpile",
                "Tidy the shed and stack the wood under cover so it dries. Wet wood smokes badly.", job4);

            bool isNew = AssetDatabase.LoadAssetAtPath<JobBook>(JobBookPath) == null;
            var book = PlaceholderAssetWriter.LoadOrCreateAsset<JobBook>(JobBookPath);
            if (isNew)
            {
                book.SetJobs(new[] { job1, job2, job3, job4, job5 });
                EditorUtility.SetDirty(book);
            }
            return book;
        }

        static JobData LoadOrCreateJob(string fileName, string title, string description, JobData requires,
            RectInt[] areas = null, MessageData completionMessage = null)
        {
            string path = JobFolder + fileName + ".asset";
            bool isNew = AssetDatabase.LoadAssetAtPath<JobData>(path) == null;
            var job = PlaceholderAssetWriter.LoadOrCreateAsset<JobData>(path);
            if (isNew)
            {
                job.title = title;
                job.description = description;
                job.requires = requires != null ? new[] { requires } : new JobData[0];
                job.areas = areas ?? new RectInt[0];
                job.completionMessage = completionMessage;
                EditorUtility.SetDirty(job);
            }
            return job;
        }

        static MessageData LoadOrCreateJob1Message() => LoadOrCreateMessage(Job1MessagePath, "Patio cleared!",
            "[Placeholder text] The patio is clear for the first time in years. " +
            "The old kettle grill could do with a good scrub next.\n\n" +
            "Next job: Clean the kettle grill. Press Tab to see the jobs board.");

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
        /// Gives the player a tool belt (the tool in hand, chosen with 1 to 6, the mouse wheel or
        /// automatically) and a tool user, which aims the tool at a weed or piece of trash, shows a
        /// highlight there and picks the tool it needs.
        /// </summary>
        public static void AddToPlayer(PlayerMovement player, InputReader input, PlayerConfig config, Assets a, ClearingSystem clearing)
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
            SetRef(user, "clearing", clearing);
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
                Vector2Int.FloorToInt(BackyardLayout.KettleGrill),
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
                    else if (Random01(cell, 12) < a.clearing.patioTrashShare)
                        what = a.trash[Pick(cell, 13, a.trash.Length)];

                    if (what != null) map.overgrowth.SetTile((Vector3Int)cell, what.tile);
                }
        }

        static float Random01(Vector2Int cell, int seed) => (PlaceholderHash.Mix(cell.x, cell.y, seed) & 0xffff) / 65535f;
        static int Pick(Vector2Int cell, int seed, int count) => (int)(PlaceholderHash.Mix(cell.x, cell.y, seed) % (uint)count);

        /// <summary>
        /// The clearing system, which handles tool uses on the Overgrowth tilemap, and the job
        /// tracker, which counts cleared tiles towards jobs. Both live on one "Restoration" object.
        /// </summary>
        public static ClearingSystem AddClearingSystem(BackyardSceneBuilder.MapRefs map, Assets a, MessageEventChannel messageChannel)
        {
            var go = new GameObject("Restoration");
            var system = go.AddComponent<ClearingSystem>();
            SetRef(system, "overgrowth", map.overgrowth);
            SetRef(system, "ground", map.ground);
            SetRef(system, "config", a.clearing);
            SetRef(system, "jobBook", a.jobBook);
            SetRef(system, "toolUseChannel", a.toolUseChannel);
            SetRef(system, "tileClearedChannel", a.tileClearedChannel);
            SetRef(system, "hintChannel", a.hintChannel);

            var tracker = go.AddComponent<JobTracker>();
            SetRef(tracker, "jobBook", a.jobBook);
            SetRef(tracker, "clearing", system);
            SetRef(tracker, "tileClearedChannel", a.tileClearedChannel);
            SetRef(tracker, "showMessageChannel", messageChannel);
            return system;
        }

        /// <summary>The jobs board, a paper panel in the middle of the screen opened with Tab.</summary>
        public static void AddJobsBoard(Transform canvas, InputReader input, Assets a)
        {
            var root = UiFactory.Root(canvas, "Jobs Board");
            var paper = UiFactory.Image(root, "Paper", PlaceholderArt.UiPanelPaper);
            UiFactory.Anchor(paper.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 280f));
            var title = UiFactory.Text(paper.transform, "Title", "Jobs", 16, TextAnchor.UpperLeft, UiFactory.InkText);
            title.fontStyle = FontStyle.Bold;
            UiFactory.Stretch(title.rectTransform, 16, 240, 16, 14);
            var list = UiFactory.Text(paper.transform, "List", "", 11, TextAnchor.UpperLeft, UiFactory.InkText);
            list.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiFactory.Stretch(list.rectTransform, 16, 30, 16, 40);
            var hint = UiFactory.Text(paper.transform, "Close Hint", "[Tab] or [Esc] Close", 10, TextAnchor.LowerRight, UiFactory.InkText);
            UiFactory.Stretch(hint.rectTransform, 16, 10, 16, 250);

            var board = root.gameObject.AddComponent<JobsBoardUI>();
            SetRef(board, "input", input);
            SetRef(board, "jobBook", a.jobBook);
            SetRef(board, "panel", paper.gameObject);
            SetRef(board, "listLabel", list);
            paper.gameObject.SetActive(false);
        }

        /// <summary>The current job in the top-left corner of the screen.</summary>
        public static void AddJobHud(Transform canvas, Assets a)
        {
            var root = UiFactory.Root(canvas, "Job HUD");
            var panel = UiFactory.Image(root, "Panel", PlaceholderArt.UiPanelDark);
            UiFactory.Anchor(panel.rectTransform, new Vector2(0f, 1f), new Vector2(6f, -6f), new Vector2(170f, 34f));
            var title = UiFactory.Text(panel.transform, "Title", "Clear the patio", 12, TextAnchor.UpperLeft, UiFactory.LightText);
            UiFactory.Stretch(title.rectTransform, 6, 14, 4, 3);
            var progress = UiFactory.Text(panel.transform, "Progress", "38 left", 10, TextAnchor.LowerLeft, UiFactory.LightText);
            UiFactory.Stretch(progress.rectTransform, 6, 3, 4, 18);

            var hud = root.gameObject.AddComponent<JobHudUI>();
            SetRef(hud, "jobBook", a.jobBook);
            SetRef(hud, "titleLabel", title);
            SetRef(hud, "progressLabel", progress);
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
            SetRef(hint, "config", a.ui);
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
            SetRef(toolbar, "config", a.ui);
        }
    }
}
