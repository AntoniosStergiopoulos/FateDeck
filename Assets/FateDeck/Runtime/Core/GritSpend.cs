namespace FateDeck.Runtime.Core
{
    /// <summary>The three ways banked Grit can be spent between actions.</summary>
    public enum GritSpend
    {
        /// <summary>Scry 2 and reorder the top of the deck.</summary>
        Foresight,

        /// <summary>+2 Force on the next declared action.</summary>
        Momentum,

        /// <summary>Return 1 escrowed card (wound) to the deck.</summary>
        Mend
    }
}
