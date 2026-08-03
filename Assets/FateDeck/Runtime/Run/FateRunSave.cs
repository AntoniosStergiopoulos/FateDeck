using System;
using System.Collections.Generic;
using System.IO;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.Game.Zones;
using AStergio.OmniCard.Runtime.Cards.Instances;
using AStergio.OmniCard.Runtime.Cards.Persistence;
using FateDeck.Runtime.Core;
using UnityEngine;

namespace FateDeck.Runtime.Run
{
    /// <summary>
    /// Between-room run persistence: JSON keyed by OmniCard stable ids, with per-instance state
    /// captured through the package's <see cref="CardInstanceSerializer"/> DTOs.
    /// Saved every time the doors are dealt; a mid-fight quit resumes at the fight's door.
    /// </summary>
    public static class FateRunSave
    {
        [Serializable]
        public class SaveData
        {
            public int ResumeSeed;
            public int Step;
            public int Biome;
            public int Gold;
            public int Keys;
            public int PocketSlots;
            public string HeroId;
            public bool EliteOffered;
            public bool ForgeOffered;
            public int ReshuffleCount;
            public int TaxModifier;
            public int ExtraTaxNextReshuffle;
            public int DoomFlips;
            public int TotalFlips;
            public List<CardInstanceState> DrawPile = new List<CardInstanceState>();
            public List<CardInstanceState> DiscardPile = new List<CardInstanceState>();
            public List<CardInstanceState> WoundRow = new List<CardInstanceState>();
            public List<CardInstanceState> PocketCards = new List<CardInstanceState>();
            public List<CardInstanceState> ExilePile = new List<CardInstanceState>();
            public List<string> RelicIds = new List<string>();
            public List<string> CharmIds = new List<string>();
        }

        public static string SavePath => Path.Combine(Application.persistentDataPath, "fatedeck-run.json");

        public static bool Exists => File.Exists(SavePath);

        public static void Save(RunController run)
        {
            if (run?.Session == null || run.Session.IsPlayerDead || run.Session.CurrentAction != null)
            {
                return;
            }

            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(Capture(run)));
            }
            catch (IOException error)
            {
                Debug.LogWarning($"[FateDeck] Could not save the run: {error.Message}");
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
            }
            catch (IOException error)
            {
                Debug.LogWarning($"[FateDeck] Could not delete the save: {error.Message}");
            }
        }

        public static SaveData Capture(RunController run)
        {
            FateSession session = run.Session;
            var data = new SaveData
            {
                ResumeSeed = session.Rng.Next(1, int.MaxValue),
                Step = run.Step,
                Biome = run.Biome,
                Gold = session.Gold,
                Keys = session.Keys,
                PocketSlots = session.PocketSlots,
                HeroId = session.Hero?.Definition.Id.Value,
                EliteOffered = run.EliteOffered,
                ForgeOffered = run.ForgeOffered,
                ReshuffleCount = session.Deck.ReshuffleCount,
                TaxModifier = session.Deck.TaxModifier,
                ExtraTaxNextReshuffle = session.Deck.ExtraTaxNextReshuffle,
                DoomFlips = session.DoomFlipsThisRun,
                TotalFlips = session.TotalFlipsThisRun
            };

            CaptureZone(session.Deck.Draw, data.DrawPile);
            CaptureZone(session.Deck.Discard, data.DiscardPile);
            CaptureZone(session.Deck.Wound, data.WoundRow);
            CaptureZone(session.Deck.Pocket, data.PocketCards);
            CaptureZone(session.Deck.Exile, data.ExilePile);

            foreach (CardInstance relic in session.RelicZone.Cards)
            {
                data.RelicIds.Add(relic.Definition.Id.Value);
            }

            foreach (CardInstance charm in session.CharmZone.Cards)
            {
                data.CharmIds.Add(charm.Definition.Id.Value);
            }

            return data;
        }

        private static void CaptureZone(CardZone zone, List<CardInstanceState> into)
        {
            foreach (CardInstance card in zone.Cards)
            {
                into.Add(CardInstanceSerializer.Capture(card));
            }
        }

        public static bool TryLoad(out SaveData data)
        {
            data = null;
            try
            {
                if (!File.Exists(SavePath))
                {
                    return false;
                }

                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                return data != null && data.Step > 0;
            }
            catch (Exception error)
            {
                Debug.LogWarning($"[FateDeck] Could not load the save: {error.Message}");
                Delete();
                return false;
            }
        }

        /// <summary>Rebuilds a session's fate zones from a save, resolving cards by stable id.</summary>
        public static void RestoreZones(FateSession session, SaveData data, Func<string, CardDefinition> resolve)
        {
            RestoreZone(session.Deck.Draw, data.DrawPile, resolve);
            RestoreZone(session.Deck.Discard, data.DiscardPile, resolve);
            RestoreZone(session.Deck.Wound, data.WoundRow, resolve);
            RestoreZone(session.Deck.Pocket, data.PocketCards, resolve);
            RestoreZone(session.Deck.Exile, data.ExilePile, resolve);
        }

        private static void RestoreZone(CardZone zone, List<CardInstanceState> states,
            Func<string, CardDefinition> resolve)
        {
            foreach (CardInstanceState state in states)
            {
                CardDefinition definition = resolve(state.SourceCardId);
                if (definition != null)
                {
                    zone.Add(CardInstanceSerializer.Restore(definition, state));
                }
            }
        }
    }
}
