using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// The backyard map plan from design.md section 4.2, in tile coordinates.
    /// (0,0) is the bottom-left tile; x goes right and y goes up, so the house is along the top.
    ///
    /// This only describes where things go when the scene is first built. Once built, you can
    /// paint the tilemaps by hand in Unity's Tile Palette window; just remember that running
    /// the builder again replaces the scene.
    /// </summary>
    public static class BackyardLayout
    {
        /// <summary>The whole map, roughly 40 by 25 tiles as in design.md section 4.2.</summary>
        public static readonly RectInt Map = new RectInt(0, 0, 40, 25);

        /// <summary>The back of the house, three tiles deep along the top edge.</summary>
        public static readonly RectInt House = new RectInt(4, 22, 24, 3);

        /// <summary>The cell in the bottom row of the house wall where the back door sits.</summary>
        public static readonly Vector2Int BackDoor = new Vector2Int(15, 22);

        /// <summary>Where the player stands at the start: on the patio, just outside the back door.</summary>
        public static readonly Vector2 PlayerStart = new Vector2(15.5f, 20.4f);

        /// <summary>Where the relative's letter lies on the patio (world position).</summary>
        public static readonly Vector2 LetterSpot = new Vector2(13.5f, 19.5f);

        /// <summary>Back door and patio: available from the start.</summary>
        public static readonly RectInt Patio = new RectInt(10, 15, 12, 7);

        /// <summary>Smoker corner: overgrown, bottom right (unlocked by Job 4).</summary>
        public static readonly RectInt SmokerCorner = new RectInt(30, 1, 9, 7);

        /// <summary>Shed and woodpile: top right, beside the house (unlocked by Job 5).</summary>
        public static readonly RectInt ShedYard = new RectInt(29, 14, 10, 10);

        /// <summary>Gravel paths linking the patio to the shed yard and the smoker corner.</summary>
        public static readonly RectInt[] Paths =
        {
            new RectInt(22, 17, 7, 2),  // patio to shed yard
            new RectInt(15, 8, 2, 7),   // patio down the lawn
            new RectInt(17, 8, 13, 2),  // across the lawn to the smoker corner
        };

        /// <summary>Fence cells that start broken (Job 3 replaces them).</summary>
        public static readonly Vector2Int[] BrokenFence =
        {
            new Vector2Int(6, 0), new Vector2Int(7, 0), new Vector2Int(19, 0),
            new Vector2Int(26, 0), new Vector2Int(0, 9), new Vector2Int(39, 11),
        };

        /// <summary>
        /// How much of a fence tile's height, from the bottom, blocks the player on fences that run
        /// across the screen (in tiles; 18 of 32 pixels). It is just over half a tile, so a player
        /// standing behind the fence always has their feet above the fence tile's sorting point and
        /// is drawn behind it.
        /// </summary>
        public const float FenceFootingDepth = 18f / 32f;

        /// <summary>Roughly what share of patio slabs start with weeds, 0 to 1.</summary>
        public const float PatioWeedShare = 0.45f;

        /// <summary>True for the cells around the outside edge that get a fence (not where the house is).</summary>
        public static bool IsFence(int x, int y)
        {
            bool edge = x == Map.xMin || x == Map.xMax - 1 || y == Map.yMin || y == Map.yMax - 1;
            return edge && !House.Contains(new Vector2Int(x, y));
        }
    }
}
