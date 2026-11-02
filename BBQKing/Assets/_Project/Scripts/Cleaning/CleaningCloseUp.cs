using System;
using System.Collections.Generic;
using BBQKing.Core;
using BBQKing.Restoration;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// Runs the CloseUp_Cleaning scene: shows the object being cleaned (from the close-up session)
    /// with a grime layer for each of its sections, such as the kettle grill's top and bottom.
    /// Holding the left mouse button wipes grime away under a round brush (design.md section 5.2),
    /// and each section has its own meter. At 95% a section's last grime is wiped away and it is
    /// finished; when every section is, the close-up waits a moment and closes, finishing the
    /// object's job. Escape leaves early, keeping the progress for next time.
    ///
    /// The sparkles and chimes are played by <see cref="CloseUpEffects"/>, which listens to the
    /// <see cref="SectionFinished"/> and <see cref="AllFinished"/> events.
    ///
    /// The close-up's art sits far away from the backyard (see the scene builder), so the two
    /// scenes can share the game world without the backyard camera ever seeing it.
    /// </summary>
    public class CleaningCloseUp : MonoBehaviour
    {
        [SerializeField] InputReader input;
        [SerializeField] CloseUpSession session;
        [SerializeField] CleaningConfig config;

        [Header("Scene parts")]
        [Tooltip("The close-up's own camera, used to find where the mouse points.")]
        [SerializeField] Camera closeUpCamera;

        [Tooltip("Draws the clean object. Each section's grime is drawn over it, at the same size and place.")]
        [SerializeField] SpriteRenderer objectRenderer;

        [Header("UI")]
        [SerializeField] Text titleLabel;
        [SerializeField] Image toolIcon;
        [SerializeField] Text toolLabel;

        [Tooltip("Each section's meter bar, in the same order as the object's sections. Its width is set to match how clean the section is.")]
        [SerializeField] RectTransform[] meterFills = new RectTransform[0];
        [SerializeField] Text[] meterLabels = new Text[0];

        [Header("Wording")]
        [Tooltip("A meter's text. {0} is the section's name, {1} how clean it is as a whole percentage.")]
        [SerializeField] string meterFormat = "{0}: {1}% clean";
        [Tooltip("A finished section's meter text. {0} is the section's name.")]
        [SerializeField] string finishedFormat = "{0}: Spotless!";

        /// <summary>A section has just reached the finishing line. The number is its place in the object's list.</summary>
        public event Action<int> SectionFinished;

        /// <summary>Every section is finished: the big celebration.</summary>
        public event Action AllFinished;

        /// <summary>What is being cleaned, or null if the scene was opened on its own.</summary>
        public CleanableData Cleaning { get; private set; }

        /// <summary>True once every section is finished.</summary>
        public bool IsFinished => grime != null && grime.AllFinished;

        public int LayerCount => layers.Count;
        public GrimeLayer Layer(int section) => layers[section];

        /// <summary>How clean one section is, 0 to 1.</summary>
        public float CleanFraction(int section) => layers[section].Mask.CleanFraction;

        SectionedGrime grime;
        readonly List<GrimeLayer> layers = new List<GrimeLayer>();
        readonly List<int> finishedNow = new List<int>();
        float[] meterFullWidths;
        float allDoneAt = -1f;      // when the big celebration is due, or -1
        float closeAt = -1f;        // when the close-up closes by itself, or -1
        bool closeRequested;

        void OnEnable()
        {
            if (input != null) input.LeaveCloseUpPressed += Close;
        }

        void OnDisable()
        {
            if (input != null) input.LeaveCloseUpPressed -= Close;
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
            SetUpSections();

            // Each bar's width in the scene is its full width; it is shrunk to match progress.
            meterFullWidths = new float[meterFills.Length];
            for (int i = 0; i < meterFills.Length; i++)
                if (meterFills[i] != null) meterFullWidths[i] = meterFills[i].sizeDelta.x;
            ShowProgress();
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
        /// Makes a grime layer for each section, drawn over the object, carrying on from an earlier
        /// visit if there was one.
        /// </summary>
        void SetUpSections()
        {
            var earlier = session.ProgressFor(Cleaning);
            var sections = Cleaning.sections;
            var masks = new GrimeMask[sections.Length];

            for (int i = 0; i < sections.Length; i++)
            {
                var source = sections[i].grimeSprite;
                if (source == null || !source.texture.isReadable)
                {
                    Debug.LogError($"BBQ King: section \"{sections[i].name}\" of {Cleaning.name} needs a grime image with Read/Write turned on in its import settings.");
                    return;
                }

                // A new renderer for this section, at the object's size and place, drawn over it.
                var go = new GameObject("Grime " + sections[i].name);
                go.transform.SetParent(objectRenderer.transform.parent, false);
                go.transform.localPosition = objectRenderer.transform.localPosition;
                go.transform.localScale = objectRenderer.transform.localScale;
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = objectRenderer.sortingOrder + 1 + i;

                var layer = new GrimeLayer(source, renderer, earlier != null && i < earlier.Count ? earlier.Mask(i) : null);
                layers.Add(layer);
                masks[i] = layer.Mask;
            }

            if (earlier != null && earlier.Count == masks.Length && SameMasks(earlier, masks))
                grime = earlier; // carry on, including which sections are already finished
            else
            {
                grime = new SectionedGrime(masks);
                session.KeepProgress(Cleaning, grime); // the same object changes as the player scrubs
            }
        }

        static bool SameMasks(SectionedGrime grime, GrimeMask[] masks)
        {
            for (int i = 0; i < masks.Length; i++) if (grime.Mask(i) != masks[i]) return false;
            return true;
        }

        void Update()
        {
            if (grime == null || config == null || input == null) return;

            if (allDoneAt >= 0f && Time.time >= allDoneAt)
            {
                allDoneAt = -1f;
                closeAt = Time.time + config.finishPauseSeconds;
                AllFinished?.Invoke();
            }
            if (closeAt >= 0f && Time.time >= closeAt) Close();
            if (grime.AllFinished) return; // admiring the result; no more scrubbing

            if (!input.IsCloseUpInputEnabled || !input.ScrubHeld || !input.HasAim) return; // a gamepad brush can come later

            finishedNow.Clear();
            if (!grime.Scrub(BrushPixel(), config.brushRadius, config.scrubPerSecond * Time.deltaTime, config.completeAt, finishedNow))
                return;

            foreach (var layer in layers) layer.Redraw();
            ShowProgress();
            foreach (int section in finishedNow) SectionFinished?.Invoke(section);
            if (grime.AllFinished) allDoneAt = Time.time + config.allDoneDelaySeconds;
        }

        /// <summary>Where the mouse points, in pixels of the close-up art from its bottom-left corner.</summary>
        Vector2 BrushPixel()
        {
            Vector3 world = closeUpCamera.ScreenToWorldPoint(input.AimScreenPosition);
            // Unity concept: InverseTransformPoint turns a world position into the object's own
            // units, undoing its position and its 3 times scale.
            Vector2 local = objectRenderer.transform.InverseTransformPoint(world);
            var sprite = objectRenderer.sprite;
            return local * sprite.pixelsPerUnit + sprite.pivot;
        }

        void ShowProgress()
        {
            if (grime == null) return;
            for (int i = 0; i < meterFills.Length && i < layers.Count; i++)
            {
                float fraction = CleanFraction(i);
                if (meterFills[i] != null)
                    meterFills[i].sizeDelta = new Vector2(meterFullWidths[i] * fraction, meterFills[i].sizeDelta.y);
                if (i < meterLabels.Length && meterLabels[i] != null)
                {
                    string name = Cleaning.sections[i].name;
                    meterLabels[i].text = grime.IsFinished(i)
                        ? string.Format(finishedFormat, name)
                        : string.Format(meterFormat, name, Mathf.FloorToInt(fraction * 100f));
                }
            }
        }

        /// <summary>
        /// Escape, or the pause after finishing. Leaving before the big celebration still counts
        /// the object as finished if every section is.
        /// </summary>
        void Close()
        {
            if (closeRequested || session == null) return;
            closeRequested = true;
            session.RequestClose(IsFinished);
        }

        void OnDestroy()
        {
            foreach (var layer in layers) layer.Dispose();
        }
    }
}
