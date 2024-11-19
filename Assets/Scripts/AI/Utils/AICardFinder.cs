using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CardManager;

namespace AI.Utils
{
    /// <summary>
    /// ���Ʋ��ҹ����� - �ṩͨ�õĿ��Ʋ��ҹ���
    /// </summary>
    public static class AICardFinder
    {

        #region ������Ϸ�����
        // ����һ��ί���������ڹ�������
        public delegate bool CardFilter(IGrouping<Rank, Card> group);

        /// <summary>
        /// �������Ͳ�����
        /// </summary>
        public static List<List<Card>> FindCardsByFilter(List<Card> cards,
                                                         CardFilter filter,
                                                         int requiredCount = -1,
                                                         int weight = -1)
        {
            var result = new List<List<Card>>();
            var groups = cards.OrderBy(c => c.rank)
                             .GroupBy(c => c.rank)
                             .Where(g => (int)g.Key > weight &&
                                       g.Count() != 4 && // ����ը��
                                       filter(g))
                             .OrderBy(g => g.Key)
                             .ToList();

            // �����Ҫ���ƽ������
            if (requiredCount > 0)
                groups = groups.Take(requiredCount).ToList();

            return groups.Select(g => g.ToList()).ToList();
        }

        // Ԥ���������
        public static readonly CardFilter SingleFilter = g => g.Count() == 1;
        public static readonly CardFilter PairFilter = g => g.Count() == 2;
        public static readonly CardFilter TripleFilter = g => g.Count() == 3;


        /// <summary>
        /// Ѱ����������
        /// </summary>
        public static List<Card> FindConsecutiveCards(List<Card> cards,
                                                      CardFilter filter,
                                                      int weight,
                                                      int length,
                                                      int cardsPerGroup)
        {
            // ���������鲢�ų�ը��
            var groups = cards.GroupBy(c => c.rank)
                             .Where(g => (int)g.Key > weight &&
                                       g.Count() != 4 &&
                                       filter(g))
                             .OrderBy(g => g.Key)
                             .ToList();

            int neededGroups = length / cardsPerGroup;

            for (int i = 0; i <= groups.Count - neededGroups; i++)
            {
                var startGroup = groups[i];
                if (startGroup.Key >= Rank.Two) continue;

                bool isValid = true;
                List<Card> result = new List<Card>();

                // ���������
                for (int j = 0; j < neededGroups; j++)
                {
                    var currentGroup = groups[i + j];
                    if ((int)currentGroup.Key != (int)startGroup.Key + j)
                    {
                        isValid = false;
                        break;
                    }
                    result.AddRange(currentGroup.Take(cardsPerGroup));
                }

                if (isValid) return result;
            }

            return new List<Card>();
        }
        #endregion

        #region �������Ͳ���
        /// <summary>
        /// ���ҵ��� - ����ʹ�ô�����,û���ٿ��ǲ���
        /// </summary>
        public static List<Card> FindSingle(List<Card> cards, int weight)
        {
            // 1. �Ҵ���
            var singles = FindCardsByFilter(cards, SingleFilter, 1, weight);
            if (singles.Any()) return singles[0];

            // 2. �Ҷ���
            var pairs = FindCardsByFilter(cards, PairFilter, 1, weight);
            if (pairs.Any()) return new List<Card> { pairs[0][0] };

            // 3. ������
            var triples = FindCardsByFilter(cards, TripleFilter, 1, weight);
            if (triples.Any() && weight > (int)Rank.Jack) return new List<Card> { triples[0][0] };

            return new List<Card>();
        }

        /// <summary>
        /// ���Ҷ��� - ����ʹ�ô�����,û���ٿ��ǲ�����
        /// </summary>
        public static List<Card> FindPair(List<Card> cards, int weight)
        {
            // 1. �Ҵ���
            var pairs = FindCardsByFilter(cards, PairFilter, 1, weight);
            if (pairs.Any()) return pairs[0];

            // 2. ������
            var triples = FindCardsByFilter(cards, TripleFilter, 1, weight);
            if (triples.Any() && weight > (int)Rank.Jack) return triples[0].Take(2).ToList();

            return new List<Card>();
        }

        /// <summary>
        /// ����˳�� - ����ը��
        /// </summary>
        public static List<Card> FindStraight(List<Card> cards, int weight, int length)
        {
            if (length < 5) return new List<Card>();
            return FindConsecutiveCards(cards, g => g.Any(), weight, length, 1);
        }

