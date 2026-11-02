using System.Collections.Generic;
using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// The grime on every section of one object, and which sections are finished. Plain C#, so
    /// EditMode tests can check the rules: each section finishes on its own at the finishing line
    /// (95%), and the object is finished when every section is.
    /// </summary>
    public class SectionedGrime
    {
        readonly GrimeMask[] masks;
        readonly bool[] finished;

        public SectionedGrime(GrimeMask[] masks)
        {
            this.masks = masks;
            finished = new bool[masks.Length];
        }

        public int Count => masks.Length;

        public GrimeMask Mask(int section) => masks[section];

        public bool IsFinished(int section) => finished[section];

        /// <summary>True when every section is finished.</summary>
        public bool AllFinished
        {
            get
            {
                foreach (bool done in finished) if (!done) return false;
                return true;
            }
        }

        /// <summary>
        /// Scrubs every unfinished section under a round brush (positions in pixels of the close-up
        /// art), then finishes any section that has reached <paramref name="completeAt"/>.
        /// Sections finished by this scrub are added to <paramref name="finishedNow"/>.
        /// Returns true if any grime came off.
        /// </summary>
        public bool Scrub(Vector2 centre, float radius, float amount, float completeAt, List<int> finishedNow)
        {
            bool changed = false;
            for (int i = 0; i < masks.Length; i++)
            {
                if (finished[i] || !masks[i].Scrub(centre, radius, amount)) continue;
                changed = true;
                if (masks[i].FinishIfCleanEnough(completeAt))
                {
                    finished[i] = true;
                    finishedNow?.Add(i);
                }
            }
            return changed;
        }
    }
}
