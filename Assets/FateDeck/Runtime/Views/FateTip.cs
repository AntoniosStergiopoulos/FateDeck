using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// Runtime hover tooltips (UI Toolkit only shows <c>tooltip</c> in the editor). Bind any
    /// element to a text provider; after a short hover the tip appears near it, clamped to the
    /// screen, and hides on leave, click, or when the element is rebuilt away.
    /// </summary>
    public static class FateTip
    {
        private const float Delay = 0.35f;
        private const float PanelWidth = 340f;

        private static VisualElement _root;
        private static VisualElement _panel;
        private static Label _label;
        private static VisualElement _target;
        private static Func<string> _provider;
        private static float _timer;
        private static bool _showing;

        /// <summary>Creates the tip panel inside a topmost, click-transparent host layer.</summary>
        public static void Install(VisualElement root, VisualElement host)
        {
            _root = root;
            _panel = new VisualElement();
            _panel.pickingMode = PickingMode.Ignore;
            _panel.style.position = Position.Absolute;
            _panel.style.maxWidth = PanelWidth;
            _panel.style.backgroundColor = new Color(0.055f, 0.05f, 0.045f, 0.97f);
            FateUi.SetBorder(_panel, new Color(0.831f, 0.678f, 0.220f, 0.45f), 1, 5);
            FateUi.Pad(_panel, 8);
            _panel.style.paddingLeft = 10;
            _panel.style.paddingRight = 10;
            _panel.style.display = DisplayStyle.None;

            _label = FateUi.Text(string.Empty, 13, FateUi.Bone);
            _label.pickingMode = PickingMode.Ignore;
            _panel.Add(_label);
            host.Add(_panel);
            _target = null;
            _showing = false;
        }

        public static void Bind(VisualElement element, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Bind(element, () => text);
            }
        }

        public static void Bind(VisualElement element, Func<string> provider)
        {
            if (element == null || provider == null)
            {
                return;
            }

            element.RegisterCallback<PointerEnterEvent>(_ =>
            {
                _target = element;
                _provider = provider;
                _timer = 0f;
                if (_showing)
                {
                    Show();
                }
            });
            element.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                if (_target == element)
                {
                    Hide();
                }
            });
            element.RegisterCallback<ClickEvent>(_ => Hide());
        }

        /// <summary>Pumped once per frame by the table view.</summary>
        public static void Update(float deltaTime)
        {
            if (_target == null)
            {
                return;
            }

            if (!IsAttached(_target))
            {
                Hide();
                return;
            }

            if (_showing)
            {
                return;
            }

            _timer += deltaTime;
            if (_timer >= Delay)
            {
                Show();
            }
        }

        public static void Clear()
        {
            Hide();
        }

        private static bool IsAttached(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current == _root)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Show()
        {
            string text = _provider?.Invoke();
            if (string.IsNullOrEmpty(text) || _panel == null || _target == null)
            {
                Hide();
                return;
            }

            _label.text = text;
            Rect bound = _target.worldBound;
            Rect screen = _root != null ? _root.worldBound : default;

            float x = bound.x;
            if (screen.width > 0 && x + PanelWidth > screen.width - 8f)
            {
                x = Mathf.Max(8f, screen.width - PanelWidth - 8f);
            }

            float y = bound.yMax + 8f;
            if (screen.height > 0 && y > screen.height - 150f)
            {
                y = Mathf.Max(8f, bound.y - 110f);
            }

            _panel.style.left = x;
            _panel.style.top = y;
            _panel.style.display = DisplayStyle.Flex;
            _showing = true;
        }

        private static void Hide()
        {
            _target = null;
            _provider = null;
            _timer = 0f;
            _showing = false;
            if (_panel != null)
            {
                _panel.style.display = DisplayStyle.None;
            }
        }
    }
}
