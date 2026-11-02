using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// Runs the CloseUp_Cleaning scene: shows the object being cleaned (from the close-up session)
    /// with its grime over it. Holding the left mouse button wipes grime away under a round brush
    /// (design.md section 5.2), and Escape leaves, keeping the progress for next time.
    ///
    /// The close-up's art sits far away from the backyard (see the scene builder), so the two
    /// scenes can share the game world without the backyard camera ever seeing it.
    ///
    /// Unity concept: the grime is drawn from a texture made while the game runs. Each frame the
    /// brush changes the <see cref="GrimeMask"/>, the new grime is written into the texture's
    /// pixels, and Apply() sends them to the graphics card to be drawn.
    /// </summary>
    public class CleaningCloseUp : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] CloseUpSession session;
        [SerializeField] CleaningConfig config;

        [Header("Scene parts")]
        [Tooltip("The close-up's own camera, used to find where the mouse points.")]
        [SerializeField] Camera closeUpCamera;
        [SerializeField] SpriteRenderer objectRenderer;
        [SerializeField] SpriteRenderer grimeRenderer;

        [Header("UI")]
        [SerializeField] Text titleLabel;
        [SerializeField] Image toolIcon;
        [SerializeField] Text toolLabel;

        /// <summary>What is being cleaned, or null if the scene was opened on its own.</summary>
        public CleanableData Cleaning { get; private set; }

        /// <summary>How clean the object is, 0 to 1.</summary>
        public float CleanFraction => mask != null ? mask.CleanFraction : 0f;

        GrimeMask mask;
        Color32[] grimeColours;   // the grime image's own colours; the mask sets their opacity
        Color32[] shownPixels;    // what is drawn, rewritten whenever grime comes off
        Texture2D grimeTexture;
        Sprite grimeSprite;

        void OnEnable()
        {
            if (input != null) input.LeaveCloseUpPressed += Leave;
        }

        void OnDisable()
        {
            if (input != null) input.LeaveCloseUpPressed -= Leave;
        }

        void Start()
        {
            Cleaning = session != null ? session.Current : null;
            if (Cleaning == null)
            {
                // Pressing Play with only this scene open: there is nothing to clean.
                Debug.LogWarning("BBQ King: the cleaning close-up opens from the Backyard scene. " +
                                 "Open Backyard and press Play, then use the kettle grill.");
                return;
            }

            if (objectRenderer != null) objectRenderer.sprite = Cleaning.closeUpSprite;
            if (titleLabel != null) titleLabel.text = Cleaning.cleanPrompt;
            ShowTool(Cleaning.tool);
            SetUpGrime();
        }

        void ShowTool(ToolData tool)
        {
            if (toolIcon != null)
            {
                toolIcon.sprite = tool != null ? tool.icon : null;
                toolIcon.enabled = toolIcon.sprite != null;
            }
            if (toolLabel != null) toolLabel.text = tool != null ? tool.displayName : string.Empty;
        }

        /// <summary>
        /// Copies the grime image into a texture the game can change, and picks up the grime left
        /// from an earlier visit, if any.
        /// </summary>
        void SetUpGrime()
        {
            var source = Cleaning.grimeSprite;
            if (source == null || grimeRenderer == null) return;
            if (!source.texture.isReadable)
            {
                Debug.LogError($"BBQ King: {source.texture.name} needs Read/Write turned on in its import settings to be scrubbed.");
                grimeRenderer.sprite = source;
                return;
            }

            var rect = source.textureRect;
            int width = (int)rect.width, height = (int)rect.height;
            var colours = source.texture.GetPixels((int)rect.x, (int)rect.y, width, height);
            grimeColours = new Color32[colours.Length];
            for (int i = 0; i < colours.Length; i++) grimeColours[i] = colours[i];

            mask = session.ProgressFor(Cleaning);
            if (mask == null || mask.Width != width || mask.Height != height)
            {
                mask = GrimeMask.FromPixels(grimeColours, width, height);
                session.KeepProgress(Cleaning, mask); // the same mask changes as the player scrubs
            }

            grimeTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point, // crisp pixels, like the imported art
                wrapMode = TextureWrapMode.Clamp,
                name = source.texture.name + " (being scrubbed)",
            };
            shownPixels = new Color32[grimeColours.Length];
            var pivot = new Vector2(source.pivot.x / width, source.pivot.y / height);
            grimeSprite = Sprite.Create(grimeTexture, new Rect(0, 0, width, height), pivot, source.pixelsPerUnit);
            grimeRenderer.sprite = grimeSprite;
            RedrawGrime();
        }

        void Update()
        {
            if (mask == null || config == null || input == null || !input.IsCloseUpInputEnabled) return;
            if (!input.ScrubHeld || !input.HasAim) return; // a gamepad brush can come later

            if (mask.Scrub(BrushPixel(), config.brushRadius, config.scrubPerSecond * Time.deltaTime))
                RedrawGrime();
        }

        /// <summary>Where the mouse points, in pixels of the grime image from its bottom-left corner.</summary>
        Vector2 BrushPixel()
        {
            Vector3 world = closeUpCamera.ScreenToWorldPoint(input.AimScreenPosition);
            // Unity concept: InverseTransformPoint turns a world position into the object's own
            // units, undoing its position and its 3 times scale.
            Vector2 local = grimeRenderer.transform.InverseTransformPoint(world);
            var sprite = grimeRenderer.sprite;
            return local * sprite.pixelsPerUnit + sprite.pivot;
        }

        void RedrawGrime()
        {
            for (int y = 0; y < mask.Height; y++)
                for (int x = 0; x < mask.Width; x++)
                {
                    int i = y * mask.Width + x;
                    var c = grimeColours[i];
                    c.a = (byte)Mathf.RoundToInt(mask.GrimeAt(x, y) * 255f);
                    shownPixels[i] = c;
                }
            grimeTexture.SetPixels32(shownPixels);
            grimeTexture.Apply();
        }

        void Leave()
        {
            if (session != null) session.RequestClose(false);
        }

        void OnDestroy()
        {
            // Textures and sprites made while the game runs are not cleaned up automatically.
            if (grimeSprite != null) Destroy(grimeSprite);
            if (grimeTexture != null) Destroy(grimeTexture);
        }
    }
}
