using System.Collections;
using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BBQKing.Tests
{
    /// <summary>
    /// M2 clearing in the Backyard, using real key and mouse presses: pulling a weed with the
    /// gloves, and the hint when the wrong tool is used.
    /// </summary>
    public class ClearingTests : BackyardTestBase
    {
        ClearingSystem clearing;
        Text hintLabel;

        protected override void FindSceneParts()
        {
            clearing = Object.FindAnyObjectByType<ClearingSystem>();
            hintLabel = GameObject.Find("UI/Hint/Label").GetComponent<Text>();
        }

        /// <summary>Finds a weed on the patio and stands the player just above it, facing down.</summary>
        Vector2Int StandAboveAWeed()
        {
            for (int y = 15; y < 21; y++)
                for (int x = 10; x < 22; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var what = clearing.ClearableAt(cell);
                    if (what == null || what.tool == null || what.tool.displayName != "Gloves") continue;
                    Teleport(new Vector2(x + 0.5f, y + 1.4f));
                    return cell;
                }
            Assert.Fail("no weed found on the patio");
            return default;
        }

        [UnityTest]
        public IEnumerator GlovesPullAWeedInFrontOfThePlayer()
        {
            var cell = StandAboveAWeed();
            yield return Frames(2);
            // Gloves are in slot 1, which is selected at the start. Hold the button long enough for
            // the largest weed (3 pulls at the PlayerConfig repeat rate).
            yield return HoldLeftMouse(1.2f);
            Assert.IsNull(clearing.ClearableAt(cell), "the weed should be gone");
        }

        [UnityTest]
        public IEnumerator TheTrashBagOnAWeedShowsAHint()
        {
            var cell = StandAboveAWeed();
            yield return Hold(Key.Digit2, 0.05f); // the trash bag
            yield return HoldLeftMouse(0.1f);
            Assert.IsNotNull(clearing.ClearableAt(cell), "the wrong tool should not clear it");
            StringAssert.Contains("gloves", hintLabel.text);
            Assert.Greater(hintLabel.color.a, 0.5f, "the hint should be showing");
        }
    }
}
