using BBQKing.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// The on-screen hint such as "[E] Read the letter". Listens to the prompt channel: any text
    /// shows the panel, an empty string hides it.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] StringEventChannel promptChannel;

        [Tooltip("The panel to show and hide (the label's background).")]
        [SerializeField] GameObject panel;
        [SerializeField] Text label;

        void OnEnable()
        {
            if (promptChannel != null) promptChannel.Raised += Show;
        }

        void OnDisable()
        {
            if (promptChannel != null) promptChannel.Raised -= Show;
        }

        void Show(string text)
        {
            bool visible = !string.IsNullOrEmpty(text);
            if (visible && label != null) label.text = text;
            if (panel != null) panel.SetActive(visible);
        }
    }
}
