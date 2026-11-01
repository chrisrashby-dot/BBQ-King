using UnityEngine;

namespace BBQKing.Core
{
    /// <summary>The reference resolutions we are trying out (design.md section 11, open questions).</summary>
    public enum ReferenceResolution
    {
        [InspectorName("640 x 360 (20 x 11 tiles)")] Small640x360,
        [InspectorName("960 x 540 (30 x 17 tiles, proposed)")] Medium960x540,
        [InspectorName("1280 x 720 (40 x 22 tiles)")] Large1280x720,
    }

    /// <summary>
    /// Display settings shared by the camera and the UI, stored in Data/Settings/DisplayConfig.asset.
    ///
    /// The reference resolution is the size of the game's "virtual screen" in art pixels. The
    /// Pixel Perfect Camera draws the game at this size, then scales it up by a whole number to fill
    /// the window, so every art pixel stays a sharp square.
    ///
    /// Changing the drop-down in the Inspector takes effect immediately, even in Play mode, so you
    /// can compare the options while walking around. Unlike changes to scene objects, changes to an
    /// asset like this one are kept when you press Stop.
    /// </summary>
    [CreateAssetMenu(fileName = "DisplayConfig", menuName = "BBQ King/Display Config")]
    public class DisplayConfig : ScriptableObject
    {
        [Tooltip("The size of the virtual screen in art pixels. 960 x 540 is the proposal in design.md.")]
        [SerializeField] ReferenceResolution referenceResolution = ReferenceResolution.Medium960x540;

        public ReferenceResolution Resolution
        {
            get => referenceResolution;
            set => referenceResolution = value;
        }

        /// <summary>Width of the virtual screen in art pixels.</summary>
        public int Width => Size.x;

        /// <summary>Height of the virtual screen in art pixels.</summary>
        public int Height => Size.y;

        Vector2Int Size => referenceResolution switch
        {
            ReferenceResolution.Small640x360 => new Vector2Int(640, 360),
            ReferenceResolution.Large1280x720 => new Vector2Int(1280, 720),
            _ => new Vector2Int(960, 540),
        };

        /// <summary>
        /// The largest whole-number scale at which the virtual screen fits in a window of the given size.
        /// Never less than 1, so a tiny window still shows something.
        /// </summary>
        public int IntegerScaleFor(int windowWidth, int windowHeight)
        {
            return Mathf.Max(1, Mathf.Min(windowWidth / Width, windowHeight / Height));
        }
    }
}
