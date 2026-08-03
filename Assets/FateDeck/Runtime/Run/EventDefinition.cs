using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using UnityEngine;

namespace FateDeck.Runtime.Run
{
    /// <summary>
    /// One force-keyed outcome of a ritual flip: the Wishing Well pays 25g on Fortune,
    /// bites on Doom, and so on. Any random event resolves by flipping the one deck.
    /// </summary>
    [Serializable]
    public class RitualOutcome
    {
        public MetadataEntry Force;

        [TextArea]
        public string ResultText;

        [SerializeReference]
        public List<CardEffect> Effects = new List<CardEffect>();

        /// <summary>The event closes after this outcome (the well bites, the shade leaves).</summary>
        public bool ClosesEvent;
    }

    /// <summary>A choice a "?" event offers, with immediate effects and/or a ritual fate flip.</summary>
    [Serializable]
    public class EventOption
    {
        public string Label = "Continue";

        [Min(0)]
        public int GoldCost;

        [SerializeReference]
        public List<CardEffect> Effects = new List<CardEffect>();

        [TextArea]
        public string ResultText;

        /// <summary>When true, taking this option flips fate as a Ritual and RitualOutcomes decide.</summary>
        public bool FlipsFate;

        public List<RitualOutcome> RitualOutcomes = new List<RitualOutcome>();

        /// <summary>The option can be taken again (the Wishing Well) until an outcome closes the event.</summary>
        public bool Repeatable;

        /// <summary>Taking this option ends the event (unless Repeatable).</summary>
        public bool ClosesEvent = true;
    }

    /// <summary>A narrative "?" encounter: intro text and options, all fate drawn from the one deck.</summary>
    public class EventDefinition : CardAsset
    {
        [TextArea(3, 8)]
        public string Intro;

        public List<EventOption> Options = new List<EventOption>();
    }
}
