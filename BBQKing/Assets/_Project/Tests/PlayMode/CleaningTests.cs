using System.Collections;
using BBQKing.Cleaning;
using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

        const string CloseUpScene = "CloseUp_Cleaning";

        /// <summary>Finishes Job 1, walks up to the grill and presses E, waiting for the close-up to load.</summary>
        IEnumerator OpenTheCloseUp()
        {
            yield return FinishJob1();
            yield return StandAboveTheGrill();
            yield return Hold(Key.E, 0.05f);
            float giveUp = Time.time + 5f;
            while (!SceneManager.GetSceneByName(CloseUpScene).isLoaded && Time.time < giveUp) yield return null;
            yield return Frames(2);
        }

        [UnityTest]
        public IEnumerator TheCloseUpOpensOverTheBackyardAndEscapeLeavesIt()
        {
            var backyardUi = GameObject.Find("UI").GetComponent<Canvas>();
            yield return OpenTheCloseUp();

            Assert.IsTrue(SceneManager.GetSceneByName(CloseUpScene).isLoaded, "E should open the close-up");
            Assert.IsTrue(input.IsCloseUpInputEnabled, "the close-up controls should be on");
            Assert.IsFalse(input.IsPlayerInputEnabled, "the player should not walk about behind the close-up");
            Assert.IsFalse(backyardUi.enabled, "the backyard HUD and toolbar should be hidden");
            var closeUp = Object.FindAnyObjectByType<CleaningCloseUp>();
            Assert.AreEqual("Clean the kettle grill", GameObject.Find("Close-up UI/Title").GetComponent<Text>().text);
            Assert.AreEqual(jobBook.Jobs[1], closeUp.Cleaning.job);

            yield return Hold(Key.Escape, 0.05f);
            yield return Frames(3);
            Assert.IsFalse(SceneManager.GetSceneByName(CloseUpScene).isLoaded, "Escape should leave the close-up");
            Assert.IsTrue(input.IsPlayerInputEnabled, "walking should come back");
            Assert.IsTrue(backyardUi.enabled, "the backyard UI should come back");
            Assert.AreEqual(JobStatus.Available, jobBook.StatusOf(jobBook.Jobs[1]), "leaving early does not finish the job");
        }

        /// <summary>Points the mouse at a spot on the close-up's object, in world units from its centre.</summary>
        IEnumerator PointAtGrime(Vector2 offset)
        {
            var cam = GameObject.Find("Close-up Camera").GetComponent<Camera>();
            var grill = GameObject.Find("Close-up/Object").transform;
            yield return PointAtScreen(cam.WorldToScreenPoint(grill.position + (Vector3)offset));
        }

        const int Top = 0, Bottom = 1; // the kettle grill's sections, in the order of its data asset

        [UnityTest]
        public IEnumerator HoldingTheMouseScrubsGrimeAwayAndLeavingKeepsIt()
        {
            yield return OpenTheCloseUp();
            var closeUp = Object.FindAnyObjectByType<CleaningCloseUp>();
            Assert.AreEqual(2, closeUp.LayerCount, "the kettle grill has a top and a bottom");
            Assert.AreEqual(0f, closeUp.CleanFraction(Top), "the grill starts grimy");

            yield return PointAtGrime(Vector2.zero); // the middle of the grate, in the top section
            yield return HoldLeftMouse(1f);
            float scrubbed = closeUp.CleanFraction(Top);
            Assert.Greater(scrubbed, 0f, "holding the mouse should scrub some grime off");
            Assert.Less(scrubbed, 0.5f, "one spot is nowhere near the whole top");
            Assert.AreEqual(0f, closeUp.CleanFraction(Bottom), "the bottom has its own percentage, still untouched");
            StringAssert.StartsWith("Top: ", GameObject.Find("Close-up UI/Meter Top Label").GetComponent<Text>().text);

            yield return Hold(Key.Escape, 0.05f);
            yield return Frames(3);
            yield return Hold(Key.E, 0.05f);
            float giveUp = Time.time + 5f;
            while (Object.FindAnyObjectByType<CleaningCloseUp>() == null && Time.time < giveUp) yield return null;
            yield return Frames(2);
            closeUp = Object.FindAnyObjectByType<CleaningCloseUp>();
            Assert.AreEqual(scrubbed, closeUp.CleanFraction(Top), 1e-5f, "coming back should keep the progress");
        }

        [UnityTest]
        public IEnumerator FinishingEachSectionCelebratesThenTheWholeGrill()
        {
            yield return OpenTheCloseUp();

            // A huge, fast brush, so the test needs only a few strokes. Put back afterwards,
            // because the config is an asset shared with the game. It is only loaded once the
            // close-up scene is open, as nothing in the backyard uses it.
            var config = Resources.FindObjectsOfTypeAll<CleaningConfig>()[0];
            float radius = config.brushRadius, speed = config.scrubPerSecond;
            config.brushRadius = 20f;
            config.scrubPerSecond = 10f;
            try
            {
                var closeUp = Object.FindAnyObjectByType<CleaningCloseUp>();
                var sectionsFinished = new System.Collections.Generic.List<int>();
                int bigCelebrations = 0;
                closeUp.SectionFinished += sectionsFinished.Add;
                closeUp.AllFinished += () => bigCelebrations++;

                // Brush over the whole grill, top and bottom, in a grid of spots 2 world units apart.
                for (float y = -6f; y <= 6f && !closeUp.IsFinished; y += 2f)
                    for (float x = -6f; x <= 6f && !closeUp.IsFinished; x += 2f)
                    {
                        yield return PointAtGrime(new Vector2(x, y));
                        yield return HoldLeftMouse(0.15f);
                    }

                Assert.IsTrue(closeUp.IsFinished,
                    $"the grill should be finished (top {closeUp.CleanFraction(Top):P0}, bottom {closeUp.CleanFraction(Bottom):P0})");
                CollectionAssert.AreEquivalent(new[] { Top, Bottom }, sectionsFinished, "each section celebrates once");
                Assert.AreEqual(1f, closeUp.CleanFraction(Top), "the last 5% is wiped away for the player");
                Assert.AreEqual(1f, closeUp.CleanFraction(Bottom));
                Assert.AreEqual("Top: Spotless!", GameObject.Find("Close-up UI/Meter Top Label").GetComponent<Text>().text);
                Assert.AreEqual("Bottom: Spotless!", GameObject.Find("Close-up UI/Meter Bottom Label").GetComponent<Text>().text);

                yield return new WaitForSeconds(0.3f);
                Assert.IsNotNull(GameObject.Find("Close-up/Sparkle"), "a finished section should sparkle");

                float waitForBig = Time.time + 3f;
                while (bigCelebrations == 0 && Time.time < waitForBig) yield return null;
                Assert.AreEqual(1, bigCelebrations, "finishing every section gives the big celebration");
                yield return null;
                Assert.Greater(GameObject.Find("Close-up UI/Flash").GetComponent<Image>().color.a, 0f, "the screen flashes");

                float giveUp = Time.time + 5f;
                while (SceneManager.GetSceneByName(CloseUpScene).isLoaded && Time.time < giveUp) yield return null;
                yield return Frames(2);
                Assert.IsFalse(SceneManager.GetSceneByName(CloseUpScene).isLoaded, "the close-up closes by itself after a short pause");
                Assert.AreEqual(JobStatus.Done, jobBook.StatusOf(jobBook.Jobs[1]), "Job 2 should be done");

                var messagePanel = GameObject.Find("UI/Message Panel").transform.Find("Paper").gameObject;
                Assert.IsTrue(messagePanel.activeSelf, "Job 2's completion message should show");
                Assert.AreEqual("Grill cleaned!", messagePanel.transform.Find("Title").GetComponent<Text>().text);
                var grill = Object.FindAnyObjectByType<CleanableObject>().GetComponent<SpriteRenderer>();
                StringAssert.Contains("clean", grill.sprite.name, "the grill in the backyard should look clean now");

                yield return Hold(Key.Escape, 0.05f);
                Assert.IsTrue(input.IsPlayerInputEnabled, "closing the message gives walking back");
            }
            finally
            {
                config.brushRadius = radius;
                config.scrubPerSecond = speed;
            }
        }
    }
}