        /// <summary>
        /// ��˫˳ 556677
        /// </summary>
        public static List<Card> FindPairStraight(List<Card> cards, int weight, int length)
        {
            if (length < 6 || length % 2 != 0) return new List<Card>();
            return FindConsecutiveCards(cards, g => g.Count() >= 2, weight, length, 2);
        }

        /// <summary>
        /// �������� - ����ը��
        /// </summary>
        public static List<Card> FindThree(List<Card> cards, int weight)
        {
            var triples = FindCardsByFilter(cards, g => g.Count() == 3, 1, weight);
            return triples.FirstOrDefault() ?? new List<Card>();
        }

        /// <summary>
        /// ��������һ
        /// </summary>
        public static List<Card> FindThreeWithOne(List<Card> cards, int weight)
        {
            var three = FindThree(cards, weight);
            if (!three.Any()) return new List<Card>();

            var remainingCards = cards.Except(three).ToList();

            // 1. �����Ҵ�����
            var single = FindSingle(remainingCards, -1);
            if (single.Any())
                return three.Concat(single).ToList();

            return new List<Card>();
        }

        /// <summary>
        /// ����������
        /// </summary>
        public static List<Card> FindThreeAndDouble(List<Card> cards, int weight)
        {
            var three = FindThree(cards, weight);
            if (!three.Any()) return new List<Card>();

            var remainingCards = cards.Except(three).ToList();

            // ֻ�Ҵ�����
            var pair = FindPurePairs(remainingCards).FirstOrDefault();
            if (pair != null)
                return three.Concat(pair).ToList();

            return new List<Card>();
        }

        /// <summary>
        /// ���ҷɻ�
        /// </summary>
        public static List<Card> FindTripleStraight(List<Card> cards, int weight, int length)
        {
            if (length < 6 || length % 3 != 0) return new List<Card>();
            return FindConsecutiveCards(cards, g => g.Count() == 3, weight, length, 3);
        }

        /// <summary>
        /// ���ҷɻ�����
        /// </summary>
        public static List<Card> FindTripleStraightWithSingle(List<Card> cards, int weight, int length)
        {
            var tripleStraight = FindTripleStraight(cards, weight, length * 3 / 4);
            if (!tripleStraight.Any()) return new List<Card>();

            var remainingCards = cards.Except(tripleStraight).ToList();
            var neededSingles = length / 4;

            // �ҳ����������ĵ���(���ȴ���)
            var singles = FindAttachingCards(remainingCards, neededSingles, true);
            if (singles.Count == neededSingles)
                return tripleStraight.Concat(singles).ToList();

            return new List<Card>();
        }

        /// <summary>
        /// ���ҷɻ�����
        /// </summary>
        public static List<Card> FindTripleStraightWithPair(List<Card> cards, int weight, int length)
        {
            var tripleStraight = FindTripleStraight(cards, weight, length * 3 / 5);
            if (!tripleStraight.Any()) return new List<Card>();

            var remainingCards = cards.Except(tripleStraight).ToList();
            int neededPairs = length / 5;

            // �Ҵ�����
            var pairs = FindPurePairs(remainingCards).Take(neededPairs).ToList();
            if (pairs.Count == neededPairs)
                return tripleStraight.Concat(pairs.SelectMany(p => p)).ToList();

            return new List<Card>();
        }

        /// <summary>
        /// ը��
        /// </summary>
        /// <param name="cards"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
        public static List<Card> FindBomb(List<Card> cards, int rank)
        {
            List<Card> select = new List<Card>();
            for (int i = 0; i < cards.Count - 4; i++)
            {
                if ((int)cards[i].rank == (int)cards[i + 1].rank &&
                    (int)cards[i].rank == (int)cards[i + 2].rank &&
                    (int)cards[i].rank == (int)cards[i + 3].rank)
                {
                    if ((int)cards[i].rank > rank)
                    {
                        select.Add(cards[i]);
                        select.Add(cards[i + 1]);
                        select.Add(cards[i + 2]);
                        select.Add(cards[i + 3]);
                        break;
                    }
                }

            }
            return select;

        }

        /// <summary>
        /// ��ը
        /// </summary>
        /// <param name="cards"></param>
        /// <returns></returns>
        public static List<Card> FindJokerBomb(List<Card> cards)
        {
            List<Card> select = new List<Card>();
            for (int i = 0; i < cards.Count - 1; i++)
            {
                if (cards[i].rank == Rank.SJoker
                    && cards[i + 1].rank == Rank.LJoker)
                {
                    select.Add(cards[i]);
                    select.Add(cards[i + 1]);
                    break;
                }
            }
            return select;
        }

