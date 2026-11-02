using System.Collections.Generic;
using BBQKing.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BBQKing.Restoration
{
    /// <summary>
    /// Clears weeds and rubbish from the Overgrowth tilemap when the player uses the right tool on
    /// them (design.md section 5.2). It listens for tool uses on a channel, so it never needs to
    /// know about the player, and announces each cleared tile on another channel for jobs (and
    /// later sounds) to hear.
    ///
    /// Unity concept: a Tilemap stores one tile per grid cell. GetTile reads a cell and SetTile
    /// changes it; setting null empties the cell.
    /// </summary>
    public class ClearingSystem : MonoBehaviour
    {
        [SerializeField] Tilemap overgrowth;
        [SerializeField] Tilemap ground;
        [SerializeField] ClearingConfig config;

        [Tooltip("Tiles in the area of a job that is still locked cannot be cleared yet.")]
        [SerializeField] JobBook jobBook;

        [SerializeField] ToolUseEventChannel toolUseChannel;
        [SerializeField] TileClearedEventChannel tileClearedChannel;

        [Tooltip("Hints such as \"Try the gloves.\" are posted here for the UI to show.")]
        [SerializeField] StringEventChannel hintChannel;

        readonly ClearingProgress progress = new ClearingProgress();
        readonly Dictionary<TileBase, ClearableData> byTile = new Dictionary<TileBase, ClearableData>();

        void Awake()
        {
            // Build a lookup once, so finding what a tile is costs nothing per click.
            if (config == null) return;
            foreach (var c in config.clearables)
                if (c != null && c.tile != null) byTile[c.tile] = c;
        }

        void OnEnable()
        {
            if (toolUseChannel != null) toolUseChannel.Raised += OnToolUsed;
        }

        void OnDisable()
        {
            if (toolUseChannel != null) toolUseChannel.Raised -= OnToolUsed;
        }

        /// <summary>What can be cleared at a cell, or null if nothing.</summary>
        public ClearableData ClearableAt(Vector2Int cell)
        {
            var tile = overgrowth != null ? overgrowth.GetTile((Vector3Int)cell) : null;
            return tile != null && byTile.TryGetValue(tile, out var c) ? c : null;
        }

        /// <summary>True if the cell is in the area of a job that is still locked, so it cannot be cleared yet.</summary>
        public bool IsLocked(Vector2Int cell)
        {
            var job = jobBook != null ? jobBook.JobCovering(cell) : null;
            return job != null && jobBook.StatusOf(job) == JobStatus.Locked;
        }

        void OnToolUsed(ToolUse use)
        {
            var what = ClearableAt(use.cell);
            if (what == null) return; // nothing here to clear

            if (IsLocked(use.cell))
            {
                Hint(config.notYetHint);
                return;
            }

            if (what.tool != use.tool)
            {
                string toolName = what.tool != null ? what.tool.displayName.ToLowerInvariant() : "right tool";
                Hint(string.Format(config.wrongToolHint, toolName));
                return;
            }

            var cell = (Vector3Int)use.cell;
            int remaining = progress.Use(use.cell, what.actionsToClear);
            if (remaining > 0)
            {
                // Shrink the tile a little with each use. Tiles lock their transform by default, so
                // unlock this cell first.
                overgrowth.SetTileFlags(cell, TileFlags.None);
                float scale = 1f - config.shrinkPerAction * progress.UsesOn(use.cell);
                overgrowth.SetTransformMatrix(cell, Matrix4x4.Scale(new Vector3(scale, scale, 1f)));
                return;
            }

            overgrowth.SetTile(cell, null);
            if (what.groundAfter != null && ground != null) ground.SetTile(cell, what.groundAfter);
            if (tileClearedChannel != null) tileClearedChannel.Raise(new TileCleared(use.cell, what));
        }

        void Hint(string text)
        {
            if (hintChannel != null) hintChannel.Raise(text);
        }
    }
}
