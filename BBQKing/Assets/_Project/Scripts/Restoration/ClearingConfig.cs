using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// Settings for clearing, stored in Data/Restoration/ClearingConfig.asset: the list of things
    /// that can be cleared, the hint texts, and how a tile shrinks as it is worked on.
    /// </summary>
    [CreateAssetMenu(fileName = "ClearingConfig", menuName = "BBQ King/Restoration/Clearing Config")]
    public class ClearingConfig : ScriptableObject
    {
        [Tooltip("Everything on the Overgrowth tilemap that a tool can clear.")]
        public ClearableData[] clearables = new ClearableData[0];

        [Tooltip("Shown when the wrong tool is used. {0} is replaced with the right tool's name.")]
        public string wrongToolHint = "Try the {0}.";

        [Tooltip("Shown when the tile belongs to a job that is not available yet.")]
        public string notYetHint = "That can wait for now.";

        [Tooltip("How much smaller a tile gets with each use before it clears (0.15 = 15%), so " +
                 "the player can see a big weed coming loose.")]
        [Range(0f, 0.3f)] public float shrinkPerAction = 0.15f;
    }
}
