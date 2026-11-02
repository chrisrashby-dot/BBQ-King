namespace BBQKing.Player
{
    /// <summary>
    /// Which toolbar slot is selected. Plain C# with no Unity parts, so the rules can be checked by
    /// EditMode tests: number keys pick a slot directly, the mouse wheel steps through them and
    /// wraps round, and slots without a tool are ignored.
    /// </summary>
    public class ToolSlots
    {
        /// <summary>How many slots hold a tool.</summary>
        public int Count { get; }

        /// <summary>The selected slot, counting from 0.</summary>
        public int Selected { get; private set; }

        public ToolSlots(int count)
        {
            Count = count < 0 ? 0 : count;
        }

        /// <summary>Selects a slot by its key number (1 to 6). Returns true if the selection changed.</summary>
        public bool SelectKey(int keyNumber)
        {
            int slot = keyNumber - 1;
            if (slot < 0 || slot >= Count || slot == Selected) return false;
            Selected = slot;
            return true;
        }

        /// <summary>Selects a slot directly (0 is the first). Returns true if the selection changed.</summary>
        public bool Select(int slot) => SelectKey(slot + 1);

        /// <summary>Steps to the next (+1) or previous (-1) slot, wrapping round. Returns true if it changed.</summary>
        public bool Cycle(int direction)
        {
            if (Count < 2 || direction == 0) return false;
            int step = direction > 0 ? 1 : -1;
            Selected = (Selected + step + Count) % Count;
            return true;
        }
    }
}
