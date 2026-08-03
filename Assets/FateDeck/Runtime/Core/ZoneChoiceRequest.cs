namespace FateDeck.Runtime.Core
{
    public enum ZoneChoiceKind
    {
        /// <summary>Pick wound cards to shuffle back into the draw pile.</summary>
        HealWounds,

        /// <summary>Pick a discard-pile card to exile forever.</summary>
        ExileFromDiscard,

        /// <summary>Pick a discard-pile card to stack on top of the draw pile.</summary>
        StackFromDiscard
    }

    /// <summary>An interactive pick the view resolves by calling back into the session's deck verbs.</summary>
    public sealed class ZoneChoiceRequest
    {
        public ZoneChoiceRequest(ZoneChoiceKind kind, int count)
        {
            Kind = kind;
            Count = count;
        }

        public ZoneChoiceKind Kind { get; }

        public int Count { get; }
    }
}
