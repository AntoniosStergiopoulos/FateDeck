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
        [Min(0)] public int StartingGold = 12;
        [Min(0)] public double ChestBaseGold = 8;
        [Min(0)] public double LockedChestBaseGold = 14;
        [Min(0)] public int KeyPrice = 6;
        [Min(0)] public int DoomExileShrinePrice = 15;
        [Min(0)] public int SurgeryBasePrice = 25;
        [Min(0)] public int SurgeryPriceStep = 10;
        [Min(0)] public int TonicPrice = 15;
        [Min(0)] public int TonicHeal = 4;
        [Min(0)] public int RestHeal = 5;
        [Min(0)] public int DoomCleansePrice = 20;
        [Min(1)] public int TrackSteps = 9;
        [Min(1)] public int DoorsPerStep = 3;

        [Tooltip("A single Strike hit of at least this much shakes one card loose from a Mantle.")]
        [Min(0)] public double MantleSpillDamage = 5;

        [Tooltip("Against 2+ enemies your Guard also strikes your target for this much (0 disables).")]
        [Min(0)] public double OutnumberedGuardDamage = 2;

        [Tooltip("Extra gold per enemy beyond the first when a fight is won (the squad purse).")]
        [Min(0)] public int SquadPursePerExtraEnemy = 2;

        [Tooltip("Grit gained per Debt flip; 0 disables the Grit system.")]
        [Min(0)] public int GritPerDebtFlip = 1;

        [Tooltip("Grit cost of one spend (scry, +2 next action, or mend 1).")]
        [Min(1)] public int GritSpendCost = 3;

        [Tooltip("Maximum Grit the player can bank.")]
        [Min(1)] public int GritMax = 6;
    }
}
