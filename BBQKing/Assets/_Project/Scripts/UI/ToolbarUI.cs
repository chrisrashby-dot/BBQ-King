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

        [Tooltip("How much and how long the slot pulses when the tool changes (Data/Settings/UiConfig).")]
        [SerializeField] UiConfig config;

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

            // With no config the slot snaps straight back (t = 1), so there is simply no pulse.
            float t = config != null ? Mathf.Clamp01((Time.unscaledTime - pulseStart) / config.toolPulseSeconds) : 1f;
            float scale = Mathf.Lerp(config != null ? config.toolPulseScale : 1f, 1f, t);
            slotFrames[pulsingSlot].rectTransform.localScale = new Vector3(scale, scale, 1f);
            if (t >= 1f) pulsingSlot = -1;
        }
    }
}
