using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static CardManager;
using AI.Utils;
using static AI.Utils.AICardFinder;

namespace AI.Utils
{
    /// <summary>
    /// AI���Ʒ��������� - �ṩ���Ʒ�������������
    /// </summary>
    public static class AICardAnalyzer
    {

        #region �������ҹ���
        /// <summary>
        /// ���������е�ը������
        /// </summary>
        public static int CountBombs(List<Card> cards)
        {
            return cards.GroupBy(c => c.rank)
                       .Count(g => g.Count() == 4);
        }

        /// <summary>
        /// ����Ƿ�����ը
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


        #region ��������
        /// <summary>
        /// ������������ (0-1)
        /// </summary>
        public static float EvaluateHandQuality(List<Card> cards)
        {
            // �����֣����Ƶ÷�
            int score = cards.Where(card => card.rank >= Rank.Jack)
                           .Sum(card => (int)card.rank);

            // ���������÷�
            int highCardCount = cards.Count(card => card.rank >= Rank.Jack);

            // ը���ӷ�
            int bombCount = CountBombs(cards);

            // ��ը�ӷ�
            bool hasJokerBomb = HasJokerBomb(cards);

            // �ۺ�����
            float quality = (score / 200f) +         // ������Ȩ��
                          (highCardCount / 17f) +    // ��������Ȩ��
                          (bombCount * 0.2f);        // ը��Ȩ��

            if (hasJokerBomb) quality += 0.3f;      // ��ը����ӷ�

            return Mathf.Clamp01(quality);
        }
        #endregion

        #region ���ͼ�ֵ����
        private static float CalculateBasicStrength(List<Card> cards)
        {
            float strength = 0;
            foreach (var card in cards)
            {
                strength += GetCardBaseValue(card) / 16.0f; // ��һ����0-1
            }
            return strength / cards.Count; // ƽ��ֵ
        }

