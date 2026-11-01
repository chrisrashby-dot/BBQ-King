using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Tuning values for the player, stored in Data/Player/PlayerConfig.asset so they can be
    /// changed in the Inspector without touching code. Changes made during Play mode are kept,
    /// because this is an asset rather than an object in the scene.
    ///
    /// These are first guesses; design.md does not set them yet, so tune them by feel.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "BBQ King/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Walking speed in tiles per second.")]
        [Range(1f, 10f)] public float moveSpeed = 3.5f;

        [Header("Animation")]
        [Tooltip("Walk animation speed in frames per second. With 4 frames, 8 gives two steps a second.")]
        [Range(1f, 24f)] public float walkFramesPerSecond = 8f;

        [Header("Interaction")]
        [Tooltip("How far in front of the player's feet to look for things to use, in tiles.")]
        [Range(0f, 2f)] public float reachOffset = 0.5f;

        [Tooltip("Radius of the circle searched for things to use, in tiles.")]
        [Range(0.1f, 2f)] public float reachRadius = 0.6f;
    }
}
