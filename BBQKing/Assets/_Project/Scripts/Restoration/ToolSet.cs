using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// The tools on the backyard toolbar, in slot order: the first tool is key 1, the second key 2
    /// and so on, up to 6 (design.md section 6.1). Stored in Data/Tools/BackyardTools.asset.
    /// Reorder or add tools in the Inspector; the toolbar follows.
    /// </summary>
    [CreateAssetMenu(fileName = "ToolSet", menuName = "BBQ King/Restoration/Tool Set")]
    public class ToolSet : ScriptableObject
    {
        /// <summary>The number of toolbar slots, one per number key.</summary>
        public const int MaxSlots = 6;

        [Tooltip("Toolbar tools in slot order. At most 6.")]
        [SerializeField] ToolData[] tools = new ToolData[0];

        public int Count => Mathf.Min(tools.Length, MaxSlots);

        /// <summary>The tool in a slot (0 is the first), or null if the slot is empty.</summary>
        public ToolData Get(int slot) => slot >= 0 && slot < Count ? tools[slot] : null;

        /// <summary>The slot holding a tool (0 is the first), or -1 if it is not on the toolbar.</summary>
        public int SlotOf(ToolData tool)
        {
            if (tool == null) return -1;
            for (int i = 0; i < Count; i++)
                if (tools[i] == tool) return i;
            return -1;
        }
    }
}
