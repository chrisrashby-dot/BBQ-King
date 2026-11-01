using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// An object that shows a message when used, such as the relative's letter or the back door.
    /// It does not open the panel itself: it posts the message to a channel and the UI shows it.
    /// </summary>
    public class MessageInteractable : MonoBehaviour, IInteractable
    {
        [Tooltip("Shown after the key, for example \"Read the letter\".")]
        [SerializeField] string prompt = "Look";

        [SerializeField] MessageData message;

        [Tooltip("The channel the message panel listens to.")]
        [SerializeField] MessageEventChannel showMessageChannel;

        public string Prompt => prompt;
        public bool CanInteract => message != null && showMessageChannel != null;

        public void Interact(GameObject interactor) => showMessageChannel.Raise(message);
    }
}
