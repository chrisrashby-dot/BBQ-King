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
        [Tooltip("How clean a section must be to count as done (design.md 5.2: 95%). The last bit " +
                 "is then wiped away for the player.")]
        [Range(0.5f, 1f)] public float completeAt = 0.95f;

        [Tooltip("Seconds between the last section's own sparkle and the big sparkle for the whole object.")]
        [Range(0f, 3f)] public float allDoneDelaySeconds = 0.5f;

        [Tooltip("Seconds the spotless object stays on screen after the big sparkle, before the close-up closes.")]
        [Range(0f, 5f)] public float finishPauseSeconds = 1.5f;

        [Header("Section done: sparkle and chime")]
        [Tooltip("How many sparkles appear over a finished section.")]
        [Range(1, 40)] public int sectionSparkles = 8;

        [Tooltip("How big each sparkle is drawn (1 = its 32 x 32 pixel art at backyard size).")]
        [Range(0.5f, 6f)] public float sectionSparkleScale = 1.5f;

        public AudioClip sectionChime;

        [Header("Everything done: bigger sparkle, flash and triple chime")]
        [Range(1, 80)] public int allDoneSparkles = 24;
        [Range(0.5f, 6f)] public float allDoneSparkleScale = 3f;

        [Tooltip("How bright the white flash over the screen starts (0 = none, 1 = solid white).")]
        [Range(0f, 1f)] public float flashStrength = 0.6f;

        [Tooltip("Seconds the flash takes to fade away.")]
        [Range(0.05f, 2f)] public float flashSeconds = 0.4f;

        public AudioClip allDoneChime;

        [Header("Sparkle timing")]
        [Tooltip("Seconds one sparkle takes to twinkle and fade.")]
        [Range(0.1f, 2f)] public float sparkleSeconds = 0.6f;

        [Tooltip("Sparkles start at random times within this many seconds, so they twinkle in turn.")]
        [Range(0f, 2f)] public float sparkleSpreadSeconds = 0.4f;
    }
}
