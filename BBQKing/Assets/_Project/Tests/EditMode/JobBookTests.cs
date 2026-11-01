using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>Checks job statuses, counting down and completion in the job book.</summary>
    public class JobBookTests
    {
        JobBook book;
        JobData first, second;

        [SetUp]
        public void CreateJobs()
        {
            first = ScriptableObject.CreateInstance<JobData>();
            first.areas = new[] { new RectInt(0, 0, 3, 2) };
            second = ScriptableObject.CreateInstance<JobData>();
            second.requires = new[] { first };
            book = ScriptableObject.CreateInstance<JobBook>();
            book.SetJobs(new[] { first, second });
        }

        [TearDown]
        public void DestroyJobs()
        {
            Object.DestroyImmediate(book);
            Object.DestroyImmediate(first);
            Object.DestroyImmediate(second);
        }

        [Test]
        public void AJobIsLockedUntilTheJobsItNeedsAreDone()
        {
            Assert.AreEqual(JobStatus.Available, book.StatusOf(first));
            Assert.AreEqual(JobStatus.Locked, book.StatusOf(second));
            book.MarkDone(first);
            Assert.AreEqual(JobStatus.Done, book.StatusOf(first));
            Assert.AreEqual(JobStatus.Available, book.StatusOf(second));
        }

        [Test]
        public void TheCurrentJobIsTheFirstAvailableOne()
        {
            Assert.AreEqual(first, book.CurrentJob);
            book.MarkDone(first);
            Assert.AreEqual(second, book.CurrentJob);
            book.MarkDone(second);
            Assert.IsNull(book.CurrentJob);
        }

        [Test]
        public void CountingDownToZeroFinishesTheJobOnce()
        {
            int completions = 0;
            book.Completed += _ => completions++;
            book.SetRemaining(first, 2);
            book.SetRemaining(first, 1);
            Assert.AreEqual(JobStatus.Available, book.StatusOf(first));
            book.SetRemaining(first, 0);
            Assert.AreEqual(JobStatus.Done, book.StatusOf(first));
            book.SetRemaining(first, 0);
            Assert.AreEqual(1, completions, "a job is only completed once");
        }

        [Test]
        public void ALockedJobIsNotFinishedByReachingZero()
        {
            second.areas = new[] { new RectInt(5, 5, 1, 1) };
            book.SetRemaining(second, 0);
            Assert.AreEqual(JobStatus.Locked, book.StatusOf(second));
        }

        [Test]
        public void JobCoveringFindsTheJobWhoseAreaHasTheTile()
        {
            Assert.AreEqual(first, book.JobCovering(new Vector2Int(2, 1)));
            Assert.IsNull(book.JobCovering(new Vector2Int(3, 1)), "RectInt excludes its far edge");
        }

        [Test]
        public void ResetForgetsProgress()
        {
            book.SetRemaining(first, 4);
            book.MarkDone(first);
            book.ResetProgress();
            Assert.AreEqual(JobStatus.Available, book.StatusOf(first));
            Assert.AreEqual(-1, book.RemainingIn(first));
        }
    }
}
