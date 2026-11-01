using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>The player used a tool on a tile. Posted by the player, handled by the clearing system.</summary>
    public readonly struct ToolUse
    {
        public readonly Vector2Int cell;
        public readonly ToolData tool;

        public ToolUse(Vector2Int cell, ToolData tool)
        {
            this.cell = cell;
            this.tool = tool;
        }
    }
}
