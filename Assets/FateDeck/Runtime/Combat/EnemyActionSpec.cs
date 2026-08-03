using System;
using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Effects.Base;
using UnityEngine;

namespace FateDeck.Runtime.Combat
{
    public enum EnemyActionKind
    {
        Attack,
        Brace,
        Special
    }

    /// <summary>
    /// One step of an enemy's visible, looping pattern (GDD <c>EnemyAction</c>):
    /// a name, a kind, a force, whether it flips fate, and optional special effects.
    /// </summary>
    [Serializable]
    public class EnemyActionSpec
    {
        public string Name = "Attack";
        public EnemyActionKind Kind = EnemyActionKind.Attack;
        [Min(0)] public double Force = 2;
        public bool FlipsFate = true;

        [SerializeReference]
        public List<CardEffect> Effects = new List<CardEffect>();

        public string IntentLabel()
        {
            switch (Kind)
            {
                case EnemyActionKind.Attack: return $"{Name} {Force:0.##}";
                case EnemyActionKind.Brace: return $"{Name} {Force:0.##}";
                default: return Name;
            }
        }
    }
}
