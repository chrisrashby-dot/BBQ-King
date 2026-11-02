using BBQKing.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// Runs the CloseUp_Cleaning scene: shows the object being cleaned (from the close-up session)
    /// with its grime over it, and leaves again when Escape is pressed.
    ///
    /// The close-up's art sits far away from the backyard (see the scene builder), so the two
    /// scenes can share the game world without the backyard camera ever seeing it.
    /// </summary>
    public class CleaningCloseUp : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] CloseUpSession session;

        [Header("Scene parts")]
        [SerializeField] SpriteRenderer objectRenderer;
        [SerializeField] SpriteRenderer grimeRenderer;

        [Header("UI")]
        [SerializeField] Text titleLabel;
        [SerializeField] Image toolIcon;
        [SerializeField] Text toolLabel;

        /// <summary>What is being cleaned, or null if the scene was opened on its own.</summary>
        public CleanableData Cleaning { get; private set; }

        void OnEnable()
        {
            if (input != null) input.LeaveCloseUpPressed += Leave;
        }

        void OnDisable()
        {
            if (input != null) input.LeaveCloseUpPressed -= Leave;
        }

        void Start()
        {
            Cleaning = session != null ? session.Current : null;
            if (Cleaning == null)
            {
                // Pressing Play with only this scene open: there is nothing to clean.
                Debug.LogWarning("BBQ King: the cleaning close-up opens from the Backyard scene. " +
                                 "Open Backyard and press Play, then use the kettle grill.");
                return;
            }

            if (objectRenderer != null) objectRenderer.sprite = Cleaning.closeUpSprite;
            if (grimeRenderer != null) grimeRenderer.sprite = Cleaning.grimeSprite;
            if (titleLabel != null) titleLabel.text = Cleaning.cleanPrompt;

            var tool = Cleaning.tool;
            if (toolIcon != null)
            {
                toolIcon.sprite = tool != null ? tool.icon : null;
                toolIcon.enabled = toolIcon.sprite != null;
            }
            if (toolLabel != null) toolLabel.text = tool != null ? tool.displayName : string.Empty;
        }

        void Leave()
        {
            if (session != null) session.RequestClose(false);
        }
    }
}
