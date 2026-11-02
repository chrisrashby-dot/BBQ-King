using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// Draws one section's grime in the close-up and keeps the picture in step with its
    /// <see cref="GrimeMask"/> as the player scrubs.
    ///
    /// Unity concept: the grime is drawn from a texture made while the game runs. When grime comes
    /// off, the new pixels are written into the texture and Apply() sends them to the graphics card.
    /// Textures and sprites made like this are not cleaned up automatically, so call
    /// <see cref="Dispose"/> when the close-up closes.
    /// </summary>
    public class GrimeLayer
    {
        public GrimeMask Mask { get; }
        public SpriteRenderer Renderer { get; }

        readonly Color32[] colours;   // the grime image's own colours; the mask sets their opacity
        readonly Color32[] shown;     // what is drawn, rewritten whenever grime comes off
        readonly Texture2D texture;
        readonly Sprite sprite;
        readonly Vector2Int[] grimyPixels; // where grime started, for placing sparkles

        /// <summary>
        /// Makes the grime picture from <paramref name="source"/>, drawn by <paramref name="renderer"/>.
        /// Pass the mask from an earlier visit to carry on from it, or null to start afresh.
        /// </summary>
        public GrimeLayer(Sprite source, SpriteRenderer renderer, GrimeMask earlier)
        {
            Renderer = renderer;
            var rect = source.textureRect;
            int width = (int)rect.width, height = (int)rect.height;

            var read = source.texture.GetPixels((int)rect.x, (int)rect.y, width, height);
            colours = new Color32[read.Length];
            int grimy = 0;
            for (int i = 0; i < read.Length; i++)
            {
                colours[i] = read[i];
                if (colours[i].a > 0) grimy++;
            }

            grimyPixels = new Vector2Int[grimy];
            for (int i = 0, n = 0; i < colours.Length; i++)
                if (colours[i].a > 0) grimyPixels[n++] = new Vector2Int(i % width, i / width);

            Mask = earlier != null && earlier.Width == width && earlier.Height == height
                ? earlier
                : GrimeMask.FromPixels(colours, width, height);

            texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point, // crisp pixels, like the imported art
                wrapMode = TextureWrapMode.Clamp,
                name = source.texture.name + " (being scrubbed)",
            };
            shown = new Color32[colours.Length];
            var pivot = new Vector2(source.pivot.x / width, source.pivot.y / height);
            sprite = Sprite.Create(texture, new Rect(0, 0, width, height), pivot, source.pixelsPerUnit);
            renderer.sprite = sprite;
            Redraw();
        }

        /// <summary>Writes the mask's grime into the picture.</summary>
        public void Redraw()
        {
            for (int i = 0; i < colours.Length; i++)
            {
                var c = colours[i];
                c.a = (byte)Mathf.RoundToInt(Mask.GrimeAt(i % Mask.Width, i / Mask.Width) * 255f);
                shown[i] = c;
            }
            texture.SetPixels32(shown);
            texture.Apply();
        }

        /// <summary>
        /// A world position on a random pixel that started grimy, so sparkles appear over this
        /// section and not over another one.
        /// </summary>
        public Vector3 RandomPointOnSection()
        {
            if (grimyPixels.Length == 0) return Renderer.transform.position;
            var pixel = grimyPixels[Random.Range(0, grimyPixels.Length)];
            Vector2 local = (pixel + new Vector2(0.5f, 0.5f) - sprite.pivot) / sprite.pixelsPerUnit;
            return Renderer.transform.TransformPoint(local);
        }

        public void Dispose()
        {
            Object.Destroy(sprite);
            Object.Destroy(texture);
        }
    }
}
