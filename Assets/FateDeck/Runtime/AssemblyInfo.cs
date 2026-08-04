using System.Runtime.CompilerServices;

// The engine's restore/upgrade seams stay internal for gameplay code but remain
// exercisable by the headless test suite.
[assembly: InternalsVisibleTo("FateDeck.Tests.Editor")]
