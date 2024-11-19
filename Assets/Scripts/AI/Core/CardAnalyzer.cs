using System;
using System.Collections.Generic;
using System.Linq;
//using AI.Models;
using UnityEngine;
using AI.Utils;
using static CardManager;
using static AI.Utils.AICardAnalyzer;
using Unity.Collections.LowLevel.Unsafe;

namespace AI.Core
{
    /// <summary>
    /// 卡牌分析器 - 负责分析手牌价值和组合
    /// </summary>
    public class CardAnalyzer
    {
        /// <summary>
        /// 分析手牌
        /// </summary>
        public CardAnalysis AnalyzeHand(List<Card> cards)
        {
            CardAnalysis analysis = new CardAnalysis
            {
                PotentialCombinations = new List<CardCombination>(),
                CardTypeCounts = new Dictionary<CardType, int>()
            };

            try
            {
                analysis.BombCount = CountBombs(cards);
                analysis.HasRocket = HasJokerBomb(cards);
                analysis.HandStrength = CalculateHandStrength(cards);
                analysis.PotentialCombinations = FindAllPotentialCombinations(cards);
                AnalyzeCardTypes(cards, analysis);
            }
            catch (Exception e)
            {
                Debug.LogError($"手牌分析错误: {e.Message}");
            }

            return analysis;
        }


        private List<CardCombination> FindAllPotentialCombinations(List<Card> sortedCards)
        {
            var combinations = new List<CardCombination>();

            // 1. 找出所有顺子
            var straights = AICardAnalyzer.FindAllStraights(sortedCards);
            if (straights.Any())
                combinations.AddRange(straights);

            // 2. 找出所有对子
            var pairs = AICardAnalyzer.FindCombPurePairs(sortedCards);
            if (pairs.Any())
                combinations.AddRange(pairs);

            // 3. 找出所有三张
            var triples = AICardAnalyzer.FindCombPureTriples(sortedCards);
            if (triples.Any())
                combinations.AddRange(triples);

            // 4. 找出所有飞机
            var planes = AICardAnalyzer.FindAllPlanes(sortedCards);
            if (planes.Any())
                combinations.AddRange(planes);

            // 5. 找出所有炸弹
            var bombs = AICardAnalyzer.FindCombBombs(sortedCards);
            if (bombs.Any())
                combinations.AddRange(bombs);

            // 6. 找出所有四带二
            var fourWithExtras = AICardAnalyzer.FindAllFourWithExtra(sortedCards);
            if (fourWithExtras.Any())
                combinations.AddRange(fourWithExtras);

            // 7. 找出所有连对
            var pairStraights = AICardAnalyzer.FindAllPairStraights(sortedCards);
            if (pairStraights.Any())
                combinations.AddRange(pairStraights);

            // 8. 找出王炸
            var jokerBomb= FindCombJokerBombs(sortedCards);
            if (jokerBomb.Any())
                combinations.AddRange(jokerBomb);

            return combinations;
        }

        private void AnalyzeCardTypes(List<Card> cards, CardAnalysis analysis)
        {
            var counts = new Dictionary<CardType, int>();
            foreach (var combo in analysis.PotentialCombinations)
            {
                if (!counts.ContainsKey(combo.Type))
                {
                    counts[combo.Type] = 0;
                }
                counts[combo.Type]++;
            }
            analysis.CardTypeCounts = counts;
        }
    }
}