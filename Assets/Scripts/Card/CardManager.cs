using System;
using System.Collections.Generic;
using System.Linq;

public static class CardManager
{
    public enum CardType
    {
        Single,         // 单张
        Pair,           // 对子
        Three,          // 三张
        ThreeWithOne,   // 三带一
        ThreeWithPair,  // 三带一对
        Straight,       // 顺子
        PairStraight, // 连对
        TripleStraight, // 飞机不带
        TripleStraightWithSingle, // 飞机带单
        TripleStraightWithPair,   // 飞机带对
        Bomb,           // 炸弹
        JokerBomb,      // 王炸
        FourWithTwo,    // 四带二
        FourWithTwoPair,    // 四带两对
        Invalid         // 无效牌型
    }

    public static readonly Dictionary<string, Card> nameCards = new Dictionary<string, Card>();
    public static List<Card> deck { get; set; } = new List<Card>();

    public static void Init()
    {
        nameCards.Clear();
        deck.Clear();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            if (suit == Suit.None) continue;
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                if (rank == Rank.SJoker || rank == Rank.LJoker) continue;
                Card card = new Card(suit, rank);
                nameCards.Add(GetName(card), card);
            }
        }
        nameCards.Add("SJoker", new Card(Suit.None, Rank.SJoker));
        nameCards.Add("LJoker", new Card(Suit.None, Rank.LJoker));
    }

    public static void CreateDeck()
    {
        deck.Clear() ;
        deck = GenerateFullDeck();
    }

    public static List<Card> GenerateFullDeck()
    {
        var deck = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            if (suit == Suit.None) continue;
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                if (rank == Rank.SJoker || rank == Rank.LJoker) continue;
                deck.Add(new Card(suit, rank, CharacterType.Deck));
            }
        }
        deck.Add(new Card(Suit.None, Rank.SJoker, CharacterType.Deck));
        deck.Add(new Card(Suit.None, Rank.LJoker, CharacterType.Deck));
        return deck;
    }

    public static string GetName(Card card)
    {
        return card.GetName();
    }

    public static Card GetCard(string name)
    {
        return nameCards.TryGetValue(name, out Card card) ? card : null;
    }

    public static CardInfo[] GetCardInfos(IEnumerable<Card> cards)
    {
        return cards.Select(c => c.GetCardInfo()).ToArray();
    }

    public static Card[] GetCards(IEnumerable<CardInfo> cardInfos)
    {
        return cardInfos.Select(ci => new Card(ci.suit, ci.rank)).ToArray();
    }

    public static CardType GetCardType(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return CardType.Invalid;

        cards = cards.OrderBy(c => c.rank).ToList();

        if (IsJokerBomb(cards)) return CardType.JokerBomb;
        if (IsBomb(cards)) return CardType.Bomb;
        if (IsSingle(cards)) return CardType.Single;
        if (IsPair(cards)) return CardType.Pair;
        if (IsThree(cards)) return CardType.Three;
        if (IsThreeWithOne(cards)) return CardType.ThreeWithOne;
        if (IsThreeWithPair(cards)) return CardType.ThreeWithPair;
        if (IsStraight(cards)) return CardType.Straight;
        if (IsPairStraight(cards)) return CardType.PairStraight;
        if (IsTripleStraight(cards)) return CardType.TripleStraight;
        if (IsTripleStraightWithSingle(cards)) return CardType.TripleStraightWithSingle;
        if (IsTripleStraightWithPair(cards)) return CardType.TripleStraightWithPair;
        if (IsFourWithTwo(cards)) return CardType.FourWithTwo;
        if (IsFourWithTwoPair(cards)) return CardType.FourWithTwoPair;

        return CardType.Invalid;
    }

    public static bool IsJokerBomb(List<Card> cards)
    {
        return cards.Count == 2 && cards.All(c => c.rank == Rank.SJoker || c.rank == Rank.LJoker);
    }

    public static bool IsBomb(List<Card> cards)
    {
        return cards.Count == 4 && cards.All(c => c.rank == cards[0].rank);
    }

    public static bool IsSingle(List<Card> cards)
    {
        return cards.Count == 1;
    }

    public static bool IsPair(List<Card> cards)
    {
        return cards.Count == 2 && cards[0].rank == cards[1].rank;
    }

    public static bool IsThree(List<Card> cards)
    {
        return cards.Count == 3 && cards.All(c => c.rank == cards[0].rank);
    }

    public static bool IsThreeWithOne(List<Card> cards)
    {
        return cards.Count == 4 && cards.GroupBy(c => c.rank).Any(g => g.Count() == 3);
    }

    public static bool IsThreeWithPair(List<Card> cards)
    {
        return cards.Count == 5 && cards.GroupBy(c => c.rank).Any(g => g.Count() == 3) && cards.GroupBy(c => c.rank).Any(g => g.Count() == 2);
    }

    public static bool IsStraight(List<Card> cards)
    {
        if (cards.Count < 5 || cards.Any(c => c.rank >= Rank.Two)) return false;
        for (int i = 1; i < cards.Count; i++)
        {
            if (cards[i].rank != cards[i - 1].rank + 1)
                return false;
        }
        return true;
    }

    public static bool IsPairStraight(List<Card> cards)
    {
        if (cards.Count < 6 || cards.Count % 2 != 0 || cards.Any(c => c.rank >= Rank.Two)) return false;
        for (int i = 0; i < cards.Count; i += 2)
        {
            if (cards[i].rank != cards[i + 1].rank)
                return false;
            if (i > 0 && cards[i].rank != cards[i - 2].rank + 1)
                return false;
        }
        return true;
    }

    public static bool IsTripleStraight(List<Card> cards)
    {
        if (cards.Count > 18 || cards.Count % 3 != 0 || cards.Any(c => c.rank >= Rank.Two)) return false;
        var groups = cards.GroupBy(c => c.rank).ToList();
        return groups.Count == cards.Count / 3 && groups.All(g => g.Count() == 3) && IsConsecutive(groups.Select(g => g.Key));
    }

    public static bool IsTripleStraightWithSingle(List<Card> cards)
    {
        if (cards.Count > 16 || cards.Count % 4 != 0) return false;
        var groups = cards.GroupBy(c => c.rank).OrderByDescending(g => g.Count()).ToList();
        var triples = groups.Where(g => g.Count() == 3).ToList();
        return triples.Count == cards.Count / 4 && IsConsecutive(triples.Select(g => g.Key));
    }

    public static bool IsTripleStraightWithPair(List<Card> cards)
    {
        if (cards.Count > 15 || cards.Count % 5 != 0) return false;
        var groups = cards.GroupBy(c => c.rank).OrderByDescending(g => g.Count()).ToList();
        var triples = groups.Where(g => g.Count() == 3).ToList();
        var pairs = groups.Where(g => g.Count() == 2).ToList();
        return triples.Count == cards.Count / 5 && pairs.Count == cards.Count / 5 && IsConsecutive(triples.Select(g => g.Key));
    }

    public static bool IsConsecutive(IEnumerable<Rank> ranks)
    {
        var orderedRanks = ranks.OrderBy(r => r).ToList();
        for (int i = 1; i < orderedRanks.Count; i++)
        {
            if (orderedRanks[i] != orderedRanks[i - 1] + 1)
                return false;
        }
        return true;
    }

    public static bool IsFourWithTwo(List<Card> cards)
    {
        if (cards.Count != 6)
            return false;

        var groups = cards.GroupBy(c => c.rank).OrderByDescending(g => g.Count()).ToList();

        // 检查是否有四张相同的牌
        if (!groups.Any(g => g.Count() == 4))
            return false;

        // 剩下的牌必须是两个单张
        return true;
    }

    public static bool IsFourWithTwoPair(List<Card> cards)
    {
        if (cards.Count != 8)
            return false;

        var groups = cards.GroupBy(c => c.rank).OrderByDescending(g => g.Count()).ToList();

        // 检查是否有四张相同的牌
        if (!groups.Any(g => g.Count() == 4))
            return false;

        // 剩下的牌必须是两对
        return groups.Count(g => g.Count() == 2) == 2;
    }

    public static bool CanBeat(List<Card> currentCards, List<Card> lastCards)
    {
        CardType currentType = GetCardType(currentCards);
        CardType lastType = GetCardType(lastCards);

        if (currentType == CardType.Invalid)
            return false;

        if (currentType == CardType.JokerBomb)
            return true;

        if (currentType == CardType.Bomb && lastType != CardType.Bomb && lastType != CardType.JokerBomb)
            return true;

        if (currentType != lastType || currentCards.Count != lastCards.Count)
            return false;

        // 对于飞机牌型，我们需要比较飞机的部分
        if (currentType == CardType.TripleStraight ||
            currentType == CardType.TripleStraightWithSingle ||
            currentType == CardType.TripleStraightWithPair)
        {
            return CompareTripleStraight(currentCards, lastCards);
        }

        return GetMaxRank(currentCards) > GetMaxRank(lastCards);
    }

    public static bool CompareTripleStraight(List<Card> currentCards, List<Card> lastCards)
    {
        var currentTriples = currentCards.GroupBy(c => c.rank).Where(g => g.Count() == 3).OrderBy(g => g.Key).ToList();
        var lastTriples = lastCards.GroupBy(c => c.rank).Where(g => g.Count() == 3).OrderBy(g => g.Key).ToList();

        return currentTriples[0].Key > lastTriples[0].Key;
    }

    public static Rank GetMaxRank(List<Card> cards)
    {
        return cards.Max(c => c.rank);
    }



    /// <summary>
    /// 获取牌的大小
    /// </summary>
    /// <param name="cards">出的牌</param>
    /// <param name="cardType">出牌类型</param>
    /// <returns></returns>
    public static int GetWeight(List<Card> cards, CardType cardType)
    {
        if (cards == null || cards.Count == 0)
            return 0;

        Sort(cards);

        switch (cardType)
        {
            case CardType.Single:
            case CardType.Pair:
            case CardType.Three:
            case CardType.Bomb:
                return (int)cards[0].rank;

            case CardType.Straight:
            case CardType.PairStraight:
            case CardType.TripleStraight:
                return (int)cards[0].rank;

            case CardType.ThreeWithOne:
            case CardType.ThreeWithPair:
                return (int)cards.GroupBy(c => c.rank)
                                 .OrderByDescending(g => g.Count())
                                 .First().Key;

            case CardType.TripleStraightWithSingle:
            case CardType.TripleStraightWithPair:
                return (int)cards.GroupBy(c => c.rank)
                                 .Where(g => g.Count() >= 3)
                                 .OrderBy(g => g.Key)
                                 .First().Key;

            case CardType.FourWithTwo:
            case CardType.FourWithTwoPair:
                return (int)cards.GroupBy(c => c.rank)
                                 .Where(g => g.Count() == 4)
                                 .First().Key;

            case CardType.JokerBomb:
                return int.MaxValue;

            default:
                return 0;
        }
    }



    public static void ShuffleDeck()
    {
        Random rng = new Random();
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Card value = deck[k];
            deck[k] = deck[n];
            deck[n] = value;
        }

        // 确保所有卡牌的 BelongTo 属性都是 Deck
        foreach (var card in deck)
        {
            card.BelongTo = CharacterType.Deck;
        }
    }

    public static void Sort(List<Card> cards, bool asc = true)
    {
        cards.Sort((a, b) =>
        {
            // 首先比较 Rank
            int rankComparison = a.rank.CompareTo(b.rank);
            if (rankComparison != 0)
            {
                // 如果是降序，我们需要反转 Rank 的比较结果
                return asc ? rankComparison : -rankComparison;
            }

            // 如果 Rank 相同，则比较 Suit
            int suitComparison = a.suit.CompareTo(b.suit);
            // 对于相同的 Rank，我们总是希望 Suit 按照 Spade > Heart > Club > Diamond 的顺序排列
            // 所以这里不需要考虑 asc 参数
            return -suitComparison;  // 使用负号来反转 Suit 的比较结果
        });
    }

    public static Card DrawCard()
    {
        if (deck.Count > 0)
        {
            Card card = deck[0];
            deck.RemoveAt(0);
            return card;
        }
        return null;
    }

    public static bool CanPop(List<Card> cards, out CardType type)
    {
        type = GetCardType(cards);
        return type != CardType.Invalid;
    }
}