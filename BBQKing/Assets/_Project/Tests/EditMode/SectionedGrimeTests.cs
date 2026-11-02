using System.Collections.Generic;
using BBQKing.Cleaning;
using NUnit.Framework;
using UnityEngine;

namespace BBQKing.Tests
{
    /// <summary>
    /// Checks the section rules: each section finishes on its own at the finishing line, it is
    /// reported once, and the object is finished only when every section is.
    /// </summary>
    public class SectionedGrimeTests
    {
        // Two sections side by side on a 20 x 10 picture: "left" covers x 0 to 9, "right" x 10 to 19.
        static SectionedGrime LeftAndRight()
        {
            GrimeMask Half(bool left)
            {
                var start = new float[20 * 10];
                for (int i = 0; i < start.Length; i++) start[i] = (i % 20 < 10) == left ? 1f : 0f;
                return new GrimeMask(20, 10, start);
            }
            return new SectionedGrime(new[] { Half(true), Half(false) });
        }

        [Test]
        public void ASectionFinishesOnItsOwnAndIsReportedOnce()
        {
            var grime = LeftAndRight();
            var finished = new List<int>();

            // A brush big enough to cover the left half but not the right.
            grime.Scrub(new Vector2(5f, 5f), 7.1f, 1f, 0.95f, finished);
            CollectionAssert.AreEqual(new[] { 0 }, finished, "the left section is finished");
            Assert.IsTrue(grime.IsFinished(0));
            Assert.IsFalse(grime.IsFinished(1));
            Assert.IsFalse(grime.AllFinished);

            finished.Clear();
            grime.Scrub(new Vector2(5f, 5f), 7.1f, 1f, 0.95f, finished);
            Assert.IsEmpty(finished, "a finished section is not reported again");
        }

        [Test]
        public void TheObjectIsFinishedWhenEverySectionIs()
        {
            var grime = LeftAndRight();
            var finished = new List<int>();
            grime.Scrub(new Vector2(10f, 5f), 20f, 1f, 0.95f, finished); // covers the whole picture
            CollectionAssert.AreEquivalent(new[] { 0, 1 }, finished);
            Assert.IsTrue(grime.AllFinished);
        }

        [Test]
        public void EachSectionHasItsOwnPercentage()
        {
            var grime = LeftAndRight();
            grime.Scrub(new Vector2(15.5f, 5.5f), 0.4f, 1f, 0.95f, null); // just pixel (15, 5), on the right
            Assert.AreEqual(0f, grime.Mask(0).CleanFraction, "the left section is untouched");
            Assert.AreEqual(0.01f, grime.Mask(1).CleanFraction, 1e-5f, "one pixel of 100 on the right");
        }
    }
}
