using BBQKing.Restoration;
using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// One object that is cleaned in a close-up scene, such as the kettle grill (design.md section
    /// 5.2: "objects open a close-up scene. A grime layer covers the object"). Stored in
    /// Data/Cleaning. The object in the backyard points at this asset, and so does the close-up
    /// while it is open, so neither needs to know about the other.
    /// </summary>
    [CreateAssetMenu(fileName = "Cleanable", menuName = "BBQ King/Cleaning/Cleanable")]
    public class CleanableData : ScriptableObject
    {
        [Tooltip("The job finished by cleaning this object, for example Job 2.")]
        public JobData job;

        [Tooltip("The tool used to scrub it in the close-up.")]
        public ToolData tool;

        [Header("Close-up")]
        [Tooltip("The clean object, drawn in the close-up.")]
        public Sprite closeUpSprite;

        [Tooltip("The grime drawn over it, the same size. Its see-through parts start clean. The " +
                 "image must have Read/Write enabled in its import settings, because the game wipes " +
                 "it away pixel by pixel.")]
        public Sprite grimeSprite;

        [Header("Wording")]
        [Tooltip("Shown after the key when the job is available, for example \"Clean the kettle grill\".")]
        public string cleanPrompt = "Clean";

        [Tooltip("Shown after the key while the job is still locked, for example \"Look at the kettle grill\".")]
        public string lookPrompt = "Look";

        [Tooltip("Shown as a hint when the player looks at it while the job is still locked.")]
        public string notYetHint = "That can wait for now.";
    }
}
