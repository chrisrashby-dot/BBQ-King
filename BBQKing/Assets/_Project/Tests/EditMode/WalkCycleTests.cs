using BBQKing.Player;
using NUnit.Framework;

namespace BBQKing.Tests
{
    public class WalkCycleTests
    {
        [Test]
        public void StartsOnTheStandingFrame()
        {
            Assert.AreEqual(0, WalkCycle.FrameAt(0f, 8f, 4));
        }

        [Test]
        public void StepsThroughFramesAtTheGivenRate()
        {
            // At 8 frames per second each frame lasts 0.125 seconds.
            Assert.AreEqual(0, WalkCycle.FrameAt(0.1f, 8f, 4));
            Assert.AreEqual(1, WalkCycle.FrameAt(0.2f, 8f, 4));
            Assert.AreEqual(2, WalkCycle.FrameAt(0.3f, 8f, 4));
            Assert.AreEqual(3, WalkCycle.FrameAt(0.4f, 8f, 4));
        }

        [Test]
        public void LoopsBackToTheFirstFrame()
        {
            Assert.AreEqual(0, WalkCycle.FrameAt(0.5f, 8f, 4));
            Assert.AreEqual(1, WalkCycle.FrameAt(0.65f, 8f, 4));
        }

        [Test]
        public void BadSettingsFallBackToTheStandingFrame()
        {
            Assert.AreEqual(0, WalkCycle.FrameAt(1f, 0f, 4));
            Assert.AreEqual(0, WalkCycle.FrameAt(1f, 8f, 0));
            Assert.AreEqual(0, WalkCycle.FrameAt(-1f, 8f, 4));
        }
    }
}
