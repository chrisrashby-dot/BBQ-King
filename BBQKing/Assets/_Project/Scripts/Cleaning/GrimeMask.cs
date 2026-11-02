using UnityEngine;

namespace BBQKing.Cleaning
{
    /// <summary>
    /// How much grime is left on each pixel of a close-up, from 0 (clean) to 1 (fully grimy).
    /// Plain C# with no Unity objects, so EditMode tests can check the scrubbing maths.
    ///
    /// Each pixel starts at the grime image's opacity there, so see-through parts of the image
    /// start clean. How clean the object is (<see cref="CleanFraction"/>) compares the grime left
    /// with the grime there was at the start, so the see-through parts never count.
    /// </summary>
    public class GrimeMask
    {
        public int Width { get; }
        public int Height { get; }

        readonly float[] grime;
        readonly float startTotal;
        float remaining;

        /// <param name="start">Grime per pixel, row by row from the bottom left, each 0 to 1.</param>
        public GrimeMask(int width, int height, float[] start)
        {
            Width = width;
            Height = height;
            grime = new float[width * height];
            for (int i = 0; i < grime.Length && i < start.Length; i++)
            {
                grime[i] = Mathf.Clamp01(start[i]);
                startTotal += grime[i];
            }
            remaining = startTotal;
        }

        /// <summary>A mask from an image's pixels: each pixel's opacity is its starting grime.</summary>
        public static GrimeMask FromPixels(Color32[] pixels, int width, int height)
        {
            var start = new float[pixels.Length];
            for (int i = 0; i < pixels.Length; i++) start[i] = pixels[i].a / 255f;
            return new GrimeMask(width, height, start);
        }

        /// <summary>0 when untouched, 1 when spotless.</summary>
        public float CleanFraction => startTotal <= 0f ? 1f : Mathf.Clamp01(1f - remaining / startTotal);

        /// <summary>The grime left on one pixel, 0 to 1. Pixels outside the image count as clean.</summary>
        public float GrimeAt(int x, int y) =>
            x < 0 || y < 0 || x >= Width || y >= Height ? 0f : grime[y * Width + x];

        /// <summary>
        /// Takes up to <paramref name="amount"/> of grime off every pixel whose centre is within
        /// <paramref name="radius"/> pixels of <paramref name="centre"/> (a round brush). Positions
        /// are in pixels from the bottom-left corner. Returns true if any grime came off.
        /// </summary>
        public bool Scrub(Vector2 centre, float radius, float amount)
        {
            if (radius <= 0f || amount <= 0f) return false;

            int xMin = Mathf.Max(0, Mathf.FloorToInt(centre.x - radius));
            int xMax = Mathf.Min(Width - 1, Mathf.CeilToInt(centre.x + radius));
            int yMin = Mathf.Max(0, Mathf.FloorToInt(centre.y - radius));
            int yMax = Mathf.Min(Height - 1, Mathf.CeilToInt(centre.y + radius));
            float radiusSquared = radius * radius;
            bool changed = false;

            for (int y = yMin; y <= yMax; y++)
                for (int x = xMin; x <= xMax; x++)
                {
                    float dx = x + 0.5f - centre.x, dy = y + 0.5f - centre.y;
                    if (dx * dx + dy * dy > radiusSquared) continue;

                    int i = y * Width + x;
                    if (grime[i] <= 0f) continue;
                    float removed = Mathf.Min(grime[i], amount);
                    grime[i] -= removed;
                    remaining -= removed;
                    changed = true;
                }

            if (remaining < 0f) remaining = 0f; // guards against rounding
            return changed;
        }

        /// <summary>Wipes every last bit away, used when the object reaches the finishing line.</summary>
        public void ClearAll()
        {
            System.Array.Clear(grime, 0, grime.Length);
            remaining = 0f;
        }
    }
}
