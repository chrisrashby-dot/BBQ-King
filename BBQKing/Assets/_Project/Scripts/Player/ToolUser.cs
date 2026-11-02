using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Works out which weed or piece of trash the player's tool is aimed at each frame, moves a
    /// highlight there and switches to the tool it needs (see <see cref="ToolTargeting"/>). With a
    /// mouse, that is the tile under the pointer if it is next to the player; tiles with nothing to
    /// clear get no highlight.
    ///
    /// Pressing the use-tool button (left mouse) uses the tool once; holding it keeps using it at
    /// the rate set in PlayerConfig. Each use is posted to a channel, and the clearing system
    /// decides what happens.
    /// </summary>
    public class ToolUser : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerConfig config;
        [SerializeField] ToolBelt toolBelt;

        [Tooltip("The square drawn over the target tile. It is moved, shown and hidden by this script.")]
        [SerializeField] SpriteRenderer highlight;

        [Tooltip("Each use of the tool is posted here. The clearing system listens.")]
        [SerializeField] ToolUseEventChannel toolUseChannel;

        [Tooltip("Asked what is on a tile, so only weeds and trash are highlighted and the right tool is picked.")]
        [SerializeField] ClearingSystem clearing;

        /// <summary>The tile the tool is aimed at this frame.</summary>
        public Vector2Int TargetCell { get; private set; }

        /// <summary>True while the tool is aimed at something to clear (and no panel is open).</summary>
        public bool HasTarget { get; private set; }

        Camera cam;
        float nextUseTime;
        Vector2Int? toolPickedFor; // the tile the tool was last switched for

        void Start() => cam = Camera.main;

        void Update()
        {
            var what = FindTarget();
            HasTarget = what != null;

            if (!HasTarget) toolPickedFor = null;
            else if (toolPickedFor != TargetCell)
            {
                // Switch tools only when the aim moves onto a new tile, not every frame, so the
                // player can still pick another tool by hand while pointing at the same weed.
                toolBelt.Select(what.tool);
                toolPickedFor = TargetCell;
            }

            if (highlight != null)
            {
                highlight.enabled = HasTarget;
                if (HasTarget) highlight.transform.position = ToolTargeting.CellCentre(TargetCell);
            }

            UseToolWhileHeld();
        }

        /// <summary>What can be cleared on the aimed tile (also setting TargetCell), or null.</summary>
        ClearableData FindTarget()
        {
            if (input == null || !input.IsPlayerInputEnabled || config == null || toolBelt == null || clearing == null)
                return null;

            Vector2 facing = movement != null ? movement.FacingVector : Vector2.down;
            var cell = ToolTargeting.Choose(transform.position, facing, AimInWorld(),
                config.toolReachTiles, config.toolFacingDistance);
            if (!cell.HasValue) return null;

            TargetCell = cell.Value;
            return clearing.ClearableAt(cell.Value);
        }

        void UseToolWhileHeld()
        {
            // Letting go resets the timer, so the next press always acts straight away.
            if (!HasTarget || toolBelt.CurrentTool == null || !input.UseToolHeld)
            {
                nextUseTime = 0f;
                return;
            }
            if (Time.time < nextUseTime) return;

            nextUseTime = Time.time + config.toolRepeatSeconds;
            if (toolUseChannel != null) toolUseChannel.Raise(new ToolUse(TargetCell, toolBelt.CurrentTool));
        }

        /// <summary>The mouse position in world units, or null if there is no mouse position yet.</summary>
        Vector2? AimInWorld()
        {
            if (!input.HasAim || cam == null) return null;
            // Unity concept: ScreenToWorldPoint turns a pixel position on screen into a position in
            // the game world, taking the camera's position and zoom into account.
            return cam.ScreenToWorldPoint(input.AimScreenPosition);
        }
    }
}
