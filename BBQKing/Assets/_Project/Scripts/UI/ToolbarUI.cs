using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// The toolbar along the bottom of the screen: one slot per tool, with the selected slot
    /// highlighted and the tool's name above. It reads the tools from the same ToolSet asset as the
    /// player and hears about changes through the selected-slot channel. When the tool changes,
    /// including when it is picked automatically, the new slot briefly grows and settles back so
    /// the player notices which tool is now in hand.
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

        [Tooltip("How big the slot grows when the tool changes (1 = no pulse).")]
        [SerializeField, Range(1f, 2f)] float pulseScale = 1.3f;

        [Tooltip("Seconds the slot takes to settle back to its normal size.")]
        [SerializeField, Range(0.05f, 1f)] float pulseSeconds = 0.25f;

        int pulsingSlot = -1;
        float pulseStart;
        bool shownOnce; // the first Show is the starting tool, which should not pulse

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

            if (shownOnce) StartPulse(selected);
            shownOnce = true;
        }

        void StartPulse(int slot)
        {
            if (pulsingSlot >= 0 && pulsingSlot < slotFrames.Length) slotFrames[pulsingSlot].rectTransform.localScale = Vector3.one;
            pulsingSlot = slot;
            // Unity concept: Time.unscaledTime keeps running even if the game is paused or slowed,
            // which suits UI feedback.
            pulseStart = Time.unscaledTime;
        }

        void Update()
        {
            if (pulsingSlot < 0 || pulsingSlot >= slotFrames.Length) return;

            float t = Mathf.Clamp01((Time.unscaledTime - pulseStart) / pulseSeconds);
            float scale = Mathf.Lerp(pulseScale, 1f, t);
            slotFrames[pulsingSlot].rectTransform.localScale = new Vector3(scale, scale, 1f);
            if (t >= 1f) pulsingSlot = -1;
        }
    }
}
