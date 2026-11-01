using System.Collections.Generic;
using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// Counts tool uses on each tile until it clears. Plain C# so EditMode tests can check it.
    /// Only tiles that have been worked on are stored, so a large map costs nothing.
    /// </summary>
    public class ClearingProgress
    {
        readonly Dictionary<Vector2Int, int> uses = new Dictionary<Vector2Int, int>();

        /// <summary>
        /// Records one use on a tile that needs <paramref name="actionsToClear"/> uses in all.
        /// Returns how many uses are still needed; 0 means it has just been cleared, and the tile
        /// is forgotten so a new weed in the same place starts from scratch.
        /// </summary>
        public int Use(Vector2Int cell, int actionsToClear)
        {
            uses.TryGetValue(cell, out int done);
            done++;
            int remaining = Mathf.Max(0, actionsToClear - done);
            if (remaining == 0) uses.Remove(cell);
            else uses[cell] = done;
            return remaining;
        }

        /// <summary>How many times a tile has been worked on without clearing yet.</summary>
        public int UsesOn(Vector2Int cell) => uses.TryGetValue(cell, out int done) ? done : 0;
    }
}
