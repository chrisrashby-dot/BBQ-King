using System.Collections;
using BBQKing.Player;
using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace BBQKing.Tests
{
    /// <summary>
    /// M2 clearing in the Backyard, using real key and mouse presses: pulling a weed, the highlight
    /// only showing over things to clear, the right tool being picked automatically, and the hint
    /// when the wrong tool is chosen by hand.
    /// </summary>
    public class ClearingTests : BackyardTestBase
    {
        ClearingSystem clearing;
        ToolBelt toolBelt;
        SpriteRenderer highlight;
        Text hintLabel, toolName;

        protected override void FindSceneParts()
        {
            clearing = Object.FindAnyObjectByType<ClearingSystem>();
            toolBelt = player.GetComponent<ToolBelt>();
            highlight = GameObject.Find("Tool Highlight").GetComponent<SpriteRenderer>();
            hintLabel = GameObject.Find("UI/Hint/Label").GetComponent<Text>();
            toolName = GameObject.Find("UI/Toolbar/Tool Name").GetComponent<Text>();
        }

        /// <summary>
        /// Finds something on the patio cleared with the named tool, with nothing on the tile above
        /// it, and stands the player on that tile above.
        /// </summary>
        Vector2Int StandAbove(string tool)
        {
            for (int y = 15; y < 21; y++)
                for (int x = 10; x < 22; x++)
                {
                    var cell = new Vector2Int(x, y);
                    var what = clearing.ClearableAt(cell);
                    if (what == null || what.tool == null || what.tool.displayName != tool) continue;
                    if (clearing.ClearableAt(cell + Vector2Int.up) != null) continue;
                    Teleport(new Vector2(x + 0.5f, y + 1.4f));
                    return cell;
                }
            Assert.Fail($"nothing for the {tool} found on the patio");
            return default;
        }

        [UnityTest]
        public IEnumerator GlovesPullAWeedNextToThePlayer()
        {
            var cell = StandAbove("Gloves");
            yield return Frames(2);
            // Hold the button long enough for the largest weed (3 pulls at the PlayerConfig repeat rate).
            yield return HoldLeftMouseOn(cell, 1.2f);
            Assert.IsNull(clearing.ClearableAt(cell), "the weed should be gone");
        }

        [UnityTest]
        public IEnumerator ClickingATileOutOfReachDoesNothing()
        {
            var cell = StandAbove("Gloves");
            // Walk away so the weed is three tiles below the player's tile.
            Teleport(new Vector2(cell.x + 0.5f, cell.y + 3.4f));
            yield return Frames(2);
            yield return HoldLeftMouseOn(cell, 1.2f);
            Assert.IsNotNull(clearing.ClearableAt(cell), "a weed out of reach should be left alone");
            Assert.IsFalse(highlight.enabled, "no highlight out of reach");
        }

        [UnityTest]
        public IEnumerator TheHighlightOnlyShowsOverSomethingToClear()
        {
            var cell = StandAbove("Gloves");
            yield return Frames(2);
            yield return PointAt(cell + Vector2Int.up); // the player's own tile, which is empty
            Assert.IsFalse(highlight.enabled, "an empty tile should not be highlighted");
            yield return PointAt(cell);
            Assert.IsTrue(highlight.enabled, "the weed should be highlighted");
        }

        [UnityTest]
        public IEnumerator PointingAtTrashPicksTheTrashBag()
        {
            var cell = StandAbove("Trash bag");
            // Use the mouse from the start; otherwise the gamepad rule would aim at the faced
            // tile, which is the trash, and pick the trash bag before the test points at it.
            yield return PointAway();
            Assert.AreEqual("Gloves", toolBelt.CurrentTool.displayName, "the gloves are in hand at the start");
            yield return PointAt(cell);
            Assert.AreEqual("Trash bag", toolBelt.CurrentTool.displayName);
            Assert.AreEqual("Trash bag", toolName.text, "the toolbar should show the new tool");
            yield return HoldLeftMouse(0.1f);
            Assert.IsNull(clearing.ClearableAt(cell), "the trash should be picked up");
        }

        [UnityTest]
        public IEnumerator TheWrongToolChosenByHandShowsAHint()
        {
            var cell = StandAbove("Gloves");
            yield return Frames(2);
            yield return PointAt(cell);            // picks the gloves automatically
            yield return Hold(Key.Digit2, 0.05f);  // then the trash bag by hand
            Assert.AreEqual("Trash bag", toolBelt.CurrentTool.displayName, "a choice by hand should stick");
            yield return HoldLeftMouse(0.1f);
            Assert.IsNotNull(clearing.ClearableAt(cell), "the wrong tool should not clear it");
            StringAssert.Contains("gloves", hintLabel.text);
            Assert.Greater(hintLabel.color.a, 0.5f, "the hint should be showing");
        }
    }
}
