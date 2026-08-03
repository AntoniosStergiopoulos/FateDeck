using UnityEngine;

namespace FateDeck.Runtime.Core
{
    /// <summary>Central tunables of the fate engine. One asset; heroes and stakes modify at run start.</summary>
    public class FateRulesDefinition : ScriptableObject
    {
        [Min(0)] public double StrikeBaseForce = 3;
        [Min(0)] public double GuardBaseForce = 2;
        [Min(0)] public int FleeMill = 1;
        [Min(1)] public int PocketSlots = 2;
        [Min(0)] public int ReshuffleTax = 1;
        [Min(1)] public int EchoMaxFlipsPerAction = 3;
        [Min(0)] public int MaxCharms = 3;
        [Min(0)] public int StartingGold = 10;
        [Min(0)] public double ChestBaseGold = 8;
        [Min(0)] public double LockedChestBaseGold = 14;
        [Min(0)] public int KeyPrice = 6;
        [Min(0)] public int DoomExileShrinePrice = 15;
        [Min(0)] public int SurgeryBasePrice = 25;
        [Min(0)] public int SurgeryPriceStep = 10;
        [Min(0)] public int TonicPrice = 15;
        [Min(0)] public int TonicHeal = 3;
        [Min(0)] public int RestHeal = 5;
        [Min(0)] public int DoomCleansePrice = 20;
        [Min(1)] public int TrackSteps = 9;
        [Min(1)] public int DoorsPerStep = 3;
    }
}
