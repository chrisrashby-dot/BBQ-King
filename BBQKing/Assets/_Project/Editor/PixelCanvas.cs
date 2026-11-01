using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// A tiny drawing canvas for making placeholder pixel art in code.
    /// (0,0) is the bottom-left pixel, matching Unity's texture coordinates.
    /// </summary>
    public class PixelCanvas
    {
        public readonly int Width, Height;
        public readonly Color32[] Pixels;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Color32[width * height]; // starts fully transparent
        }

        public void Set(int x, int y, Color32 c)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            Pixels[y * Width + x] = c;
        }

        public Color32 Get(int x, int y) => Pixels[y * Width + x];

        public void Fill(Color32 c) => FillRect(0, 0, Width, Height, c);

        public void FillRect(int x, int y, int w, int h, Color32 c)
        {
            for (int j = y; j < y + h; j++)
                for (int i = x; i < x + w; i++)
                    Set(i, j, c);
        }

        public void Outline(int x, int y, int w, int h, Color32 c)
        {
            for (int i = x; i < x + w; i++) { Set(i, y, c); Set(i, y + h - 1, c); }
            for (int j = y; j < y + h; j++) { Set(x, j, c); Set(x + w - 1, j, c); }
        }

        public void FillCircle(float cx, float cy, float r, Color32 c)
        {
            for (int j = Mathf.FloorToInt(cy - r); j <= Mathf.CeilToInt(cy + r); j++)
                for (int i = Mathf.FloorToInt(cx - r); i <= Mathf.CeilToInt(cx + r); i++)
                {
                    float dx = i + 0.5f - cx, dy = j + 0.5f - cy;
                    if (dx * dx + dy * dy <= r * r) Set(i, j, c);
                }
        }

        public void FillEllipse(float cx, float cy, float rx, float ry, Color32 c)
        {
            for (int j = Mathf.FloorToInt(cy - ry); j <= Mathf.CeilToInt(cy + ry); j++)
                for (int i = Mathf.FloorToInt(cx - rx); i <= Mathf.CeilToInt(cx + rx); i++)
                {
                    float dx = (i + 0.5f - cx) / rx, dy = (j + 0.5f - cy) / ry;
                    if (dx * dx + dy * dy <= 1f) Set(i, j, c);
                }
        }

        /// <summary>Darkens or lightens an existing pixel colour, ignoring transparent pixels.</summary>
        public void Shade(int x, int y, float factor)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            var p = Get(x, y);
            if (p.a == 0) return;
            Set(x, y, new Color32(
                (byte)Mathf.Clamp(p.r * factor, 0, 255),
                (byte)Mathf.Clamp(p.g * factor, 0, 255),
                (byte)Mathf.Clamp(p.b * factor, 0, 255), p.a));
        }

        public static Color32 Hex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}
