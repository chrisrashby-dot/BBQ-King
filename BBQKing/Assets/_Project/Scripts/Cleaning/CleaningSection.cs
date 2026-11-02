using System;
using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// One part of an object that is cleaned on its own, with its own percentage, such as the top
    /// or the bottom of the kettle grill (design.md section 5.2).
    ///
    /// Unity concept: [Serializable] lets a plain class like this be edited as a list inside an
    /// asset in the Inspector, without being an asset of its own.
    /// </summary>
    [Serializable]
    public class CleaningSection
    {
        [Tooltip("Shown on its meter, for example \"Top\".")]
        public string name = "Section";

        [Tooltip("This section's grime, the same size as the close-up art. Its see-through parts " +
                 "belong to other sections or start clean. The image must have Read/Write enabled " +
                 "in its import settings, because the game wipes it away pixel by pixel.")]
        public Sprite grimeSprite;
    }
}
