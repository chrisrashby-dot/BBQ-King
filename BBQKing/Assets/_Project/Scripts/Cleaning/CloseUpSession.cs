using System;
using System.Collections.Generic;
using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// The shared notice board for close-up cleaning, stored in Data/Cleaning/CloseUpSession.asset.
    /// An object in the backyard asks for its close-up here, the loader in the backyard opens the
    /// close-up scene, and the close-up asks to be closed again here. Like the JobBook, everyone is
    /// given the same asset, so the backyard and the close-up scene never reference each other.
    /// </summary>
    [CreateAssetMenu(fileName = "CloseUpSession", menuName = "BBQ King/Cleaning/Close-up Session")]
    public class CloseUpSession : ScriptableObject
    {
        /// <summary>Something in the backyard wants its close-up opened.</summary>
        public event Action<CleanableData> OpenRequested;

        /// <summary>The close-up wants to close. True if the object was finished.</summary>
        public event Action<bool> CloseRequested;

        /// <summary>The object being cleaned while a close-up is open, or null.</summary>
        public CleanableData Current { get; private set; }

        public void RequestOpen(CleanableData what) => OpenRequested?.Invoke(what);

        public void RequestClose(bool finished) => CloseRequested?.Invoke(finished);

        /// <summary>Called by the loader as the close-up opens and closes.</summary>
        public void SetCurrent(CleanableData what) => Current = what;

        // The grime left on each object, kept when the player leaves part way so they can carry
        // on later (design.md decision log, 2 Nov 2026). Not saved into the asset; saving to a
        // file arrives in M5.
        [NonSerialized] readonly Dictionary<CleanableData, SectionedGrime> progress = new Dictionary<CleanableData, SectionedGrime>();

        /// <summary>The grime left on an object from an earlier visit, or null if it has not been started.</summary>
        public SectionedGrime ProgressFor(CleanableData what) =>
            what != null && progress.TryGetValue(what, out var grime) ? grime : null;

        /// <summary>Remembers an object's grime. The close-up keeps changing the same object as the player scrubs.</summary>
        public void KeepProgress(CleanableData what, SectionedGrime grime)
        {
            if (what != null && grime != null) progress[what] = grime;
        }

        /// <summary>
        /// Forgets everything. Needed at the start of play because this project keeps
        /// ScriptableObject data between Play sessions (domain reload is off).
        /// </summary>
        public void ResetProgress()
        {
            progress.Clear();
            Current = null;
        }
    }
}
