using BBQKing.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// Shows a letter or note on a paper panel. While it is open, the controls switch to the UI
    /// map, so the player stands still; Escape, right mouse or E closes it again.
    /// </summary>
    public class MessagePanelUI : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] MessageEventChannel showMessageChannel;

        [SerializeField] GameObject panel;
        [SerializeField] Text titleLabel;
        [SerializeField] Text bodyLabel;

        public bool IsOpen => panel != null && panel.activeSelf;

        void OnEnable()
        {
            if (showMessageChannel != null) showMessageChannel.Raised += Open;
            if (input != null)
            {
                input.SubmitPressed += Close;
                input.UICancelPressed += Close;
            }
        }

        void OnDisable()
        {
            if (showMessageChannel != null) showMessageChannel.Raised -= Open;
            if (input != null)
            {
                input.SubmitPressed -= Close;
                input.UICancelPressed -= Close;
            }
        }

        void Open(MessageData message)
        {
            if (message == null || panel == null) return;
            if (titleLabel != null) titleLabel.text = message.title;
            if (bodyLabel != null) bodyLabel.text = message.body;
            panel.SetActive(true);
            if (input != null) input.EnableUIInput();
        }

        void Close()
        {
            if (!IsOpen) return;
            panel.SetActive(false);
            if (input != null) input.EnablePlayerInput();
        }
    }
}
