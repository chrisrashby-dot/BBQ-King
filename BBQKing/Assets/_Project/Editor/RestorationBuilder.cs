using BBQKing.Core;
using BBQKing.Player;
using BBQKing.Restoration;
using BBQKing.UI;
using UnityEditor;
using UnityEngine;
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
        const string SelectedSlotChannelPath = "Assets/_Project/Data/Events/SelectedToolSlotChannel.asset";

        /// <summary>The assets the Restoration parts of the scene share.</summary>
        public struct Assets
        {
            public ToolSet tools;
            public ToolData gloves, trashBag;
            public IntEventChannel selectedSlotChannel;
        }

        /// <summary>Loads or creates the tool assets. Call after the scene has been created.</summary>
        public static Assets LoadAssets()
        {
            var a = new Assets
            {
                gloves = LoadOrCreateTool("Gloves", "Gloves", PlaceholderArt.ToolGloves),
                trashBag = LoadOrCreateTool("TrashBag", "Trash bag", PlaceholderArt.ToolTrashBag),
                selectedSlotChannel = PlaceholderAssetWriter.LoadOrCreateAsset<IntEventChannel>(SelectedSlotChannelPath),
            };

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

        /// <summary>Gives the player a tool belt (the tool in hand, chosen with 1 to 6 or the mouse wheel).</summary>
        public static void AddToPlayer(GameObject player, InputReader input, Assets a)
        {
            var belt = player.AddComponent<ToolBelt>();
            SetRef(belt, "input", input);
            SetRef(belt, "tools", a.tools);
            SetRef(belt, "selectedSlotChannel", a.selectedSlotChannel);
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
