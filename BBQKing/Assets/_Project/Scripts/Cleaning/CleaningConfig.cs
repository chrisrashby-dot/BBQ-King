using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// Tuning values for close-up cleaning, stored in Data/Cleaning/CleaningConfig.asset.
    ///
    /// The 95% finishing line comes from design.md section 5.2. The brush size and speed are first
    /// guesses (design.md does not set them; tool upgrades will raise them later), so tune them by
    /// feel. Changes made during Play mode are kept, because this is an asset.
    /// </summary>
    [CreateAssetMenu(fileName = "CleaningConfig", menuName = "BBQ King/Cleaning/Cleaning Config")]
    public class CleaningConfig : ScriptableObject
    {
        [Header("Brush")]
        [Tooltip("The brush's radius, in pixels of the close-up art (before it is drawn 3 times bigger).")]
        [Range(1f, 20f)] public float brushRadius = 6f;

        [Tooltip("How much grime the brush takes off each pixel per second while held. 1 wipes a " +
                 "fully grimy pixel in one second; 3 in a third of a second.")]
        [Range(0.1f, 10f)] public float scrubPerSecond = 3f;

        [Header("Finishing")]
        [Tooltip("How clean the object must be for the job to count as done (design.md 5.2: 95%). " +
                 "The last bit is then wiped away for the player.")]
        [Range(0.5f, 1f)] public float completeAt = 0.95f;

        [Tooltip("Seconds the spotless object stays on screen before the close-up closes.")]
        [Range(0f, 5f)] public float finishPauseSeconds = 1.5f;
    }
}
