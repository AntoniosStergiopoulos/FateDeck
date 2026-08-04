namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// Implemented by law atoms whose meaning changes with perspective. The text the player
    /// reads is generated from the same object that executes, per context and in second
    /// person - "YOU suffer 2 Burn" on enemy actions, "your target suffers 2 Burn" on yours.
    /// </summary>
    public interface IContextDescribed
    {
        string DescribeFor(LawContext context);
    }
}
