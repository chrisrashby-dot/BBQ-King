using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// One restoration job from design.md section 4.3, such as "Clear the patio". Stored in
    /// Data/Jobs. Which jobs exist, and in what order, is set in the <see cref="JobBook"/> asset.
    /// </summary>
    [CreateAssetMenu(fileName = "Job", menuName = "BBQ King/Restoration/Job")]
    public class JobData : ScriptableObject
    {
        public string title = "Job";

        [TextArea(2, 5)]
        public string description = "";

        [Tooltip("Jobs that must be done before this one is available.")]
        public JobData[] requires = new JobData[0];

        [Tooltip("For clearing jobs: the map areas (in tiles) whose weeds and trash must all be " +
                 "cleared. Leave empty for jobs finished another way, such as a close-up clean.")]
        public RectInt[] areas = new RectInt[0];

        [Tooltip("Optional: shown in a panel when the job is done.")]
        public MessageData completionMessage;

        /// <summary>True if this is a clearing job and the tile is in one of its areas.</summary>
        public bool Covers(Vector2Int cell)
        {
            foreach (var area in areas)
                if (area.Contains(cell)) return true;
            return false;
        }

        public bool IsClearingJob => areas.Length > 0;
    }
}
