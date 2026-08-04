using System;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Layout;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using AStergio.OmniCard.Runtime.Cards.Views.UIToolkit;
using FateDeck.Runtime.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// Card visuals for the UI Toolkit table: full card faces rendered through OmniCard's
    /// UIToolkitCardViewBuilder (scaled into UI space), and compact native force tiles for
    /// piles where a whole card face would be unreadable.
    /// </summary>
    public static class CardElementBuilder
    {
        /// <summary>Builds a full card face from its OmniCard layout, scaled to fit UI space.</summary>
        public static VisualElement ScaledCard(CardLayout layout, CardInstance card, float scale)
        {
            var holder = new VisualElement();
            if (layout == null || card == null)
            {
                return holder;
            }

            holder.style.width = layout.DesignSize.x * scale;
            holder.style.height = layout.DesignSize.y * scale;
            holder.style.overflow = Overflow.Hidden;
            FateUi.SetBorder(holder, FateUi.Line, 1, 8);

            UIToolkitCardViewBuilder.BuildInto(layout, card, holder);
            if (holder.childCount > 0)
            {
                VisualElement cardRoot = holder[holder.childCount - 1];
                cardRoot.style.position = Position.Absolute;
                cardRoot.style.left = 0;
                cardRoot.style.top = 0;
                cardRoot.style.transformOrigin = new TransformOrigin(Length.Percent(0), Length.Percent(0));
                cardRoot.style.scale = new Scale(new Vector2(scale, scale));
            }

            return holder;
        }

        /// <summary>The hover text for a force: name, law summary, and any special flags.</summary>
        public static string ForceTipText(FateContentCatalog catalog, MetadataEntry force, string extra = null)
        {
            if (force == null)
            {
                return extra;
            }

            var text = new System.Text.StringBuilder();
            text.Append("<b>").Append(force.name.ToUpperInvariant()).Append("</b>");
            string law = force.GetText(catalog.DescriptionField);
            if (!string.IsNullOrEmpty(law))
            {
                text.Append("\n").Append(law);
            }

            if (force.GetBoolean(catalog.CannotPocketField))
            {
                text.Append("\nCannot be pocketed.");
            }

            if (catalog.ExileAfterFlipField != null && force.GetBoolean(catalog.ExileAfterFlipField))
            {
                text.Append("\nShatters (exiled) after any flip.");
            }

            if (!string.IsNullOrEmpty(extra))
            {
                text.Append("\n\n").Append(extra);
            }

            return text.ToString();
        }

        /// <summary>
        /// A compact, readable tile for one fate card: force glyph, name, and force color.
        /// Piles, pockets and wound rows use these instead of shrunken card faces. Every tile
        /// carries a hover tooltip with the force's full law.
        /// </summary>
        public static VisualElement ForceTile(FateContentCatalog catalog, CardInstance card,
            float width = 74, Action onClick = null, string footnote = null, string extraTip = null)
        {
            MetadataEntry force = catalog.ForceOf(card);
            Color color = ForceColor(catalog, force);
            string glyph = force != null ? force.GetText(catalog.ForceGlyphField) : "?";
            string name = card != null ? card.DisplayName : string.Empty;

            var tile = new VisualElement();
            tile.style.width = width;
            tile.style.height = width * 1.32f;
            tile.style.backgroundColor = new Color(color.r * 0.24f, color.g * 0.24f, color.b * 0.24f);
            FateUi.SetBorder(tile, new Color(color.r, color.g, color.b, 0.65f), 1, 6);
            tile.style.marginRight = 5;
            tile.style.marginBottom = 5;
            tile.style.alignItems = Align.Center;
            tile.style.justifyContent = Justify.Center;

            Label glyphLabel = FateUi.Text(glyph, width * 0.42f, color);
            glyphLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            glyphLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            tile.Add(glyphLabel);

            Label nameLabel = FateUi.Text(name, Mathf.Max(10f, width * 0.155f), FateUi.Bone);
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nameLabel.style.whiteSpace = WhiteSpace.NoWrap;
            tile.Add(nameLabel);

            if (!string.IsNullOrEmpty(footnote))
            {
                Label note = FateUi.Text(footnote, 10, FateUi.BoneDim);
                note.style.unityTextAlign = TextAnchor.MiddleCenter;
                tile.Add(note);
            }

            if (onClick != null)
            {
                FateUi.MakeClickable(tile, onClick);
            }

            FateTip.Bind(tile, ForceTipText(catalog, force, extraTip));
            return tile;
        }

        /// <summary>A face-down deck tile with a count.</summary>
        public static VisualElement DeckTile(int count, float width = 74, Action onClick = null)
        {
            var tile = new VisualElement();
            tile.style.width = width;
            tile.style.height = width * 1.32f;
            tile.style.backgroundColor = new Color(0.13f, 0.11f, 0.18f);
            FateUi.SetBorder(tile, FateUi.Violet, 1, 6);
            tile.style.alignItems = Align.Center;
            tile.style.justifyContent = Justify.Center;
            tile.style.marginRight = 5;

            Label countLabel = FateUi.Text(count.ToString(), width * 0.36f, FateUi.Bone);
            countLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            tile.Add(countLabel);
            Label caption = FateUi.Text("cards", 11, FateUi.BoneDim);
            tile.Add(caption);

            if (onClick != null)
            {
                FateUi.MakeClickable(tile, onClick);
            }

            return tile;
        }

        public static Color ForceColor(FateContentCatalog catalog, MetadataEntry force)
        {
            if (force == null)
            {
                return new Color(0.5f, 0.47f, 0.44f);
            }

            return force.GetColor(catalog.ForceColorField);
        }
    }
}
