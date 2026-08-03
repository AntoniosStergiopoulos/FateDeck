namespace FateDeck.Runtime.Core
{
    /// <summary>Where the current action sits in the interactive fate pipeline.</summary>
    public enum FateResolutionPhase
    {
        Idle,

        /// <summary>A flip is imminent; a pocketed card may replace it entirely.</summary>
        AwaitPreFlip,

        /// <summary>Two cards were revealed (Opening Hand / Loaded Coin); the player picks one.</summary>
        AwaitDoubleDrawChoice,

        /// <summary>A card was revealed on the player's own action; it may be banked instead of applied.</summary>
        AwaitBank
    }
}
