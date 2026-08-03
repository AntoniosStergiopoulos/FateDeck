using System.Collections.Generic;
using AStergio.OmniCard.Runtime.Cards.Data;
using AStergio.OmniCard.Runtime.Cards.MetaData;
using FateDeck.Runtime.Core;

namespace FateDeck.Runtime.Run
{
    public enum ShopItemKind
    {
        Charm,
        Relic,
        FateCard,
        Key,
        Tonic,
        Surgery
    }

    /// <summary>One purchasable line of a shop's stock.</summary>
    public sealed class ShopItem
    {
        public ShopItemKind Kind;
        public CardDefinition Card;
        public int Price;
        public bool Sold;

        public string Label()
        {
            switch (Kind)
            {
                case ShopItemKind.Key: return $"Key (opens one locked chest) - {Price}g";
                case ShopItemKind.Tonic: return $"Tonic: heal 3 - {Price}g";
                case ShopItemKind.Surgery: return $"Surgery: exile any card - {Price}g";
                default: return $"{Card?.name} - {Price}g";
            }
        }
    }

    /// <summary>
    /// Rolls shop stock from the catalog pools (GDD 9.2) and executes purchases.
    /// Bought fate cards join the discard pile - they enter your luck on the next shuffle.
    /// </summary>
    public sealed class ShopService
    {
        private readonly FateSession _session;

        public ShopService(FateSession session, bool miniShop)
        {
            _session = session;
            RollStock(miniShop);
        }

        public List<ShopItem> Stock { get; } = new List<ShopItem>();

        public int SurgeriesBought { get; set; }

        private void RollStock(bool miniShop)
        {
            FateContentCatalog catalog = _session.Catalog;
            System.Random rng = _session.Rng;

            int charmCount = miniShop ? 1 : 2;
            for (int i = 0; i < charmCount && catalog.CharmPool.Count > 0; i++)
            {
                CardDefinition charm = catalog.CharmPool[rng.Next(catalog.CharmPool.Count)];
                Stock.Add(new ShopItem { Kind = ShopItemKind.Charm, Card = charm, Price = 8 + rng.Next(7) });
            }

            if (!miniShop)
            {
                var relicPool = new List<CardDefinition>(catalog.RelicPool);
                foreach (var owned in _session.RelicZone.Cards)
                {
                    relicPool.Remove(owned.Definition);
                }

                if (relicPool.Count > 0)
                {
                    CardDefinition relic = relicPool[rng.Next(relicPool.Count)];
                    Stock.Add(new ShopItem { Kind = ShopItemKind.Relic, Card = relic, Price = 45 + rng.Next(21) });
                }
            }

            int cardCount = miniShop ? 2 : 3;
            MetadataEntry[] basics = { catalog.Iron, catalog.Flame, catalog.Decay, catalog.Fortune };
            for (int i = 0; i < cardCount; i++)
            {
                MetadataEntry force = basics[rng.Next(basics.Length)];
                bool upgraded = rng.Next(4) == 0;
                MetadataEntry stocked = upgraded ? catalog.PlusVersionOf(force) : force;
                CardDefinition card = catalog.FateCardFor(stocked ?? force);
                if (card != null)
                {
                    Stock.Add(new ShopItem
                    {
                        Kind = ShopItemKind.FateCard,
                        Card = card,
                        Price = upgraded ? 20 : 12
                    });
                }
            }

            if (!miniShop)
            {
                Stock.Add(new ShopItem { Kind = ShopItemKind.Key, Price = _session.Rules.KeyPrice });
                Stock.Add(new ShopItem { Kind = ShopItemKind.Tonic, Price = _session.Rules.TonicPrice });
                Stock.Add(new ShopItem { Kind = ShopItemKind.Surgery, Price = SurgeryPrice() });
            }
        }

        public int SurgeryPrice()
        {
            return _session.Rules.SurgeryBasePrice + SurgeriesBought * _session.Rules.SurgeryPriceStep;
        }

        /// <summary>Doom costs double at any paid exile service.</summary>
        public int SurgeryPriceFor(MetadataEntry force)
        {
            int price = SurgeryPrice();
            return force == _session.Catalog.Doom ? price * 2 : price;
        }

        public bool Buy(ShopItem item)
        {
            if (item == null || item.Sold || _session.Gold < item.Price)
            {
                return false;
            }

            switch (item.Kind)
            {
                case ShopItemKind.Charm:
                    if (!_session.AcquireCharm(item.Card))
                    {
                        return false;
                    }

                    break;

                case ShopItemKind.Relic:
                    _session.AcquireRelic(item.Card);
                    break;

                case ShopItemKind.FateCard:
                    _session.Deck.AddCard(item.Card, _session.Deck.Discard, randomPosition: false);
                    break;

                case ShopItemKind.Key:
                    _session.AddKeys(1);
                    break;

                case ShopItemKind.Tonic:
                    _session.Deck.HealWounds(_session.Rules.TonicHeal);
                    break;

                case ShopItemKind.Surgery:
                    return false;
            }

            _session.AddGold(-item.Price);
            item.Sold = item.Kind != ShopItemKind.Key && item.Kind != ShopItemKind.Tonic;
            return true;
        }

        /// <summary>Pays for and performs a surgery exile of the given card from a fate zone.</summary>
        public bool BuySurgery(AStergio.OmniCard.Runtime.Cards.Game.Zones.CardZone zone,
            AStergio.OmniCard.Runtime.Cards.Instances.CardInstance card)
        {
            MetadataEntry force = _session.Catalog.ForceOf(card);
            int price = SurgeryPriceFor(force);
            if (_session.Gold < price || !_session.Deck.ExileCard(zone, card))
            {
                return false;
            }

            _session.AddGold(-price);
            SurgeriesBought++;
            foreach (ShopItem item in Stock)
            {
                if (item.Kind == ShopItemKind.Surgery)
                {
                    item.Price = SurgeryPrice();
                }
            }

            return true;
        }
    }
}
