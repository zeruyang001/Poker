using static CardManager;
using System.Collections.Generic;

public class GameStateTracker
{
    #region Singleton
    private static GameStateTracker instance;
    public static GameStateTracker Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameStateTracker();
            }
            return instance;
        }
    }
    #endregion

    #region ��Ϸ����״̬
    // ��Ϸ״̬
    public GameState CurrentGameState { get; private set; } = GameState.Idle;

    // �غ���
    public int RoundCount { get;  set; } = 0;

    // ������
    public List<Card> landlordCards = new List<Card>();

    // ������ɫ
    public CharacterType Landlord { get; private set; } = CharacterType.Desk;

    // ���ʣ������
    public Dictionary<CharacterType, int> remainingCards = new Dictionary<CharacterType, int>();
    #endregion

    #region �غ�״̬
    public int CurrentWeight { get; private set; }
    public int CurrentLength { get; private set; }
    public CardType CurrentType { get; private set; }
    public CharacterType BiggestCharacter { get;  set; }
    public CharacterType CurrentCharacter { get;  set; }
    #endregion

    #region ��ʷ��¼
    // Pass��¼
    private Dictionary<CharacterType, List<PlayCardArgs>> passHistory = new Dictionary<CharacterType, List<PlayCardArgs>>();

    // �Ѵ������
    private Dictionary<CharacterType, List<Card>> playedCards = new Dictionary<CharacterType, List<Card>>();

    #endregion

    #region ���캯���ͳ�ʼ��
    private GameStateTracker()
    {
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        remainingCards = new Dictionary<CharacterType, int>
        {
            { CharacterType.HostPlayer, 17 },
            { CharacterType.LeftPlayer, 17 },
            { CharacterType.RightPlayer, 17 }
        };

        playedCards = new Dictionary<CharacterType, List<Card>>
        {
            { CharacterType.HostPlayer, new List<Card>() },
            { CharacterType.LeftPlayer, new List<Card>() },
            { CharacterType.RightPlayer, new List<Card>() }
        };

        passHistory = new Dictionary<CharacterType, List<PlayCardArgs>>
        {
            { CharacterType.HostPlayer, new List<PlayCardArgs>() },
            { CharacterType.LeftPlayer, new List<PlayCardArgs>() },
            { CharacterType.RightPlayer, new List<PlayCardArgs>() }
        };
    }

    public void StartNewGame()
    {
        RoundCount = 0;
        CurrentGameState = GameState.Preparing;
        landlordCards.Clear();
        Landlord = CharacterType.Desk;
        InitializeDictionaries();
        ResetRoundState();
    }
    #endregion

    #region ״̬���·���
    public void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    public void SetLandlord(CharacterType character)
    {
        Landlord = character;
        BiggestCharacter = character;
        CurrentCharacter = character;
        remainingCards[character] += 3; // ������3����
        OnLandlordSet?.Invoke(character);
    }

    public void UpdateRemainingCards(CharacterType character, int count)
    {
        if (remainingCards.ContainsKey(character))
        {
            remainingCards[character] = count;
            OnRemainingCardsChanged?.Invoke(character, count);
        }
    }

    public void RecordPlayedCards(PlayCardArgs playCardArgs, List<Card> cards)
    {
        if (playedCards.ContainsKey(playCardArgs.CharacterType))
        {
            playedCards[playCardArgs.CharacterType].AddRange(cards);
            UpdateRemainingCards(playCardArgs.CharacterType, remainingCards[playCardArgs.CharacterType] - cards.Count);
            OnCardsPlayed?.Invoke(playCardArgs.CharacterType, cards);
        }
        UpdateRoundState(playCardArgs.CardType, playCardArgs.Weight, playCardArgs.Length, playCardArgs.CharacterType);
    }

    public void RecordPass(CharacterType character)
    {
        if (passHistory.ContainsKey(character))
        {
            passHistory[character].Add(new PlayCardArgs
            {
                CardType = CurrentType,
                Weight = CurrentWeight,
                Length = CurrentLength,
                CharacterType = BiggestCharacter
            });
            OnPlayerPassed?.Invoke(character);
        }
    }

    public void UpdateRoundState(CardType type, int weight, int length, CharacterType biggest)
    {
        CurrentType = type;
        CurrentWeight = weight;
        CurrentLength = length;
        BiggestCharacter = biggest;
        RoundCount++;
        OnRoundStateUpdated?.Invoke();
    }

    public void SetCurrentCharacter(CharacterType character)
    {
        CurrentCharacter = character;
        OnCurrentCharacterChanged?.Invoke(character);
    }

    public void ResetRoundState()
    {
        CurrentType = CardType.Invalid;
        CurrentWeight = -1;
        CurrentLength = -1;
        BiggestCharacter = CharacterType.Desk;
        CurrentCharacter = CharacterType.Desk;
    }
    #endregion

    #region ��ѯ����
    public List<Card> GetPlayedCards(CharacterType character)
    {
        return playedCards.ContainsKey(character)
            ? new List<Card>(playedCards[character])
            : new List<Card>();
    }

    public List<PlayCardArgs> GetPassHistory(CharacterType character)
    {
        return passHistory.ContainsKey(character)
            ? new List<PlayCardArgs>(passHistory[character])
            : new List<PlayCardArgs>();
    }

    public bool IsLandlord(CharacterType character)
    {
        return Landlord == character;
    }

    public int GetRemainingCards(CharacterType character)
    {
        return remainingCards.ContainsKey(character) ? remainingCards[character] : 0;
    }

    public List<Card> GetAllPlayedCards()
    {
        List<Card> allCards = new List<Card>();
        foreach (var playerCards in playedCards.Values)
        {
            allCards.AddRange(playerCards);
        }
        return allCards;
    }
    #endregion

    #region �¼�ϵͳ
    public event System.Action<GameState> OnGameStateChanged;
    public event System.Action<CharacterType> OnLandlordSet;
    public event System.Action<CharacterType, int> OnRemainingCardsChanged;
    public event System.Action<CharacterType, List<Card>> OnCardsPlayed;
    public event System.Action<CharacterType> OnPlayerPassed;
    public event System.Action OnRoundStateUpdated;
    public event System.Action<CharacterType> OnCurrentCharacterChanged;
    #endregion
}