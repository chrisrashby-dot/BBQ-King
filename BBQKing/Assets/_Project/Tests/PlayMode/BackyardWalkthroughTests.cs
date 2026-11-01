using System.Collections;
using BBQKing.Core;
using BBQKing.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BBQKing.Tests
{
    /// <summary>
    /// PlayMode tests load a real scene and run it, pressing keys on a pretend keyboard.
    /// Open them with Window > General > Test Runner, choose PlayMode, then Run All.
    ///
    /// These check the M1 milestone end to end in the Backyard scene. The positions used match
    /// the map in Editor/BackyardLayout.cs; if you move the letter or door, update them here.
    /// </summary>
    public class BackyardWalkthroughTests : BackyardTestBase
    {
        GameObject promptPanel;
        Text promptLabel;
        GameObject letterPanel;

        protected override void FindSceneParts()
        {
            promptPanel = GameObject.Find("UI/Interaction Prompt").transform.Find("Panel").gameObject;
            promptLabel = promptPanel.GetComponentInChildren<Text>(true);
            letterPanel = GameObject.Find("UI/Message Panel").transform.Find("Paper").gameObject;
        }

        [UnityTest]
        public IEnumerator WalkingMovesAndTurnsThePlayer()
        {
            Vector2 start = player.transform.position;
            yield return Hold(Key.D, 0.5f);
            Assert.Greater(player.transform.position.x - start.x, 1f, "should walk over a tile in half a second");
            Assert.AreEqual(Facing.Right, movement.Facing);
        }

        [UnityTest]
        public IEnumerator LetterCanBeReadAndClosed()
        {
            // Stand just above the letter, facing down towards it.
            yield return Hold(Key.S, 0.02f);
            Teleport(new Vector2(13.5f, 20.2f));
            yield return Frames(3);
            Assert.IsTrue(promptPanel.activeSelf, "prompt should show near the letter");
            StringAssert.Contains("Read the letter", promptLabel.text);

            yield return Hold(Key.E, 0.05f);
            Assert.IsTrue(letterPanel.activeSelf, "E should open the letter");
            Assert.IsFalse(input.IsPlayerInputEnabled, "player controls should be off while reading");
            Assert.IsFalse(promptPanel.activeSelf, "prompt should hide while reading");

            Vector2 before = player.transform.position;
            yield return Hold(Key.D, 0.4f);
            Assert.Less(Vector2.Distance(before, player.transform.position), 0.01f, "player should not walk while reading");

            yield return Hold(Key.Escape, 0.05f);
            Assert.IsFalse(letterPanel.activeSelf, "Escape should close the letter");
            Assert.IsTrue(input.IsPlayerInputEnabled);

            yield return Hold(Key.E, 0.05f);
            Assert.IsTrue(letterPanel.activeSelf, "E should open it again");
            yield return Hold(Key.E, 0.05f);
            Assert.IsFalse(letterPanel.activeSelf, "E should also close it, without reopening on the same press");
        }

        [UnityTest]
        public IEnumerator BackDoorBlocksAndOffersAPrompt()
        {
            Teleport(new Vector2(15.5f, 20.5f));
            yield return Hold(Key.W, 1f);
            Assert.Less(player.transform.position.y, 22f, "the house wall should stop the player");
            Assert.IsTrue(promptPanel.activeSelf);
            StringAssert.Contains("back door", promptLabel.text);
        }

        [UnityTest]
        public IEnumerator FencesBlockThePlayer()
        {
            Teleport(new Vector2(3f, 10f));
            yield return Hold(Key.A, 1.5f);
            Assert.Greater(player.transform.position.x, 1f, "the west fence should stop the player");
        }

        [UnityTest]
        public IEnumerator PlayerCanStandBehindTheBottomFence()
        {
            // The bottom fence only blocks its footing, so the player should walk into the upper
            // part of the fence tile (y between 0.5 and 1), where Y-sorting draws them behind it.
            Teleport(new Vector2(10.5f, 3f));
            yield return Hold(Key.S, 1.5f);
            float y = player.transform.position.y;
            Assert.Less(y, 1f, "the player should step into the fence's tile, behind the pickets");
            Assert.Greater(y, 0.5f, "the fence footing should still stop the player");
        }

        [UnityTest]
        public IEnumerator DisplayConfigChangesTheCameraLive()
        {
            var display = Resources.FindObjectsOfTypeAll<DisplayConfig>()[0];
            var pixelCamera = Camera.main.GetComponent<PixelPerfectCamera>();
            var original = display.Resolution;
            try
            {
                display.Resolution = ReferenceResolution.Small640x360;
                yield return Frames(2);
                Assert.AreEqual(640, pixelCamera.refResolutionX);

                display.Resolution = ReferenceResolution.Large1280x720;
                yield return Frames(2);
                Assert.AreEqual(1280, pixelCamera.refResolutionX);
            }
            finally
            {
                display.Resolution = original; // do not leave the asset changed
            }
        }
    }
}