        private static float CalculateBombStrength(List<Card> cards)
        {
            float strength = 0;

            // ը����ֵ
            int bombCount = CountBombs(cards);
            strength += bombCount * 30;

            // ��ը��ֵ
            if (HasJokerBomb(cards))
            {
                strength += 40; // �����ը��ֵ
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
            return bonus ; // ���Ƽ�ֵ�ʵ�����
        }

        /// <summary>
        /// �Ż�������ǿ�ȼ���
        /// </summary>
        public static float CalculateHandStrength(List<Card> cards, bool isLandlord = false)
        {
            float strength = 0;

            // 1. ������ֵ���� (30%)
            strength += CalculateBasicStrength(cards) * 0.3f;

            // 2. ����������� (40%)
            var combinations = FindAllPotentialCombinations(cards);
            strength += CalculateCombinationStrength(combinations) * 0.4f;

            // 3. �ؼ������� (30%)
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

                // �������͵�����ֵ
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

            return Mathf.Clamp01(totalValue / (combinations.Count * 20f)); // ��һ��
        }

        private static float CalculateKeyCardsStrength(List<Card> cards, bool isLandlord)
        {
            float strength = 0;

            // 1. ����ؼ����Ƽ�ֵ
            int bigCards = cards.Count(c => c.rank >= Rank.Ace);
            strength += bigCards * 0.15f;

            // 2. ���Ƽ�ֵ
            if (HasJokerBomb(cards))
            {
                strength += 0.4f;
            }
            else
            {
                strength += cards.Count(c => c.rank >= Rank.SJoker) * 0.2f;
            }

            // 3. ������ݵ���
            if (isLandlord)
            {
                strength *= 1.2f; // �����Ĺؼ��Ƹ���Ҫ
            }

            return Mathf.Clamp01(strength);
        }
        private static float CalculatePatternStrength(List<Card> cards)
        {
            float patternStrength = 0;

            patternStrength += CalculateSingleBonus(cards);         // ���Ƽ�ֵ
            patternStrength += CalculateTripleBonus(cards);         // ���ż�ֵ
            patternStrength += CalculateStraightsBonus(cards);      // ˳�Ӽ�ֵ
            patternStrength += CalculatePairStraightsBonus(cards);  // ���Լ�ֵ
            patternStrength += CalculatePlanesBonus(cards);         // �ɻ���ֵ
            patternStrength += CalculateBombStrength(cards);        // ը����ֵ

            return patternStrength;
        }
        #endregion

        #region ���Ը�������
        public static float EvaluateCombinationValue(List<Card> cards, CardType type, bool isEndgame = false)
        {
            float value = cards.Count; // ����ֵΪ��������

            // �������ͼ�Ȩ
            value *= GetTypeWeight(type);

            // �оּӳ�
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
        /// ����ĳ���Ƶ���Ҫ��
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

        #region ����˳�Ӳ����Ż�
        public static List<CardCombination> FindAllStraights(List<Card> cards)
        {
            var straights = new List<CardCombination>();

            // ʹ��FindConsecutiveCards���Ҳ�ͬ���ȵ�˳��
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
            float baseValue = straight.Count * 1.5f; // ������ֵ
            float rankBonus = (float)straight[0].rank * 1f; // ��ʼ�Ƶ����ӳ�
            return baseValue + rankBonus;
        }

        private static bool IsStraightKey(List<Card> straight)
        {
            return straight.Count >= 8 || // ����˳��
                   straight.Any(c => c.rank >= Rank.Ace); // ��������
        }
        #endregion

        #region �������Բ����Ż�
        public static List<CardCombination> FindAllPairStraights(List<Card> cards)
        {
            var pairStraights = new List<CardCombination>();

            // ʹ��FindConsecutiveCardsѰ�Ҳ�ͬ���ȵ�����
            for (int pairs = 3; pairs <= 7; pairs++) // 3-10��
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
            return pairStraight.Count >= 8 || // 4�Ի����
                   pairStraight[0].rank >= Rank.Ten; // ��10��ʼ������
        }
        #endregion


        #region ���зɻ������Ż�
        public static List<CardCombination> FindAllPlanes(List<Card> cards)
        {
            var planes = new List<CardCombination>();

            // ʹ��FindConsecutiveCardsѰ�Ҳ�ͬ���ȵķɻ�
            for (int triples = 2; triples <= 4; triples++) // 2-6������
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
            return plane.Count >= 9 || // 3����������
                   plane[0].rank >= Rank.Ten; // ��10��ʼ�ķɻ�
        }
        #endregion

        #region �����Ͳ��ҿ���
        public static List<CardCombination> FindCombPurePairs(List<Card> cards)
        {
            var purePairs = new List<CardCombination>();

            // ʹ��FindCardsByFilter�������ж���
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
        /// ���Ҵ�����(��ը��)
        /// </summary>
        public static List<CardCombination> FindCombPureTriples(List<Card> cards)
        {
            var triples = new List<CardCombination>();

            // ʹ��FindCardsByFilter������������
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

            // �ҳ�ը��
            var allBombs = cards.GroupBy(c => c.rank)
                               .Where(g => g.Count() == 4);

            foreach (var bomb in allBombs)
            {
                bombs.Add(new CardCombination
                {
                    Cards = bomb.ToList(),
                    Type = CardType.Bomb,
                    Value = (float)bomb.Key * 5,
                    IsKey = true // ը��ʼ���ǹؼ���
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
                    IsKey = true // ը��ʼ���ǹؼ���
                }); ;
            }
            return bombs;
        }

        public static List<CardCombination> FindAllPotentialCombinations(List<Card> cards)
        {
            var combinations = new List<CardCombination>();

            // 1. �߼�ֵ���� (����������Ȩ�����)
            combinations.AddRange(FindCombBombs(cards));         // ը��
            combinations.AddRange(FindCombJokerBombs(cards));    // ��ը
            combinations.AddRange(FindAllFourWithExtra(cards));  // �Ĵ���/����

            // 2. �������� (�ڶ����ȼ�)
            combinations.AddRange(FindAllStraights(cards));      // ˳��
            combinations.AddRange(FindAllPairStraights(cards));  // ����
            combinations.AddRange(FindAllPlanes(cards));         // �ɻ�

            // 3. �������� (�������)
            combinations.AddRange(FindCombPureTriples(cards));   // ����
            combinations.AddRange(FindCombPurePairs(cards));     // ����

            return combinations;
        }
        #endregion

        #region �����Ĵ�X�����Ż�
        /// <summary>
        /// �������п��ܵ��Ĵ������Ĵ��������
        /// </summary>
        public static List<CardCombination> FindAllFourWithExtra(List<Card> cards)
        {
            var combinations = new List<CardCombination>();
            var sortedCards = cards.OrderBy(c => c.rank).ToList();
            var fours = FindAllFours(sortedCards);

            foreach (var four in fours)
            {
                // �Ƴ����ŵ���,���ʣ�����
                var remainingCards = sortedCards.Where(c => !four.Contains(c)).ToList();

                // Ѱ���Ĵ��������
                var fourWithTwo = AICardFinder.TryFourWithTwo(four, remainingCards);
                if (fourWithTwo != null)
                {
                    combinations.Add(new CardCombination
                    {
                        Cards = fourWithTwo,
                        Type = CardType.FourWithTwo,
                        Value = CalculateFourValue(four) * 1.2f,
                        IsKey = true // �Ĵ������ǹؼ���
                    });
                }

                // Ѱ���Ĵ����Ե����
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
        /// ������������
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
                    i += 3; // �����Ѿ��ҵ�������
                }
            }
            return fours;
        }



        /// <summary>
        /// ���������ƵĻ�����ֵ
        /// </summary>
        private static float CalculateFourValue(List<Card> four)
        {
            float baseValue = 10.0f; // ������ֵ
            float rankBonus = (float)four[0].rank * 1f; // �����ӳ�
            return baseValue + rankBonus;
        }
        #endregion


        #region ��������

        #endregion
    }
}