using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// The walk frames for one character, one list per direction (design.md: four directions,
    /// a 4-frame walk cycle). Frame 0 doubles as the standing pose.
    ///
    /// Stored as an asset (Data/Characters/PlayerSprites.asset) so that guests and neighbours can
    /// later reuse the same animator with their own sprite sets.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSprites", menuName = "BBQ King/Character Sprites")]
    public class CharacterSprites : ScriptableObject
    {
        public Sprite[] down = new Sprite[0];
        public Sprite[] up = new Sprite[0];
        public Sprite[] left = new Sprite[0];
        public Sprite[] right = new Sprite[0];

        /// <summary>The frames for a facing direction (never null).</summary>
        public Sprite[] FramesFor(Facing facing) => facing switch
        {
            Facing.Up => up,
            Facing.Left => left,
            Facing.Right => right,
            _ => down,
        } ?? new Sprite[0];
    }
}
