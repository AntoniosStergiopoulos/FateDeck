using System;
using AStergio.OmniCard.Runtime.Cards.Fields.Core;

namespace FateDeck.Runtime.Combat
{
    /// <summary>
    /// A designer-authored looping script of enemy actions - a one-class OmniCard field-kind
    /// extension, auto-discovered by the package's type pickers.
    /// </summary>
    [Serializable]
    public class EnemyPatternFieldKind : CardFieldKind
    {
        public override Type ValueType => typeof(EnemyPatternFieldValue);
    }
}
