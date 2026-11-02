using System.Collections;
using BBQKing.Cleaning;
using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BBQKing.Tests
{
    /// <summary>
    /// M3 Cleaning in the Backyard: the kettle grill on the patio, and (from step 2) its close-up.
    /// The grill's position matches Editor/BackyardLayout.cs; if you move it, update it here.
    /// </summary>
    public class CleaningTests : BackyardTestBase
    {
        static readonly Vector2 GrillFeet = new Vector2(19.5f, 19.1f);

        JobBook jobBook;
        CloseUpSession session;
        Text promptLabel, hintLabel;

        protected override void FindSceneParts()
        {
            jobBook = Resources.FindObjectsOfTypeAll<JobBook>()[0];
            session = Resources.FindObjectsOfTypeAll<CloseUpSession>()[0];
            promptLabel = GameObject.Find("UI/Interaction Prompt").transform.Find("Panel").GetComponentInChildren<Text>(true);
            hintLabel = GameObject.Find("UI/Hint/Label").GetComponent<Text>();
        }

        /// <summary>Stands the player just above the grill, facing down at it.</summary>
        IEnumerator StandAboveTheGrill()
        {
            yield return Hold(Key.S, 0.02f);
            Teleport(GrillFeet + new Vector2(0f, 1.1f));
            yield return Frames(3);
        }

        /// <summary>Finishes Job 1 by hand and closes its "Patio cleared!" message.</summary>
        IEnumerator FinishJob1()
        {
            jobBook.MarkDone(jobBook.Jobs[0]);
            yield return Frames(2);
            yield return Hold(Key.Escape, 0.05f);
        }

        [UnityTest]
        public IEnumerator TheGrillBlocksThePlayer()
        {
            Teleport(GrillFeet + new Vector2(0f, -1f));
            yield return Hold(Key.W, 0.6f);
            Assert.Less(player.transform.position.y, GrillFeet.y, "the grill should stop the player walking through it");
        }

        [UnityTest]
        public IEnumerator BeforeJob1TheGrillOnlySaysWhyItMustWait()
        {
            yield return StandAboveTheGrill();
            StringAssert.Contains("Look at the kettle grill", promptLabel.text);

            CleanableData asked = null;
            System.Action<CleanableData> listen = d => asked = d;
            session.OpenRequested += listen;
            yield return Hold(Key.E, 0.05f);
            StringAssert.Contains("patio comes first", hintLabel.text);
            session.OpenRequested -= listen;
            Assert.IsNull(asked, "no close-up while Job 2 is locked");
        }

        [UnityTest]
        public IEnumerator AfterJob1TheGrillAsksForItsCloseUp()
        {
            yield return FinishJob1();
            yield return StandAboveTheGrill();
            StringAssert.Contains("Clean the kettle grill", promptLabel.text);

            CleanableData asked = null;
            System.Action<CleanableData> listen = d => asked = d;
            session.OpenRequested += listen;
            yield return Hold(Key.E, 0.05f);
            session.OpenRequested -= listen;
            Assert.IsNotNull(asked, "E should ask for the kettle grill's close-up");
            Assert.AreEqual(jobBook.Jobs[1], asked.job);
        }
    }
}
