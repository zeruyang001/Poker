using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static CardManager;
using AI.Utils;
using static AI.Utils.AICardFinder;

namespace AI.Utils
{
    /// <summary>
    /// AI卡牌分析工具类 - 提供卡牌分析和评估功能
    /// </summary>
    public static class AICardAnalyzer
    {

        #region 基本查找功能
        /// <summary>
        /// 计算手牌中的炸弹数量
        /// </summary>
        public static int CountBombs(List<Card> cards)
        {
            return cards.GroupBy(c => c.rank)
                       .Count(g => g.Count() == 4);
        }

        /// <summary>
        /// 检查是否有王炸
        /// </summary>
        public static bool HasJokerBomb(List<Card> cards)
        {
            return cards.Any(c => c.rank == Rank.SJoker) &&
                   cards.Any(c => c.rank == Rank.LJoker);
        }

        public static float GetCardBaseValue(Card card)
        {
            if (card.rank == Rank.LJoker) return 16;
            if (card.rank == Rank.SJoker) return 15;
            if (card.rank == Rank.Two) return 14;
            if (card.rank == Rank.Ace) return 12;
            return (int)card.rank;
        }

        #endregion


        #region 手牌评估
        /// <summary>
        /// 评估手牌质量 (0-1)
        /// </summary>
        public static float EvaluateHandQuality(List<Card> cards)
        {
            // 基础分：大牌得分
            int score = cards.Where(card => card.rank >= Rank.Jack)
                           .Sum(card => (int)card.rank);

            // 高牌数量得分
            int highCardCount = cards.Count(card => card.rank >= Rank.Jack);

            // 炸弹加分
            int bombCount = CountBombs(cards);

            // 王炸加分
            bool hasJokerBomb = HasJokerBomb(cards);

            // 综合评分
            float quality = (score / 200f) +         // 基础分权重
                          (highCardCount / 17f) +    // 高牌数量权重
                          (bombCount * 0.2f);        // 炸弹权重

            if (hasJokerBomb) quality += 0.3f;      // 王炸额外加分

            return Mathf.Clamp01(quality);
        }
        #endregion

        #region 牌型价值计算
        private static float CalculateBasicStrength(List<Card> cards)
        {
            float strength = 0;
            foreach (var card in cards)
            {
                strength += GetCardBaseValue(card) / 16.0f; // 归一化到0-1
            }
            return strength / cards.Count; // 平均值
        }

        private static float CalculateBombStrength(List<Card> cards)
        {
            float strength = 0;

            // 炸弹价值
            int bombCount = CountBombs(cards);
            strength += bombCount * 30;

            // 王炸价值
            if (HasJokerBomb(cards))
            {
                strength += 40; // 提高王炸价值
            }

            return strength;
        }

        public static float CalculateStraightsBonus(List<Card> cards)
        {
            float bonus = 0;
            var straights = FindAllStraights(cards);
            foreach (var straight in straights)
            {
                bonus += straight.Value;
                if (straight.IsKey)
                {
                    bonus += 10;
                }
            }
            return bonus;
        }

        private static float CalculatePairStraightsBonus(List<Card> cards)
        {
            var pairStraights = FindAllPairStraights(cards);
            float bonus = 0;
            foreach (var pairStraight in pairStraights)
            {
                bonus += pairStraight.Value;
                if (pairStraight.IsKey)
                {
                    bonus += 10;
                }
            }
            return bonus;
        }

        private static float CalculatePlanesBonus(List<Card> cards)
        {
            var planes = FindAllPlanes(cards);
            float bonus = 0;
            foreach (var plane in planes)
            {
                bonus += plane.Value;
                if (plane.IsKey)
                {
                    bonus += 10;
                }
            }
            return bonus;
        }

        private static float CalculateTripleBonus(List<Card> cards)
        {
            var triples = FindCombPureTriples(cards);
            float bonus = 0;
            foreach (var triple in triples)
            {
                bonus += triple.Value;
            }
            return bonus;
        }

        private static float CalculateSingleBonus(List<Card> cards)
        {
            var singles = AICardFinder.FindAllPureSingles(cards);
            float bonus = -singles.Count;
            return bonus ; // 单牌价值适当降低
        }

