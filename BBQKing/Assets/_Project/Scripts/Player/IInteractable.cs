using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Anything the player can walk up to and use with the interact key (E): the letter and the
    /// back door now, and later the grill, smoker, shed or a guest.
    ///
    /// To make an object interactable, give it a component that implements this interface and a
    /// Collider2D (a trigger is fine) so the player's interactor can find it.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>What pressing E will do, for example "Read the letter".</summary>
        string Prompt { get; }

        /// <summary>False when there is nothing to do right now, so no prompt is shown.</summary>
        bool CanInteract { get; }

        /// <summary>Called when the player presses E nearby.</summary>
        void Interact(GameObject interactor);
    }
}
