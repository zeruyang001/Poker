/*using System;
using System.Collections.Generic;
using System.Linq;
using AI.Models;
using AI.Utils;
using UnityEngine;
using static CardManager;

namespace AI.Strategies
{
    /// <summary>
    /// ���ز��� - ���ȳ�С��,�����ؼ���
    /// </summary>
    public class ConservativeStrategy : StrategyBase
    {
        private new readonly ComputerAI computerAI;

        public ConservativeStrategy()
        {
            computerAI = new ComputerAI();
        }

        public override StrategyType Type => StrategyType.Conservative;
        public override string Name => "����";

        protected override StrategyExecutionResult ExecuteCore(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            try
            {
                return args.CardType == CardType.Invalid
                    ? ExecuteInitiativePlay(cards, context)
                    : ExecuteResponsePlay(cards, args, context);
            }
            catch (Exception e)
            {
                Debug.LogError($"ִ�б��ز���ʧ��: {e.Message}");
                return CreateResult(false);
            }
        }

        private StrategyExecutionResult ExecuteInitiativePlay(List<Card> cards, GameContext context)
        {
            // 1. ����С����
            var smallestCards = FindSmallestSingle(cards);
            if (smallestCards.Any())
            {
                return CreateResult(true, smallestCards, CardType.Single, PlayPurpose.Discard, "����С����");
            }

            // 2. ��С����
            var smallPairCards = FindSmallPair(cards);
            if (smallPairCards.Any())
            {
                return CreateResult(true, smallPairCards, CardType.Pair, PlayPurpose.Discard, "��С����");
            }

            // 3. ��С˳��
            var straightResult = TryPlaySmallStraight(cards);
            if (straightResult.Success)
            {
                return straightResult;
            }

            // 4. ��Ҫʱ������
            var threeResult = TryPlayThreeWithCard(cards);
            if (threeResult.Success)
            {
                return threeResult;
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult ExecuteResponsePlay(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            // 1. ����С��ѹ����
            var minResponse = TryMinimumResponse(cards, args);
            if (minResponse.Success)
            {
                return minResponse;
            }

            // 2. �ؼ�ʱ����ը��
            if (IsInCriticalSituation(context))
            {
                var bombResponse = TryUseBomb(cards, args, context);
                if (bombResponse.Success)
                {
                    return bombResponse;
                }
            }

            return CreateResult(false);
        }

        #region ���Ƹ�������
        private List<Card> FindSmallestSingle(List<Card> cards)
        {
            var nonKeyCard = cards.Where(c => !IsKeyCard(c))
                                 .OrderBy(c => c.rank)
                                 .FirstOrDefault();
            return nonKeyCard != null
                ? new List<Card> { nonKeyCard }
                : new List<Card>();
        }

        private List<Card> FindSmallPair(List<Card> cards)
        {
            for (int i = 0; i < cards.Count - 1; i++)
            {
                if (cards[i].rank == cards[i + 1].rank && !IsKeyCard(cards[i]))
                {
                    return new List<Card> { cards[i], cards[i + 1] };
                }
            }
            return new List<Card>();
        }

        private StrategyExecutionResult TryPlaySmallStraight(List<Card> cards)
        {
            var orderedCards = cards.Where(c => (int)c.rank <= (int)Rank.Ten)
                                  .OrderBy(c => c.rank)
                                  .ToList();

            for (int i = 5; i <= 8; i++)
            {
                var straight = computerAI.FindStraight(orderedCards, -1, i);
                if (straight.Any())
                {
                    return CreateResult(true, straight, CardType.Straight, PlayPurpose.Discard, "��С˳��");
                }
            }
            return CreateResult(false);
        }

        private StrategyExecutionResult TryPlayThreeWithCard(List<Card> cards)
        {
            var smallThree = computerAI.FindThree(cards.Where(c => (int)c.rank <= (int)Rank.Ten).ToList(), -1);
            if (!smallThree.Any())
                return CreateResult(false);

            var remainingCards = cards.Except(smallThree).ToList();
            var extra = remainingCards.OrderBy(c => c.rank).FirstOrDefault();

            if (extra != null)
            {
                var combined = new List<Card>(smallThree) { extra };
                return CreateResult(true, combined, CardType.ThreeWithOne, PlayPurpose.Discard, "��С����һ");
            }

            return CreateResult(true, smallThree, CardType.Three, PlayPurpose.Discard, "��С����");
        }

        private StrategyExecutionResult TryMinimumResponse(List<Card> cards, ComputerSmartArgs args)
        {
            List<Card> selectedCards = null;

            switch (args.CardType)
            {
                case CardType.Single:
                    selectedCards = FindMinimumBiggerSingle(cards, args.Weight);
                    break;
                case CardType.Pair:
                    selectedCards = FindMinimumBiggerPair(cards, args.Weight);
                    break;
                case CardType.Three:
                    selectedCards = computerAI.FindThree(cards, args.Weight);
                    break;
                case CardType.ThreeWithOne:
                    selectedCards = TryMinimumThreeWithOne(cards, args.Weight);
                    break;
                case CardType.Straight:
                    selectedCards = computerAI.FindStraight(cards, args.Weight, args.Length);
                    break;
            }

            return selectedCards?.Any() == true
                ? CreateResult(true, selectedCards, args.CardType, PlayPurpose.Control, "��Сѹ�Ƴ���")
                : CreateResult(false);
        }

        private List<Card> FindMinimumBiggerSingle(List<Card> cards, int weight)
        {
            var bigger = cards.Where(c => (int)c.rank > weight)
                            .OrderBy(c => c.rank)
                            .FirstOrDefault();
            return bigger != null
                ? new List<Card> { bigger }
                : new List<Card>();
        }

        private List<Card> FindMinimumBiggerPair(List<Card> cards, int weight)
        {
            for (int i = 0; i < cards.Count - 1; i++)
            {
                if (cards[i].rank == cards[i + 1].rank && (int)cards[i].rank > weight)
                {
                    return new List<Card> { cards[i], cards[i + 1] };
                }
            }
            return new List<Card>();
        }

        private List<Card> TryMinimumThreeWithOne(List<Card> cards, int weight)
        {
            var three = computerAI.FindThree(cards, weight);
            if (!three.Any())
                return new List<Card>();

            var remainingCards = cards.Except(three).ToList();
            var smallest = remainingCards.OrderBy(c => c.rank).FirstOrDefault();

            return smallest != null
                ? three.Concat(new[] { smallest }).ToList()
                : new List<Card>();
        }
        #endregion

        #region �����жϷ���
        private StrategyExecutionResult TryUseBomb(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            if (!IsWorthUsingBomb(context))
                return CreateResult(false);

            var bomb = computerAI.FindBomb(cards, args.CardType == CardType.Bomb ? args.Weight : -1);
            if (bomb.Any())
            {
                return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "��Ҫʱʹ��ը��");
            }

            return CreateResult(false);
        }

        private bool IsInCriticalSituation(GameContext context)
        {
            return context.CurrentPhase == GamePhase.Endgame ||
                   context.PassCount >= 2;
        }

        private bool IsWorthUsingBomb(GameContext context)
        {
            return context.CurrentPhase == GamePhase.Endgame &&
                   context.PassCount >= 2;
        }

        private bool IsKeyCard(Card card)
        {
            return card.rank >= Rank.Ace;
        }
        #endregion

        protected override float EvaluateCore(SituationAnalysis situation)
        {
            float score = 0f;

            // ����ǿ������(Խ��Խ�ʺϱ��ز���)
            if (situation.HandStrength <= HandStrength.Weak)
                score += 0.4f;

            // ��Ϸ�׶�����(���ָ��ʺϱ���)
            if (situation.Phase == GamePhase.Opening)
                score += 0.3f;

            // ��������(���Ƹ��ʺϱ���)
            if (situation.State == SituationState.Disadvantageous)
                score += 0.3f;

            // �����������
            if (situation.IsUnderPressure)
                score += 0.2f;

            return Mathf.Clamp01(score);
        }
    }
}*/