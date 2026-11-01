using UnityEngine;
using UnityEngine.UI;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Creates UI objects for the scene builders. Sizes and positions are in art pixels at the
    /// reference resolution; the PixelCanvasScaler multiplies them up by a whole number.
    ///
    /// Unity concept: UI objects use a RectTransform instead of a Transform. Its anchors say which
    /// part of the parent it is attached to (a corner, an edge, or stretched across), and its
    /// position and size are measured from there.
    /// </summary>
    public static class UiFactory
    {
        public static readonly Color32 LightText = new Color32(0xf6, 0xec, 0xd6, 0xff);
        public static readonly Color32 InkText = new Color32(0x3a, 0x2a, 0x20, 0xff);

        /// <summary>An empty object covering its parent, used to group UI and hold its controller script.</summary>
        public static RectTransform Root(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = (RectTransform)go.transform;
            Stretch(rt, 0, 0, 0, 0);
            return rt;
        }

        static Font Font => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        /// <summary>A 9-sliced image, so a small frame sprite stretches to any size without blurring its edges.</summary>
        public static Image Image(Transform parent, string name, string spritePath)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = BuilderUtil.LoadSprite(spritePath);
            img.type = UnityEngine.UI.Image.Type.Sliced;
            img.raycastTarget = false;
            return img;
        }

        /// <summary>An image shown at its own size, such as a tool icon.</summary>
        public static Image Icon(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            Anchor(img.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            return img;
        }

        public static Text Text(Transform parent, string name, string text, int size, TextAnchor align, Color32 colour)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.alignment = align;
            t.color = colour;
            t.raycastTarget = false;
            return t;
        }

        /// <summary>Pins a fixed-size element to a point of its parent (for example 0.5, 0 is bottom centre).</summary>
        public static void Anchor(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        /// <summary>Stretches an element across its parent, inset by the given margins.</summary>
        public static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }
    }
}
