using System.Collections;
using BBQKing.Core;
using BBQKing.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace BBQKing.Tests
{
    /// <summary>
    /// Shared set-up for PlayMode tests in the Backyard: loads the scene, adds a pretend keyboard
    /// and mouse, and offers helpers to press keys and move the player.
    /// </summary>
    public abstract class BackyardTestBase
    {
        protected Keyboard keyboard;
        protected Mouse mouse;
        protected GameObject player;
        protected PlayerMovement movement;
        protected InputReader input;

        InputSettings.BackgroundBehavior savedBackground;
        InputSettings.EditorInputBehaviorInPlayMode savedEditorBehaviour;

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
            mouse = InputSystem.AddDevice<Mouse>();

            SceneManager.LoadScene("Backyard");
            yield return Frames(5);

            player = GameObject.Find("Player");
            movement = player.GetComponent<PlayerMovement>();
            input = Resources.FindObjectsOfTypeAll<InputReader>()[0];
            FindSceneParts();
        }

        /// <summary>Override to look up the objects a test class needs once the scene has loaded.</summary>
        protected virtual void FindSceneParts() { }

        [TearDown]
        public void RestoreInput()
        {
            InputSystem.RemoveDevice(keyboard);
            InputSystem.RemoveDevice(mouse);
            InputSystem.settings.backgroundBehavior = savedBackground;
            InputSystem.settings.editorInputBehaviorInPlayMode = savedEditorBehaviour;
        }

        protected IEnumerator Hold(Key key, float seconds)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return Seconds(seconds);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return Frames(2);
        }

        /// <summary>Points the mouse at the centre of a tile, then holds the left button there.</summary>
        protected IEnumerator HoldLeftMouseOn(Vector2Int cell, float seconds)
        {
            Vector2 screen = Camera.main.WorldToScreenPoint(new Vector3(cell.x + 0.5f, cell.y + 0.5f, 0f));
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            yield return Frames(2);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen }.WithButton(MouseButton.Left));
            yield return Seconds(seconds);
            InputSystem.QueueStateEvent(mouse, new MouseState { position = screen });
            yield return Frames(2);
        }

        static IEnumerator Seconds(float seconds)
        {
            float end = Time.time + seconds;
            while (Time.time < end) yield return null;
        }

        protected static IEnumerator Frames(int count)
        {
            for (int i = 0; i < count; i++) yield return null;
        }

        protected void Teleport(Vector2 position)
        {
            player.GetComponent<Rigidbody2D>().position = position;
            player.transform.position = position;
        }
    }
}
