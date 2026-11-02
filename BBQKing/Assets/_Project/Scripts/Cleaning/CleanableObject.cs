using BBQKing.Core;
using BBQKing.Player;
using BBQKing.Restoration;
using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// An object in the backyard that is cleaned in a close-up, such as the kettle grill. Walk up
    /// and press E: while its job is locked it only says why it has to wait; once the job is
    /// available it opens the close-up. When the job is done it swaps to its clean sprite and
    /// stops offering a prompt.
    /// </summary>
    public class CleanableObject : MonoBehaviour, IInteractable
    {
        [SerializeField] CleanableData data;
        [SerializeField] JobBook jobBook;

        [Tooltip("Where the close-up is asked for. The close-up loader listens.")]
        [SerializeField] CloseUpSession session;

        [Tooltip("The \"not yet\" hint is posted here for the UI to show.")]
        [SerializeField] StringEventChannel hintChannel;

        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Sprite dirtySprite;
        [SerializeField] Sprite cleanSprite;

        JobStatus Status => jobBook.StatusOf(data.job);

        public string Prompt => Status == JobStatus.Locked ? data.lookPrompt : data.cleanPrompt;

        public bool CanInteract => data != null && data.job != null && jobBook != null && Status != JobStatus.Done;

        public void Interact(GameObject interactor)
        {
            if (Status == JobStatus.Locked)
            {
                if (hintChannel != null) hintChannel.Raise(data.notYetHint);
                return;
            }
            if (session != null) session.RequestOpen(data);
        }

        void OnEnable()
        {
            if (jobBook != null) jobBook.Changed += RefreshSprite;
            RefreshSprite();
        }

        void OnDisable()
        {
            if (jobBook != null) jobBook.Changed -= RefreshSprite;
        }

        void RefreshSprite()
        {
            if (spriteRenderer == null || data == null || data.job == null || jobBook == null) return;
            var sprite = Status == JobStatus.Done ? cleanSprite : dirtySprite;
            if (sprite != null) spriteRenderer.sprite = sprite;
        }
    }
}
