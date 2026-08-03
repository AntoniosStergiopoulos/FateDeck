namespace FateDeck.Runtime.Core
{
    /// <summary>
    /// Implemented by law effect atoms whose outcome the Odds Panel can compute ahead of time.
    /// Keeps "No Surprise Math" honest: the panel derives outcomes from the same data the laws use.
    /// </summary>
    public interface IActionLawPreview
    {
        /// <summary>Returns the action force after this atom, given the force before it.</summary>
        double PreviewForce(double force);

        /// <summary>A short side-effect annotation for the odds row ("+3g", "Burn 2"), or null.</summary>
        string PreviewNote { get; }
    }
}
