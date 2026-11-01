using System.Collections;
using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BBQKing.Tests
{
    /// <summary>
    /// Job 1 in the Backyard: the HUD counts the patio's weeds and trash, and clearing them all
    /// finishes the job, shows the completion message and moves the HUD on to Job 2.
    /// </summary>
    public class JobTests : BackyardTestBase
    {
        ClearingSystem clearing;
        JobBook jobBook;
        ToolUseEventChannel toolUses;
        Text hudTitle, hudProgress;
        GameObject messagePanel;

        protected override void FindSceneParts()
        {
            clearing = Object.FindAnyObjectByType<ClearingSystem>();
            jobBook = Resources.FindObjectsOfTypeAll<JobBook>()[0];
            toolUses = Resources.FindObjectsOfTypeAll<ToolUseEventChannel>()[0];
            hudTitle = GameObject.Find("UI/Job HUD/Panel/Title").GetComponent<Text>();
            hudProgress = GameObject.Find("UI/Job HUD/Panel/Progress").GetComponent<Text>();
            messagePanel = GameObject.Find("UI/Message Panel").transform.Find("Paper").gameObject;
        }

        static readonly RectInt Patio = new RectInt(10, 15, 12, 7); // matches Editor/BackyardLayout.cs

        int CountOnPatio()
        {
            int n = 0;
            foreach (var cell in Patio.allPositionsWithin)
                if (clearing.ClearableAt(cell) != null) n++;
            return n;
        }

        [UnityTest]
        public IEnumerator TheHudShowsJob1AndWhatIsLeft()
        {
            yield return null;
            Assert.AreEqual("Clear the patio", hudTitle.text);
            int count = CountOnPatio();
            Assert.Greater(count, 0, "the patio should start with weeds and trash");
            Assert.AreEqual($"{count} left", hudProgress.text);
        }

        [UnityTest]
        public IEnumerator ClearingTheWholePatioFinishesJob1()
        {
            var job1 = jobBook.CurrentJob;
            // Use the right tool on every tile, as many times as it needs.
            foreach (var cell in Patio.allPositionsWithin)
            {
                var what = clearing.ClearableAt(cell);
                if (what == null) continue;
                for (int i = 0; i < what.actionsToClear; i++) toolUses.Raise(new ToolUse(cell, what.tool));
            }
            yield return null;

            Assert.AreEqual(0, CountOnPatio());
            Assert.AreEqual(JobStatus.Done, jobBook.StatusOf(job1));
            Assert.IsTrue(messagePanel.activeSelf, "the 'Patio cleared!' message should show");
            Assert.AreEqual("Clean the kettle grill", hudTitle.text, "the HUD moves on to Job 2");
            Assert.AreEqual(string.Empty, hudProgress.text, "Job 2 is not a clearing job");
        }
    }
}