        /// <summary>
        /// �Ĵ��� - ���ȴ�������
        /// </summary>
        public static List<Card> FindFourWithTwo(List<Card> cards, int rank)
        {
            List<Card> bomb = FindBomb(cards, rank);
            if (bomb.Count == 4)
            {
                var remainingCards = cards.Except(bomb).ToList();

                // 1. ���������Ŵ�����
                var pureSingles = FindAllPureSingles(remainingCards)
                                 .Take(2)
                                 .ToList();

                if (pureSingles.Count == 2)
                {
                    return bomb.Concat(pureSingles).ToList();
                }

                // 2. ��������Ʋ���,���ǲ�һ������
                if (pureSingles.Count == 1)
                {
                    var pair = FindPair(remainingCards.Except(pureSingles).ToList(), -1);
                    if (pair.Any())
                    {
                        return bomb.Concat(pureSingles).Concat(new[] { pair[0] }).ToList();
                    }
                }

                // 3. ���������,���ǲ����
                var pairs = FindPurePairs(remainingCards);
                if (pairs.Count >= 1)
                {
                    var cards1 = pairs[0].Take(1);
                    var cards2 = pairs.Count >= 2 ?
                        pairs[1].Take(1) :
                        pairs[0].Skip(1).Take(1);

                    return bomb.Concat(cards1).Concat(cards2).ToList();
                }
            }
            return new List<Card>();
        }

        /// <summary>
        /// �Ĵ����� - ���ȴ�������
        /// </summary>
        public static List<Card> FindFourWithTwoPairs(List<Card> cards, int rank)
        {
            List<Card> bomb = FindBomb(cards, rank);
            if (bomb.Count == 4)
            {
                var remainingCards = cards.Except(bomb).ToList();

                // ֻ�Ҵ�����
                var purePairs = FindPurePairs(remainingCards)
                               .OrderBy(p => p[0].rank)
                               .Take(2)
                               .ToList();

                if (purePairs.Count == 2)
                {
                    return bomb.Concat(purePairs.SelectMany(p => p)).ToList();
                }
            }
            return new List<Card>();
        }

        // ... �������ҷ��� ...

        #endregion

        #region �������Ͳ���
        public static List<Card> FindLongestStraight(List<Card> cards)
        {
            for (int i = 12; i >= 5; i--)
            {
                var straight = FindStraight(cards, -1, i);
                if (straight.Count > 0) return straight;
            }
            return new List<Card>();
        }

        public static List<Card> FindThreeWithExtra(List<Card> cards, CardType type)
        {
            for (int i = 0; i < cards.Count - 2; i++)
            {
                if (cards[i].rank == cards[i + 1].rank && cards[i].rank == cards[i + 2].rank)
                {
                    var three = new List<Card> { cards[i], cards[i + 1], cards[i + 2] };
                    if (type == CardType.Three) return three;

                    var remainingCards = cards.Except(three).ToList();
                    if (type == CardType.ThreeWithOne)
                    {
                        return remainingCards.Count > 0 ? three.Concat(remainingCards.Take(1)).ToList() : new List<Card>();
                    }
                    else // ThreeWithPair
                    {
                        var pair = FindPair(remainingCards, -1);
                        return pair.Count == 2 ? three.Concat(pair).ToList() : new List<Card>();
                    }
                }
            }
            return new List<Card>();
        }

        // ... �����������Ͳ��ҷ��� ...

        #endregion

        #region �������Ͳ���
/*        public static List<CardCombination> FindAllValidCombinations(List<Card> cards, LastPlayInfo lastPlay = null)
        {
            var combinations = new List<CardCombination>();

            // �����Ƿ���Ҫ����ϼ����������
            if (lastPlay == null || lastPlay.CardType == CardType.Invalid)
            {
                combinations.AddRange(FindFreeCombinations(cards));
            }
            else
            {
                combinations.AddRange(FindResponseCombinations(cards, lastPlay));
            }

            // ������ʱʹ�õ�ը��
            combinations.AddRange(FindBombCombinations(cards));

            return combinations;
        }

        private static List<CardCombination> FindFreeCombinations(List<Card> cards)
        {
            var combinations = new List<CardCombination>();

            // ��ӵ������
            combinations.AddRange(FindSingleCombinations(cards));
            // ��Ӷ������
            combinations.AddRange(FindPairCombinations(cards));
            // ����������
            combinations.AddRange(FindTripleCombinations(cards));
            // ���˳�����
            combinations.AddRange(FindStraightCombinations(cards));

            return combinations;
        }*/
        #endregion

