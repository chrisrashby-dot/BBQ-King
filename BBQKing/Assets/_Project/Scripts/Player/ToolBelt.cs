using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;

namespace BBQKing.Player
{
    /// <summary>
    /// The tool the player is holding. Number keys 1 to 6 pick a toolbar slot and the mouse wheel
    /// steps through them. Each change is posted to a channel so the toolbar can highlight it
    /// without a reference to the player.
    /// </summary>
    public class ToolBelt : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] ToolSet tools;

        [Tooltip("Posts the selected slot (0 is the first) whenever it changes. The toolbar listens.")]
        [SerializeField] IntEventChannel selectedSlotChannel;

        ToolSlots slots;

        /// <summary>The tool in the player's hand, or null if the toolbar is empty.</summary>
        public ToolData CurrentTool => tools != null && slots != null ? tools.Get(slots.Selected) : null;

        void Awake()
        {
            slots = new ToolSlots(tools != null ? tools.Count : 0);
        }

        void OnEnable()
        {
            if (input == null) return;
            input.ToolSlotSelected += OnSlotKey;
            input.ToolCycled += OnCycle;
        }

        void OnDisable()
        {
            if (input == null) return;
            input.ToolSlotSelected -= OnSlotKey;
            input.ToolCycled -= OnCycle;
        }

        // Start runs after every object's OnEnable, so the toolbar is already listening.
        void Start() => Post();

        void OnSlotKey(int keyNumber)
        {
            if (slots.SelectKey(keyNumber)) Post();
        }

        void OnCycle(int direction)
        {
            // The wheel scrolls "up" to go back along the toolbar, as in most games.
            if (slots.Cycle(-direction)) Post();
        }

        void Post()
        {
            if (selectedSlotChannel != null) selectedSlotChannel.Raise(slots.Selected);
        }
    }
}
