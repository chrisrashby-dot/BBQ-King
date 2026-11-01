using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Shows the right walk frame for the way the player faces. Stands on frame 0 when still.
    ///
    /// This flips sprites directly rather than using Unity's Animator, because a four-direction,
    /// four-frame cycle is simple and this is easier to follow. To change the art, replace the
    /// PNGs or point the CharacterSprites asset at new ones.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [SerializeField] PlayerMovement movement;
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] CharacterSprites sprites;
        [SerializeField] PlayerConfig config;

        float walkSeconds;

        // LateUpdate runs after PlayerMovement.Update, so it sees this frame's facing.
        void LateUpdate()
        {
            if (movement == null || spriteRenderer == null || sprites == null) return;

            walkSeconds = movement.IsMoving ? walkSeconds + Time.deltaTime : 0f;

            Sprite[] frames = sprites.FramesFor(movement.Facing);
            if (frames.Length == 0) return;

            float fps = config != null ? config.walkFramesPerSecond : 0f;
            spriteRenderer.sprite = frames[WalkCycle.FrameAt(walkSeconds, fps, frames.Length)];
        }
    }
}
