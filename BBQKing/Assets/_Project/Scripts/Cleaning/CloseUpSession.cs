using System;
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
    }
}
