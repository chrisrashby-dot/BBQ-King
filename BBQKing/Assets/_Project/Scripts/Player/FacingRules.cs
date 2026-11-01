using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Pure rules for turning a movement direction into one of the four facings. Kept separate
    /// from the MonoBehaviour so it can be checked by EditMode tests without running the game.
    /// </summary>
    public static class FacingRules
    {
        // Below this squared length the input counts as "not moving". This is a technical
        // tolerance for tiny stick drift, not a gameplay value, so it stays in code.
        const float StillThresholdSquared = 0.0001f;

        /// <summary>True if the input is strong enough to count as walking.</summary>
        public static bool IsMoving(Vector2 input) => input.sqrMagnitude > StillThresholdSquared;

        /// <summary>
        /// Picks the facing from the stronger input axis. Movement can be diagonal (like Stardew
        /// Valley) but the sprite only has four directions. On an exact diagonal, the current facing
        /// is kept if it is one of the two directions involved, so the sprite does not flicker.
        /// With no input, the current facing is kept.
        /// </summary>
        public static Facing FromInput(Vector2 input, Facing current)
        {
            if (!IsMoving(input)) return current;

            float ax = Mathf.Abs(input.x), ay = Mathf.Abs(input.y);
            if (Mathf.Approximately(ax, ay))
            {
                bool currentStillFits =
                    (current == Facing.Left && input.x < 0) || (current == Facing.Right && input.x > 0) ||
                    (current == Facing.Down && input.y < 0) || (current == Facing.Up && input.y > 0);
                if (currentStillFits) return current;
            }

            if (ax > ay) return input.x < 0 ? Facing.Left : Facing.Right;
            return input.y < 0 ? Facing.Down : Facing.Up;
        }

        /// <summary>A one-tile-long vector pointing the way a character faces.</summary>
        public static Vector2 ToVector(Facing facing) => facing switch
        {
            Facing.Up => Vector2.up,
            Facing.Left => Vector2.left,
            Facing.Right => Vector2.right,
            _ => Vector2.down,
        };
    }
}
