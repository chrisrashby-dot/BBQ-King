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

        [Header("Tools")]
        [Tooltip("How many tiles from the tile the player stands on the mouse can pick a tile to work " +
                 "on. 1 means only the tiles touching it, diagonals included. Further away, clicks do nothing.")]
        [Range(0, 3)] public int toolReachTiles = 1;

        [Tooltip("With no mouse (a gamepad), how far in front of the player's feet the faced tile is " +
                 "picked, in tiles.")]
        [Range(0.1f, 1.5f)] public float toolFacingDistance = 0.6f;

        [Tooltip("While the use-tool button is held, seconds between one use and the next.")]
        [Range(0.05f, 2f)] public float toolRepeatSeconds = 0.3f;
    }
}
