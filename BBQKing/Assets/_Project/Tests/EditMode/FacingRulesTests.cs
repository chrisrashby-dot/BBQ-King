using BBQKing.Player;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>
    /// EditMode tests run in the Editor without pressing Play.
    /// Open them with Window > General > Test Runner, choose EditMode, then Run All.
    /// </summary>
    public class FacingRulesTests
    {
        [TestCase(0f, 1f, Facing.Up)]
        [TestCase(0f, -1f, Facing.Down)]
        [TestCase(-1f, 0f, Facing.Left)]
        [TestCase(1f, 0f, Facing.Right)]
        public void StraightInputFacesThatWay(float x, float y, Facing expected)
        {
            Assert.AreEqual(expected, FacingRules.FromInput(new Vector2(x, y), Facing.Down));
        }

        [Test]
        public void StrongerAxisWins()
        {
            Assert.AreEqual(Facing.Right, FacingRules.FromInput(new Vector2(0.9f, 0.3f), Facing.Up));
            Assert.AreEqual(Facing.Down, FacingRules.FromInput(new Vector2(0.2f, -0.8f), Facing.Left));
        }

        [Test]
        public void NoInputKeepsCurrentFacing()
        {
            Assert.AreEqual(Facing.Left, FacingRules.FromInput(Vector2.zero, Facing.Left));
        }

        [Test]
        public void ExactDiagonalKeepsFacingIfItStillFits()
        {
            var upRight = new Vector2(0.7071f, 0.7071f);
            Assert.AreEqual(Facing.Up, FacingRules.FromInput(upRight, Facing.Up));
            Assert.AreEqual(Facing.Right, FacingRules.FromInput(upRight, Facing.Right));
        }

        [Test]
        public void ExactDiagonalChangesFacingIfItNoLongerFits()
        {
            var upRight = new Vector2(0.7071f, 0.7071f);
            // Facing down while moving up-right: down no longer fits, so it must change.
            Assert.AreNotEqual(Facing.Down, FacingRules.FromInput(upRight, Facing.Down));
        }

        [Test]
        public void TinyStickDriftDoesNotCountAsMoving()
        {
            Assert.IsFalse(FacingRules.IsMoving(new Vector2(0.001f, 0f)));
            Assert.IsTrue(FacingRules.IsMoving(new Vector2(0.5f, 0f)));
        }

        [Test]
        public void FacingVectorsPointTheRightWay()
        {
            Assert.AreEqual(Vector2.up, FacingRules.ToVector(Facing.Up));
            Assert.AreEqual(Vector2.down, FacingRules.ToVector(Facing.Down));
            Assert.AreEqual(Vector2.left, FacingRules.ToVector(Facing.Left));
            Assert.AreEqual(Vector2.right, FacingRules.ToVector(Facing.Right));
        }
    }
}
