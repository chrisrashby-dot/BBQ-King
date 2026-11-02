using BBQKing.Player;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>
    /// Checks which tile a tool aims at: with a mouse, the pointer's tile if it touches the player's
    /// tile, otherwise nothing; with no mouse, the faced tile.
    /// </summary>
    public class ToolTargetingTests
    {
        const int Reach = 1;
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
        public void MouseOnADiagonalNeighbourIsUsed()
        {
            var cell = ToolTargeting.Choose(Feet, Vector2.down, new Vector2(9.2f, 11.7f), Reach, FacingDistance);
            Assert.AreEqual(new Vector2Int(9, 11), cell);
        }

        [Test]
        public void MouseOnThePlayersOwnTileIsUsed()
        {
            var cell = ToolTargeting.Choose(Feet, Vector2.down, new Vector2(10.9f, 10.1f), Reach, FacingDistance);
            Assert.AreEqual(new Vector2Int(10, 10), cell);
        }

        [Test]
        public void MouseTwoTilesAwayGivesNoTarget()
        {
            var cell = ToolTargeting.Choose(Feet, Vector2.down, new Vector2(12.5f, 10.5f), Reach, FacingDistance);
            Assert.IsNull(cell, "a click two tiles away should do nothing, not fall back to the faced tile");
        }

        [Test]
        public void NegativePositionsRoundDown()
        {
            Assert.AreEqual(new Vector2Int(-1, -1), ToolTargeting.CellAt(new Vector2(-0.2f, -0.9f)));
        }
    }
}
