using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// Keeps the job book up to date in the Backyard scene. At the start it counts the weeds and
    /// trash in each clearing job's area; then it hears every cleared tile and counts down. When a
    /// job reaches zero the job book marks it done, and its completion message (if it has one) is
    /// shown.
    ///
    /// Design decision for M2: a clearing job is done when every tile in it is cleared. The 95%
    /// rule in design.md section 5.2 is for close-up scrubbing only.
    /// </summary>
    public class JobTracker : MonoBehaviour
    {
        [SerializeField] JobBook jobBook;
        [SerializeField] ClearingSystem clearing;
        [SerializeField] TileClearedEventChannel tileClearedChannel;

        [Tooltip("Completion messages are posted here; the message panel shows them.")]
        [SerializeField] MessageEventChannel showMessageChannel;

        void Awake()
        {
            if (jobBook != null) jobBook.ResetProgress();
        }

        void OnEnable()
        {
            if (tileClearedChannel != null) tileClearedChannel.Raised += OnTileCleared;
            if (jobBook != null) jobBook.Completed += OnJobCompleted;
        }

        void OnDisable()
        {
            if (tileClearedChannel != null) tileClearedChannel.Raised -= OnTileCleared;
            if (jobBook != null) jobBook.Completed -= OnJobCompleted;
        }

        void Start()
        {
            if (jobBook == null || clearing == null) return;
            foreach (var job in jobBook.Jobs)
                if (job != null && job.IsClearingJob) jobBook.SetRemaining(job, CountClearables(job));
        }

        int CountClearables(JobData job)
        {
            int count = 0;
            foreach (var area in job.areas)
                foreach (var cell in area.allPositionsWithin)
                    if (clearing.ClearableAt(cell) != null) count++;
            return count;
        }

        void OnTileCleared(TileCleared cleared)
        {
            var job = jobBook.JobCovering(cleared.cell);
            if (job == null || jobBook.StatusOf(job) != JobStatus.Available) return;
            jobBook.SetRemaining(job, jobBook.RemainingIn(job) - 1);
        }

        void OnJobCompleted(JobData job)
        {
            if (job.completionMessage != null && showMessageChannel != null)
                showMessageChannel.Raise(job.completionMessage);
        }
    }
}
