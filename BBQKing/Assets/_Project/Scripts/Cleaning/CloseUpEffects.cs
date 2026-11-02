using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// The rewards in the cleaning close-up (design.md section 5.2): a sparkle and a chime when a
    /// section is finished, and a bigger sparkle, a white flash and a triple chime when the whole
    /// object is. It only listens to <see cref="CleaningCloseUp"/>'s events, so the cleaning rules
    /// never need to know how the rewards look or sound.
    ///
    /// Unity concept: a coroutine (StartCoroutine) is a method that can pause with "yield" and
    /// carry on in a later frame. Each sparkle is one coroutine that steps through its frames over
    /// a fraction of a second, then removes itself.
    /// </summary>
    public class CloseUpEffects : MonoBehaviour
    {
        [SerializeField] CleaningCloseUp closeUp;
        [SerializeField] CleaningConfig config;

        [Tooltip("Plays the chimes. Its sound is not placed in the world, so it is the same volume anywhere.")]
        [SerializeField] AudioSource audioSource;

        [Tooltip("The sparkle's animation frames, played in order.")]
        [SerializeField] Sprite[] sparkleFrames = new Sprite[0];

        [Tooltip("Drawn above the object and its grime.")]
        [SerializeField] int sparkleSortingOrder = 20;

        [Tooltip("A white image covering the screen, see-through until the flash.")]
        [SerializeField] Image flash;

        void OnEnable()
        {
            if (closeUp == null) return;
            closeUp.SectionFinished += OnSectionFinished;
            closeUp.AllFinished += OnAllFinished;
        }

        void OnDisable()
        {
            if (closeUp == null) return;
            closeUp.SectionFinished -= OnSectionFinished;
            closeUp.AllFinished -= OnAllFinished;
        }

        void OnSectionFinished(int section)
        {
            var layer = closeUp.Layer(section);
            Sparkle(layer.RandomPointOnSection, config.sectionSparkles, config.sectionSparkleScale);
            PlayChime(config.sectionChime);
        }

        void OnAllFinished()
        {
            // Over the whole object: each sparkle picks a random section.
            Sparkle(() => closeUp.Layer(UnityEngine.Random.Range(0, closeUp.LayerCount)).RandomPointOnSection(),
                config.allDoneSparkles, config.allDoneSparkleScale);
            PlayChime(config.allDoneChime);
            if (flash != null) StartCoroutine(Flash());
        }

        void PlayChime(AudioClip clip)
        {
            // PlayOneShot lets a chime overlap the one before instead of cutting it off.
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }

        void Sparkle(Func<Vector3> pickPoint, int count, float scale)
        {
            if (sparkleFrames.Length == 0) return;
            for (int i = 0; i < count; i++)
                StartCoroutine(OneSparkle(pickPoint(), scale, UnityEngine.Random.Range(0f, config.sparkleSpreadSeconds)));
        }

        IEnumerator OneSparkle(Vector3 position, float scale, float delay)
        {
            if (delay > 0f) yield return new WaitForSeconds(delay);

            var go = new GameObject("Sparkle");
            go.transform.SetParent(transform, true);
            go.transform.position = new Vector3(position.x, position.y, transform.position.z);
            go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = sparkleSortingOrder;

            float duration = Mathf.Max(0.01f, config.sparkleSeconds);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                sr.sprite = sparkleFrames[Mathf.Min(sparkleFrames.Length - 1, (int)(progress * sparkleFrames.Length))];
                // Fade out over the last third.
                sr.color = new Color(1f, 1f, 1f, Mathf.Clamp01((1f - progress) * 3f));
                yield return null;
            }
            Destroy(go);
        }

        IEnumerator Flash()
        {
            float duration = Mathf.Max(0.01f, config.flashSeconds);
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                flash.color = new Color(1f, 1f, 1f, config.flashStrength * (1f - t / duration));
                yield return null;
            }
            flash.color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
