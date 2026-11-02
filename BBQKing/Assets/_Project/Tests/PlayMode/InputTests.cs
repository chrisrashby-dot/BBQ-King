using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace BBQKing.Tests
{
    /// <summary>
    /// How the InputReader hands control between walking and panels, and between the mouse and a
    /// gamepad for aiming tools.
    /// </summary>
    public class InputTests : BackyardTestBase
    {
        [UnityTest]
        public IEnumerator WalkingOnlyComesBackWhenTheLastPanelCloses()
        {
            Assert.IsTrue(input.IsPlayerInputEnabled);

            // For example a completion message opening on top of a close-up.
            input.OpenPanel();
            input.OpenPanel();
            input.ClosePanel();
            Assert.IsFalse(input.IsPlayerInputEnabled, "one panel is still open");

            input.ClosePanel();
            Assert.IsTrue(input.IsPlayerInputEnabled);
            yield return null;
        }

        [UnityTest]
        public IEnumerator UsingAGamepadHandsAimingBackToTheFacedTile()
        {
            yield return PointAt(new Vector2Int(16, 19));
            Assert.IsTrue(input.HasAim, "the mouse aims once it moves");

            var gamepad = InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState { leftStick = new Vector2(1f, 0f) });
                yield return Frames(2);
                Assert.IsFalse(input.HasAim, "the gamepad's facing rule takes over");
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                yield return Frames(2);
            }
            finally
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }
    }
}
