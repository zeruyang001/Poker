/*using static CardManager;
using System.Collections.Generic;
using AI.Utils;
using System.Linq;
using UnityEngine;

public class MCTSGameState
{
    #region ����״̬����
    // ������Ϸ��Ϣ
    public CharacterType CurrentPlayer { get; private set; }
    public Identity PlayerRole { get; private set; }
    public bool IsLandlord => PlayerRole == Identity.Landlord;

    // �ƾ���Ϣ
    public List<Card> HandCards { get; private set; }
    public Dictionary<CharacterType, int> RemainingCards { get; private set; }
    public Dictionary<CharacterType, List<Card>> PlayedCards { get; private set; }
    public List<Card> PossibleCards { get; private set; }
    public LastPlayInfo LastPlay { get; private set; }

    // �غ���Ϣ
    public int CurrentRound { get; private set; }
    public int ConsecutivePassCount { get; private set; }

    // ��������Ϣ
    private Dictionary<CharacterType, Identity> PlayerIdentities { get; set; }
    private CharacterType LandlordPlayer { get; set; }
    #endregion

    #region ���캯��
    public MCTSGameState(GameStateContext context)
    {
        InitializeFromContext(context);
    }

    // ������캯��
    public MCTSGameState(MCTSGameState other)
    {
        HandCards = new List<Card>(other.HandCards);
        RemainingCards = new Dictionary<CharacterType, int>(other.RemainingCards);
        PlayedCards = new Dictionary<CharacterType, List<Card>>();
        PlayerIdentities = new Dictionary<CharacterType, Identity>(other.PlayerIdentities);

        foreach (var kvp in other.PlayedCards)
        {
            PlayedCards[kvp.Key] = new List<Card>(kvp.Value);
        }

        PossibleCards = new List<Card>(other.PossibleCards);
        LastPlay = other.LastPlay?.Clone();
        CurrentPlayer = other.CurrentPlayer;
        PlayerRole = other.PlayerRole;
        CurrentRound = other.CurrentRound;
        ConsecutivePassCount = other.ConsecutivePassCount;
        LandlordPlayer = other.LandlordPlayer;
    }
    #endregion

    #region ��ʼ������
    private void InitializeFromContext(GameStateContext context)
    {
        // �������Գ�ʼ��
        HandCards = new List<Card>(context.Self.cardList);
        RemainingCards = new Dictionary<CharacterType, int>(context.RemainingCards);
        PlayedCards = new Dictionary<CharacterType, List<Card>>();
        PlayerIdentities = new Dictionary<CharacterType, Identity>();

        foreach (var kvp in context.PlayedCards)
        {
            PlayedCards[kvp.Key] = new List<Card>(kvp.Value);
        }

        PossibleCards = new List<Card>(context.PossibleRemainingCards);
        LastPlay = context.LastPlay?.Clone();
        PlayerRole = context.SelfIdentity;
        CurrentRound = 0;
        ConsecutivePassCount = 0;

        InitializePlayerIdentities(context);
    }

    private void InitializePlayerIdentities(GameStateContext context)
    {
        // �����ݳ�ʼ��
        PlayerIdentities[CharacterType.Player] = context.Self.Identity;
        PlayerIdentities[CharacterType.LeftComputer] = context.LeftPlayer.Identity;
        PlayerIdentities[CharacterType.RightComputer] = context.RightPlayer.Identity;

        // ��¼�������
        foreach (var kvp in PlayerIdentities)
        {
            if (kvp.Value == Identity.Landlord)
            {
                LandlordPlayer = kvp.Key;
                break;
            }
        }
    }
    #endregion

    #region ��Ϸ״̬�ж�
    private float EvaluateStateValue()
    {
        // ��Ϸ����״̬������
        if (IsTerminal())
        {
            return GetScore();
        }

        float value = 0;

        // ������������ (40%)
        value += EvaluateHandStrength() * 0.4f;

        // �������� (35%)
        value += EvaluateSituation() * 0.35f;

        // ����Ȩ���� (25%)
        value += EvaluateControl() * 0.25f;

        // ѹ��״̬����
        if (ConsecutivePassCount >= 2)
        {
            value *= 0.8f;
        }

        return Mathf.Clamp01(value);
    }

    private float EvaluateHandStrength()
    {
        float strength = AICardAnalyzer.CalculateHandStrength(HandCards, IsLandlord);

        // �оּӳ�
        if (IsEndgame())
        {
            float endgameBonus = CalculateEndgameBonus();
            strength = Mathf.Clamp01(strength + endgameBonus);
        }

        return strength;
    }
    private float EvaluateSituation()
    {
        float score = 0;

        // 1. �����Ա����� (40%)
        score += EvaluateCardCount() * 0.4f;

        // 2. ����������� (35%)
        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        score += EvaluateCombinations(combinations) * 0.35f;

        // 3. �ؼ��ƿ������� (25%)
        score += EvaluateKeyCards() * 0.25f;

        return Mathf.Clamp01(score);
    }

    private float EvaluateCardCount()
    {
        float score = 0;
        float baseScore = 1.0f - (HandCards.Count / 20.0f); // �������ȷ�

        if (IsLandlord)
        {
            // ���������߼�
            score = EvaluateLandlordProgress(baseScore);
        }
        else
        {
            // ũ�������߼�
            score = EvaluateFarmerProgress(baseScore);
        }

        return Mathf.Clamp01(score);
    }


    private float EvaluateControl()
    {
        float controlScore = 0;

        // 1. ����Ȩ���� (35%)
        controlScore += EvaluatePlayingRights() * 0.35f;

        // 2. ���Ϳ��������� (35%)
        controlScore += EvaluatePatternControl() * 0.35f;

        // 3. �����ص��� (30%)
        controlScore += EvaluateRoleControl() * 0.3f;

        return Mathf.Clamp01(controlScore);
    }

    public bool IsTerminal()
    {
        // ����ҳ�����
        if (HandCards.Count == 0) return true;

        foreach (var count in RemainingCards.Values)
        {
            if (count == 0) return true;
        }

        // ������ֹ�������糬�����غ�����
        if (CurrentRound >= 100) return true;

        return false;
    }

    public float GetScore()
    {
        if (!IsTerminal()) return 0;

        if (HandCards.Count == 0)
        {
            // Ӯ�ҵ÷�
            return CalculateWinScore();
        }
        else if (RemainingCards.Any(kv => kv.Value == 0))
        {
            // ��ҵ÷�
            return CalculateLoseScore();
        }

        // ƽ�ֻ��������
        return 0;
    }
    #endregion

    #region ������������
    private float CalculateEndgameBonus()
    {
        float bonus = 0;

        // ը������ը����ӳ�
        int bombCount = AICardAnalyzer.CountBombs(HandCards);
        bool hasRocket = AICardAnalyzer.HasJokerBomb(HandCards);

        bonus += bombCount * 0.15f;
        if (hasRocket) bonus += 0.2f;

        // �ؼ��Ƽӳ�
        int bigCards = HandCards.Count(c => c.rank >= Rank.Two);
        bonus += bigCards * 0.05f;

        return bonus;
    }

    private float EvaluateCombinations(List<CardCombination> combinations)
    {
        if (!combinations.Any()) return 0;

        float totalValue = 0;
        foreach (var combo in combinations)
        {
            // ���ݲ�ͬ���ͺ���ݸ��費ͬȨ��
            float weight = GetCombinationWeight(combo.Type);
            totalValue += combo.Value * weight;
        }

        return Mathf.Clamp01(totalValue / (combinations.Count * 5f));
    }

    private float EvaluateKeyCards()
    {
        float score = 0;

        // ����ؼ��Ƽ�ֵ
        foreach (var card in HandCards)
        {
            if (card.rank >= Rank.Two)
            {
                score += GetKeyCardValue(card);
            }
        }

        // ��ݼӳ�
        if (IsLandlord)
        {
            score *= 1.2f; // �����Ĺؼ��Ƹ���Ҫ
        }

        return Mathf.Clamp01(score);
    }

    private float EvaluateLandlordProgress(float baseScore)
    {
        float score = baseScore * 1.2f; // ���������ָ���

        // �Ա�ũ������
        var farmerMinCards = GetMinFarmerCards();
        if (HandCards.Count <= farmerMinCards)
        {
            score *= 1.3f;
        }

        return score;
    }

    private float EvaluateFarmerProgress(float baseScore)
    {
        float score = baseScore;

        // ����������Ա�
        int landlordCards = RemainingCards[LandlordPlayer];
        if (HandCards.Count < landlordCards)
        {
            score *= 1.2f;
        }

        // ��������
        var partnerCards = GetPartnerRemainingCards();
        if (partnerCards <= 3 && HandCards.Count <= 5)
        {
            score *= 1.3f;
        }

        return score;
    }

    private float GetCombinationWeight(CardType type)
    {
        return type switch
        {
            CardType.JokerBomb => 2.0f,
            CardType.Bomb => 1.8f,
            CardType.TripleStraight => 1.5f,
            CardType.Straight => 1.3f,
            CardType.PairStraight => 1.3f,
            CardType.ThreeWithPair => 1.2f,
            _ => 1.0f
        };
    }

    private float GetKeyCardValue(Card card)
    {
        if (card.rank == Rank.LJoker) return 0.3f;
        if (card.rank == Rank.SJoker) return 0.25f;
        if (card.rank == Rank.Two) return 0.2f;
        return (float)(card.rank - Rank.Three) / 20f;
    }

    private float CalculateWinScore()
    {
        float baseScore = IsLandlord ? 1.0f : 0.5f;
        // ����ʣ��������������÷�
        float remainingPenalty = GetMinOpponentCards() * 0.05f;
        return Mathf.Clamp01(baseScore - remainingPenalty);
    }

    private float CalculateLoseScore()
    {
        float baseScore = IsLandlord ? -1.0f : -0.5f;
        // �����Լ�ʣ�����������÷�
        float remainingPenalty = HandCards.Count * 0.05f;
        return Mathf.Clamp01(baseScore - remainingPenalty);
    }
    #endregion

    #region ״̬�жϸ�������
    private bool IsEndgame()
    {
        return CurrentRound >= 10 || HandCards.Count <= 5 || GetMinOpponentCards() <= 3;
    }

    private int GetMinOpponentCards()
    {
        var opponents = GetOpponents();
        int minCards = int.MaxValue;

        if (RemainingCards.ContainsKey(opponents.opponent1))
        {
            minCards = Mathf.Min(minCards, RemainingCards[opponents.opponent1]);
        }
        if (opponents.opponent2 != CharacterType.Desk &&
            RemainingCards.ContainsKey(opponents.opponent2))
        {
            minCards = Mathf.Min(minCards, RemainingCards[opponents.opponent2]);
        }

        return minCards == int.MaxValue ? 0 : minCards;
    }

    private int GetMinFarmerCards()
    {
        return RemainingCards.Where(kv => PlayerIdentities[kv.Key] == Identity.Farmer)
                            .Min(kv => kv.Value);
    }
    #endregion

    #region ����Ȩ��������
    private float EvaluatePlayingRights()
    {
        float score = 0;

        // 1. ��������Ȩ����
        if (LastPlay == null || LastPlay.Character == CurrentPlayer)
        {
            score += 0.4f; // ���ɳ���Ȩ
        }

        // 2. ���ƽ�������
        if (ConsecutivePassCount == 0)
        {
            score += 0.3f; // ���õĳ��ƽ���
        }
        else
        {
            score += Mathf.Max(0, 0.3f - ConsecutivePassCount * 0.1f); // ��ѹ��ʱ���ͷ���
        }

        // 3. �غ�λ������
        if (IsInGoodPosition())
        {
            score += 0.3f;
        }

        return Mathf.Clamp01(score);
    }

    private float EvaluatePatternControl()
    {
        float controlScore = 0;
        var cardCombos = AICardAnalyzer.FindAllPotentialCombinations(HandCards);

        // 1. ������������������
        foreach (var combo in cardCombos)
        {
            switch (combo.Type)
            {
                case CardType.Bomb:
                    controlScore += 0.3f;
                    break;
                case CardType.JokerBomb:
                    controlScore += 0.4f;
                    break;
                case CardType.TripleStraight:
                case CardType.TripleStraightWithPair:
                    controlScore += 0.2f * (combo.Cards.Count / 6.0f);
                    break;
                case CardType.Straight:
                case CardType.PairStraight:
                    controlScore += 0.15f * (combo.Cards.Count / 5.0f);
                    break;
                case CardType.ThreeWithPair:
                    controlScore += 0.1f;
                    break;
            }
        }

        // 2. �ؼ��ƿ�������
        if (HasKeyCards())
        {
            controlScore += 0.2f;
        }

        // 3. �����������
        if (HasBeatPotential())
        {
            controlScore += 0.2f;
        }

        return Mathf.Clamp01(controlScore);
    }

    private float EvaluateRoleControl()
    {
        float controlScore = 0;

        // 1. ��ݻ�������
        if (IsLandlord)
        {
            controlScore = 0.6f; // ����������������ǿ
        }
        else
        {
            controlScore = 0.4f; // ũ���������������
        }

        // 2. ���Ƶ���
        if (IsInLeadingPosition())
        {
            controlScore *= 1.2f;
        }
        else if (IsInTrailingPosition())
        {
            controlScore *= 0.8f;
        }

        // 3. ������ϣ�ũ��
        if (!IsLandlord && IsGoodCooperation())
        {
            controlScore *= 1.2f;
        }

        return Mathf.Clamp01(controlScore);
    }
    #endregion

    #region ����Ȩ������������
    private bool IsInGoodPosition()
    {
        // ����Ƿ��������ĳ���λ��
        if (IsLandlord)
        {
            // ����������ũ��û�к��Ƶ������λ�ú�
            return GetConsecutivePassCount() >= 1;
        }
        else
        {
            // ũ���ڵ�����ѹ��ʱλ�ú�
            var landlordPasses = GetPlayerPassCount(LandlordPlayer);
            return landlordPasses > 0;
        }
    }

    private bool HasBeatPotential()
    {
        // ����Ƿ����㹻���ƿ���ѹ�ƶ���
        if (LastPlay == null) return true;

        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        return combinations.Any(combo => CanBeatLastPlay(combo.Cards, combo.Type));
    }

    private bool IsInLeadingPosition()
    {
        if (IsLandlord)
        {
            // ���������ж�
            return HandCards.Count <= GetMinFarmerCards();
        }
        else
        {
            // ũ�������ж�
            int landlordCards = RemainingCards[LandlordPlayer];
            return HandCards.Count < landlordCards;
        }
    }

    private bool IsInTrailingPosition()
    {
        if (IsLandlord)
        {
            // ��������ж�
            return HandCards.Count > GetMinFarmerCards() + 2;
        }
        else
        {
            // ũ������ж�
            int landlordCards = RemainingCards[LandlordPlayer];
            return HandCards.Count > landlordCards + 2;
        }
    }

    private bool IsGoodCooperation()
    {
        if (IsLandlord) return false;

        var partnerCards = GetPartnerRemainingCards();
        // ���������������ܹ�����ѹ�Ƶ���
        return partnerCards <= 5 && HandCards.Count <= 6 &&
               Mathf.Max(partnerCards, HandCards.Count) < RemainingCards[LandlordPlayer];
    }

    private int GetConsecutivePassCount()
    {
        return ConsecutivePassCount;
    }

    private int GetPlayerPassCount(CharacterType playerType)
    {
        // ���������Ӿ���Ĺ��ƴ���ͳ���߼�
        // ��ʱ�����������ƴ���
        if (LastPlay?.Character == playerType)
        {
            return ConsecutivePassCount;
        }
        return 0;
    }
    #endregion

    #region �ж����ɺ�ִ��
    public List<CardCombination> GetPossibleActions()
    {
        var actions = new List<CardCombination>();

        // 1. ����ѡ��
        if (CanPass())
        {
            actions.Add(CreatePassAction());
        }

        // 2. ����ѡ��
        actions.AddRange(GeneratePlayActions());

        // 3. ����ֵ�������ƶ�������
        return FilterAndSortActions(actions);
    }

    private List<CardCombination> GeneratePlayActions()
    {
        return LastPlay == null || LastPlay.CardType == CardType.Invalid
            ? GenerateFreePlayActions()
            : GenerateResponseActions(LastPlay);
    }

    private List<CardCombination> GenerateFreePlayActions()
    {
        var actions = new List<CardCombination>();

        // �����ȼ���������
        actions.AddRange(GenerateHighValueCombinations());   // �߼�ֵ���
        actions.AddRange(GenerateNormalCombinations());      // ��ͨ���
        actions.AddRange(GenerateBasicCombinations());       // �������

        return actions;
    }

    private List<CardCombination> GenerateHighValueCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. ���
        var rockets = AICardFinder.FindJokerBomb(HandCards);
        if (rockets.Any())
        {
            actions.Add(CreatePlayAction(rockets, CardType.JokerBomb));
        }

        // 2. ը��
        var bombs = AICardFinder.FindBomb(HandCards, -1);
        if (bombs.Any())
        {
            actions.Add(CreatePlayAction(bombs, CardType.Bomb));
        }

        // 3. �ɻ�ϵ��
        actions.AddRange(GenerateTripleStraightCombinations());

        return actions;
    }

    private List<CardCombination> GenerateNormalCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. ˳��
        for (int len = 5; len <= 12 && len <= HandCards.Count; len++)
        {
            var straights = AICardFinder.FindStraight(HandCards, -1, len);
            if (straights.Any())
            {
                actions.Add(CreatePlayAction(straights, CardType.Straight));
            }
        }

        // 2. ����
        for (int len = 6; len <= 20 && len <= HandCards.Count; len += 2)
        {
            var pairStraights = AICardFinder.FindPairStraight(HandCards, -1, len);
            if (pairStraights.Any())
            {
                actions.Add(CreatePlayAction(pairStraights, CardType.PairStraight));
            }
        }

        // 3. �Ĵ���
        actions.AddRange(GenerateFourWithExtraCombinations());

        return actions;
    }

    private List<CardCombination> GenerateBasicCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. ���������
        var threeCardCombos = GenerateThreeCardCombinations();
        actions.AddRange(threeCardCombos);

        // 2. ����
        var pairs = AICardFinder.FindPurePairs(HandCards);
        foreach (var pair in pairs)
        {
            actions.Add(CreatePlayAction(pair, CardType.Pair));
        }

        // 3. ���ƣ�����ֵɸѡ��
        actions.AddRange(GenerateSmartSingleCards());

        return actions;
    }

    private List<CardCombination> GenerateSmartSingleCards()
    {
        var actions = new List<CardCombination>();
        var sortedCards = HandCards.OrderBy(c => GetCardValue(c)).ToList();

        // ���ɵ��ƣ����������м�ֵ�����
        foreach (var card in sortedCards)
        {
            if (!WouldBreakValuableCombination(card))
            {
                actions.Add(CreatePlayAction(new List<Card> { card }, CardType.Single));
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateResponseActions(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // 1. ���ǿ�����ը
        var rocket = AICardFinder.FindJokerBomb(HandCards);
        if (rocket.Any())
        {
            actions.Add(CreatePlayAction(rocket, CardType.JokerBomb));
        }

        // 2. �������Ӧ
        if (lastPlay.CardType == CardType.Bomb)
        {
            // ֻ���ø����ը����Ӧ
            var biggerBomb = AICardFinder.FindBomb(HandCards, lastPlay.Weight);
            if (biggerBomb.Any())
            {
                actions.Add(CreatePlayAction(biggerBomb, CardType.Bomb));
            }
        }
        else
        {
            // ������ը��
            var bombs = AICardFinder.FindBomb(HandCards, -1);
            if (bombs.Any())
            {
                actions.Add(CreatePlayAction(bombs, CardType.Bomb));
            }

            // ͬ������Ӧ
            actions.AddRange(GenerateTypeSpecificResponse(lastPlay));
        }

        return actions;
    }

    private CardCombination CreatePlayAction(List<Card> cards, CardType type)
    {
        return new CardCombination
        {
            Cards = new List<Card>(cards),
            Type = type,
            Value = CalculateActionValue(cards, type)
        };
    }

    private float CalculateActionValue(List<Card> cards, CardType type)
    {
        float baseValue = cards.Count; // �����֣���������

        // 1. ���ͼ�ֵ
        float typeValue = GetTypeBaseValue(type);

        // 2. ���Ƽӳ�
        float situationMultiplier = GetSituationMultiplier();

        // 3. ʣ����������
        float remainingBonus = CalculateRemainingBonus(cards);

        return (baseValue * typeValue * situationMultiplier) + remainingBonus;
    }

    private List<CardCombination> FilterAndSortActions(List<CardCombination> actions)
    {
        // 1. �Ƴ���Ч����
        actions = actions.Where(a => IsValidAction(a)).ToList();

        // 2. ����ֵ����
        actions = actions.OrderByDescending(a => a.Value).ToList();

        // 3. ���ƶ������������ܿ��ǣ�
        int maxActions = DetermineMaxActions();
        return actions.Take(maxActions).ToList();
    }

    public MCTSGameState ApplyAction(CardCombination action)
    {
        var newState = new MCTSGameState(this);

        // ����״̬
        UpdateStateAfterAction(newState, action);

        return newState;
    }

    private void UpdateStateAfterAction(MCTSGameState newState, CardCombination action)
    {
        // 1. ��������
        if (action.Cards.Any())
        {
            foreach (var card in action.Cards)
            {
                newState.HandCards.Remove(card);
            }

            // ���³��Ƽ�¼
            UpdatePlayedCards(newState, action);

            // ������������Ϣ
            UpdateLastPlay(newState, action);
        }
        else
        {
            // ���ƴ���
            newState.ConsecutivePassCount++;
        }

        // 2. ����ʣ������
        newState.RemainingCards[CurrentPlayer] = newState.HandCards.Count;

        // 3. ���»غ���Ϣ
        newState.CurrentRound++;
        newState.CurrentPlayer = GetNextPlayer(newState.CurrentPlayer);
    }
    #endregion

    #region �ж����ɸ�������
    private float GetTypeBaseValue(CardType type)
    {
        return type switch
        {
            CardType.JokerBomb => 5.0f,
            CardType.Bomb => 4.0f,
            CardType.FourWithTwo => 3.0f,
            CardType.FourWithTwoPair => 3.0f,
            CardType.TripleStraight => 2.5f,
            CardType.Straight => 2.0f,
            CardType.PairStraight => 2.0f,
            CardType.ThreeWithPair => 1.8f,
            CardType.ThreeWithOne => 1.5f,
            _ => 1.0f
        };
    }

    private float GetSituationMultiplier()
    {
        float multiplier = 1.0f;

        // ���ݾ��Ƶ���
        if (IsEndgame())
        {
            multiplier *= 1.5f;
        }

        if (ConsecutivePassCount >= 2)
        {
            multiplier *= 1.3f;
        }

        return multiplier;
    }

    private float CalculateRemainingBonus(List<Card> cards)
    {
        float bonus = 0;

        // ���ƺ�ʣ����������
        int remainingCount = HandCards.Count - cards.Count;
        if (remainingCount <= 4)
        {
            bonus += 0.5f;
        }
        else if (remainingCount <= 8)
        {
            bonus += 0.3f;
        }

        return bonus;
    }

    private bool IsValidAction(CardCombination action)
    {
        if (action.Cards.Count == 0) return true; // ����������Ч

        // 1. ����Ƿ�ӵ����Щ��
        foreach (var card in action.Cards)
        {
            if (!HandCards.Contains(card)) return false;
        }

        // 2. ��������Ƿ�Ϸ�
        if (action.Type == CardType.Invalid) return false;

        // 3. ����Ƿ��ܴ���ϼ�
        if (LastPlay != null && LastPlay.CardType != CardType.Invalid)
        {
            return CanBeatLastPlay(action.Cards, action.Type);
        }

        return true;
    }

    private int DetermineMaxActions()
    {
        // ���ݾ��ƾ��������Ķ�������
        if (IsEndgame())
        {
            return 20; // �оֱ�������ѡ��
        }
        return 15; // һ�����
    }
    #endregion

    #region �������ɲ��䷽��
    private bool CanPass()
    {
        // 1. �Ƿ��ǵ�һ����
        if (LastPlay == null || LastPlay.CardType == CardType.Invalid)
            return false;

        // 2. �Ƿ����Լ�����
        if (LastPlay.Character == CurrentPlayer)
            return false;

        return true;
    }

    private CardCombination CreatePassAction()
    {
        return new CardCombination
        {
            Cards = new List<Card>(),
            Type = CardType.Invalid,
            Value = CalculatePassValue()
        };
    }

    private float CalculatePassValue()
    {
        float passValue = 0.1f; // �������Ƽ�ֵ�ϵ�

        // 1. ������ѳ��˺���
        if (!IsLandlord && LastPlay?.Character == GetPartnerPlayer())
        {
            passValue += 0.3f;
        }

        // 2. ������ƽϲ�
        if (AICardAnalyzer.CalculateHandStrength(HandCards) < 0.4f)
        {
            passValue += 0.2f;
        }

        // 3. ���������������
        if (GetMinOpponentCards() <= 2)
        {
            passValue -= 0.2f; // ���͹��Ƽ�ֵ
        }

        return Mathf.Clamp01(passValue);
    }

    private List<CardCombination> GenerateTripleStraightCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. ���ɻ�
        for (int len = 6; len <= Mathf.Min(18, HandCards.Count); len += 3)
        {
            var planes = AICardFinder.FindTripleStraight(HandCards, -1, len);
            if (planes.Any())
            {
                actions.Add(CreatePlayAction(planes, CardType.TripleStraight));

                // 2. ������
                var singles = FindAttachableCards(planes, planes.Count / 3);
                foreach (var attachCards in singles)
                {
                    var planeWithSingle = new List<Card>(planes);
                    planeWithSingle.AddRange(attachCards);
                    actions.Add(CreatePlayAction(planeWithSingle, CardType.TripleStraightWithSingle));
                }

                // 3. ������
                var pairs = AICardFinder.FindPurePairs(HandCards.Except(planes).ToList());
                if (pairs.Count >= planes.Count / 3)
                {
                    var planeWithPairs = new List<Card>(planes);
                    for (int i = 0; i < planes.Count / 3; i++)
                    {
                        planeWithPairs.AddRange(pairs[i]);
                    }
                    actions.Add(CreatePlayAction(planeWithPairs, CardType.TripleStraightWithPair));
                }
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateFourWithExtraCombinations()
    {
        var actions = new List<CardCombination>();
        var bombs = AICardFinder.FindBomb(HandCards, -1);
        foreach (var bomb in bombs)
        {
            // 1. �Ĵ���
            var singles = FindAttachableCards(bomb, 2);
            foreach (var attachCards in singles)
            {
                var fourWithTwo = new List<Card>(bomb);
                fourWithTwo.AddRange(attachCards);
                actions.Add(CreatePlayAction(fourWithTwo, CardType.FourWithTwo));
            }

            // 2. �Ĵ�����
            var pairs = AICardFinder.FindPurePairs(HandCards.Except(bomb).ToList());
            if (pairs.Count >= 2)
            {
                var fourWithPairs = new List<Card>(bomb);
                fourWithPairs.AddRange(pairs[0]);
                fourWithPairs.AddRange(pairs[1]);
                actions.Add(CreatePlayAction(fourWithPairs, CardType.FourWithTwoPair));
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateThreeCardCombinations()
    {
        var actions = new List<CardCombination>();
        var threes = AICardFinder.FindPureTriples(HandCards);
        foreach (var three in threes)
        {
            // 1. ������
            actions.Add(CreatePlayAction(three, CardType.Three));

            var remainingCards = HandCards.Except(three).ToList();

            // 2. ����һ
            var singles = FindNonKeyCards(remainingCards, 1);
            foreach (var single in singles)
            {
                var threeWithOne = new List<Card>(three);
                threeWithOne.AddRange(single);
                actions.Add(CreatePlayAction(threeWithOne, CardType.ThreeWithOne));
            }

            // 3. ������
            var pairs = AICardFinder.FindPurePairs(remainingCards);
            foreach (var pair in pairs)
            {
                var threeWithPair = new List<Card>(three);
                threeWithPair.AddRange(pair);
                actions.Add(CreatePlayAction(threeWithPair, CardType.ThreeWithPair));
            }
        }

        return actions;
    }

    private float GetCardValue(Card card)
    {
        // ������
        float value = (float)card.rank;

        // �����Ƽӳ�
        if (card.rank == Rank.LJoker) value += 20;
        if (card.rank == Rank.SJoker) value += 18;
        if (card.rank == Rank.Two) value += 15;

        return value;
    }

    private bool WouldBreakValuableCombination(Card card)
    {
        var currentCombos = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        var remainingCombos = AICardAnalyzer.FindAllPotentialCombinations(
            HandCards.Where(c => c != card).ToList());

        // �����Ƴ������ƺ�ļ�ֵ��ʧ
        float currentValue = currentCombos.Sum(c => c.Value);
        float remainingValue = remainingCombos.Sum(c => c.Value);

        return (currentValue - remainingValue) > 1.5f; // ��ֵ��ʧ��ֵ
    }

    private List<CardCombination> GenerateTypeSpecificResponse(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        switch (lastPlay.CardType)
        {
            case CardType.Single:
                actions.AddRange(GenerateBiggerSingles(lastPlay.Weight));
                break;
            case CardType.Pair:
                actions.AddRange(GenerateBiggerPairs(lastPlay.Weight));
                break;
            case CardType.Three:
            case CardType.ThreeWithOne:
            case CardType.ThreeWithPair:
                actions.AddRange(GenerateBiggerThreePatterns(lastPlay));
                break;
            case CardType.Straight:
                actions.AddRange(GenerateBiggerStraight(lastPlay));
                break;
            case CardType.PairStraight:
                actions.AddRange(GenerateBiggerPairStraight(lastPlay));
                break;
            case CardType.TripleStraight:
            case CardType.TripleStraightWithSingle:
            case CardType.TripleStraightWithPair:
                actions.AddRange(GenerateBiggerTripleStraight(lastPlay));
                break;
        }

        return actions;
    }

    private void UpdatePlayedCards(MCTSGameState state, CardCombination action)
    {
        if (!state.PlayedCards.ContainsKey(CurrentPlayer))
        {
            state.PlayedCards[CurrentPlayer] = new List<Card>();
        }
        state.PlayedCards[CurrentPlayer].AddRange(action.Cards);
    }

    private void UpdateLastPlay(MCTSGameState state, CardCombination action)
    {
        state.LastPlay = new LastPlayInfo
        {
            Character = CurrentPlayer,
            CardType = action.Type,
            Length = action.Cards.Count,
            Weight = CardManager.GetWeight(action.Cards, action.Type)
        };
    }
    #endregion
    #region �ж����ɺ�ִ��
    private List<CardCombination> GenerateAllPossiblePlays()
    {
        var plays = new List<CardCombination>();

        // ��������ɳ���
        if (LastPlay == null || LastPlay.CardType == CardType.Invalid)
        {
            // ���ճ��������ͼ�ֵ��������ȼ�����
            plays.AddRange(GenerateTripleStraightPatterns());  // �ɻ�ϵ�У�����ƣ�
            plays.AddRange(GenerateFourWithExtraPatterns());   // �Ĵ���ϵ��
            plays.AddRange(GenerateStraightPatterns());        // ˳�Ӻ�����
            plays.AddRange(GenerateThreeCardsPatterns());      // ����ϵ��
            plays.AddRange(GeneratePairs());                   // ����
            plays.AddRange(GenerateSingleCards());             // ���ƣ����٣�
            plays.AddRange(GenerateBombPatterns());           // ը������ը�����⣩


            // ���ճ��Ƽ�ֵ����
            plays = plays.OrderByDescending(p => p.Value).ToList();
        }
        else
        {
            // �����ܴ���ϼҵ���
            plays.AddRange(GenerateBetterPlays(LastPlay));
        }

        return plays;
    }

    private List<CardCombination> GenerateBetterPlays(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // ����ϼҳ�����ը��
        if (lastPlay.CardType == CardType.Bomb)
        {
            // ֻ���ø����ը������ը
            var bombs = AICardFinder.FindBomb(HandCards, lastPlay.Weight);
            if (bombs.Any())
            {
                actions.Add(new CardCombination
                {
                    Cards = bombs,
                    Type = CardType.Bomb,
                    Value = CalculatePlayValue(bombs, CardType.Bomb) * 2
                });
            }

            var rocket = AICardFinder.FindJokerBomb(HandCards);
            if (rocket.Any())
            {
                actions.Add(new CardCombination
                {
                    Cards = rocket,
                    Type = CardType.JokerBomb,
                    Value = CalculatePlayValue(rocket, CardType.JokerBomb) * 2
                });
            }
            return actions;
        }

        // �������ͨ���ͣ������������ɶ�Ӧ�ĸ�����
        switch (lastPlay.CardType)
        {
            case CardType.Single:
                return GenerateSingleCards();
            case CardType.Pair:
                return GeneratePairs();
            case CardType.Three:
            case CardType.ThreeWithOne:
            case CardType.ThreeWithPair:
                return GenerateThreeCardsPatterns();
            case CardType.Straight:
            case CardType.PairStraight:
                return GenerateStraightPatterns();
            case CardType.TripleStraight:
            case CardType.TripleStraightWithSingle:
            case CardType.TripleStraightWithPair:
                return GenerateTripleStraightPatterns();
            case CardType.FourWithTwo:
            case CardType.FourWithTwoPair:
                return GenerateFourWithExtraPatterns();
        }

        // ������ʱ��ը������ը
        actions.AddRange(GenerateBombPatterns());

        return actions;
    }
    #endregion

    #region GenerateBiggerϵ�з���
    private List<CardCombination> GenerateBiggerSingles(int weight)
    {
        var actions = new List<CardCombination>();

        // ��˳���ҳ������ܴ��weight�ĵ���
        var validSingles = HandCards
            .Where(card => GetCardValue(card) > weight)
            .OrderBy(card => GetCardValue(card));

        foreach (var card in validSingles)
        {
            // �������м�ֵ�����
            if (!WouldBreakValuableCombination(card))
            {
                actions.Add(CreatePlayAction(
                    new List<Card> { card },
                    CardType.Single
                ));
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateBiggerPairs(int weight)
    {
        var actions = new List<CardCombination>();

        // ʹ��AICardFinder�ҵ����ж���
        var pairs = AICardFinder.FindPurePairs(HandCards)
            .Where(pair => CardManager.GetWeight(pair, CardType.Pair) > weight)
            .OrderBy(pair => CardManager.GetWeight(pair, CardType.Pair));

        foreach (var pair in pairs)
        {
            if (!WouldBreakValuableCombination(pair[0]))
            {
                actions.Add(CreatePlayAction(pair, CardType.Pair));
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateBiggerThreePatterns(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // �ҵ���������
        var threes = AICardFinder.FindPureTriples(HandCards)
            .Where(three => CardManager.GetWeight(three, CardType.Three) > lastPlay.Weight);

        foreach (var three in threes)
        {
            switch (lastPlay.CardType)
            {
                case CardType.Three:
                    actions.Add(CreatePlayAction(three, CardType.Three));
                    break;

                case CardType.ThreeWithOne:
                    var singles = FindAttachableSingles(three);
                    foreach (var single in singles)
                    {
                        var combined = new List<Card>(three);
                        combined.Add(single);
                        actions.Add(CreatePlayAction(combined, CardType.ThreeWithOne));
                    }
                    break;

                case CardType.ThreeWithPair:
                    var pairs = FindAttachablePairs(three);
                    foreach (var pair in pairs)
                    {
                        var combined = new List<Card>(three);
                        combined.AddRange(pair);
                        actions.Add(CreatePlayAction(combined, CardType.ThreeWithPair));
                    }
                    break;
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateBiggerStraight(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // �ҵ���ͬ���ȵ�˳��
        var straights = AICardFinder.FindStraight(
            HandCards,
            lastPlay.Weight,
            lastPlay.Length
        );

        if (straights.Any())
        {
            actions.Add(CreatePlayAction(straights, CardType.Straight));
        }

        return actions;
    }

    private List<CardCombination> GenerateBiggerPairStraight(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // �ҵ���ͬ���ȵ�����
        var pairStraights = AICardFinder.FindPairStraight(
            HandCards,
            lastPlay.Weight,
            lastPlay.Length
        );

        if (pairStraights.Any())
        {
            actions.Add(CreatePlayAction(pairStraights, CardType.PairStraight));
        }

        return actions;
    }

    private List<CardCombination> GenerateBiggerTripleStraight(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // �ҵ������ɻ�
        var planes = AICardFinder.FindTripleStraight(
            HandCards,
            lastPlay.Weight,
            lastPlay.Length
        );

        if (!planes.Any()) return actions;

        switch (lastPlay.CardType)
        {
            case CardType.TripleStraight:
                actions.Add(CreatePlayAction(planes, CardType.TripleStraight));
                break;

            case CardType.TripleStraightWithSingle:
                var singles = FindAttachableSinglesForPlane(planes);
                foreach (var singleCombo in singles)
                {
                    var combined = new List<Card>(planes);
                    combined.AddRange(singleCombo);
                    actions.Add(CreatePlayAction(
                        combined,
                        CardType.TripleStraightWithSingle
                    ));
                }
                break;

            case CardType.TripleStraightWithPair:
                var pairs = FindAttachablePairsForPlane(planes);
                foreach (var pairCombo in pairs)
                {
                    var combined = new List<Card>(planes);
                    combined.AddRange(pairCombo);
                    actions.Add(CreatePlayAction(
                        combined,
                        CardType.TripleStraightWithPair
                    ));
                }
                break;
        }

        return actions;
    }
    #endregion

    #region GenerateBigger��������
    private List<Card> FindAttachableSingles(List<Card> mainCards)
    {
        var result = new List<Card>();
        var remainingCards = HandCards.Except(mainCards).ToList();

        // ����ѡ��ǹؼ���
        var nonKeyCards = remainingCards
            .Where(card => !IsKeyCard(card))
            .OrderBy(card => GetCardValue(card))
            .Take(2)
            .ToList();

        if (nonKeyCards.Any())
        {
            result.AddRange(nonKeyCards);
        }
        else
        {
            // ���û�зǹؼ��ƣ�ѡ����С����
            var smallestCards = remainingCards
                .OrderBy(card => GetCardValue(card))
                .Take(2)
                .ToList();
            result.AddRange(smallestCards);
        }

        return result;
    }

    private List<List<Card>> FindAttachablePairs(List<Card> mainCards)
    {
        var result = new List<List<Card>>();
        var remainingCards = HandCards.Except(mainCards).ToList();

        // �ҵ����ж���
        var pairs = AICardFinder.FindPurePairs(remainingCards);

        // ����ֵ����ѡ���С�Ķ���
        result.AddRange(pairs.OrderBy(pair => CardManager.GetWeight(pair, CardType.Pair)));

        return result;
    }

    private List<List<Card>> FindAttachableSinglesForPlane(List<Card> planes)
    {
        var result = new List<List<Card>>();
        var remainingCards = HandCards.Except(planes).ToList();

        // ��Ҫ�ĵ�������
        int requiredCount = planes.Count / 3;

        // ʹ������㷨�ҵ����п��ܵĵ������
        var combinations = GetCombinations(remainingCards, requiredCount);

        // ���˺��������
        result.AddRange(combinations
            .Where(combo => !WouldBreakValuableCombinations(combo))
            .OrderBy(combo => combo.Sum(card => GetCardValue(card)))
        );

        return result;
    }

    private List<List<Card>> FindAttachablePairsForPlane(List<Card> planes)
    {
        var result = new List<List<Card>>();
        var remainingCards = HandCards.Except(planes).ToList();

        // ��Ҫ�Ķ�������
        int requiredPairs = planes.Count / 3;

        // �ҵ����п��õĶ���
        var pairs = AICardFinder.FindPurePairs(remainingCards);

        // �����������������ֱ�ӷ��ؿ��б�
        if (pairs.Count < requiredPairs)
            return result;

        // ʹ������㷨�ҵ����п��ܵĶ������
        var pairCombinations = GetCombinations(pairs, requiredPairs);
        foreach (var pairCombo in pairCombinations)
        {
            var flattenedCombo = pairCombo.SelectMany(pair => pair).ToList();
            if (!WouldBreakValuableCombinations(flattenedCombo))
            {
                result.Add(flattenedCombo);
            }
        }

        return result;
    }

    private bool WouldBreakValuableCombinations(List<Card> cards)
    {
        // ����Ƴ���Щ���Ƿ���ƻ���Ҫ���
        var remainingCards = HandCards.Except(cards).ToList();
        var currentValue = EvaluateHandStrength();
        var remainingValue = AICardAnalyzer.CalculateHandStrength(remainingCards, IsLandlord);

        return (currentValue - remainingValue) > 0.3f; // ��ֵ��ʧ��ֵ
    }

    private bool IsKeyCard(Card card)
    {
        return card.rank >= Rank.Two ||
               card.rank == Rank.SJoker ||
               card.rank == Rank.LJoker;
    }
    #endregion

    #region ��������
    private bool CanBeatLastPlay(List<Card> cards, CardType type)
    {
        // ��ը�ܹ��κ���
        if (type == CardType.JokerBomb) return true;

        // ը���ܹ��κη���ը����
        if (type == CardType.Bomb && LastPlay.CardType != CardType.JokerBomb)
        {
            if (LastPlay.CardType != CardType.Bomb) return true;
            return CardManager.GetWeight(cards, type) > LastPlay.Weight;
        }

        // �������ͱ�����ͬ�Ҹ���
        if (type != LastPlay.CardType || cards.Count != LastPlay.Length)
            return false;

        return CardManager.GetWeight(cards, type) > LastPlay.Weight;
    }

    private CharacterType GetNextPlayer()
    {
        // ���ݵ�ǰ��ҷ�����һ�����
        switch (CurrentPlayer)
        {
            case CharacterType.Player:
                return CharacterType.RightComputer;
            case CharacterType.RightComputer:
                return CharacterType.LeftComputer;
            case CharacterType.LeftComputer:
                return CharacterType.Player;
            default:
                return CharacterType.Player;
        }
    }
    #endregion

    // ��ͬ���͵����ɷ���
    #region �������ɷ���
    private List<CardCombination> GenerateSingleCards()
    {
        var actions = new List<CardCombination>();
        var weight = LastPlay?.CardType == CardType.Single ? LastPlay.Weight : -1;

        // ʹ��AICardFinder���ҵ���
        var singles = AICardFinder.FindSingle(HandCards, weight);
        if (singles.Any())
        {
            actions.Add(new CardCombination
            {
                Cards = singles,
                Type = CardType.Single,
                Value = CalculatePlayValue(singles, CardType.Single)
            });
        }

        return actions;
    }

    private List<CardCombination> GeneratePairs()
    {
        var actions = new List<CardCombination>();
        var weight = LastPlay?.CardType == CardType.Pair ? LastPlay.Weight : -1;

        // ʹ��AICardFinder���Ҷ���
        var pairs = AICardFinder.FindPurePairs(HandCards);
        foreach (var pair in pairs)
        {
            if (CardManager.GetWeight(pair, CardType.Pair) > weight)
            {
                actions.Add(new CardCombination
                {
                    Cards = new List<Card>(pair),
                    Type = CardType.Pair,
                    Value = CalculatePlayValue(pair, CardType.Pair)
                });
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateThreeCardsPatterns()
    {
        var actions = new List<CardCombination>();
        var weight = LastPlay?.CardType == CardType.Three ? LastPlay.Weight : -1;

        // 1. ������
        var threes = AICardFinder.FindPureTriples(HandCards);
        foreach (var three in threes)
        {
            if (CardManager.GetWeight(three, CardType.Three) > weight)
            {
                actions.Add(new CardCombination
                {
                    Cards = new List<Card>(three),
                    Type = CardType.Three,
                    Value = CalculatePlayValue(three, CardType.Three)
                });

                // 2. ����һ
                var singles = FindAttachableCards(three, 1);
                foreach (var single in singles)
                {
                    var threeWithOne = new List<Card>(three);
                    threeWithOne.AddRange(single);
                    actions.Add(new CardCombination
                    {
                        Cards = threeWithOne,
                        Type = CardType.ThreeWithOne,
                        Value = CalculatePlayValue(threeWithOne, CardType.ThreeWithOne)
                    });
                }

                // 3. ������
                var pairs = AICardFinder.FindPurePairs(HandCards.Except(three).ToList());
                foreach (var pair in pairs)
                {
                    var threeWithPair = new List<Card>(three);
                    threeWithPair.AddRange(pair);
                    actions.Add(new CardCombination
                    {
                        Cards = threeWithPair,
                        Type = CardType.ThreeWithPair,
                        Value = CalculatePlayValue(threeWithPair, CardType.ThreeWithPair)
                    });
                }
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateStraightPatterns()
    {
        var actions = new List<CardCombination>();

        // 1. ˳�� (5�ż�����)
        for (int len = 5; len <= Mathf.Min(12, HandCards.Count); len++)
        {
            var straights = AICardFinder.FindStraight(HandCards, LastPlay?.Weight ?? -1, len);
            if (straights.Any())
            {
                actions.Add(new CardCombination
                {
                    Cards = straights,
                    Type = CardType.Straight,
                    Value = CalculatePlayValue(straights, CardType.Straight)
                });
            }
        }

        // 2. ���� (3�Լ�����)
        for (int len = 6; len <= Mathf.Min(20, HandCards.Count); len += 2)
        {
            var pairStraights = AICardFinder.FindPairStraight(HandCards, LastPlay?.Weight ?? -1, len);
            if (pairStraights.Any())
            {
                actions.Add(new CardCombination
                {
                    Cards = pairStraights,
                    Type = CardType.PairStraight,
                    Value = CalculatePlayValue(pairStraights, CardType.PairStraight)
                });
            }
        }

        return actions;
    }

    private List<CardCombination> GenerateTripleStraightPatterns()
    {

        var actions = new List<CardCombination>();
        // 1. ���ɻ�
        for (int len = 6; len <= Mathf.Min(18, HandCards.Count); len += 3)
        {
            var planes = AICardFinder.FindTripleStraight(HandCards, LastPlay?.Weight ?? -1, len);
            if (planes.Any())
            {
                actions.Add(new CardCombination
                {
                    Cards = planes,
                    Type = CardType.TripleStraight,
                    Value = CalculatePlayValue(planes, CardType.TripleStraight)
                });

                // 2. �ɻ�����
                var singles = FindAttachableCards(planes, planes.Count / 3);
                foreach (var attachCards in singles)
                {
                    var planeWithSingle = new List<Card>(planes);
                    planeWithSingle.AddRange(attachCards);
                    actions.Add(new CardCombination
                    {
                        Cards = planeWithSingle,
                        Type = CardType.TripleStraightWithSingle,
                        Value = CalculatePlayValue(planeWithSingle, CardType.TripleStraightWithSingle)
                    });
                }

                // 3. �ɻ�����
                var pairs = AICardFinder.FindPurePairs(HandCards.Except(planes).ToList());
                if (pairs.Count >= planes.Count / 3)
                {
                    var planeWithPairs = new List<Card>(planes);
                    for (int i = 0; i < planes.Count / 3; i++)
                    {
                        planeWithPairs.AddRange(pairs[i]);
                    }
                    actions.Add(new CardCombination
                    {
                        Cards = planeWithPairs,
                        Type = CardType.TripleStraightWithPair,
                        Value = CalculatePlayValue(planeWithPairs, CardType.TripleStraightWithPair)
                    });
                }
            }

        }
        return actions;
    }

    private List<CardCombination> GenerateFourWithExtraPatterns()
    {
        var actions = new List<CardCombination>();
        var bombs = AICardFinder.FindBomb(HandCards, -1);
        if (bombs.Any())
        {
            // 1. �Ĵ���
            var singles = FindAttachableCards(bombs, 2);
            foreach (var attachCards in singles)
            {
                var fourWithTwo = new List<Card>(bombs);
                fourWithTwo.AddRange(attachCards);
                actions.Add(new CardCombination
                {
                    Cards = fourWithTwo,
                    Type = CardType.FourWithTwo,
                    Value = CalculatePlayValue(fourWithTwo, CardType.FourWithTwo)
                });
            }

            // 2. �Ĵ�����
            var pairs = AICardFinder.FindPurePairs(HandCards.Except(bombs).ToList());
            if (pairs.Count >= 2)
            {
                var fourWithPairs = new List<Card>(bombs);
                fourWithPairs.AddRange(pairs[0]);
                fourWithPairs.AddRange(pairs[1]);
                actions.Add(new CardCombination
                {
                    Cards = fourWithPairs,
                    Type = CardType.FourWithTwoPair,
                    Value = CalculatePlayValue(fourWithPairs, CardType.FourWithTwoPair)
                });
            }
        }
        return actions;
    }

    private List<CardCombination> GenerateBombPatterns()
    {
        var actions = new List<CardCombination>();
        // 1. ��ͨը��
        var bombs = AICardFinder.FindBomb(HandCards, LastPlay?.CardType == CardType.Bomb ? LastPlay.Weight : -1);
        if (bombs.Any())
        {
            actions.Add(new CardCombination
            {
                Cards = bombs,
                Type = CardType.Bomb,
                Value = CalculatePlayValue(bombs, CardType.Bomb) * 2 // ը����ֵ����
            });
        }

        // 2. ��ը
        var rocket = AICardFinder.FindJokerBomb(HandCards);
        if (rocket.Any())
        {
            actions.Add(new CardCombination
            {
                Cards = rocket,
                Type = CardType.JokerBomb,
                Value = CalculatePlayValue(rocket, CardType.JokerBomb) * 2 // ��ը��ֵ����
            });
        }
        return actions;
    }
    #endregion

    #region ��������
    private float CalculatePlayValue(List<Card> cards, CardType type)
    {
        float baseValue = cards.Count; // ������Ϊ��������

        // ������������Ȩ��
        switch (type)
        {
            case CardType.JokerBomb:
                baseValue *= 5.0f; // ��ը��߼�ֵ
                break;
            case CardType.Bomb:
                baseValue *= 4.0f; // ը���θ߼�ֵ
                break;
            case CardType.FourWithTwo:
            case CardType.FourWithTwoPair:
                baseValue *= 3.0f; // �Ĵ���/�Ĵ����Ը�Ȩ��
                break;
            case CardType.TripleStraight:
            case CardType.TripleStraightWithSingle:
            case CardType.TripleStraightWithPair:
                baseValue *= 2.5f; // �ɻ�ϵ�нϸ�Ȩ��
                break;
            case CardType.Straight:
            case CardType.PairStraight:
                baseValue *= 2.0f; // ˳�Ӻ������е�Ȩ��
                break;
            case CardType.ThreeWithPair:
                baseValue *= 1.8f;
                break;
            case CardType.ThreeWithOne:
                baseValue *= 1.5f;
                break;
            default:
                baseValue *= 1.0f;
                break;
        }

        // ����ʣ��������Ӱ��
        if (HandCards.Count - cards.Count <= 4)
        {
            baseValue *= 1.5f; // ��������ʣ���������٣����Ȩ��
        }

        // ���Ƕ���ʣ��������Ӱ��
        var minOpponentCards = RemainingCards.Values.Where(v => v > 0).Min();
        if (minOpponentCards <= 3)
        {
            baseValue *= 1.3f; // ������ֿ�����ˣ����Ȩ��
        }

        return baseValue;
    }

    private List<List<Card>> FindAttachableCards(List<Card> mainCards, int count)
    {
        var result = new List<List<Card>>();
        var remainingCards = HandCards.Except(mainCards).ToList();

        // ���ʣ���Ʋ�����ֱ�ӷ��ؿ��б�
        if (remainingCards.Count < count) return result;

        // ʹ���Ż��������㷨
        return GetCombinations(remainingCards, count);
    }

    private List<List<Card>> GetCombinations(List<Card> cards, int count)
    {
        var result = new List<List<Card>>();
        if (count == 0)
        {
            result.Add(new List<Card>());
            return result;
        }

        if (count > cards.Count) return result;

        // �ݹ��������
        void GenerateCombinations(List<Card> current, int start, int remaining)
        {
            if (remaining == 0)
            {
                result.Add(new List<Card>(current));
                return;
            }

            for (int i = start; i <= cards.Count - remaining; i++)
            {
                current.Add(cards[i]);
                GenerateCombinations(current, i + 1, remaining - 1);
                current.RemoveAt(current.Count - 1);
            }
        }

        GenerateCombinations(new List<Card>(), 0, count);
        return result;
    }

    // �Ż�����㷨�ĸ�������
    private bool IsValidCombination(List<Card> cards)
    {
        // �����ֶ��ӡ����ŵ��м�ֵ�����
        var groups = cards.GroupBy(c => c.rank).ToList();
        foreach (var group in groups)
        {
            if (group.Count() > 1) return false;
        }
        return true;
    }
    #endregion

    #region ��Ϸ��������ϵͳ
    private const float END_GAME_THRESHOLD = 5;    // �о���ֵ
    private const float WIN_BONUS = 2.0f;          // ��ʤ�ӳ�
    private const float CONTROL_WEIGHT = 0.4f;     // ����Ȩ��Ҫ��
    private const float COMBO_WEIGHT = 0.3f;       // �����Ҫ��
    private const float PROGRESS_WEIGHT = 0.3f;    // ������Ҫ��

    /// <summary>
    /// ��Ϸ�׶�����
    /// </summary>
    private GamePhase GetGamePhase()
    {
        // �������������ͻغ����ж���Ϸ�׶�
        if (HandCards.Count <= END_GAME_THRESHOLD || GetMinOpponentCards() <= END_GAME_THRESHOLD)
            return GamePhase.Endgame;
        if (HandCards.Count <= 10)
            return GamePhase.Middle;
        return GamePhase.Opening;
    }

    /// <summary>
    /// ����Ԥ������
    /// </summary>
    private float PredictPositionScore()
    {
        float score = 0;

        // 1. ������������
        score += EvaluateBasePosition();

        // 2. ����ض�����
        score += EvaluateRoleSpecificPosition();

        // 3. �ؼ�ʱ������
        score += EvaluateCriticalMoment();

        return Mathf.Clamp01(score);
    }

    private float EvaluateBasePosition()
    {
        float score = 0;
        var phase = GetGamePhase();

        // 1. ���������Ա�
        float cardCountScore = EvaluateCardCountAdvantage();
        score += cardCountScore * PROGRESS_WEIGHT;

        // 2. �����������
        float comboScore = EvaluateComboStrength();
        score += comboScore * COMBO_WEIGHT;

        // 3. ����������
        float controlScore = EvaluateControlStrength();
        score += controlScore * CONTROL_WEIGHT;

        // ������Ϸ�׶ε���
        switch (phase)
        {
            case GamePhase.Endgame:
                score *= 1.5f;  // �оּӳ�
                break;
            case GamePhase.Middle:
                score *= 1.2f;  // �оּӳ�
                break;
        }

        return score;
    }

    private float EvaluateRoleSpecificPosition()
    {
        if (IsLandlord)
        {
            return EvaluateLandlordPosition();
        }
        return EvaluateFarmerPosition();
    }

    private float EvaluateLandlordPosition()
    {
        float score = 0;

        // 1. ũ����������
        var farmerMinCards = GetMinFarmerCards();
        if (HandCards.Count <= farmerMinCards)
        {
            score += 0.3f;
        }

        // 2. �ؼ��ƿ���
        if (HasDominantCards())
        {
            score += 0.2f;
        }

        // 3. �������
        if (HasGoodTempo())
        {
            score += 0.2f;
        }

        return score;
    }

    private float EvaluateFarmerPosition()
    {
        float score = 0;

        // 1. ����������Ա�
        var landlordCards = RemainingCards[LandlordPlayer];
        if (HandCards.Count < landlordCards)
        {
            score += 0.3f;
        }

        // 2. ����Эͬ
        if (HasGoodCooperation())
        {
            score += 0.3f;
        }

        // 3. ��������
        if (HasGoodDefense())
        {
            score += 0.2f;
        }

        return score;
    }

    private float EvaluateCriticalMoment()
    {
        float score = 0;

        // 1. ������ʤ
        if (IsCloseToWinning())
        {
            score += 0.4f;
        }

        // 2. Σ�վ���
        if (IsInDanger())
        {
            score -= 0.3f;
        }

        // 3. �ؼ��غ�
        if (IsCriticalTurn())
        {
            score *= 1.5f;
        }

        return Mathf.Clamp01(score);
    }
    #endregion

    #region ����������������
    private float EvaluateCardCountAdvantage()
    {
        float advantage;

        if (IsLandlord)
        {
            // ������������
            var avgFarmerCards = RemainingCards
                .Where(kv => PlayerIdentities[kv.Key] == Identity.Farmer)
                .Average(kv => kv.Value);

            advantage = 1.0f - (HandCards.Count / (avgFarmerCards * 2));
        }
        else
        {
            // ũ����������
            var landlordCards = RemainingCards[LandlordPlayer];
            advantage = 1.0f - (HandCards.Count / (float)landlordCards);
        }

        return Mathf.Clamp01(advantage);
    }

    private float EvaluateComboStrength()
    {
        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        if (!combinations.Any()) return 0;

        float totalStrength = combinations.Sum(combo =>
        {
            float baseValue = combo.Value;

            // ���ͼ�Ȩ
            float typeMultiplier = GetComboTypeMultiplier(combo.Type);

            // �ؼ��Լ�Ȩ
            float keyMultiplier = combo.IsKey ? 1.5f : 1.0f;

            return baseValue * typeMultiplier * keyMultiplier;
        });

        return Mathf.Clamp01(totalStrength / (combinations.Count * 5f));
    }

    private float EvaluateControlStrength()
    {
        float control = 0;

        // 1. ����Ȩ
        if (HasInitiative())
        {
            control += 0.3f;
        }

        // 2. ը������
        int bombCount = AICardAnalyzer.CountBombs(HandCards);
        control += bombCount * 0.15f;

        // 3. ���ƿ���
        float bigCardRatio = HandCards.Count(IsKeyCard) / (float)HandCards.Count;
        control += bigCardRatio * 0.2f;

        // 4. ���Ͷ�����
        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        float diversityBonus = Mathf.Min(combinations.Select(c => c.Type).Distinct().Count() * 0.05f, 0.3f);
        control += diversityBonus;

        return Mathf.Clamp01(control);
    }

    private float GetComboTypeMultiplier(CardType type)
    {
        return type switch
        {
            CardType.JokerBomb => 2.0f,
            CardType.Bomb => 1.8f,
            CardType.TripleStraight => 1.5f,
            CardType.Straight => 1.3f,
            CardType.PairStraight => 1.3f,
            CardType.ThreeWithPair => 1.2f,
            _ => 1.0f
        };
    }

    private bool HasDominantCards()
    {
        return AICardAnalyzer.CountBombs(HandCards) >= 1 ||
               AICardAnalyzer.HasJokerBomb(HandCards) ||
               HandCards.Count(c => c.rank >= Rank.Two) >= 2;
    }

    private bool HasGoodTempo()
    {
        return LastPlay?.Character == CurrentPlayer ||
               ConsecutivePassCount >= 2;
    }

    private bool HasGoodDefense()
    {
        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);

        // ����Ƿ����㹻�ķ�������
        return combinations.Any(c => c.Type == CardType.Bomb) ||
               combinations.Any(c => c.Type == CardType.JokerBomb) ||
               HandCards.Count(IsKeyCard) >= 2;
    }

    private bool IsCloseToWinning()
    {
        return HandCards.Count <= 3 ||
              (HandCards.Count <= 4 && AICardAnalyzer.CalculateHandStrength(HandCards) > 0.7f);
    }

    private bool IsInDanger()
    {
        if (IsLandlord)
        {
            return GetMinFarmerCards() <= 2;
        }
        return RemainingCards[LandlordPlayer] <= 2;
    }

    private bool IsCriticalTurn()
    {
        return IsCloseToWinning() ||
               IsInDanger() ||
               ConsecutivePassCount >= 2 ||
               GetGamePhase() == GamePhase.Endgame;
    }

    private bool HasInitiative()
    {
        return LastPlay == null ||
               LastPlay.Character == CurrentPlayer ||
               ConsecutivePassCount >= 2;
    }
    #endregion

    #region ��������
    private CharacterType GetLandlordPlayer()
    {
        // GameStateContext���Ѿ�����Landlord����Ϣ
        if (IsLandlord)
            return CurrentPlayer;
        else
        {
            // ����Լ����ǵ������� LeftPlayer �� RightPlayer ���ҵ�����
            if (LeftPlayer?.Identity == Identity.Landlord)
                return CharacterType.LeftComputer;
            else if (RightPlayer?.Identity == Identity.Landlord)
                return CharacterType.RightComputer;
        }

        // �����ڼ䱣��
        Debug.LogWarning("�޷�ȷ��������ɫ�����ص�ǰ���");
        return CurrentPlayer;
    }

    private CharacterType GetPartnerPlayer()
    {
        // ����ǵ�����û�ж���
        if (IsLandlord)
            return CurrentPlayer;

        // ����Լ���ũ���ҵ���һ��ũ��
        CharacterType leftType = CharacterType.LeftComputer;
        CharacterType rightType = CharacterType.RightComputer;

        // �������ǵ������Ҽ��Ƕ���
        if (LeftPlayer?.Identity == Identity.Landlord)
            return rightType;
        // ����Ҽ��ǵ���������Ƕ���
        else if (RightPlayer?.Identity == Identity.Landlord)
            return leftType;
        // ���ݵ�ǰ���λ��ȷ������
        else
        {
            switch (CurrentPlayer)
            {
                case CharacterType.Player:
                    return leftType;  // ����Ĭ������Ƕ���
                case CharacterType.LeftComputer:
                    return rightType;
                case CharacterType.RightComputer:
                    return leftType;
                default:
                    Debug.LogWarning("�޷�ȷ�����ѽ�ɫ�����ص�ǰ���");
                    return CurrentPlayer;
            }
        }
    }

    // ���һ���µĸ����������ж��Ƿ��Ƕ���
    public bool IsPartner(CharacterType playerType)
    {
        if (IsLandlord)
            return false;  // ����û�ж���

        var partnerType = GetPartnerPlayer();
        return playerType == partnerType;
    }

    // ���һ����ȡ�������ֵķ���
    public (CharacterType opponent1, CharacterType opponent2) GetOpponents()
    {
        if (IsLandlord)
        {
            // �����Ķ���������ũ��
            return (CharacterType.LeftComputer, CharacterType.RightComputer);
        }
        else
        {
            // ũ��Ķ����ǵ���
            return (GetLandlordPlayer(), CharacterType.Desk); // ʹ��Desk��Ϊ�ڶ�������ֵ��ʾû�еڶ�������
        }
    }

    // ��ȡ��һ�����Ƶ����
    public CharacterType GetNextPlayer(CharacterType currentPlayer)
    {
        switch (currentPlayer)
        {
            case CharacterType.Player:
                return CharacterType.RightComputer;
            case CharacterType.RightComputer:
                return CharacterType.LeftComputer;
            case CharacterType.LeftComputer:
                return CharacterType.Player;
            default:
                Debug.LogWarning("δ֪��������ͣ��������");
                return CharacterType.Player;
        }
    }

    // ��ȡ��һ�����Ƶ����
    public CharacterType GetPreviousPlayer(CharacterType currentPlayer)
    {
        switch (currentPlayer)
        {
            case CharacterType.Player:
                return CharacterType.LeftComputer;
            case CharacterType.LeftComputer:
                return CharacterType.RightComputer;
            case CharacterType.RightComputer:
                return CharacterType.Player;
            default:
                Debug.LogWarning("δ֪��������ͣ��������");
                return CharacterType.Player;
        }
    }
    #endregion

    private int GetPartnerRemainingCards()
    {
        var partner = GetPartnerPlayer();
        return RemainingCards.ContainsKey(partner) ? RemainingCards[partner] : 0;
    }

    private bool HasKeyCards()
    {
        return HandCards.Any(c => c.rank >= Rank.Two) ||
               HandCards.Any(c => c.rank == Rank.SJoker || c.rank == Rank.LJoker);
    }
}*/