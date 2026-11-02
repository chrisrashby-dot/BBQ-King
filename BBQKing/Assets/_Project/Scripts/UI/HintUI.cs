using BBQKing.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.UI
{
    /// <summary>
    /// A short message that appears above the toolbar and fades away, such as "Try the gloves."
    /// Anything can post to the hint channel to show one.
    /// </summary>
    public class HintUI : MonoBehaviour
    {
        [SerializeField] StringEventChannel hintChannel;
        [SerializeField] Text label;

        [Tooltip("How long hints show and fade (Data/Settings/UiConfig).")]
        [SerializeField] UiConfig config;

        float shownAt = float.NegativeInfinity;

        void OnEnable()
        {
            if (hintChannel != null) hintChannel.Raised += Show;
            SetAlpha(0f);
        }

        void OnDisable()
        {
            if (hintChannel != null) hintChannel.Raised -= Show;
        }

        void Show(string text)
        {
            if (label == null) return;
            label.text = text;
            shownAt = Time.time;
            SetAlpha(1f);
        }

        void Update()
        {
            if (config == null) return;
            float showSeconds = config.hintShowSeconds, fadeSeconds = config.hintFadeSeconds;
            float age = Time.time - shownAt;
            if (age <= showSeconds) return;
            SetAlpha(fadeSeconds > 0f ? 1f - Mathf.Clamp01((age - showSeconds) / fadeSeconds) : 0f);
        }

        void SetAlpha(float alpha)
        {
            if (label == null) return;
            var c = label.color;
            if (Mathf.Approximately(c.a, alpha)) return;
            c.a = alpha;
            label.color = c;
        }
    }
}
