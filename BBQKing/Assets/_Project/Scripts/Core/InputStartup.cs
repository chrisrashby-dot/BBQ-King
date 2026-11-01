using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>
    /// Switches the player's controls on when a gameplay scene starts, and off when it closes.
    ///
    /// This is needed because the project uses "Enter Play Mode Options" with domain reload turned
    /// off, which makes pressing Play much faster. The catch is that ScriptableObjects such as the
    /// InputReader keep their state between Play sessions, so we reset the input state explicitly here.
    /// </summary>
    public class InputStartup : MonoBehaviour
    {
        [SerializeField] InputReader input;

        void Start()
        {
            if (input != null) input.EnablePlayerInput();
        }

        void OnDestroy()
        {
            if (input != null) input.DisableAllInput();
        }
    }
}
