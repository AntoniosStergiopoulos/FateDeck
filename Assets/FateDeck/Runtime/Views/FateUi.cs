using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// The Fate Deck UI Toolkit style kit: occult casino-noir palette, panels, buttons,
    /// chips and bars, all built in code so the whole interface stays one self-assembling
    /// document with readable, screen-space text.
    /// </summary>
    public static class FateUi
    {
        public static readonly Color Bone = new Color(0.910f, 0.878f, 0.816f);
        public static readonly Color BoneDim = new Color(0.910f, 0.878f, 0.816f, 0.55f);
        public static readonly Color Ink = new Color(0.071f, 0.063f, 0.055f);
        public static readonly Color Panel = new Color(0.106f, 0.094f, 0.082f);
        public static readonly Color PanelLight = new Color(0.145f, 0.129f, 0.110f);
        public static readonly Color Line = new Color(0.28f, 0.25f, 0.20f);
        public static readonly Color Ember = new Color(0.886f, 0.345f, 0.133f);
        public static readonly Color Verdigris = new Color(0.263f, 0.702f, 0.682f);
        public static readonly Color GoldLeaf = new Color(0.831f, 0.678f, 0.220f);
        public static readonly Color Violet = new Color(0.545f, 0.463f, 0.686f);
        public static readonly Color Blood = new Color(0.780f, 0.290f, 0.290f);

        public static VisualElement Column(float gap = 8)
        {
            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Column;
            ApplyGapOnAttach(element, gap, horizontal: false);
            return element;
        }

        public static VisualElement Row(float gap = 8)
        {
            var element = new VisualElement();
            element.style.flexDirection = FlexDirection.Row;
            element.style.alignItems = Align.Center;
            ApplyGapOnAttach(element, gap, horizontal: true);
            return element;
        }

        /// <summary>
        /// UI Toolkit has no flex gap, so the gap becomes margins when the subtree attaches
        /// to the panel. Children that set their own inline margin are left alone.
        /// </summary>
        private static void ApplyGapOnAttach(VisualElement element, float gap, bool horizontal)
        {
            if (gap <= 0f)
            {
                return;
            }

            element.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                for (int i = 0; i < element.childCount - 1; i++)
                {
                    VisualElement child = element[i];
                    if (horizontal)
                    {
                        if (child.style.marginRight.keyword == StyleKeyword.Null)
                        {
                            child.style.marginRight = gap;
                        }
                    }
                    else if (child.style.marginBottom.keyword == StyleKeyword.Null)
                    {
                        child.style.marginBottom = gap;
                    }
                }
            });
        }

        /// <summary>A framed panel with the felt-table background.</summary>
        public static VisualElement MakePanel(string title = null)
        {
            var panel = new VisualElement();
            panel.style.backgroundColor = Panel;
            panel.style.borderTopWidth = 1;
            panel.style.borderBottomWidth = 1;
            panel.style.borderLeftWidth = 1;
            panel.style.borderRightWidth = 1;
            panel.style.borderTopColor = Line;
            panel.style.borderBottomColor = Line;
            panel.style.borderLeftColor = Line;
            panel.style.borderRightColor = Line;
            panel.style.borderTopLeftRadius = 6;
            panel.style.borderTopRightRadius = 6;
            panel.style.borderBottomLeftRadius = 6;
            panel.style.borderBottomRightRadius = 6;
            panel.style.paddingTop = 8;
            panel.style.paddingBottom = 8;
            panel.style.paddingLeft = 10;
            panel.style.paddingRight = 10;

            if (!string.IsNullOrEmpty(title))
            {
                panel.Add(Title(title));
            }

            return panel;
        }

        public static Label Title(string text, float size = 14)
        {
            Label label = Text(text, size, GoldLeaf);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginBottom = 4;
            return label;
        }

        public static Label Heading(string text, float size = 30, Color? color = null)
        {
            Label label = Text(text, size, color ?? Bone);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            return label;
        }

        public static Label Text(string text, float size = 14, Color? color = null)
        {
            var label = new Label(text ?? string.Empty);
            label.style.fontSize = size;
            label.style.color = color ?? Bone;
            label.style.whiteSpace = WhiteSpace.Normal;
            label.enableRichText = true;
            return label;
        }

        /// <summary>A styled button with hover feedback; disabled state greys it out.</summary>
        public static Button MakeButton(string text, Action onClick, Color? accent = null, float size = 15,
            bool enabled = true)
        {
            Color color = accent ?? GoldLeaf;
            var button = new Button(() =>
            {
                if (enabled)
                {
                    onClick?.Invoke();
                }
            })
            {
                text = text
            };

            button.style.fontSize = size;
            button.style.color = enabled ? color : BoneDim;
            button.style.backgroundColor = PanelLight;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            Color border = enabled ? new Color(color.r, color.g, color.b, 0.55f) : Line;
            button.style.borderTopColor = border;
            button.style.borderBottomColor = border;
            button.style.borderLeftColor = border;
            button.style.borderRightColor = border;
            button.style.borderTopLeftRadius = 5;
            button.style.borderTopRightRadius = 5;
            button.style.borderBottomLeftRadius = 5;
            button.style.borderBottomRightRadius = 5;
            button.style.paddingTop = 6;
            button.style.paddingBottom = 6;
            button.style.paddingLeft = 14;
            button.style.paddingRight = 14;
            button.style.marginTop = 2;
            button.style.marginBottom = 2;
            button.style.marginLeft = 2;
            button.style.marginRight = 2;
            button.style.whiteSpace = WhiteSpace.Normal;

            if (enabled)
            {
                Color rest = PanelLight;
                Color hover = new Color(0.20f, 0.175f, 0.14f);
                button.RegisterCallback<PointerEnterEvent>(_ => button.style.backgroundColor = hover);
                button.RegisterCallback<PointerLeaveEvent>(_ => button.style.backgroundColor = rest);
            }

            return button;
        }

        /// <summary>A small rounded status chip ("Block 3", "Burn 2").</summary>
        public static VisualElement Chip(string text, Color color, float size = 12)
        {
            var chip = new VisualElement();
            chip.style.backgroundColor = new Color(color.r, color.g, color.b, 0.16f);
            chip.style.borderTopLeftRadius = 8;
            chip.style.borderTopRightRadius = 8;
            chip.style.borderBottomLeftRadius = 8;
            chip.style.borderBottomRightRadius = 8;
            chip.style.paddingLeft = 7;
            chip.style.paddingRight = 7;
            chip.style.paddingTop = 1;
            chip.style.paddingBottom = 2;
            chip.style.marginRight = 4;

            Label label = Text(text, size, color);
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            chip.Add(label);
            return chip;
        }

        /// <summary>A labeled progress bar (enemy HP).</summary>
        public static VisualElement Bar(double value, double max, Color fill, string label)
        {
            var holder = new VisualElement();
            holder.style.height = 18;
            holder.style.backgroundColor = new Color(0f, 0f, 0f, 0.45f);
            holder.style.borderTopLeftRadius = 4;
            holder.style.borderTopRightRadius = 4;
            holder.style.borderBottomLeftRadius = 4;
            holder.style.borderBottomRightRadius = 4;
            holder.style.overflow = Overflow.Hidden;

            var filled = new VisualElement();
            float percent = max > 0 ? Mathf.Clamp01((float)(value / max)) * 100f : 0f;
            filled.style.width = Length.Percent(percent);
            filled.style.height = Length.Percent(100);
            filled.style.backgroundColor = new Color(fill.r, fill.g, fill.b, 0.55f);
            holder.Add(filled);

            Label text = Text(label, 12, Bone);
            text.style.position = Position.Absolute;
            text.style.left = 0;
            text.style.right = 0;
            text.style.top = 0;
            text.style.bottom = 0;
            text.style.unityTextAlign = TextAnchor.MiddleCenter;
            text.style.unityFontStyleAndWeight = FontStyle.Bold;
            holder.Add(text);
            return holder;
        }

        public static VisualElement Divider()
        {
            var line = new VisualElement();
            line.style.height = 1;
            line.style.backgroundColor = Line;
            line.style.marginTop = 6;
            line.style.marginBottom = 6;
            return line;
        }

        public static VisualElement Spacer(float grow = 1)
        {
            var spacer = new VisualElement();
            spacer.style.flexGrow = grow;
            return spacer;
        }

        /// <summary>Makes an element clickable with a subtle hover highlight.</summary>
        public static void MakeClickable(VisualElement element, Action onClick)
        {
            element.RegisterCallback<ClickEvent>(_ => onClick?.Invoke());
            element.RegisterCallback<PointerEnterEvent>(_ => element.style.opacity = 0.82f);
            element.RegisterCallback<PointerLeaveEvent>(_ => element.style.opacity = 1f);
        }

        public static void SetBorder(VisualElement element, Color color, float width = 1, float radius = 6)
        {
            element.style.borderTopWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderTopColor = color;
            element.style.borderBottomColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderTopLeftRadius = radius;
            element.style.borderTopRightRadius = radius;
            element.style.borderBottomLeftRadius = radius;
            element.style.borderBottomRightRadius = radius;
        }

        public static void Pad(VisualElement element, float padding)
        {
            element.style.paddingTop = padding;
            element.style.paddingBottom = padding;
            element.style.paddingLeft = padding;
            element.style.paddingRight = padding;
        }
    }
}
