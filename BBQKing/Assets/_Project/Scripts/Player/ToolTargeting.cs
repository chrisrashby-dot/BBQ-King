using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Chooses which tile a tool works on. Plain C# so EditMode tests can check it.
    ///
    /// The rule (decided for M2): the tile under the mouse, if its centre is within reach of the
    /// player's feet; otherwise the tile just in front of the player's feet. This lets mouse players
    /// point at a nearby weed while keyboard and gamepad players simply face it.
    ///
    /// Tiles are 1 x 1 world unit with the grid at the origin, so a position's tile is found by
    /// rounding down.
    /// </summary>
    public static class ToolTargeting
    {
        /// <param name="feet">The player's position (the feet).</param>
        /// <param name="facing">A one-tile vector the way the player faces.</param>
        /// <param name="aim">The mouse position in world units, or null with no mouse.</param>
        /// <param name="reach">How far from the feet the mouse can pick a tile, in tiles.</param>
        /// <param name="facingDistance">How far in front of the feet the faced tile is picked.</param>
        public static Vector2Int Choose(Vector2 feet, Vector2 facing, Vector2? aim, float reach, float facingDistance)
        {
            if (aim.HasValue)
            {
                var aimed = CellAt(aim.Value);
                if (Vector2.Distance(feet, CellCentre(aimed)) <= reach) return aimed;
            }
            return CellAt(feet + facing * facingDistance);
        }

        public static Vector2Int CellAt(Vector2 world) =>
            new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));

        public static Vector2 CellCentre(Vector2Int cell) => new Vector2(cell.x + 0.5f, cell.y + 0.5f);
    }
}
