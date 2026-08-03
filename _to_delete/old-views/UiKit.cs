using System;
using AStergio.OmniCard.Runtime.Cards.Views.Core;
using UnityEngine;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// Minimal world-space UI helpers in the SpireClimb sample's style: TextMesh labels and
    /// collider buttons routed through OmniCard's input-backend-agnostic PointerClickable.
    /// </summary>
    public static class UiKit
    {
        public static readonly Color Bone = new Color(0.910f, 0.878f, 0.816f);
        public static readonly Color Ink = new Color(0.071f, 0.063f, 0.055f);
        public static readonly Color Ember = new Color(0.886f, 0.345f, 0.133f);
        public static readonly Color Verdigris = new Color(0.263f, 0.702f, 0.682f);
        public static readonly Color GoldLeaf = new Color(0.788f, 0.635f, 0.153f);
        public static readonly Color Violet = new Color(0.427f, 0.353f, 0.557f);
        public static readonly Color Dim = new Color(0.910f, 0.878f, 0.816f, 0.45f);

        private static Sprite _white;

        /// <summary>A shared 8x8 white sprite for quads, backs and highlights.</summary>
        public static Sprite WhiteSprite
        {
            get
            {
                if (_white == null)
                {
                    var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                    var pixels = new Color[64];
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        pixels[i] = Color.white;
                    }

                    texture.SetPixels(pixels);
                    texture.Apply();
                    texture.hideFlags = HideFlags.DontSave;
                    _white = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
                    _white.hideFlags = HideFlags.DontSave;
                }

                return _white;
            }
        }

        /// <summary>Sorting order for regular chrome text; overlay content should use OverlayOrder.</summary>
        public const int TextOrder = 600;

        /// <summary>Sorting order for overlay content, above the 800-order backdrop.</summary>
        public const int OverlayOrder = 860;

        public static TextMesh Label(Transform parent, Vector3 position, string text,
            int fontSize = 32, Color? color = null, TextAnchor anchor = TextAnchor.MiddleCenter,
            int sortingOrder = TextOrder)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = position;
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = text ?? string.Empty;
            label.fontSize = fontSize;
            label.characterSize = 0.045f;
            label.anchor = anchor;
            label.alignment = anchor == TextAnchor.MiddleLeft || anchor == TextAnchor.UpperLeft
                ? TextAlignment.Left
                : anchor == TextAnchor.MiddleRight || anchor == TextAnchor.UpperRight
                    ? TextAlignment.Right
                    : TextAlignment.Center;
            label.color = color ?? Bone;
            var renderer = labelObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = sortingOrder;
            }

            return label;
        }

        public static TextMesh Button(Transform parent, Vector3 position, string text, Action onClick,
            Vector2? colliderSize = null, int fontSize = 34, Color? color = null, int sortingOrder = TextOrder)
        {
            TextMesh label = Label(parent, position, text, fontSize, color ?? GoldLeaf, TextAnchor.MiddleCenter,
                sortingOrder);
            label.gameObject.name = $"Button {text}";
            BoxCollider2D collider = label.gameObject.AddComponent<BoxCollider2D>();
            collider.size = colliderSize ?? new Vector2(Mathf.Max(2.2f, 0.14f * (text?.Length ?? 4)), 0.7f);
            PointerClickable clickable = label.gameObject.AddComponent<PointerClickable>();
            clickable.Clicked = onClick;
            return label;
        }

        public static SpriteRenderer Quad(Transform parent, Vector3 position, Vector2 size, Color color,
            int sortingOrder = 0, string name = "Quad")
        {
            var quadObject = new GameObject(name);
            quadObject.transform.SetParent(parent, false);
            quadObject.transform.localPosition = position;
            SpriteRenderer renderer = quadObject.AddComponent<SpriteRenderer>();
            renderer.sprite = WhiteSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            quadObject.transform.localScale = new Vector3(size.x, size.y, 1f);
            return renderer;
        }

        public static void Clear(Transform root)
        {
            if (root == null)
            {
                return;
            }

            for (int i = root.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
            }
        }

        public static GameObject Group(Transform parent, string name, Vector3 position = default)
        {
            var group = new GameObject(name);
            group.transform.SetParent(parent, false);
            group.transform.localPosition = position;
            return group;
        }
    }
}
