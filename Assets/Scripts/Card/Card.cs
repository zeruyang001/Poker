using System;
using System.Collections.Generic;

public enum Suit
{
    None,
    Diamond,
    Club,
    Heart,
    Spade,
}

public enum Rank
{
    Three,
    Four,
    Five,
    Six,
    Seven,
    Eight,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace,    // Changed from One to Ace for clarity
    Two,
    SJoker,
    LJoker,
}



[Serializable]  // Make it serializable for network transmission
public class Card : IEquatable<Card>, IComparable<Card>
{
    public Suit suit;
    public Rank rank;
    public CharacterType BelongTo { get; set; }

    // 新增属性：卡牌的数值大小
    public int Value => GetCardValue();

    // 新增属性：是否为王牌
    public bool IsJoker => suit == Suit.None && (rank == Rank.SJoker || rank == Rank.LJoker);

    public Card(Suit suit, Rank rank)
    {
        this.suit = suit;
        this.rank = rank;
    }

    public Card(Suit suit, Rank rank, CharacterType belongTo) : this(suit, rank)
    {
        BelongTo = belongTo;
    }

    public Card(int suit, int rank)
    {
        this.suit = (Suit)suit;
        this.rank = (Rank)rank;
    }

    public override bool Equals(object obj) => Equals(obj as Card);

    public bool Equals(Card other)
    {
        return other != null && suit == other.suit && rank == other.rank;
    }

    public override int GetHashCode() => HashCode.Combine(suit, rank);

    public CardInfo GetCardInfo() => new CardInfo { suit = (int)suit, rank = (int)rank };

    public string GetName()
    {
        return suit == Suit.None ? rank.ToString() : $"{suit}{rank}";
    }
    public int CompareTo(Card other)
    {
        if (other == null) return 1;
        int rankComparison = rank.CompareTo(other.rank);
        return rankComparison != 0 ? rankComparison : suit.CompareTo(other.suit);
    }

    public static bool operator ==(Card left, Card right)
    {
        return EqualityComparer<Card>.Default.Equals(left, right);
    }

    public static bool operator !=(Card left, Card right)
    {
        return !(left == right);
    }

    // 新增方法：获取卡牌的数值大小
    private int GetCardValue()
    {
        if (IsJoker)
        {
            return rank == Rank.SJoker ? 16 : 17;
        }
        return (int)rank + 3;
    }
}
