using static CardManager;
using System.Collections.Generic;
using System;

public class GameStateContext : IDisposable
{
    #region ��ҹ�ϵ�����
    public Player Self { get; private set; }
    public Player LeftPlayer { get; private set; }
    public Player RightPlayer { get; private set; }
    public Player Landlord { get; private set; }
    public Identity SelfIdentity { get; private set; }
    public bool IsLandlord => SelfIdentity == Identity.Landlord;
    #endregion

    #region ��Ϸ����
    // �������ƣ�����ȷ������Щ�ƾ͹������������
    private List<Card> landlordCards = new List<Card>();

    // ��ǰ�����������
    public Dictionary<CharacterType, int> RemainingCards { get; private set; }

    // ��������Ѿ��������
    public Dictionary<CharacterType, List<Card>> PlayedCards { get; private set; }

    // ������֪���ƣ������Լ������ƺ������Ѿ����ֵ��ƣ�
    private List<Card> KnownCards { get; set; } = new List<Card>();

    // ���ܳ��ֵ��ƣ���������ȥ��֪���ƣ�
    public List<Card> PossibleRemainingCards { get; private set; }

    // ���Pass��¼
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

    #region ��ʼ��������
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
        // ���¿��ܳ��ֵ���
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

    #region ��Ϸ״̬���·���
    public void OnCardsPlayed(CharacterType playerType, List<Card> playedCards, CardType playType, int weight)
    {
        // �����ѳ��Ƽ�¼
        if (!PlayedCards.ContainsKey(playerType))
        {
            PlayedCards[playerType] = new List<Card>();
        }
        PlayedCards[playerType].AddRange(playedCards);

        // ������֪�Ƽ���
        KnownCards.AddRange(playedCards);

        // ����ʣ������
        if (RemainingCards.ContainsKey(playerType))
        {
            RemainingCards[playerType] -= playedCards.Count;
        }

        // ���¿��ܳ��ֵ���
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

    #region ��������
    private void InitializeCardInfo()
    {
        // ��ʼ����������
        UpdatePlayerCardCounts();

        // ��ʼ����֪�ƣ��Լ������ƣ�
        KnownCards.AddRange(Self.cardList);

        // ��ʼ�����ܳ��ֵ���
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
        // ������������
        PossibleRemainingCards = CardManager.GenerateFullDeck();

        // �Ƴ�������֪����
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