        /// <summary>
        /// 优化的手牌强度计算
        /// </summary>
        public static float CalculateHandStrength(List<Card> cards, bool isLandlord = false)
        {
            float strength = 0;

            // 1. 基本牌值评估 (30%)
            strength += CalculateBasicStrength(cards) * 0.3f;

            // 2. 牌型组合评估 (40%)
            var combinations = FindAllPotentialCombinations(cards);
            strength += CalculateCombinationStrength(combinations) * 0.4f;

            // 3. 关键牌评估 (30%)
            strength += CalculateKeyCardsStrength(cards, isLandlord) * 0.3f;

            return Mathf.Clamp01(strength);
        }


        private static float CalculateCombinationStrength(List<CardCombination> combinations)
        {
            if (!combinations.Any()) return 0;

            float totalValue = 0;
            foreach (var combo in combinations)
            {
                float comboValue = combo.Value;

                // 根据牌型调整价值
                switch (combo.Type)
                {
                    case CardType.JokerBomb:
                        comboValue *= 2.0f;
                        break;
                    case CardType.Bomb:
                        comboValue *= 1.8f;
                        break;
                    case CardType.TripleStraight:
                    case CardType.TripleStraightWithPair:
                        comboValue *= 1.5f;
                        break;
                    case CardType.Straight:
                    case CardType.PairStraight:
                        comboValue *= 1.3f;
                        break;
                }

                totalValue += comboValue;
            }

            return Mathf.Clamp01(totalValue / (combinations.Count * 20f)); // 归一化
        }

        private static float CalculateKeyCardsStrength(List<Card> cards, bool isLandlord)
        {
            float strength = 0;

            // 1. 计算关键单牌价值
            int bigCards = cards.Count(c => c.rank >= Rank.Ace);
            strength += bigCards * 0.15f;

            // 2. 王牌价值
            if (HasJokerBomb(cards))
            {
                strength += 0.4f;
            }
            else
            {
                strength += cards.Count(c => c.rank >= Rank.SJoker) * 0.2f;
            }

            // 3. 根据身份调整
            if (isLandlord)
            {
                strength *= 1.2f; // 地主的关键牌更重要
            }

            return Mathf.Clamp01(strength);
        }
        private static float CalculatePatternStrength(List<Card> cards)
        {
            float patternStrength = 0;

            patternStrength += CalculateSingleBonus(cards);         // 单牌价值
            patternStrength += CalculateTripleBonus(cards);         // 三张价值
            patternStrength += CalculateStraightsBonus(cards);      // 顺子价值
            patternStrength += CalculatePairStraightsBonus(cards);  // 连对价值
            patternStrength += CalculatePlanesBonus(cards);         // 飞机价值
            patternStrength += CalculateBombStrength(cards);        // 炸弹价值

            return patternStrength;
        }
        #endregion

        #region 策略辅助方法
        public static float EvaluateCombinationValue(List<Card> cards, CardType type, bool isEndgame = false)
        {
            float value = cards.Count; // 基础值为出牌数量

            // 根据牌型加权
            value *= GetTypeWeight(type);

            // 残局加成
            if (isEndgame && cards.Count <= 3)
            {
                value *= 1.5f;
            }

            return value;
        }

