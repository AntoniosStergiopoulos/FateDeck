using AStergio.OmniCard.Runtime.Cards.Layout;
using UnityEngine;

namespace FateDeck.Editor
{
    public static partial class FateDeckContentGenerator
    {
        private sealed class Layouts
        {
            public CardLayout FateCard;
            public CardLayout Enemy;
            public CardLayout Item;
        }

        private static Layouts CreateLayouts(Fields fields)
        {
            var layouts = new Layouts();

            layouts.FateCard = GetOrCreate<CardLayout>("Fate Card Layout", layout =>
            {
                layout.Schema = fields.FateCardSchema;
                layout.DesignSize = new Vector2(640f, 900f);
                layout.BackgroundColor = new Color(0.09f, 0.08f, 0.07f);
                layout.Elements.Add(new NameLayoutElement
                {
                    Rect = new Rect(40f, 40f, 560f, 110f),
                    Text = new TextStyle { FontSize = 64f, Alignment = TextAnchor.MiddleCenter, Color = Color.white }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Force,
                    EntryField = fields.ForceGlyph,
                    Rect = new Rect(120f, 240f, 400f, 360f),
                    Text = new TextStyle { FontSize = 230f, Alignment = TextAnchor.MiddleCenter, Color = Color.white }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Description,
                    Rect = new Rect(50f, 650f, 540f, 210f),
                    Text = new TextStyle { FontSize = 34f, Alignment = TextAnchor.MiddleCenter, Color = new Color(0.95f, 0.93f, 0.88f) }
                });
            }, "Layouts");

            layouts.Enemy = GetOrCreate<CardLayout>("Enemy Layout", layout =>
            {
                layout.Schema = fields.EnemySchema;
                layout.DesignSize = new Vector2(760f, 960f);
                layout.BackgroundColor = new Color(0.08f, 0.07f, 0.09f);
                layout.Elements.Add(new NameLayoutElement
                {
                    Rect = new Rect(30f, 30f, 700f, 100f),
                    Text = new TextStyle { FontSize = 56f, Alignment = TextAnchor.MiddleCenter, Color = Color.white }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Gimmick,
                    Rect = new Rect(50f, 190f, 660f, 300f),
                    Text = new TextStyle { FontSize = 30f, Alignment = TextAnchor.MiddleCenter, Color = new Color(0.85f, 0.72f, 0.55f) }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Pattern,
                    Rect = new Rect(40f, 540f, 680f, 130f),
                    Text = new TextStyle { FontSize = 32f, Alignment = TextAnchor.MiddleCenter, Color = new Color(0.7f, 0.75f, 0.85f) }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Hp,
                    Rect = new Rect(60f, 760f, 300f, 150f),
                    Text = new TextStyle { FontSize = 96f, Alignment = TextAnchor.MiddleLeft, Color = new Color(0.95f, 0.45f, 0.4f) }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Bounty,
                    Rect = new Rect(420f, 780f, 280f, 120f),
                    Text = new TextStyle { FontSize = 54f, Alignment = TextAnchor.MiddleRight, Color = new Color(0.79f, 0.64f, 0.15f) }
                });
            }, "Layouts");

            layouts.Item = GetOrCreate<CardLayout>("Item Layout", layout =>
            {
                layout.Schema = fields.CharmSchema;
                layout.DesignSize = new Vector2(640f, 900f);
                layout.BackgroundColor = new Color(0.10f, 0.09f, 0.11f);
                layout.Elements.Add(new NameLayoutElement
                {
                    Rect = new Rect(30f, 60f, 580f, 140f),
                    Text = new TextStyle { FontSize = 60f, Alignment = TextAnchor.MiddleCenter, Color = new Color(0.79f, 0.64f, 0.15f) }
                });
                layout.Elements.Add(new FieldLayoutElement
                {
                    Field = fields.Description,
                    Rect = new Rect(50f, 300f, 540f, 480f),
                    Text = new TextStyle { FontSize = 40f, Alignment = TextAnchor.MiddleCenter, Color = new Color(0.95f, 0.93f, 0.88f) }
                });
            }, "Layouts");

            return layouts;
        }
    }
}
