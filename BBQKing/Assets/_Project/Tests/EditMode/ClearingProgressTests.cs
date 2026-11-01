using BBQKing.Restoration;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>Checks that tiles clear after the right number of tool uses (design.md section 5.2).</summary>
    public class ClearingProgressTests
    {
        static readonly Vector2Int A = new Vector2Int(3, 4);
        static readonly Vector2Int B = new Vector2Int(5, 4);

        [Test]
        public void OneActionTileClearsAtOnce()
        {
            Assert.AreEqual(0, new ClearingProgress().Use(A, 1));
        }

        [Test]
        public void ThreeActionTileClearsOnTheThirdUse()
        {
            var progress = new ClearingProgress();
            Assert.AreEqual(2, progress.Use(A, 3));
            Assert.AreEqual(1, progress.Use(A, 3));
            Assert.AreEqual(0, progress.Use(A, 3));
        }

        [Test]
        public void TilesAreCountedSeparately()
        {
            var progress = new ClearingProgress();
            progress.Use(A, 3);
            Assert.AreEqual(1, progress.UsesOn(A));
            Assert.AreEqual(0, progress.UsesOn(B));
            Assert.AreEqual(1, progress.Use(B, 2));
        }

        [Test]
        public void AClearedTileStartsAgainFromScratch()
        {
            var progress = new ClearingProgress();
            progress.Use(A, 2);
            progress.Use(A, 2);
            Assert.AreEqual(0, progress.UsesOn(A), "forgotten once cleared");
            Assert.AreEqual(1, progress.Use(A, 2), "a new weed in the same place needs both pulls");
        }
    }
}
