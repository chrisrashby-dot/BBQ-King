using UnityEngine;
using UnityEngine.Tilemaps;

namespace BBQKing.Restoration
{
    /// <summary>
    /// One kind of overgrowth or rubbish that can be cleared from the Overgrowth tilemap, such as a
    /// large weed or a trash bag (design.md section 5.2: "one to three actions, depending on the
    /// tile and tool"). Stored in Data/Restoration/Clearables.
    /// </summary>
    [CreateAssetMenu(fileName = "Clearable", menuName = "BBQ King/Restoration/Clearable")]
    public class ClearableData : ScriptableObject
    {
        [Tooltip("The tile on the Overgrowth tilemap that this describes.")]
        public TileBase tile;

        [Tooltip("The tool that clears it. Any other tool shows a hint naming this one.")]
        public ToolData tool;

        [Tooltip("How many uses of the tool it takes to clear.")]
        [Range(1, 3)] public int actionsToClear = 1;

        [Tooltip("Optional: the ground tile to put underneath once cleared, for example a clean slab " +
                 "where a weedy one was. Leave empty to keep the ground as it is.")]
        public TileBase groundAfter;
    }
}
