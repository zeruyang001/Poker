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
    /// ���Ʋ��� - רע��ά�ֳ���Ȩ,������Ϸ����
    /// </summary>
    public class ControlStrategy : StrategyBase
    {
        private new readonly ComputerAI computerAI;

        public ControlStrategy()
        {
            computerAI = new ComputerAI();
        }

        public override StrategyType Type => StrategyType.Control;
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
                Debug.LogError($"ִ�п��Ʋ���ʧ��: {e.Message}");
                return CreateResult(false);
            }
        }

        private StrategyExecutionResult ExecuteInitiativePlay(List<Card> cards, GameContext context)
        {
            // 1. �ҿ�������
            var controlPlay = FindControlCombination(cards);
            if (controlPlay.Success)
            {
                return controlPlay;
            }

            // 2. ����������
            var chainPlay = TryPlayChainCards(cards);
            if (chainPlay.Success)
            {
                return chainPlay;
            }

            // 3. ��Ҫʱ��ը��
            if (ShouldUseControlBomb(cards, context))
            {
                var bomb = computerAI.FindBomb(cards, -1);
                if (bomb.Any())
                {
                    return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "ʹ�ÿ�����ը��");
                }
            }

            // 4. ���е���
            var mediumCards = FindMediumValueCard(cards);
            if (mediumCards.Any())
            {
                return CreateResult(true, mediumCards, CardType.Single, PlayPurpose.Control, "���еȵ���");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult ExecuteResponsePlay(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            // 1. �س�ʱ���ֿ���
            if (context.BiggestPlayer == args.CurrentCharacter)
            {
                var maintainControl = TryMaintainControl(cards, args, context);
                if (maintainControl.Success)
                {
                    return maintainControl;
                }
            }

            // 2. ��ؿ���Ȩ
            var takeControl = TryTakeControl(cards, args, context);
            if (takeControl.Success)
            {
                return takeControl;
            }

            // 3. ��Ҫʱ��ը��
            if (ShouldUseControlBomb(cards, context))
            {
                var bomb = computerAI.FindBomb(cards, -1);
                if (bomb.Any())
                {
                    return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "ʹ��ը����ȡ����Ȩ");
                }
            }

            return CreateResult(false);
        }

        #region �������ͷ���
        private StrategyExecutionResult FindControlCombination(List<Card> cards)
        {
            // ������
            var threeWithExtra = TryFindThreeWithExtra(cards);
            if (threeWithExtra.Success)
            {
                return threeWithExtra;
            }

            // ����
            var pairChain = computerAI.FindPairStraight(cards, -1, 6);
            if (pairChain.Any())
            {
                return CreateResult(true, pairChain, CardType.PairStraight, PlayPurpose.Control, "�����Կس�");
            }

            // �ɻ�
            var plane = TryFindPlane(cards);
            if (plane.Success)
            {
                return plane;
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult TryFindThreeWithExtra(List<Card> cards)
        {
            var threes = computerAI.FindThree(cards, -1);
            if (!threes.Any())
                return CreateResult(false);

            var remainingCards = cards.Except(threes).ToList();

            // ���ȴ�����
            var pair = computerAI.FindPair(remainingCards, -1);
            if (pair.Any())
            {
                var combination = threes.Concat(pair).ToList();
                return CreateResult(true, combination, CardType.ThreeWithPair, PlayPurpose.Control, "��������");
            }

            // ��δ�����  
            var single = FindAppropriateExtra(remainingCards);
            if (single != null)
            {
                var combination = threes.Concat(new[] { single }).ToList();
                return CreateResult(true, combination, CardType.ThreeWithOne, PlayPurpose.Control, "������һ");
            }

            return CreateResult(false);
        }

        private Card FindAppropriateExtra(List<Card> cards)
        {
            return cards.Where(c => c.rank < Rank.Two)
                       .OrderBy(c => c.rank)
                       .FirstOrDefault();
        }

        private StrategyExecutionResult TryFindPlane(List<Card> cards)
        {
            var plane = computerAI.FindTripleStraight(cards, -1, 6);
            if (plane.Any())
            {
                return CreateResult(true, plane, CardType.TripleStraight, PlayPurpose.Control, "���ɻ�");
            }
            return CreateResult(false);
        }

        private StrategyExecutionResult TryPlayChainCards(List<Card> cards)
        {
            // ˳��
            for (int i = 8; i >= 5; i--)
            {
                var straight = computerAI.FindStraight(cards, -1, i);
                if (straight.Any())
                {
                    return CreateResult(true, straight, CardType.Straight, PlayPurpose.Control, "��˳��");
                }
            }

            return CreateResult(false);
        }
        #endregion

        #region Ӧ�Բ��Է���
        private List<Card> FindMediumValueCard(List<Card> cards)
        {
            var mediumRanks = new[] { Rank.Eight, Rank.Nine, Rank.Ten, Rank.Jack };
            var mediumCard = cards.FirstOrDefault(c => mediumRanks.Contains(c.rank));
            return mediumCard != null
                ? new List<Card> { mediumCard }
                : new List<Card>();
        }

        private StrategyExecutionResult TryMaintainControl(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            var appropriateCards = FindAppropriateResponse(cards, args);
            if (appropriateCards.Any())
            {
                return CreateResult(true, appropriateCards, args.CardType, PlayPurpose.Control, "ά�ֿ���");
            }
            return CreateResult(false);
        }

        private List<Card> FindAppropriateResponse(List<Card> cards, ComputerSmartArgs args)
        {
            return ShouldUseStrongCard(args)
                ? FindBiggerCards(cards, args)
                : FindMinimumWinningCards(cards, args);
        }

        private List<Card> FindBiggerCards(List<Card> cards, ComputerSmartArgs args)
        {
            return cards.Where(c => (int)c.rank > args.Weight + 2)
                       .OrderBy(c => c.rank)
                       .Take(args.Length)
                       .ToList();
        }

        private List<Card> FindMinimumWinningCards(List<Card> cards, ComputerSmartArgs args)
        {
            return cards.Where(c => (int)c.rank > args.Weight)
                       .OrderBy(c => c.rank)
                       .Take(args.Length)
                       .ToList();
        }

        private StrategyExecutionResult TryTakeControl(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            List<Card> selectedCards = args.CardType switch
            {
                CardType.Single => FindBiggerSingle(cards, args.Weight),
                CardType.Pair => computerAI.FindPair(cards, args.Weight),
                CardType.Three => computerAI.FindThree(cards, args.Weight),
                CardType.ThreeWithOne => computerAI.FindThreeWithOne(cards, args.Weight),
                CardType.ThreeWithPair => computerAI.FindThreeAndDouble(cards, args.Weight),
                CardType.Straight => computerAI.FindStraight(cards, args.Weight, args.Length),
                _ => new List<Card>()
            };

            return selectedCards.Any()
                ? CreateResult(true, selectedCards, args.CardType, PlayPurpose.Control, "��ȡ����")
                : CreateResult(false);
        }

        private List<Card> FindBiggerSingle(List<Card> cards, int weight)
        {
            var card = cards.Where(c => (int)c.rank > weight && c.rank <= Rank.Two)
                           .OrderBy(c => c.rank)
                           .FirstOrDefault();
            return card != null
                ? new List<Card> { card }
                : new List<Card>();
        }
        #endregion

        #region �����жϷ���
        private bool ShouldUseControlBomb(List<Card> cards, GameContext context) =>
            context.CurrentPhase == GamePhase.Middle &&
            cards.Count <= 10 &&
            context.PassCount >= 1;

        private bool ShouldUseStrongCard(ComputerSmartArgs args) =>
            args.RemainingCards[args.BiggestCharacter] <= 5;

        protected override float EvaluateCore(SituationAnalysis situation)
        {
            float score = 0f;

            // �е�����ǿ�����ʺ�
            if (situation.HandStrength == HandStrength.Normal)
                score += 0.4f;

            // �о����ʺϿ���
            if (situation.Phase == GamePhase.Middle)
                score += 0.3f;

            // ��ռ����ʱ���ʺϿ���
            if (situation.State is SituationState.Balanced or SituationState.Advantageous)
                score += 0.3f;

            // �س�ʱ�ӷ�
            if (situation.IsInControl)
                score += 0.2f;

            return Mathf.Clamp01(score);
        }
        #endregion
    }
}*/