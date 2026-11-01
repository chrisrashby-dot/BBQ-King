using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// The toolbar along the bottom of the screen: one slot per tool, with the selected slot
    /// highlighted and the tool's name above. It reads the tools from the same ToolSet asset as the
    /// player and hears about changes through the selected-slot channel.
    /// </summary>
    public class ToolbarUI : MonoBehaviour
    {
        [SerializeField] ToolSet tools;
        [SerializeField] IntEventChannel selectedSlotChannel;

        [Tooltip("The slot backgrounds, left to right (6 of them).")]
        [SerializeField] Image[] slotFrames = new Image[0];

        [Tooltip("The icons inside the slots, in the same order.")]
        [SerializeField] Image[] slotIcons = new Image[0];

        [SerializeField] Sprite normalFrame;
        [SerializeField] Sprite selectedFrame;

        [Tooltip("Shows the selected tool's name.")]
        [SerializeField] Text nameLabel;

        void OnEnable()
        {
            if (selectedSlotChannel != null) selectedSlotChannel.Raised += Show;
            FillIcons();
        }

        void OnDisable()
        {
            if (selectedSlotChannel != null) selectedSlotChannel.Raised -= Show;
        }

        /// <summary>Puts each tool's icon in its slot; slots past the last tool stay empty.</summary>
        void FillIcons()
        {
            for (int i = 0; i < slotIcons.Length; i++)
            {
                var tool = tools != null ? tools.Get(i) : null;
                slotIcons[i].sprite = tool != null ? tool.icon : null;
                slotIcons[i].enabled = tool != null && tool.icon != null;
            }
        }

        void Show(int selected)
        {
            for (int i = 0; i < slotFrames.Length; i++)
                slotFrames[i].sprite = i == selected ? selectedFrame : normalFrame;

            var tool = tools != null ? tools.Get(selected) : null;
            if (nameLabel != null) nameLabel.text = tool != null ? tool.displayName : string.Empty;
        }
    }
}