        #region ��������
        /// <summary>
        /// ���Ҵ�����(��˳�ӡ��Ƕ��ӡ������š���ը���ĵ���)
        /// </summary>
        public static List<Card> FindAllPureSingles(List<Card> cards)
        {
            var pureSingles = new List<Card>();
            var groups = cards.GroupBy(c => c.rank).ToList();

            foreach (var group in groups)
            {
                if (group.Count() == 1) // ֻ����һ�ε���
                {
                    var card = group.First();
                    // ����������Ƿ�������˳��
                    if (!CouldFormStraight(cards, card))
                    {
                        pureSingles.Add(card);
                    }
                }
            }

            return pureSingles.OrderBy(c => c.rank).ToList();
        }

        /// <summary>
        /// ���һ�����Ƿ�������˳��
        /// </summary>
        private static bool CouldFormStraight(List<Card> cards, Card card)
        {
            if (card.rank >= Rank.Two) return false;

            // ���ǰ��4�����Ƿ����,�������˳��
            int count = 0;
            for (int i = -4; i <= 4; i++)
            {
                if (i == 0) continue;
                var checkRank = card.rank + i;
                if (checkRank >= Rank.Three && checkRank <= Rank.Ace)
                {
                    if (cards.Any(c => c.rank == checkRank))
                    {
                        count++;
                        if (count >= 4) return true; // �������˳��
                    }
                    else
                    {
                        count = 0;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// ���Ҵ�����(�����š���ը���Ķ���)
        /// </summary>
        public static List<List<Card>> FindPurePairs(List<Card> cards)
        {
            var purePairs = new List<List<Card>>();
            var groups = cards.GroupBy(c => c.rank).ToList();

            foreach (var group in groups)
            {
                if (group.Count() == 2) // ֻ�������ε���
                {
                    purePairs.Add(group.ToList());
                }
            }

            return purePairs.OrderBy(p => p[0].rank).ToList();
        }

        /// <summary>
        /// ���Ҵ�����(��ը��)
        /// </summary>
        public static List<List<Card>> FindPureTriples(List<Card> cards)
        {
            var triples = new List<List<Card>>(); ;
            var groups = cards.GroupBy(c => c.rank).ToList();

            foreach (var group in groups)
            {
                // ֻѡ��ǡ����3�ŵ���
                if (group.Count() == 3)
                {
                    triples.Add(group.ToList());
                }
            }

            return triples.OrderBy(p => p[0].rank).ToList();
        }


        /// <summary>
        /// ��������Ĵ���
        /// </summary>
        public static List<Card> TryFourWithTwo(List<Card> four, List<Card> remainingCards)
        {
            var pureSingleCards = AICardFinder.FindAllPureSingles(remainingCards);
            if (pureSingleCards.Count >= 2)
            {
                // ����ѡ���С������Ϊ����
                var extras = pureSingleCards
                    .Take(2)
                    .ToList();

                var result = new List<Card>();
                result.AddRange(four);
                result.AddRange(extras);
                return result;
            }
            return null;
        }

        /// <summary>
        /// ��������Ĵ�����
        /// </summary>
        public static List<Card> TryFourWithTwoPairs(List<Card> four, List<Card> remainingCards)
        {
            var pairs = FindPurePairs(remainingCards);
            if (pairs.Count >= 2)
            {
                var result = new List<Card>();
                result.AddRange(four);
                result.AddRange(pairs[0]);
                result.AddRange(pairs[1]);
                return result;
            }
            return null;
        }

        /// <summary>
        /// ����ָ������������(��ѡ�Ƿ�����Ǵ���)
        /// </summary>
        private static List<Card> FindAttachingCards(List<Card> cards, int count, bool pureOnly = true)
        {
            var result = new List<Card>();

            // 1. �����Ҵ�����
            var pureCards = FindAllPureSingles(cards)
                            .OrderBy(c => c.rank)
                            .Take(count)
                            .ToList();
            result.AddRange(pureCards);

            // 2. �������Ҫ��������������,���ǲ����
            if (!pureOnly && result.Count < count)
            {
                var pairs = FindPurePairs(cards.Except(result).ToList());
                foreach (var pair in pairs)
                {
                    if (result.Count < count)
                        result.Add(pair[0]);
                    else
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// �������������
        /// </summary>
        private static bool CheckConsecutive(List<IGrouping<Rank, Card>> groups, int startIndex, int count)
        {
            var startRank = groups[startIndex].Key;
            for (int i = 1; i < count; i++)
            {
                if ((int)groups[startIndex + i].Key != (int)startRank + i)
                    return false;
            }
            return true;
        }
        #endregion
    }
}