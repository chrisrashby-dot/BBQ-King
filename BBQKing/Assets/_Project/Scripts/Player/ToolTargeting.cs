using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Chooses which tile a tool works on. Plain C# so EditMode tests can check it.
    ///
    /// The rule (changed after the M2 play-test): with a mouse, the tile under the pointer, but only
    /// if it is next to the tile the player stands on (diagonals included) or is that tile itself.
    /// Anywhere further away there is no target, so nothing is highlighted and clicks do nothing.
    /// With no mouse (a gamepad), the tile just in front of the player's feet.
    ///
    /// Tiles are 1 x 1 world unit with the grid at the origin, so a position's tile is found by
    /// rounding down.
    /// </summary>
    public static class ToolTargeting
    {
        /// <param name="feet">The player's position (the feet).</param>
        /// <param name="facing">A one-tile vector the way the player faces.</param>
        /// <param name="aim">The mouse position in world units, or null with no mouse.</param>
        /// <param name="reachTiles">How many tiles from the player's own tile the mouse can pick (1 = adjacent).</param>
        /// <param name="facingDistance">How far in front of the feet the faced tile is picked.</param>
        /// <returns>The tile to work on, or null if the mouse is not over a tile within reach.</returns>
        public static Vector2Int? Choose(Vector2 feet, Vector2 facing, Vector2? aim, int reachTiles, float facingDistance)
        {
            if (!aim.HasValue) return CellAt(feet + facing * facingDistance);

            var aimed = CellAt(aim.Value);
            return IsWithin(CellAt(feet), aimed, reachTiles) ? aimed : (Vector2Int?)null;
        }

        /// <summary>True if <paramref name="other"/> is at most <paramref name="tiles"/> steps from
        /// <paramref name="centre"/>, counting a diagonal step as one.</summary>
        public static bool IsWithin(Vector2Int centre, Vector2Int other, int tiles) =>
            Mathf.Abs(other.x - centre.x) <= tiles && Mathf.Abs(other.y - centre.y) <= tiles;

        public static Vector2Int CellAt(Vector2 world) =>
            new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));

        public static Vector2 CellCentre(Vector2Int cell) => new Vector2(cell.x + 0.5f, cell.y + 0.5f);
    }
}
