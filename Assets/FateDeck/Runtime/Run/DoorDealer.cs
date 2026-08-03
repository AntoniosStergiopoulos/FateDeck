using System;
using System.Collections.Generic;

namespace FateDeck.Runtime.Run
{
    /// <summary>
    /// Deals the face-up doors for a track step: GDD weights (Fight 55, Chest 15, Event 15,
    /// Shrine 10, Mini-shop 5), exactly one elite among steps 5-7, no duplicate rooms per step.
    /// Pure and seeded - the run's RNG stream decides everything.
    /// </summary>
    public static class DoorDealer
    {
        public const int FightWeight = 55;
        public const int ChestWeight = 15;
        public const int EventWeight = 15;
        public const int ShrineWeight = 10;
        public const int MiniShopWeight = 5;

        /// <summary>
        /// Deals doors for a step. One room from <paramref name="elitePool"/> is injected once in
        /// the 5-7 window when no elite has been offered; <paramref name="forcedRoom"/> pins a
        /// scripted door.
        /// </summary>
        public static List<RoomDefinition> Deal(IReadOnlyList<RoomDefinition> pool,
            IReadOnlyList<FightRoomDefinition> elitePool, int step, int doorCount, Random rng,
            ref bool eliteOffered, RoomDefinition forcedRoom = null)
        {
            var doors = new List<RoomDefinition>();
            if (forcedRoom != null)
            {
                doors.Add(forcedRoom);
            }

            bool includeElite = elitePool != null && elitePool.Count > 0 && !eliteOffered
                && ShouldOfferElite(step, rng);
            if (includeElite && doors.Count < doorCount)
            {
                doors.Add(elitePool[rng.Next(elitePool.Count)]);
                eliteOffered = true;
            }

            int guard = 0;
            while (doors.Count < doorCount && guard++ < 64)
            {
                RoomDefinition room = PickWeighted(pool, rng);
                if (room != null && !doors.Contains(room))
                {
                    doors.Add(room);
                }
            }

            Shuffle(doors, rng);
            return doors;
        }

        private static bool ShouldOfferElite(int step, Random rng)
        {
            if (step < 5 || step > 7)
            {
                return false;
            }

            if (step == 7)
            {
                return true;
            }

            int remaining = 7 - step + 1;
            return rng.Next(remaining) == 0;
        }

        public static List<RoomDefinition> OfKind<T>(IReadOnlyList<RoomDefinition> pool) where T : RoomDefinition
        {
            var result = new List<RoomDefinition>();
            foreach (RoomDefinition room in pool)
            {
                if (room is T fight && !(room is FightRoomDefinition f && (f.IsElite || f.IsBoss)))
                {
                    result.Add(fight);
                }
            }

            return result;
        }

        private static RoomDefinition PickWeighted(IReadOnlyList<RoomDefinition> pool, Random rng)
        {
            int roll = rng.Next(FightWeight + ChestWeight + EventWeight + ShrineWeight + MiniShopWeight);
            if (roll < FightWeight)
            {
                return PickRandom(OfKind<FightRoomDefinition>(pool), rng);
            }

            roll -= FightWeight;
            if (roll < ChestWeight)
            {
                return PickRandom(OfKind<ChestRoomDefinition>(pool), rng);
            }

            roll -= ChestWeight;
            if (roll < EventWeight)
            {
                return PickRandom(OfKind<EventRoomDefinition>(pool), rng);
            }

            roll -= EventWeight;
            if (roll < ShrineWeight)
            {
                return PickRandom(OfKind<ShrineRoomDefinition>(pool), rng);
            }

            return PickRandom(OfKind<ShopRoomDefinition>(pool), rng);
        }

        private static RoomDefinition PickRandom(List<RoomDefinition> options, Random rng)
        {
            if (options.Count == 0)
            {
                return null;
            }

            return options[rng.Next(options.Count)];
        }

        private static void Shuffle(List<RoomDefinition> doors, Random rng)
        {
            for (int i = doors.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (doors[i], doors[j]) = (doors[j], doors[i]);
            }
        }
    }
}
