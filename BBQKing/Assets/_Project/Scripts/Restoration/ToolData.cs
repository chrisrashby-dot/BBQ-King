using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// One tool the player can hold, such as the gloves or the trash bag (design.md section 5.2).
    /// Each tool is an asset in Data/Tools. What a tool can clear is set on the things being
    /// cleared (see <see cref="ClearableData"/>), not here, so adding a new kind of weed never
    /// means editing a tool.
    /// </summary>
    [CreateAssetMenu(fileName = "Tool", menuName = "BBQ King/Restoration/Tool")]
    public class ToolData : ScriptableObject
    {
        [Tooltip("Shown on the toolbar and in hints, for example \"Gloves\".")]
        public string displayName = "Tool";

        [Tooltip("The toolbar icon, 32 x 32 pixels.")]
        public Sprite icon;
    }
}
