using UnityEngine;

namespace BBQKing.UI
{
    /// <summary>
    /// Timings for on-screen feedback, stored in Data/Settings/UiConfig.asset. They live in an
    /// asset rather than on the UI objects so that tuning them survives rebuilding the scene, and
    /// changes made during Play mode are kept.
    ///
    /// These are first guesses; design.md does not set them, so tune them by feel.
    /// </summary>
    [CreateAssetMenu(fileName = "UiConfig", menuName = "BBQ King/UI Config")]
    public class UiConfig : ScriptableObject
    {
        [Header("Hints (such as \"Try the gloves.\")")]
        [Tooltip("Seconds a hint stays fully visible.")]
        [Min(0f)] public float hintShowSeconds = 2f;

        [Tooltip("Seconds a hint takes to fade out afterwards.")]
        [Min(0f)] public float hintFadeSeconds = 0.5f;

        [Header("Toolbar")]
        [Tooltip("How big the slot grows when the tool changes (1 = no pulse).")]
        [Range(1f, 2f)] public float toolPulseScale = 1.3f;

        [Tooltip("Seconds the slot takes to settle back to its normal size.")]
        [Range(0.05f, 1f)] public float toolPulseSeconds = 0.25f;
    }
}
