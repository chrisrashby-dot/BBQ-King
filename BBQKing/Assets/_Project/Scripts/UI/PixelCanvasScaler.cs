using BBQKing.Core;
using UnityEngine;

namespace BBQKing.UI
{
    /// <summary>
    /// Scales a UI canvas by whole numbers only (x1, x2, x3...), using the same reference resolution
    /// as the camera (from <see cref="DisplayConfig"/>). Whole-number scaling keeps pixel-art UI
    /// sharp; Unity's built-in Canvas Scaler can pick in-between values like x2.7, which blurs pixels.
    ///
    /// Lay the UI out in art pixels, as if the screen were the reference size, and this does the rest.
    /// </summary>
    [ExecuteAlways] // also runs in Edit mode, so the Game view shows the right size while you lay out UI
    [RequireComponent(typeof(Canvas))]
    public class PixelCanvasScaler : MonoBehaviour
    {
        [SerializeField] DisplayConfig displayConfig;

        Canvas canvas;

        void OnEnable() => canvas = GetComponent<Canvas>();

        void Update()
        {
            if (canvas == null || displayConfig == null) return;
            int scale = displayConfig.IntegerScaleFor(Screen.width, Screen.height);
            if (!Mathf.Approximately(canvas.scaleFactor, scale)) canvas.scaleFactor = scale;
        }
    }
}
