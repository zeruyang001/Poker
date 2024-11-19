using static CardManager;
using System.Collections.Generic;
using System;

public class GameStateContext : IDisposable
{
    #region 玩家关系和身份
    public Player Self { get; private set; }
    public Player LeftPlayer { get; private set; }
    public Player RightPlayer { get; private set; }
    public Player Landlord { get; private set; }
    public Identity SelfIdentity { get; private set; }
    public bool IsLandlord => SelfIdentity == Identity.Landlord;
    #endregion

    #region 游戏数据
    // 地主底牌，地主确定后，这些牌就归入地主的手牌
    private List<Card> landlordCards = new List<Card>();

    // 当前玩家手牌数量
    public Dictionary<CharacterType, int> RemainingCards { get; private set; }

    // 所有玩家已经打出的牌
    public Dictionary<CharacterType, List<Card>> PlayedCards { get; private set; }

    // 所有已知的牌（包括自己的手牌和所有已经出现的牌）
    private List<Card> KnownCards { get; set; } = new List<Card>();

    // 可能出现的牌（总牌数减去已知的牌）
    public List<Card> PossibleRemainingCards { get; private set; }

    // 玩家Pass记录
    public class PassRecord
    {
        public CardType Type { get; set; }
        public int Weight { get; set; }
        public int Length { get; set; }
    }
    public Dictionary<CharacterType, List<PassRecord>> PassHistory { get; private set; }
    #endregion

    public GameStateContext()
    {
        RemainingCards = new Dictionary<CharacterType, int>();
        PlayedCards = new Dictionary<CharacterType, List<Card>>();
        PassHistory = new Dictionary<CharacterType, List<PassRecord>>();
        PossibleRemainingCards = new List<Card>();
    }

    #region 初始化和重置
    public void Initialize(List<Card> threeCards, Player self, Player left, Player right)
    {
        Self = self;
        LeftPlayer = left;
        RightPlayer = right;
        SelfIdentity = self.Identity;
        landlordCards = threeCards;
        DetermineRelationships();
        Reset();
        InitializeCardInfo();
        // 更新可能出现的牌
        UpdatePossibleRemainingCards();
    }

    private void DetermineRelationships()
    {
        if (IsLandlord)
        {
            Landlord = Self;
        }
        else
        {
            Landlord = LeftPlayer.Identity == Identity.Landlord ? LeftPlayer : RightPlayer;
        }
    }

    public void Reset()
    {
        RemainingCards.Clear();
        PlayedCards.Clear();
        PassHistory.Clear();
        KnownCards.Clear();
        PossibleRemainingCards.Clear();
    }
    #endregion

    #region 游戏状态更新方法
    public void OnCardsPlayed(CharacterType playerType, List<Card> playedCards, CardType playType, int weight)
    {
        // 更新已出牌记录
        if (!PlayedCards.ContainsKey(playerType))
        {
            PlayedCards[playerType] = new List<Card>();
        }
        PlayedCards[playerType].AddRange(playedCards);

        // 更新已知牌集合
        KnownCards.AddRange(playedCards);

        // 更新剩余牌数
        if (RemainingCards.ContainsKey(playerType))
        {
            RemainingCards[playerType] -= playedCards.Count;
        }

        // 更新可能出现的牌
        UpdatePossibleRemainingCards();
    }

    public void OnPlayerPass(CharacterType playerType, CardType requiredType, int weight, int length)
    {
        if (!PassHistory.ContainsKey(playerType))
        {
            PassHistory[playerType] = new List<PassRecord>();
        }

        PassHistory[playerType].Add(new PassRecord
        {
            Type = requiredType,
            Weight = weight,
            Length = length
        });
    }
    #endregion

    #region 辅助方法
    private void InitializeCardInfo()
    {
        // 初始化手牌数量
        UpdatePlayerCardCounts();

        // 初始化已知牌（自己的手牌）
        KnownCards.AddRange(Self.cardList);

        // 初始化可能出现的牌
        UpdatePossibleRemainingCards();
    }

    private void UpdatePlayerCardCounts()
    {
        RemainingCards[Self.characterType] = Self.cardList.Count;
        RemainingCards[LeftPlayer.characterType] = LeftPlayer.cardList.Count;
        RemainingCards[RightPlayer.characterType] = RightPlayer.cardList.Count;
    }

    private void UpdatePossibleRemainingCards()
    {
        // 创建完整牌组
        PossibleRemainingCards = CardManager.GenerateFullDeck();

        // 移除所有已知的牌
        foreach (var card in KnownCards)
        {
            PossibleRemainingCards.RemoveAll(c => c.suit == card.suit && c.rank == card.rank);
        }
    }

    public bool IsPartner(Player player)
    {
        if (IsLandlord) return false;
        return player != Landlord;
    }

    public List<PassRecord> GetPlayerPassHistory(CharacterType playerType)
    {
        return PassHistory.TryGetValue(playerType, out var history) ? history : new List<PassRecord>();
    }
    #endregion

    public void Dispose()
    {
        RemainingCards?.Clear();
        PlayedCards?.Clear();
        PassHistory?.Clear();
        KnownCards?.Clear();
        PossibleRemainingCards?.Clear();
        Self = null;
        LeftPlayer = null;
        RightPlayer = null;
        Landlord = null;
    }
}