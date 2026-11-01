using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBQKing.Restoration
{
    /// <summary>
    /// The list of jobs and how far the player has got with each, shared by everything that needs
    /// to know: the job tracker, the clearing system, the HUD and the jobs board. Stored in
    /// Data/Jobs/JobBook.asset; edit the job order there.
    ///
    /// Like the InputReader, this is a ScriptableObject used as a shared notice board: everyone is
    /// given the same asset, so nobody needs a reference to anyone else. The progress itself is not
    /// saved into the asset (it is marked NonSerialized); saving to a file arrives in M5.
    /// </summary>
    [CreateAssetMenu(fileName = "JobBook", menuName = "BBQ King/Restoration/Job Book")]
    public class JobBook : ScriptableObject
    {
        [Tooltip("Every job in the order they appear on the jobs board.")]
        [SerializeField] JobData[] jobs = new JobData[0];

        /// <summary>Called whenever any job's progress or status changes.</summary>
        public event Action Changed;

        /// <summary>Called once when a job is finished.</summary>
        public event Action<JobData> Completed;

        [NonSerialized] readonly HashSet<JobData> done = new HashSet<JobData>();
        [NonSerialized] readonly Dictionary<JobData, int> remaining = new Dictionary<JobData, int>();

        public IReadOnlyList<JobData> Jobs => jobs;

        /// <summary>Replaces the job list. Used by the scene builder and tests; in the Editor, use the Inspector.</summary>
        public void SetJobs(JobData[] list) => jobs = list ?? new JobData[0];

        /// <summary>
        /// Forgets all progress. Needed at the start of play because this project keeps
        /// ScriptableObject data between Play sessions (domain reload is off).
        /// </summary>
        public void ResetProgress()
        {
            done.Clear();
            remaining.Clear();
            Changed?.Invoke();
        }

        public JobStatus StatusOf(JobData job)
        {
            if (done.Contains(job)) return JobStatus.Done;
            foreach (var required in job.requires)
                if (required != null && !done.Contains(required)) return JobStatus.Locked;
            return JobStatus.Available;
        }

        /// <summary>The first available job in the list, which the HUD shows; null if none.</summary>
        public JobData CurrentJob
        {
            get
            {
                foreach (var job in jobs)
                    if (job != null && StatusOf(job) == JobStatus.Available) return job;
                return null;
            }
        }

        /// <summary>The clearing job whose area contains a tile, or null.</summary>
        public JobData JobCovering(Vector2Int cell)
        {
            foreach (var job in jobs)
                if (job != null && job.Covers(cell)) return job;
            return null;
        }

        /// <summary>How many tiles are left to clear in a clearing job, or -1 if not counted yet.</summary>
        public int RemainingIn(JobData job) => remaining.TryGetValue(job, out int n) ? n : -1;

        /// <summary>Sets how many tiles are left. Reaching 0 on an available job finishes it.</summary>
        public void SetRemaining(JobData job, int count)
        {
            remaining[job] = Mathf.Max(0, count);
            if (count <= 0 && StatusOf(job) == JobStatus.Available) MarkDone(job);
            else Changed?.Invoke();
        }

        public void MarkDone(JobData job)
        {
            if (!done.Add(job)) return;
            Completed?.Invoke(job);
            Changed?.Invoke();
        }
    }
}
