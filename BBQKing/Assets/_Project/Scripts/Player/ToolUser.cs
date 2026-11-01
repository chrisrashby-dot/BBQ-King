using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Works out which tile the player's tool is aimed at each frame and moves a highlight there,
    /// so the player can always see what a click will affect (see <see cref="ToolTargeting"/>).
    /// </summary>
    public class ToolUser : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerConfig config;
        [SerializeField] ToolBelt toolBelt;

        [Tooltip("The square drawn over the target tile. It is moved, shown and hidden by this script.")]
        [SerializeField] SpriteRenderer highlight;

        /// <summary>The tile the tool is aimed at this frame.</summary>
        public Vector2Int TargetCell { get; private set; }

        /// <summary>False while a panel is open or no tool is held.</summary>
        public bool HasTarget { get; private set; }

        Camera cam;

        void Start() => cam = Camera.main;

        void Update()
        {
            HasTarget = input != null && input.IsPlayerInputEnabled && config != null
                        && toolBelt != null && toolBelt.CurrentTool != null;

            if (HasTarget)
            {
                Vector2 facing = movement != null ? movement.FacingVector : Vector2.down;
                TargetCell = ToolTargeting.Choose(transform.position, facing, AimInWorld(),
                    config.toolReach, config.toolFacingDistance);
            }

            if (highlight != null)
            {
                highlight.enabled = HasTarget;
                if (HasTarget) highlight.transform.position = ToolTargeting.CellCentre(TargetCell);
            }
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
