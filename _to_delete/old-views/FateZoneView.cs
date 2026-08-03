using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Game.Views;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Layout;
using AStergio.OmniCard.Runtime.Cards.Views.Building;
using AStergio.OmniCard.Runtime.Cards.Views.Core;
using UnityEngine;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// Renders one fate zone from a plain <see cref="CardZone"/> - OmniCard's ZoneView requires a
    /// GameSession, so this thin sibling reuses the package's arrangements, world card builder and
    /// click handling against the Fate Deck session instead.
    /// </summary>
    public sealed class FateZoneView : MonoBehaviour
    {
        private readonly List<GameObject> _spawned = new List<GameObject>();
        private Func<CardZone> _zoneProvider;
        private bool _dirty;

        public CardLayout Layout;

        [SerializeReference]
        public ZoneArrangement Arrangement = new RowArrangement();

        public bool FaceDown;
        public bool ShowCount;
        public float PixelsPerUnit = 420f;
        public bool Clickable = true;
        public Color BackColor = new Color(0.13f, 0.11f, 0.16f);
        public float CardScale = 1f;

        /// <summary>Invoked with the clicked card instance when <see cref="Clickable"/>.</summary>
        public Action<CardInstance> CardClicked;

        /// <summary>Per-card face tint (force colors); null for the default parchment.</summary>
        public Func<CardInstance, Color?> FaceTint;

        /// <summary>Extra per-card dressing after a face is built (intents, status pips).</summary>
        public Action<CardInstance, GameObject> Decorate;

        private TextMesh _countLabel;

        public CardZone Zone => _zoneProvider?.Invoke();

        public void Bind(Func<CardZone> zoneProvider)
        {
            _zoneProvider = zoneProvider;
            MarkDirty();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        private void LateUpdate()
        {
            if (_dirty)
            {
                _dirty = false;
                Rebuild();
            }
        }

        public void Rebuild()
        {
            foreach (GameObject spawned in _spawned)
            {
                if (spawned != null)
                {
                    Destroy(spawned);
                }
            }

            _spawned.Clear();
            CardZone zone = Zone;
            int count = zone?.Count ?? 0;

            for (int i = 0; i < count; i++)
            {
                if (Arrangement != null && !Arrangement.IsVisible(i, count))
                {
                    continue;
                }

                CardInstance card = zone.Cards[i];
                ZonePose pose = Arrangement?.GetPose(i, count) ?? new ZonePose(Vector3.zero, 0f, i);
                GameObject built = FaceDown ? BuildBack(card, pose) : BuildFace(card, pose);
                if (built == null)
                {
                    continue;
                }

                built.transform.localPosition = pose.LocalPosition;
                built.transform.localRotation = Quaternion.Euler(0f, 0f, pose.RotationZ);
                _spawned.Add(built);
            }

            RefreshCount(count);
        }

        private GameObject BuildFace(CardInstance card, ZonePose pose)
        {
            if (Layout == null)
            {
                return null;
            }

            int orderBase = pose.Order * 40 + 5;
            CardView view = WorldCardViewBuilder.Build(Layout, card.Definition, transform, PixelsPerUnit, orderBase);
            if (view == null)
            {
                return null;
            }

            view.Bind(card);
            view.transform.localScale = Vector3.one * CardScale;
            AddBackground(view.gameObject, FaceTint?.Invoke(card) ?? new Color(0.16f, 0.14f, 0.13f), orderBase - 1);
            if (Clickable)
            {
                AddClick(view.gameObject, card);
            }

            Decorate?.Invoke(card, view.gameObject);
            return view.gameObject;
        }

        private GameObject BuildBack(CardInstance card, ZonePose pose)
        {
            var back = new GameObject("Card Back");
            back.transform.SetParent(transform, false);
            SpriteRenderer renderer = back.AddComponent<SpriteRenderer>();
            renderer.sprite = UiKit.WhiteSprite;
            renderer.color = BackColor;
            renderer.sortingOrder = pose.Order * 40;
            Vector2 size = CardSize() * CardScale;
            back.transform.localScale = new Vector3(size.x, size.y, 1f);
            if (Clickable)
            {
                AddClick(back, card, unscaled: true);
            }

            return back;
        }

        private void AddBackground(GameObject cardObject, Color color, int sortingOrder)
        {
            Vector2 size = CardSize();
            UiKit.Quad(cardObject.transform, new Vector3(0f, 0f, 0.02f), size, color, sortingOrder, "Face");
        }

        private Vector2 CardSize()
        {
            Vector2 design = Layout != null ? Layout.DesignSize : new Vector2(640f, 900f);
            return new Vector2(design.x / PixelsPerUnit, design.y / PixelsPerUnit);
        }

        private void AddClick(GameObject cardObject, CardInstance card, bool unscaled = false)
        {
            BoxCollider2D collider = cardObject.AddComponent<BoxCollider2D>();
            collider.size = unscaled ? Vector2.one : CardSize();
            ZoneCardClickHandler handler = cardObject.AddComponent<ZoneCardClickHandler>();
            handler.Card = card;
            handler.Clicked = clicked => CardClicked?.Invoke(clicked);
        }

        private void RefreshCount(int count)
        {
            if (!ShowCount)
            {
                if (_countLabel != null)
                {
                    _countLabel.text = string.Empty;
                }

                return;
            }

            if (_countLabel == null)
            {
                _countLabel = UiKit.Label(transform, new Vector3(0f, -CardSize().y * CardScale * 0.5f - 0.32f, 0f),
                    string.Empty, 30, UiKit.Dim);
                _countLabel.gameObject.name = "Count";
            }

            _countLabel.text = count.ToString();
        }
    }
}
