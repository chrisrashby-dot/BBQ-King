using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Works out which tile the player's tool is aimed at each frame and moves a highlight there,
    /// so the player can see what a click will affect (see <see cref="ToolTargeting"/>). With a
    /// mouse the highlight only appears while the pointer is over a tile next to the player.
    ///
    /// Pressing the use-tool button (left mouse) uses the tool once; holding it keeps using it at
    /// the rate set in PlayerConfig. Each use is posted to a channel, and whichever system owns
    /// that tile (clearing, for now) decides what happens.
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

        /// <summary>The tile the tool is aimed at this frame.</summary>
        public Vector2Int TargetCell { get; private set; }

        /// <summary>False while a panel is open or no tool is held.</summary>
        public bool HasTarget { get; private set; }

        Camera cam;
        float nextUseTime;

        void Start() => cam = Camera.main;

        void Update()
        {
            HasTarget = input != null && input.IsPlayerInputEnabled && config != null
                        && toolBelt != null && toolBelt.CurrentTool != null;

            if (HasTarget)
            {
                Vector2 facing = movement != null ? movement.FacingVector : Vector2.down;
                var cell = ToolTargeting.Choose(transform.position, facing, AimInWorld(),
                    config.toolReachTiles, config.toolFacingDistance);
                // With a mouse, a pointer away from the player gives no target: no highlight, no use.
                HasTarget = cell.HasValue;
                if (cell.HasValue) TargetCell = cell.Value;
            }

            if (highlight != null)
            {
                highlight.enabled = HasTarget;
                if (HasTarget) highlight.transform.position = ToolTargeting.CellCentre(TargetCell);
            }

            UseToolWhileHeld();
        }

        void UseToolWhileHeld()
        {
            // Letting go resets the timer, so the next press always acts straight away.
            if (!HasTarget || !input.UseToolHeld)
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
