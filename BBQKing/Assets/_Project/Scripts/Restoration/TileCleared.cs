using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>A tile has just been cleared. Jobs count these; later, sounds and effects can listen too.</summary>
    public readonly struct TileCleared
    {
        public readonly Vector2Int cell;
        public readonly ClearableData what;

        public TileCleared(Vector2Int cell, ClearableData what)
        {
            this.cell = cell;
            this.what = what;
        }
    }
}
