using UnityEngine;
using UnityEngine.UIElements;

namespace FateDeck.Runtime.Views
{
    /// <summary>
    /// The table's scrollable event log: every flip, mill, purchase and bark lands here in
    /// plain sentences so the run always reads back like a story.
    /// </summary>
    public sealed class GameLogPanel
    {
        private const int MaxEntries = 250;

        private readonly ScrollView _scroll;

        public GameLogPanel()
        {
            Root = FateUi.MakePanel("EVENT LOG");
            Root.style.flexGrow = 1;
            Root.style.minHeight = 120;

            _scroll = new ScrollView(ScrollViewMode.Vertical);
            _scroll.style.flexGrow = 1;
            Root.Add(_scroll);
        }

        public VisualElement Root { get; }

        public void Append(string message, Color? color = null, bool bold = false)
        {
            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            Label line = FateUi.Text(message, 13, color ?? FateUi.Bone);
            line.style.marginBottom = 3;
            if (bold)
            {
                line.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            _scroll.Add(line);
            while (_scroll.childCount > MaxEntries)
            {
                _scroll.RemoveAt(0);
            }

            ScrollToBottom();
        }

        public void Divider(string caption)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 6;
            row.style.marginBottom = 4;

            var line = new VisualElement();
            line.style.flexGrow = 1;
            line.style.height = 1;
            line.style.backgroundColor = FateUi.Line;
            row.Add(line);

            if (!string.IsNullOrEmpty(caption))
            {
                Label label = FateUi.Text($"  {caption}  ", 12, FateUi.GoldLeaf);
                label.style.whiteSpace = WhiteSpace.NoWrap;
                row.Add(label);
                var lineRight = new VisualElement();
                lineRight.style.flexGrow = 1;
                lineRight.style.height = 1;
                lineRight.style.backgroundColor = FateUi.Line;
                row.Add(lineRight);
            }

            _scroll.Add(row);
            ScrollToBottom();
        }

        public void Clear()
        {
            _scroll.Clear();
        }

        private void ScrollToBottom()
        {
            _scroll.schedule.Execute(() =>
            {
                _scroll.scrollOffset = new Vector2(0f, float.MaxValue);
            });
        }
    }
}