        private static float GetTypeWeight(CardType type)
        {
            switch (type)
            {
                case CardType.JokerBomb: return 5.0f;
                case CardType.Bomb: return 4.0f;
                case CardType.Straight: return 2.0f;
                case CardType.ThreeWithPair: return 1.8f;
                case CardType.ThreeWithOne: return 1.5f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// 分析某张牌的重要性
        /// </summary>
        public static float AnalyzeCardImportance(Card card)
        {
            if (card.rank == Rank.LJoker || card.rank == Rank.SJoker)
                return 1.0f;

            if (card.rank >= Rank.Two)
                return 0.8f;

            if (card.rank >= Rank.Jack)
                return 0.6f;

            return 0.4f;
        }
        #endregion

        #region 所有顺子查找优化
        public static List<CardCombination> FindAllStraights(List<Card> cards)
        {
            var straights = new List<CardCombination>();

            // 使用FindConsecutiveCards查找不同长度的顺子
            for (int length = 5; length <= 12; length++)
            {
                var straight = FindConsecutiveCards(cards, g => g.Any(), -1, length, 1);
                if (straight.Any())
                {
                    straights.Add(new CardCombination
                    {
                        Cards = straight,
                        Type = CardType.Straight,
                        Value = CalculateStraightValue(straight),
                        IsKey = IsStraightKey(straight)
                    });
                }
            }

            return straights;
        }

        private static float CalculateStraightValue(List<Card> straight)
        {
            float baseValue = straight.Count * 1.5f; // 基础分值
            float rankBonus = (float)straight[0].rank * 1f; // 起始牌点数加成
            return baseValue + rankBonus;
        }

        private static bool IsStraightKey(List<Card> straight)
        {
            return straight.Count >= 8 || // 超长顺子
                   straight.Any(c => c.rank >= Rank.Ace); // 包含大牌
        }
        #endregion

        #region 所有连对查找优化
        public static List<CardCombination> FindAllPairStraights(List<Card> cards)
        {
            var pairStraights = new List<CardCombination>();

            // 使用FindConsecutiveCards寻找不同长度的连对
            for (int pairs = 3; pairs <= 7; pairs++) // 3-10对
            {
                var pairStraight = FindConsecutiveCards(cards, g => g.Count() >= 2, -1, pairs * 2, 2);
                if (pairStraight.Any())
                {
                    pairStraights.Add(new CardCombination
                    {
                        Cards = pairStraight,
                        Type = CardType.PairStraight,
                        Value = CalculatePairStraightValue(pairStraight),
                        IsKey = IsPairStraightKey(pairStraight)
                    });
                }
            }

            return pairStraights;
        }

        private static float CalculatePairStraightValue(List<Card> pairStraight)
        {
            float baseValue = pairStraight.Count * 1.5f;
            float rankBonus = (float)pairStraight[0].rank * 2f;
            return baseValue + rankBonus;
        }

        private static bool IsPairStraightKey(List<Card> pairStraight)
        {
            return pairStraight.Count >= 8 || // 4对或更多
                   pairStraight[0].rank >= Rank.Ten; // 从10开始的连对
        }
        #endregion


        #region 所有飞机查找优化
        public static List<CardCombination> FindAllPlanes(List<Card> cards)
        {
            var planes = new List<CardCombination>();

            // 使用FindConsecutiveCards寻找不同长度的飞机
            for (int triples = 2; triples <= 4; triples++) // 2-6个三张
            {
                var plane = FindConsecutiveCards(cards, g => g.Count() >= 3, -1, triples * 3, 3);
                if (plane.Any())
                {
                    planes.Add(new CardCombination
                    {
                        Cards = plane,
                        Type = CardType.TripleStraight,
                        Value = CalculatePlaneValue(plane),
                        IsKey = IsPlanesKey(plane)
                    });
                }
            }

            return planes;
        }

        private static float CalculatePlaneValue(List<Card> plane)
        {
            float baseValue = plane.Count * 2f;
            float rankBonus = (float)plane[0].rank * 3f;
            return baseValue + rankBonus;
        }

        private static bool IsPlanesKey(List<Card> plane)
        {
            return plane.Count >= 9 || // 3组或更多三张
                   plane[0].rank >= Rank.Ten; // 从10开始的飞机
        }
        #endregion

        #region 复合型查找卡牌
        public static List<CardCombination> FindCombPurePairs(List<Card> cards)
        {
            var purePairs = new List<CardCombination>();

            // 使用FindCardsByFilter查找所有对子
            var pairs = FindCardsByFilter(cards, PairFilter);
            foreach (var pair in pairs)
            {
                purePairs.Add(new CardCombination
                {
                    Cards = pair,
                    Type = CardType.Pair,
                    Value = (float)pair[0].rank * 2,
                    IsKey = pair[0].rank >= Rank.Ace
                });
            }

            return purePairs.OrderBy(p => p.Value).ToList();
        }

        /// <summary>
        /// 查找纯三张(非炸弹)
        /// </summary>
        public static List<CardCombination> FindCombPureTriples(List<Card> cards)
        {
            var triples = new List<CardCombination>();

            // 使用FindCardsByFilter查找所有三张
            var allTriples = FindCardsByFilter(cards, TripleFilter);
            foreach (var triple in allTriples)
            {
                triples.Add(new CardCombination
                {
                    Cards = triple,
                    Type = CardType.Three,
                    Value = (float)triple[0].rank * 3,
                    IsKey = triple[0].rank >= Rank.King
                });
            }

            return triples.OrderBy(t => t.Value).ToList();
        }

        public static List<CardCombination> FindCombBombs(List<Card> cards)
        {
            var bombs = new List<CardCombination>();

            // 找出炸弹
            var allBombs = cards.GroupBy(c => c.rank)
                               .Where(g => g.Count() == 4);

            foreach (var bomb in allBombs)
            {
                bombs.Add(new CardCombination
                {
                    Cards = bomb.ToList(),
                    Type = CardType.Bomb,
                    Value = (float)bomb.Key * 5,
                    IsKey = true // 炸弹始终是关键牌
                });
            }

            return bombs.OrderBy(b => b.Value).ToList();
        }

        public static List<CardCombination> FindCombJokerBombs(List<Card> cards)
        {
            var bombs = new List<CardCombination>();
            if (HasJokerBomb(cards))
            {
                bombs.Add(new CardCombination
                {
                    Cards = FindJokerBomb(cards),
                    Type = CardType.JokerBomb,
                    Value = 30,
                    IsKey = true // 炸弹始终是关键牌
                }); ;
            }
            return bombs;
        }

        public static List<CardCombination> FindAllPotentialCombinations(List<Card> cards)
        {
            var combinations = new List<CardCombination>();

            // 1. 高价值牌型 (首先搜索，权重最高)
            combinations.AddRange(FindCombBombs(cards));         // 炸弹
            combinations.AddRange(FindCombJokerBombs(cards));    // 王炸
            combinations.AddRange(FindAllFourWithExtra(cards));  // 四带二/两对

            // 2. 连续牌型 (第二优先级)
            combinations.AddRange(FindAllStraights(cards));      // 顺子
            combinations.AddRange(FindAllPairStraights(cards));  // 连对
            combinations.AddRange(FindAllPlanes(cards));         // 飞机

            // 3. 基础牌型 (最后搜索)
            combinations.AddRange(FindCombPureTriples(cards));   // 三张
            combinations.AddRange(FindCombPurePairs(cards));     // 对子

            return combinations;
        }
        #endregion

        #region 所有四带X查找优化
        /// <summary>
        /// 查找所有可能的四带二和四带两对组合
        /// </summary>
        public static List<CardCombination> FindAllFourWithExtra(List<Card> cards)
        {
            var combinations = new List<CardCombination>();
            var sortedCards = cards.OrderBy(c => c.rank).ToList();
            var fours = FindAllFours(sortedCards);

            foreach (var four in fours)
            {
                // 移除四张的牌,获得剩余的牌
                var remainingCards = sortedCards.Where(c => !four.Contains(c)).ToList();

                // 寻找四带二的组合
                var fourWithTwo = AICardFinder.TryFourWithTwo(four, remainingCards);
                if (fourWithTwo != null)
                {
                    combinations.Add(new CardCombination
                    {
                        Cards = fourWithTwo,
                        Type = CardType.FourWithTwo,
                        Value = CalculateFourValue(four) * 1.2f,
                        IsKey = true // 四带二总是关键牌
                    });
                }

                // 寻找四带两对的组合
                var fourWithTwoPairs = AICardFinder.TryFourWithTwoPairs(four, remainingCards);
                if (fourWithTwoPairs != null)
                {
                    combinations.Add(new CardCombination
                    {
                        Cards = fourWithTwoPairs,
                        Type = CardType.FourWithTwoPair,
                        Value = CalculateFourValue(four) * 1.2f,
                        IsKey = true
                    });
                }
            }

            return combinations;
        }

        /// <summary>
        /// 查找所有四张
        /// </summary>
        private static List<List<Card>> FindAllFours(List<Card> cards)
        {
            var fours = new List<List<Card>>();
            for (int i = 0; i <= cards.Count - 4; i++)
            {
                if (i + 3 < cards.Count &&
                    cards[i].rank == cards[i + 1].rank &&
                    cards[i].rank == cards[i + 2].rank &&
                    cards[i].rank == cards[i + 3].rank)
                {
                    fours.Add(new List<Card>
                {
                    cards[i],
                    cards[i + 1],
                    cards[i + 2],
                    cards[i + 3]
                });
                    i += 3; // 跳过已经找到的四张
                }
            }
            return fours;
        }



        /// <summary>
        /// 计算四张牌的基础价值
        /// </summary>
        private static float CalculateFourValue(List<Card> four)
        {
            float baseValue = 10.0f; // 基础分值
            float rankBonus = (float)four[0].rank * 1f; // 点数加成
            return baseValue + rankBonus;
        }
        #endregion


        #region 辅助方法

        #endregion
    }
}