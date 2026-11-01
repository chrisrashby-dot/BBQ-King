using BBQKing.Core;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// Lets the player use nearby objects. Each frame it looks for the closest
    /// <see cref="IInteractable"/> just in front of the player's feet, posts its prompt to the
    /// prompt channel for the UI to show, and calls Interact() when E is pressed.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] PlayerMovement movement;
        [SerializeField] PlayerConfig config;

        [Tooltip("Posts the prompt text (or an empty string to hide it). The UI listens to this.")]
        [SerializeField] StringEventChannel promptChannel;

        IInteractable current;
        string lastPrompt;
        readonly Collider2D[] hits = new Collider2D[8];

        void OnEnable()
        {
            if (input != null) input.InteractPressed += OnInteractPressed;
        }

        void OnDisable()
        {
            if (input != null) input.InteractPressed -= OnInteractPressed;
            current = null;
            PostPrompt(string.Empty);
        }

        void Update()
        {
            // While a panel is open the player's controls are off, so offer nothing.
            bool active = input != null && input.IsPlayerInputEnabled;
            current = active ? FindClosest() : null;

            PostPrompt(current != null ? $"[{input.InteractKeyLabel}] {current.Prompt}" : string.Empty);
        }

        void OnInteractPressed()
        {
            if (current != null && current.CanInteract) current.Interact(gameObject);
        }

        /// <summary>Only posts when the text changes, so the UI is not updated every frame.</summary>
        void PostPrompt(string text)
        {
            if (text == lastPrompt) return;
            lastPrompt = text;
            if (promptChannel != null) promptChannel.Raise(text);
        }

        Vector2 SearchCentre()
        {
            float offset = config != null ? config.reachOffset : 0f;
            Vector2 facing = movement != null ? movement.FacingVector : Vector2.down;
            return (Vector2)transform.position + facing * offset;
        }

        IInteractable FindClosest()
        {
            if (config == null) return null;
            Vector2 centre = SearchCentre();

            // Unity concept: OverlapCircle asks the 2D physics engine for every collider touching a
            // circle. "noFilter" includes trigger colliders, which interactables often use.
            int count = Physics2D.OverlapCircle(centre, config.reachRadius, ContactFilter2D.noFilter, hits);

            IInteractable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var interactable = hits[i].GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract) continue;

                float d = Vector2.Distance(centre, hits[i].ClosestPoint(centre));
                if (d < bestDistance) { bestDistance = d; best = interactable; }
            }
            return best;
        }

        // Draws the search circle in the Scene view when the player is selected, to help tune the reach.
        void OnDrawGizmosSelected()
        {
            if (config == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(SearchCentre(), config.reachRadius);
        }
    }
}
