using BBQKing.Player;
using NUnit.Framework;

namespace BBQKing.Tests
{
    /// <summary>Checks the toolbar selection rules: number keys, the mouse wheel and empty slots.</summary>
    public class ToolSlotsTests
    {
        [Test]
        public void StartsOnTheFirstSlot()
        {
            Assert.AreEqual(0, new ToolSlots(2).Selected);
        }

        [Test]
        public void NothingIsSelectedWithNoTools()
        {
            var slots = new ToolSlots(0);
            Assert.AreEqual(-1, slots.Selected);
            Assert.IsFalse(slots.SelectKey(1));
            Assert.IsFalse(slots.Cycle(1));
        }

        [Test]
        public void NumberKeySelectsItsSlot()
        {
            var slots = new ToolSlots(2);
            Assert.IsTrue(slots.SelectKey(2));
            Assert.AreEqual(1, slots.Selected);
        }

        [Test]
        public void SelectPicksASlotDirectly()
        {
            var slots = new ToolSlots(2);
            Assert.IsTrue(slots.Select(1));
            Assert.AreEqual(1, slots.Selected);
            Assert.IsFalse(slots.Select(1), "selecting the same slot again is no change");
        }

        [Test]
        public void NumberKeyForAnEmptySlotIsIgnored()
        {
            var slots = new ToolSlots(2);
            Assert.IsFalse(slots.SelectKey(5));
            Assert.AreEqual(0, slots.Selected);
        }

        [Test]
        public void PressingTheSelectedKeyAgainIsNotAChange()
        {
            var slots = new ToolSlots(2);
            Assert.IsFalse(slots.SelectKey(1));
        }

        [Test]
        public void CyclingWrapsRoundBothWays()
        {
            var slots = new ToolSlots(3);
            slots.Cycle(-1);
            Assert.AreEqual(2, slots.Selected, "back from the first slot goes to the last");
            slots.Cycle(1);
            Assert.AreEqual(0, slots.Selected, "on from the last slot goes to the first");
        }

        [Test]
        public void CyclingWithOneToolDoesNothing()
        {
            var slots = new ToolSlots(1);
            Assert.IsFalse(slots.Cycle(1));
            Assert.AreEqual(0, slots.Selected);
        }

        [Test]
        public void AnEmptyToolbarIgnoresEverything()
        {
            var slots = new ToolSlots(0);
            Assert.IsFalse(slots.SelectKey(1));
            Assert.IsFalse(slots.Cycle(1));
        }
    }
}
