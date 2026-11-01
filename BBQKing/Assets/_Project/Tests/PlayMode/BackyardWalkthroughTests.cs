using System.Collections;
using BBQKing.Core;
using BBQKing.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
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
    public class BackyardWalkthroughTests
    {
        Keyboard keyboard;
        InputSettings.BackgroundBehavior savedBackground;
        InputSettings.EditorInputBehaviorInPlayMode savedEditorBehaviour;

        GameObject player;
        PlayerMovement movement;
        GameObject promptPanel;
        Text promptLabel;
        GameObject letterPanel;
        InputReader input;

        [UnitySetUp]
        public IEnumerator LoadBackyard()
        {
            // The test runner window usually has focus, not the Game view, so tell the Input System
            // to accept our pretend key presses anyway. Restored in TearDown.
            savedBackground = InputSystem.settings.backgroundBehavior;
            savedEditorBehaviour = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>();

            SceneManager.LoadScene("Backyard");
            yield return Frames(5);

            player = GameObject.Find("Player");
            movement = player.GetComponent<PlayerMovement>();
            promptPanel = GameObject.Find("UI/Interaction Prompt").transform.Find("Panel").gameObject;
            promptLabel = promptPanel.GetComponentInChildren<Text>(true);
            letterPanel = GameObject.Find("UI/Message Panel").transform.Find("Paper").gameObject;
            input = Resources.FindObjectsOfTypeAll<InputReader>()[0];
        }

        [TearDown]
        public void RestoreInput()
        {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = savedBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = savedEditorBehaviour;
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

        // ------------------------------------------------------------------ helpers

        IEnumerator Hold(Key key, float seconds)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return Frames(2);
        }

        static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        void Teleport(Vector2 position)
        {
            player.GetComponent<Rigidbody2D>().position = position;
            player.transform.position = position;
        }
    }
}
