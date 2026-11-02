using BBQKing.Cleaning;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>Checks the scrubbing maths: the round brush, partial scrubs and how clean is counted.</summary>
    public class GrimeMaskTests
    {
        /// <summary>A mask with every pixel fully grimy.</summary>
        static GrimeMask Solid(int width, int height)
        {
            var start = new float[width * height];
            for (int i = 0; i < start.Length; i++) start[i] = 1f;
            return new GrimeMask(width, height, start);
        }

        [Test]
        public void StartsWithNothingClean()
        {
            Assert.AreEqual(0f, Solid(10, 10).CleanFraction);
        }

        [Test]
        public void TheBrushIsRound()
        {
            var mask = Solid(20, 20);
            mask.Scrub(new Vector2(10f, 10f), 3f, 1f);
            Assert.AreEqual(0f, mask.GrimeAt(10, 10), "the centre is scrubbed");
            Assert.AreEqual(0f, mask.GrimeAt(12, 10), "inside the radius is scrubbed");
            Assert.AreEqual(1f, mask.GrimeAt(13, 13), "the corner of the square around the brush is not");
            Assert.AreEqual(1f, mask.GrimeAt(14, 10), "outside the radius is not");
        }

        [Test]
        public void ALightScrubOnlyTakesSomeGrimeOff()
        {
            var mask = Solid(4, 4);
            Assert.IsTrue(mask.Scrub(new Vector2(2f, 2f), 10f, 0.25f));
            Assert.AreEqual(0.75f, mask.GrimeAt(1, 1), 1e-5f);
            Assert.AreEqual(0.25f, mask.CleanFraction, 1e-5f);
        }

        [Test]
        public void ScrubbingACleanSpotChangesNothing()
        {
            var mask = Solid(4, 4);
            mask.Scrub(new Vector2(2f, 2f), 10f, 1f);
            Assert.IsFalse(mask.Scrub(new Vector2(2f, 2f), 10f, 1f));
            Assert.AreEqual(1f, mask.CleanFraction);
        }

        [Test]
        public void SeeThroughPixelsDoNotCountTowardsClean()
        {
            // The left half starts clean (see-through); only the right half has grime.
            var pixels = new Color32[4 * 2];
            for (int y = 0; y < 2; y++)
                for (int x = 0; x < 4; x++)
                    pixels[y * 4 + x] = new Color32(0, 0, 0, (byte)(x < 2 ? 0 : 255));
            var mask = GrimeMask.FromPixels(pixels, 4, 2);
            Assert.AreEqual(0f, mask.CleanFraction, "nothing has been scrubbed yet");

            mask.Scrub(new Vector2(3.5f, 0.5f), 0.4f, 1f); // just pixel (3, 0): one grimy pixel of four
            Assert.AreEqual(0.25f, mask.CleanFraction, 1e-5f);
        }

        [Test]
        public void ScrubbingOffTheEdgeIsSafe()
        {
            var mask = Solid(4, 4);
            Assert.DoesNotThrow(() => mask.Scrub(new Vector2(-5f, 50f), 3f, 1f));
            Assert.AreEqual(0f, mask.CleanFraction);
            Assert.AreEqual(0f, mask.GrimeAt(-1, 0), "outside the image counts as clean");
        }

        [Test]
        public void At95PercentTheRestIsWipedAway()
        {
            // 100 pixels in a row; the brush takes off whole pixels from the left.
            var mask = Solid(100, 1);
            mask.Scrub(new Vector2(47f, 0.5f), 47f, 1f);  // pixels 0 to 93: 94% clean
            Assert.AreEqual(0.94f, mask.CleanFraction, 1e-5f);
            Assert.IsFalse(mask.FinishIfCleanEnough(0.95f), "94% is not finished");
            Assert.AreEqual(1f, mask.GrimeAt(99, 0), "and the grime stays");

            mask.Scrub(new Vector2(94.5f, 0.5f), 0.4f, 1f); // one more pixel: 95%
            Assert.IsTrue(mask.FinishIfCleanEnough(0.95f), "95% is finished");
            Assert.AreEqual(1f, mask.CleanFraction, "the last bits are wiped away");
        }

        [Test]
        public void ClearAllMakesItSpotless()
        {
            var mask = Solid(4, 4);
            mask.ClearAll();
            Assert.AreEqual(1f, mask.CleanFraction);
            Assert.AreEqual(0f, mask.GrimeAt(3, 3));
        }
    }
}
