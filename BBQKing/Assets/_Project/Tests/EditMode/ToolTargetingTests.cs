using BBQKing.Player;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>Checks which tile a tool aims at: the mouse tile within reach, otherwise the faced tile.</summary>
    public class ToolTargetingTests
    {
        const float Reach = 1.5f;
        const float FacingDistance = 0.6f;
        static readonly Vector2 Feet = new Vector2(10.5f, 10.4f); // standing in tile (10, 10)

        [Test]
        public void WithoutAMouseTheFacedTileIsUsed()
        {
            var cell = ToolTargeting.Choose(Feet, Vector2.down, null, Reach, FacingDistance);
            Assert.AreEqual(new Vector2Int(10, 9), cell);
        }

        [Test]
        public void FacingRightPicksTheTileToTheRight()
        {
            var cell = ToolTargeting.Choose(Feet, Vector2.right, null, Reach, FacingDistance);
            Assert.AreEqual(new Vector2Int(11, 10), cell);
        }

        [Test]
        public void MouseOnANearbyTileWins()
        {
            // The tile diagonally up and left, whose centre is about 1.3 tiles from the feet.
            var cell = ToolTargeting.Choose(Feet, Vector2.down, new Vector2(9.2f, 11.7f), Reach, FacingDistance);
            Assert.AreEqual(new Vector2Int(9, 11), cell);
        }

        [Test]
        public void MouseOutOfReachFallsBackToTheFacedTile()
        {
            var cell = ToolTargeting.Choose(Feet, Vector2.down, new Vector2(14.5f, 10.5f), Reach, FacingDistance);
            Assert.AreEqual(new Vector2Int(10, 9), cell);
        }

        [Test]
        public void NegativePositionsRoundDown()
        {
            Assert.AreEqual(new Vector2Int(-1, -1), ToolTargeting.CellAt(new Vector2(-0.2f, -0.9f)));
        }
    }
}
