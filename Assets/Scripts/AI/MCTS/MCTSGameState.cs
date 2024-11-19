/*using static CardManager;
using System.Collections.Generic;
using AI.Utils;
using System.Linq;
using UnityEngine;

public class MCTSGameState
{
    #region 基础状态属性
    // 基本游戏信息
    public CharacterType CurrentPlayer { get; private set; }
    public Identity PlayerRole { get; private set; }
    public bool IsLandlord => PlayerRole == Identity.Landlord;

    // 牌局信息
    public List<Card> HandCards { get; private set; }
    public Dictionary<CharacterType, int> RemainingCards { get; private set; }
    public Dictionary<CharacterType, List<Card>> PlayedCards { get; private set; }
    public List<Card> PossibleCards { get; private set; }
    public LastPlayInfo LastPlay { get; private set; }

    // 回合信息
    public int CurrentRound { get; private set; }
    public int ConsecutivePassCount { get; private set; }

    // 玩家身份信息
    private Dictionary<CharacterType, Identity> PlayerIdentities { get; set; }
    private CharacterType LandlordPlayer { get; set; }
    #endregion

    #region 构造函数
    public MCTSGameState(GameStateContext context)
    {
        InitializeFromContext(context);
    }

    // 深拷贝构造函数
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

    #region 初始化方法
    private void InitializeFromContext(GameStateContext context)
    {
        // 基础属性初始化
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
        // 玩家身份初始化
        PlayerIdentities[CharacterType.Player] = context.Self.Identity;
        PlayerIdentities[CharacterType.LeftComputer] = context.LeftPlayer.Identity;
        PlayerIdentities[CharacterType.RightComputer] = context.RightPlayer.Identity;

        // 记录地主玩家
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

    #region 游戏状态判断
    private float EvaluateStateValue()
    {
        // 游戏结束状态的评估
        if (IsTerminal())
        {
            return GetScore();
        }

        float value = 0;

        // 基础牌力评估 (40%)
        value += EvaluateHandStrength() * 0.4f;

        // 局势评估 (35%)
        value += EvaluateSituation() * 0.35f;

        // 控制权评估 (25%)
        value += EvaluateControl() * 0.25f;

        // 压制状态调整
        if (ConsecutivePassCount >= 2)
        {
            value *= 0.8f;
        }

        return Mathf.Clamp01(value);
    }

    private float EvaluateHandStrength()
    {
        float strength = AICardAnalyzer.CalculateHandStrength(HandCards, IsLandlord);

        // 残局加成
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

        // 1. 牌数对比评分 (40%)
        score += EvaluateCardCount() * 0.4f;

        // 2. 牌型组合评分 (35%)
        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        score += EvaluateCombinations(combinations) * 0.35f;

        // 3. 关键牌控制评分 (25%)
        score += EvaluateKeyCards() * 0.25f;

        return Mathf.Clamp01(score);
    }

    private float EvaluateCardCount()
    {
        float score = 0;
        float baseScore = 1.0f - (HandCards.Count / 20.0f); // 基础进度分

        if (IsLandlord)
        {
            // 地主评分逻辑
            score = EvaluateLandlordProgress(baseScore);
        }
        else
        {
            // 农民评分逻辑
            score = EvaluateFarmerProgress(baseScore);
        }

        return Mathf.Clamp01(score);
    }


    private float EvaluateControl()
    {
        float controlScore = 0;

        // 1. 出牌权评估 (35%)
        controlScore += EvaluatePlayingRights() * 0.35f;

        // 2. 牌型控制力评估 (35%)
        controlScore += EvaluatePatternControl() * 0.35f;

        // 3. 身份相关调整 (30%)
        controlScore += EvaluateRoleControl() * 0.3f;

        return Mathf.Clamp01(controlScore);
    }

    public bool IsTerminal()
    {
        // 有玩家出完牌
        if (HandCards.Count == 0) return true;

        foreach (var count in RemainingCards.Values)
        {
            if (count == 0) return true;
        }

        // 特殊终止条件（如超过最大回合数）
        if (CurrentRound >= 100) return true;

        return false;
    }

    public float GetScore()
    {
        if (!IsTerminal()) return 0;

        if (HandCards.Count == 0)
        {
            // 赢家得分
            return CalculateWinScore();
        }
        else if (RemainingCards.Any(kv => kv.Value == 0))
        {
            // 输家得分
            return CalculateLoseScore();
        }

        // 平局或其他情况
        return 0;
    }
    #endregion

    #region 评估辅助方法
    private float CalculateEndgameBonus()
    {
        float bonus = 0;

        // 炸弹和王炸额外加成
        int bombCount = AICardAnalyzer.CountBombs(HandCards);
        bool hasRocket = AICardAnalyzer.HasJokerBomb(HandCards);

        bonus += bombCount * 0.15f;
        if (hasRocket) bonus += 0.2f;

        // 关键牌加成
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
            // 根据不同牌型和身份赋予不同权重
            float weight = GetCombinationWeight(combo.Type);
            totalValue += combo.Value * weight;
        }

        return Mathf.Clamp01(totalValue / (combinations.Count * 5f));
    }

    private float EvaluateKeyCards()
    {
        float score = 0;

        // 计算关键牌价值
        foreach (var card in HandCards)
        {
            if (card.rank >= Rank.Two)
            {
                score += GetKeyCardValue(card);
            }
        }

        // 身份加成
        if (IsLandlord)
        {
            score *= 1.2f; // 地主的关键牌更重要
        }

        return Mathf.Clamp01(score);
    }

    private float EvaluateLandlordProgress(float baseScore)
    {
        float score = baseScore * 1.2f; // 地主基础分更高

        // 对比农民牌数
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

        // 与地主牌数对比
        int landlordCards = RemainingCards[LandlordPlayer];
        if (HandCards.Count < landlordCards)
        {
            score *= 1.2f;
        }

        // 与队友配合
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
        // 根据剩余对手牌数调整得分
        float remainingPenalty = GetMinOpponentCards() * 0.05f;
        return Mathf.Clamp01(baseScore - remainingPenalty);
    }

    private float CalculateLoseScore()
    {
        float baseScore = IsLandlord ? -1.0f : -0.5f;
        // 根据自己剩余牌数调整得分
        float remainingPenalty = HandCards.Count * 0.05f;
        return Mathf.Clamp01(baseScore - remainingPenalty);
    }
    #endregion

    #region 状态判断辅助方法
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

    #region 控制权评估方法
    private float EvaluatePlayingRights()
    {
        float score = 0;

        // 1. 基础出牌权评估
        if (LastPlay == null || LastPlay.Character == CurrentPlayer)
        {
            score += 0.4f; // 自由出牌权
        }

        // 2. 出牌节奏评估
        if (ConsecutivePassCount == 0)
        {
            score += 0.3f; // 良好的出牌节奏
        }
        else
        {
            score += Mathf.Max(0, 0.3f - ConsecutivePassCount * 0.1f); // 被压制时降低分数
        }

        // 3. 回合位置评估
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

        // 1. 牌型数量和质量评估
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

        // 2. 关键牌控制评估
        if (HasKeyCards())
        {
            controlScore += 0.2f;
        }

        // 3. 打击能力评估
        if (HasBeatPotential())
        {
            controlScore += 0.2f;
        }

        return Mathf.Clamp01(controlScore);
    }

    private float EvaluateRoleControl()
    {
        float controlScore = 0;

        // 1. 身份基础评分
        if (IsLandlord)
        {
            controlScore = 0.6f; // 地主基础控制力更强
        }
        else
        {
            controlScore = 0.4f; // 农民基础控制力较弱
        }

        // 2. 局势调整
        if (IsInLeadingPosition())
        {
            controlScore *= 1.2f;
        }
        else if (IsInTrailingPosition())
        {
            controlScore *= 0.8f;
        }

        // 3. 队友配合（农民）
        if (!IsLandlord && IsGoodCooperation())
        {
            controlScore *= 1.2f;
        }

        return Mathf.Clamp01(controlScore);
    }
    #endregion

    #region 控制权评估辅助方法
    private bool IsInGoodPosition()
    {
        // 检查是否在有利的出牌位置
        if (IsLandlord)
        {
            // 地主在两个农民都没有好牌的情况下位置好
            return GetConsecutivePassCount() >= 1;
        }
        else
        {
            // 农民在地主被压制时位置好
            var landlordPasses = GetPlayerPassCount(LandlordPlayer);
            return landlordPasses > 0;
        }
    }

    private bool HasBeatPotential()
    {
        // 检查是否有足够的牌可以压制对手
        if (LastPlay == null) return true;

        var combinations = AICardAnalyzer.FindAllPotentialCombinations(HandCards);
        return combinations.Any(combo => CanBeatLastPlay(combo.Cards, combo.Type));
    }

    private bool IsInLeadingPosition()
    {
        if (IsLandlord)
        {
            // 地主领先判断
            return HandCards.Count <= GetMinFarmerCards();
        }
        else
        {
            // 农民领先判断
            int landlordCards = RemainingCards[LandlordPlayer];
            return HandCards.Count < landlordCards;
        }
    }

    private bool IsInTrailingPosition()
    {
        if (IsLandlord)
        {
            // 地主落后判断
            return HandCards.Count > GetMinFarmerCards() + 2;
        }
        else
        {
            // 农民落后判断
            int landlordCards = RemainingCards[LandlordPlayer];
            return HandCards.Count > landlordCards + 2;
        }
    }

    private bool IsGoodCooperation()
    {
        if (IsLandlord) return false;

        var partnerCards = GetPartnerRemainingCards();
        // 队友牌数合适且能够联合压制地主
        return partnerCards <= 5 && HandCards.Count <= 6 &&
               Mathf.Max(partnerCards, HandCards.Count) < RemainingCards[LandlordPlayer];
    }

    private int GetConsecutivePassCount()
    {
        return ConsecutivePassCount;
    }

    private int GetPlayerPassCount(CharacterType playerType)
    {
        // 这里可以添加具体的过牌次数统计逻辑
        // 暂时返回连续过牌次数
        if (LastPlay?.Character == playerType)
        {
            return ConsecutivePassCount;
        }
        return 0;
    }
    #endregion

    #region 行动生成和执行
    public List<CardCombination> GetPossibleActions()
    {
        var actions = new List<CardCombination>();

        // 1. 过牌选项
        if (CanPass())
        {
            actions.Add(CreatePassAction());
        }

        // 2. 出牌选项
        actions.AddRange(GeneratePlayActions());

        // 3. 按价值排序并限制动作数量
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

        // 按优先级生成牌型
        actions.AddRange(GenerateHighValueCombinations());   // 高价值组合
        actions.AddRange(GenerateNormalCombinations());      // 普通组合
        actions.AddRange(GenerateBasicCombinations());       // 基础组合

        return actions;
    }

    private List<CardCombination> GenerateHighValueCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. 火箭
        var rockets = AICardFinder.FindJokerBomb(HandCards);
        if (rockets.Any())
        {
            actions.Add(CreatePlayAction(rockets, CardType.JokerBomb));
        }

        // 2. 炸弹
        var bombs = AICardFinder.FindBomb(HandCards, -1);
        if (bombs.Any())
        {
            actions.Add(CreatePlayAction(bombs, CardType.Bomb));
        }

        // 3. 飞机系列
        actions.AddRange(GenerateTripleStraightCombinations());

        return actions;
    }

    private List<CardCombination> GenerateNormalCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. 顺子
        for (int len = 5; len <= 12 && len <= HandCards.Count; len++)
        {
            var straights = AICardFinder.FindStraight(HandCards, -1, len);
            if (straights.Any())
            {
                actions.Add(CreatePlayAction(straights, CardType.Straight));
            }
        }

        // 2. 连对
        for (int len = 6; len <= 20 && len <= HandCards.Count; len += 2)
        {
            var pairStraights = AICardFinder.FindPairStraight(HandCards, -1, len);
            if (pairStraights.Any())
            {
                actions.Add(CreatePlayAction(pairStraights, CardType.PairStraight));
            }
        }

        // 3. 四带牌
        actions.AddRange(GenerateFourWithExtraCombinations());

        return actions;
    }

    private List<CardCombination> GenerateBasicCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. 三带牌组合
        var threeCardCombos = GenerateThreeCardCombinations();
        actions.AddRange(threeCardCombos);

        // 2. 对子
        var pairs = AICardFinder.FindPurePairs(HandCards);
        foreach (var pair in pairs)
        {
            actions.Add(CreatePlayAction(pair, CardType.Pair));
        }

        // 3. 单牌（按价值筛选）
        actions.AddRange(GenerateSmartSingleCards());

        return actions;
    }

    private List<CardCombination> GenerateSmartSingleCards()
    {
        var actions = new List<CardCombination>();
        var sortedCards = HandCards.OrderBy(c => GetCardValue(c)).ToList();

        // 生成单牌，但避免拆分有价值的组合
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

        // 1. 总是考虑王炸
        var rocket = AICardFinder.FindJokerBomb(HandCards);
        if (rocket.Any())
        {
            actions.Add(CreatePlayAction(rocket, CardType.JokerBomb));
        }

        // 2. 针对性响应
        if (lastPlay.CardType == CardType.Bomb)
        {
            // 只能用更大的炸弹响应
            var biggerBomb = AICardFinder.FindBomb(HandCards, lastPlay.Weight);
            if (biggerBomb.Any())
            {
                actions.Add(CreatePlayAction(biggerBomb, CardType.Bomb));
            }
        }
        else
        {
            // 可以用炸弹
            var bombs = AICardFinder.FindBomb(HandCards, -1);
            if (bombs.Any())
            {
                actions.Add(CreatePlayAction(bombs, CardType.Bomb));
            }

            // 同类型响应
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
        float baseValue = cards.Count; // 基础分：出牌数量

        // 1. 牌型价值
        float typeValue = GetTypeBaseValue(type);

        // 2. 局势加成
        float situationMultiplier = GetSituationMultiplier();

        // 3. 剩余牌数考虑
        float remainingBonus = CalculateRemainingBonus(cards);

        return (baseValue * typeValue * situationMultiplier) + remainingBonus;
    }

    private List<CardCombination> FilterAndSortActions(List<CardCombination> actions)
    {
        // 1. 移除无效动作
        actions = actions.Where(a => IsValidAction(a)).ToList();

        // 2. 按价值排序
        actions = actions.OrderByDescending(a => a.Value).ToList();

        // 3. 限制动作数量（性能考虑）
        int maxActions = DetermineMaxActions();
        return actions.Take(maxActions).ToList();
    }

    public MCTSGameState ApplyAction(CardCombination action)
    {
        var newState = new MCTSGameState(this);

        // 更新状态
        UpdateStateAfterAction(newState, action);

        return newState;
    }

    private void UpdateStateAfterAction(MCTSGameState newState, CardCombination action)
    {
        // 1. 更新手牌
        if (action.Cards.Any())
        {
            foreach (var card in action.Cards)
            {
                newState.HandCards.Remove(card);
            }

            // 更新出牌记录
            UpdatePlayedCards(newState, action);

            // 更新最后出牌信息
            UpdateLastPlay(newState, action);
        }
        else
        {
            // 过牌处理
            newState.ConsecutivePassCount++;
        }

        // 2. 更新剩余牌数
        newState.RemainingCards[CurrentPlayer] = newState.HandCards.Count;

        // 3. 更新回合信息
        newState.CurrentRound++;
        newState.CurrentPlayer = GetNextPlayer(newState.CurrentPlayer);
    }
    #endregion

    #region 行动生成辅助方法
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

        // 根据局势调整
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

        // 出牌后剩余牌数考虑
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
        if (action.Cards.Count == 0) return true; // 过牌总是有效

        // 1. 检查是否拥有这些牌
        foreach (var card in action.Cards)
        {
            if (!HandCards.Contains(card)) return false;
        }

        // 2. 检查牌型是否合法
        if (action.Type == CardType.Invalid) return false;

        // 3. 检查是否能大过上家
        if (LastPlay != null && LastPlay.CardType != CardType.Invalid)
        {
            return CanBeatLastPlay(action.Cards, action.Type);
        }

        return true;
    }

    private int DetermineMaxActions()
    {
        // 根据局势决定保留的动作数量
        if (IsEndgame())
        {
            return 20; // 残局保留更多选项
        }
        return 15; // 一般情况
    }
    #endregion

    #region 动作生成补充方法
    private bool CanPass()
    {
        // 1. 是否是第一手牌
        if (LastPlay == null || LastPlay.CardType == CardType.Invalid)
            return false;

        // 2. 是否是自己的牌
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
        float passValue = 0.1f; // 基础过牌价值较低

        // 1. 如果队友出了好牌
        if (!IsLandlord && LastPlay?.Character == GetPartnerPlayer())
        {
            passValue += 0.3f;
        }

        // 2. 如果手牌较差
        if (AICardAnalyzer.CalculateHandStrength(HandCards) < 0.4f)
        {
            passValue += 0.2f;
        }

        // 3. 如果对手牌数很少
        if (GetMinOpponentCards() <= 2)
        {
            passValue -= 0.2f; // 降低过牌价值
        }

        return Mathf.Clamp01(passValue);
    }

    private List<CardCombination> GenerateTripleStraightCombinations()
    {
        var actions = new List<CardCombination>();

        // 1. 纯飞机
        for (int len = 6; len <= Mathf.Min(18, HandCards.Count); len += 3)
        {
            var planes = AICardFinder.FindTripleStraight(HandCards, -1, len);
            if (planes.Any())
            {
                actions.Add(CreatePlayAction(planes, CardType.TripleStraight));

                // 2. 带单牌
                var singles = FindAttachableCards(planes, planes.Count / 3);
                foreach (var attachCards in singles)
                {
                    var planeWithSingle = new List<Card>(planes);
                    planeWithSingle.AddRange(attachCards);
                    actions.Add(CreatePlayAction(planeWithSingle, CardType.TripleStraightWithSingle));
                }

                // 3. 带对子
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
            // 1. 四带二
            var singles = FindAttachableCards(bomb, 2);
            foreach (var attachCards in singles)
            {
                var fourWithTwo = new List<Card>(bomb);
                fourWithTwo.AddRange(attachCards);
                actions.Add(CreatePlayAction(fourWithTwo, CardType.FourWithTwo));
            }

            // 2. 四带两对
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
            // 1. 纯三张
            actions.Add(CreatePlayAction(three, CardType.Three));

            var remainingCards = HandCards.Except(three).ToList();

            // 2. 三带一
            var singles = FindNonKeyCards(remainingCards, 1);
            foreach (var single in singles)
            {
                var threeWithOne = new List<Card>(three);
                threeWithOne.AddRange(single);
                actions.Add(CreatePlayAction(threeWithOne, CardType.ThreeWithOne));
            }

            // 3. 三带对
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
        // 基础分
        float value = (float)card.rank;

        // 特殊牌加成
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

        // 计算移除这张牌后的价值损失
        float currentValue = currentCombos.Sum(c => c.Value);
        float remainingValue = remainingCombos.Sum(c => c.Value);

        return (currentValue - remainingValue) > 1.5f; // 价值损失阈值
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
    #region 行动生成和执行
    private List<CardCombination> GenerateAllPossiblePlays()
    {
        var plays = new List<CardCombination>();

        // 如果是自由出牌
        if (LastPlay == null || LastPlay.CardType == CardType.Invalid)
        {
            // 按照出牌数量和价值排序的优先级生成
            plays.AddRange(GenerateTripleStraightPatterns());  // 飞机系列（最多牌）
            plays.AddRange(GenerateFourWithExtraPatterns());   // 四带二系列
            plays.AddRange(GenerateStraightPatterns());        // 顺子和连对
            plays.AddRange(GenerateThreeCardsPatterns());      // 三带系列
            plays.AddRange(GeneratePairs());                   // 对子
            plays.AddRange(GenerateSingleCards());             // 单牌（最少）
            plays.AddRange(GenerateBombPatterns());           // 炸弹和王炸（特殊）


            // 按照出牌价值排序
            plays = plays.OrderByDescending(p => p.Value).ToList();
        }
        else
        {
            // 生成能大过上家的牌
            plays.AddRange(GenerateBetterPlays(LastPlay));
        }

        return plays;
    }

    private List<CardCombination> GenerateBetterPlays(LastPlayInfo lastPlay)
    {
        var actions = new List<CardCombination>();

        // 如果上家出的是炸弹
        if (lastPlay.CardType == CardType.Bomb)
        {
            // 只能用更大的炸弹或王炸
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

        // 如果是普通牌型，根据类型生成对应的更大牌
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

        // 可以随时用炸弹和王炸
        actions.AddRange(GenerateBombPatterns());

        return actions;
    }
    #endregion

    #region GenerateBigger系列方法
    private List<CardCombination> GenerateBiggerSingles(int weight)
    {
        var actions = new List<CardCombination>();

        // 按顺序找出所有能大过weight的单牌
        var validSingles = HandCards
            .Where(card => GetCardValue(card) > weight)
            .OrderBy(card => GetCardValue(card));

        foreach (var card in validSingles)
        {
            // 避免拆分有价值的组合
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

        // 使用AICardFinder找到所有对子
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

        // 找到所有三张
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

        // 找到相同长度的顺子
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

        // 找到相同长度的连对
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

        // 找到基础飞机
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

    #region GenerateBigger辅助方法
    private List<Card> FindAttachableSingles(List<Card> mainCards)
    {
        var result = new List<Card>();
        var remainingCards = HandCards.Except(mainCards).ToList();

        // 优先选择非关键牌
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
            // 如果没有非关键牌，选择最小的牌
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

        // 找到所有对子
        var pairs = AICardFinder.FindPurePairs(remainingCards);

        // 按价值排序并选择较小的对子
        result.AddRange(pairs.OrderBy(pair => CardManager.GetWeight(pair, CardType.Pair)));

        return result;
    }

    private List<List<Card>> FindAttachableSinglesForPlane(List<Card> planes)
    {
        var result = new List<List<Card>>();
        var remainingCards = HandCards.Except(planes).ToList();

        // 需要的单牌数量
        int requiredCount = planes.Count / 3;

        // 使用组合算法找到所有可能的单牌组合
        var combinations = GetCombinations(remainingCards, requiredCount);

        // 过滤和排序组合
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

        // 需要的对子数量
        int requiredPairs = planes.Count / 3;

        // 找到所有可用的对子
        var pairs = AICardFinder.FindPurePairs(remainingCards);

        // 如果对子数量不够，直接返回空列表
        if (pairs.Count < requiredPairs)
            return result;

        // 使用组合算法找到所有可能的对子组合
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
        // 检查移除这些牌是否会破坏重要组合
        var remainingCards = HandCards.Except(cards).ToList();
        var currentValue = EvaluateHandStrength();
        var remainingValue = AICardAnalyzer.CalculateHandStrength(remainingCards, IsLandlord);

        return (currentValue - remainingValue) > 0.3f; // 价值损失阈值
    }

    private bool IsKeyCard(Card card)
    {
        return card.rank >= Rank.Two ||
               card.rank == Rank.SJoker ||
               card.rank == Rank.LJoker;
    }
    #endregion

    #region 辅助方法
    private bool CanBeatLastPlay(List<Card> cards, CardType type)
    {
        // 王炸能管任何牌
        if (type == CardType.JokerBomb) return true;

        // 炸弹能管任何非王炸的牌
        if (type == CardType.Bomb && LastPlay.CardType != CardType.JokerBomb)
        {
            if (LastPlay.CardType != CardType.Bomb) return true;
            return CardManager.GetWeight(cards, type) > LastPlay.Weight;
        }

        // 其他牌型必须相同且更大
        if (type != LastPlay.CardType || cards.Count != LastPlay.Length)
            return false;

        return CardManager.GetWeight(cards, type) > LastPlay.Weight;
    }

    private CharacterType GetNextPlayer()
    {
        // 根据当前玩家返回下一个玩家
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

    // 不同牌型的生成方法
    #region 牌型生成方法
    private List<CardCombination> GenerateSingleCards()
    {
        var actions = new List<CardCombination>();
        var weight = LastPlay?.CardType == CardType.Single ? LastPlay.Weight : -1;

        // 使用AICardFinder查找单牌
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

        // 使用AICardFinder查找对子
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

        // 1. 纯三张
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

                // 2. 三带一
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

                // 3. 三带对
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

        // 1. 顺子 (5张及以上)
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

        // 2. 连对 (3对及以上)
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
        // 1. 纯飞机
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

                // 2. 飞机带单
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

                // 3. 飞机带对
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
            // 1. 四带二
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

            // 2. 四带两对
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
        // 1. 普通炸弹
        var bombs = AICardFinder.FindBomb(HandCards, LastPlay?.CardType == CardType.Bomb ? LastPlay.Weight : -1);
        if (bombs.Any())
        {
            actions.Add(new CardCombination
            {
                Cards = bombs,
                Type = CardType.Bomb,
                Value = CalculatePlayValue(bombs, CardType.Bomb) * 2 // 炸弹价值翻倍
            });
        }

        // 2. 王炸
        var rocket = AICardFinder.FindJokerBomb(HandCards);
        if (rocket.Any())
        {
            actions.Add(new CardCombination
            {
                Cards = rocket,
                Type = CardType.JokerBomb,
                Value = CalculatePlayValue(rocket, CardType.JokerBomb) * 2 // 王炸价值翻倍
            });
        }
        return actions;
    }
    #endregion

    #region 辅助方法
    private float CalculatePlayValue(List<Card> cards, CardType type)
    {
        float baseValue = cards.Count; // 基础分为出牌数量

        // 根据牌型增加权重
        switch (type)
        {
            case CardType.JokerBomb:
                baseValue *= 5.0f; // 王炸最高价值
                break;
            case CardType.Bomb:
                baseValue *= 4.0f; // 炸弹次高价值
                break;
            case CardType.FourWithTwo:
            case CardType.FourWithTwoPair:
                baseValue *= 3.0f; // 四带二/四带两对高权重
                break;
            case CardType.TripleStraight:
            case CardType.TripleStraightWithSingle:
            case CardType.TripleStraightWithPair:
                baseValue *= 2.5f; // 飞机系列较高权重
                break;
            case CardType.Straight:
            case CardType.PairStraight:
                baseValue *= 2.0f; // 顺子和连对中等权重
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

        // 考虑剩余牌数的影响
        if (HandCards.Count - cards.Count <= 4)
        {
            baseValue *= 1.5f; // 如果出完后剩余牌数很少，提高权重
        }

        // 考虑对手剩余牌数的影响
        var minOpponentCards = RemainingCards.Values.Where(v => v > 0).Min();
        if (minOpponentCards <= 3)
        {
            baseValue *= 1.3f; // 如果对手快出完了，提高权重
        }

        return baseValue;
    }

    private List<List<Card>> FindAttachableCards(List<Card> mainCards, int count)
    {
        var result = new List<List<Card>>();
        var remainingCards = HandCards.Except(mainCards).ToList();

        // 如果剩余牌不够，直接返回空列表
        if (remainingCards.Count < count) return result;

        // 使用优化后的组合算法
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

        // 递归生成组合
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

    // 优化组合算法的辅助方法
    private bool IsValidCombination(List<Card> cards)
    {
        // 避免拆分对子、三张等有价值的组合
        var groups = cards.GroupBy(c => c.rank).ToList();
        foreach (var group in groups)
        {
            if (group.Count() > 1) return false;
        }
        return true;
    }
    #endregion

    #region 游戏策略评分系统
    private const float END_GAME_THRESHOLD = 5;    // 残局阈值
    private const float WIN_BONUS = 2.0f;          // 获胜加成
    private const float CONTROL_WEIGHT = 0.4f;     // 控制权重要性
    private const float COMBO_WEIGHT = 0.3f;       // 组合重要性
    private const float PROGRESS_WEIGHT = 0.3f;    // 进度重要性

    /// <summary>
    /// 游戏阶段评估
    /// </summary>
    private GamePhase GetGamePhase()
    {
        // 根据手牌数量和回合数判断游戏阶段
        if (HandCards.Count <= END_GAME_THRESHOLD || GetMinOpponentCards() <= END_GAME_THRESHOLD)
            return GamePhase.Endgame;
        if (HandCards.Count <= 10)
            return GamePhase.Middle;
        return GamePhase.Opening;
    }

    /// <summary>
    /// 局势预测评分
    /// </summary>
    private float PredictPositionScore()
    {
        float score = 0;

        // 1. 基础局势评分
        score += EvaluateBasePosition();

        // 2. 身份特定评分
        score += EvaluateRoleSpecificPosition();

        // 3. 关键时刻评分
        score += EvaluateCriticalMoment();

        return Mathf.Clamp01(score);
    }

    private float EvaluateBasePosition()
    {
        float score = 0;
        var phase = GetGamePhase();

        // 1. 手牌数量对比
        float cardCountScore = EvaluateCardCountAdvantage();
        score += cardCountScore * PROGRESS_WEIGHT;

        // 2. 牌型组合评分
        float comboScore = EvaluateComboStrength();
        score += comboScore * COMBO_WEIGHT;

        // 3. 控制力评分
        float controlScore = EvaluateControlStrength();
        score += controlScore * CONTROL_WEIGHT;

        // 根据游戏阶段调整
        switch (phase)
        {
            case GamePhase.Endgame:
                score *= 1.5f;  // 残局加成
                break;
            case GamePhase.Middle:
                score *= 1.2f;  // 中局加成
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

        // 1. 农民牌数评估
        var farmerMinCards = GetMinFarmerCards();
        if (HandCards.Count <= farmerMinCards)
        {
            score += 0.3f;
        }

        // 2. 关键牌控制
        if (HasDominantCards())
        {
            score += 0.2f;
        }

        // 3. 节奏控制
        if (HasGoodTempo())
        {
            score += 0.2f;
        }

        return score;
    }

    private float EvaluateFarmerPosition()
    {
        float score = 0;

        // 1. 与地主牌数对比
        var landlordCards = RemainingCards[LandlordPlayer];
        if (HandCards.Count < landlordCards)
        {
            score += 0.3f;
        }

        // 2. 队友协同
        if (HasGoodCooperation())
        {
            score += 0.3f;
        }

        // 3. 防守能力
        if (HasGoodDefense())
        {
            score += 0.2f;
        }

        return score;
    }

    private float EvaluateCriticalMoment()
    {
        float score = 0;

        // 1. 即将获胜
        if (IsCloseToWinning())
        {
            score += 0.4f;
        }

        // 2. 危险局势
        if (IsInDanger())
        {
            score -= 0.3f;
        }

        // 3. 关键回合
        if (IsCriticalTurn())
        {
            score *= 1.5f;
        }

        return Mathf.Clamp01(score);
    }
    #endregion

    #region 策略评估辅助方法
    private float EvaluateCardCountAdvantage()
    {
        float advantage;

        if (IsLandlord)
        {
            // 地主牌数优势
            var avgFarmerCards = RemainingCards
                .Where(kv => PlayerIdentities[kv.Key] == Identity.Farmer)
                .Average(kv => kv.Value);

            advantage = 1.0f - (HandCards.Count / (avgFarmerCards * 2));
        }
        else
        {
            // 农民牌数优势
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

            // 牌型加权
            float typeMultiplier = GetComboTypeMultiplier(combo.Type);

            // 关键性加权
            float keyMultiplier = combo.IsKey ? 1.5f : 1.0f;

            return baseValue * typeMultiplier * keyMultiplier;
        });

        return Mathf.Clamp01(totalStrength / (combinations.Count * 5f));
    }

    private float EvaluateControlStrength()
    {
        float control = 0;

        // 1. 出牌权
        if (HasInitiative())
        {
            control += 0.3f;
        }

        // 2. 炸弹数量
        int bombCount = AICardAnalyzer.CountBombs(HandCards);
        control += bombCount * 0.15f;

        // 3. 大牌控制
        float bigCardRatio = HandCards.Count(IsKeyCard) / (float)HandCards.Count;
        control += bigCardRatio * 0.2f;

        // 4. 牌型多样性
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

        // 检查是否有足够的防守牌型
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

    #region 辅助方法
    private CharacterType GetLandlordPlayer()
    {
        // GameStateContext中已经有了Landlord的信息
        if (IsLandlord)
            return CurrentPlayer;
        else
        {
            // 如果自己不是地主，从 LeftPlayer 和 RightPlayer 中找到地主
            if (LeftPlayer?.Identity == Identity.Landlord)
                return CharacterType.LeftComputer;
            else if (RightPlayer?.Identity == Identity.Landlord)
                return CharacterType.RightComputer;
        }

        // 调试期间保护
        Debug.LogWarning("无法确定地主角色，返回当前玩家");
        return CurrentPlayer;
    }

    private CharacterType GetPartnerPlayer()
    {
        // 如果是地主，没有队友
        if (IsLandlord)
            return CurrentPlayer;

        // 如果自己是农民，找到另一个农民
        CharacterType leftType = CharacterType.LeftComputer;
        CharacterType rightType = CharacterType.RightComputer;

        // 如果左家是地主，右家是队友
        if (LeftPlayer?.Identity == Identity.Landlord)
            return rightType;
        // 如果右家是地主，左家是队友
        else if (RightPlayer?.Identity == Identity.Landlord)
            return leftType;
        // 根据当前玩家位置确定队友
        else
        {
            switch (CurrentPlayer)
            {
                case CharacterType.Player:
                    return leftType;  // 假设默认左家是队友
                case CharacterType.LeftComputer:
                    return rightType;
                case CharacterType.RightComputer:
                    return leftType;
                default:
                    Debug.LogWarning("无法确定队友角色，返回当前玩家");
                    return CurrentPlayer;
            }
        }
    }

    // 添加一个新的辅助方法来判断是否是队友
    public bool IsPartner(CharacterType playerType)
    {
        if (IsLandlord)
            return false;  // 地主没有队友

        var partnerType = GetPartnerPlayer();
        return playerType == partnerType;
    }

    // 添加一个获取两个对手的方法
    public (CharacterType opponent1, CharacterType opponent2) GetOpponents()
    {
        if (IsLandlord)
        {
            // 地主的对手是两个农民
            return (CharacterType.LeftComputer, CharacterType.RightComputer);
        }
        else
        {
            // 农民的对手是地主
            return (GetLandlordPlayer(), CharacterType.Desk); // 使用Desk作为第二个返回值表示没有第二个对手
        }
    }

    // 获取下一个出牌的玩家
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
                Debug.LogWarning("未知的玩家类型，返回玩家");
                return CharacterType.Player;
        }
    }

    // 获取上一个出牌的玩家
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
                Debug.LogWarning("未知的玩家类型，返回玩家");
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