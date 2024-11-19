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
    /// 控制策略 - 专注于维持出牌权,控制游戏节奏
    /// </summary>
    public class ControlStrategy : StrategyBase
    {
        private new readonly ComputerAI computerAI;

        public ControlStrategy()
        {
            computerAI = new ComputerAI();
        }

        public override StrategyType Type => StrategyType.Control;
        public override string Name => "控制";

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
                Debug.LogError($"执行控制策略失败: {e.Message}");
                return CreateResult(false);
            }
        }

        private StrategyExecutionResult ExecuteInitiativePlay(List<Card> cards, GameContext context)
        {
            // 1. 找控制牌型
            var controlPlay = FindControlCombination(cards);
            if (controlPlay.Success)
            {
                return controlPlay;
            }

            // 2. 找连续牌型
            var chainPlay = TryPlayChainCards(cards);
            if (chainPlay.Success)
            {
                return chainPlay;
            }

            // 3. 需要时用炸弹
            if (ShouldUseControlBomb(cards, context))
            {
                var bomb = computerAI.FindBomb(cards, -1);
                if (bomb.Any())
                {
                    return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "使用控制性炸弹");
                }
            }

            // 4. 出中等牌
            var mediumCards = FindMediumValueCard(cards);
            if (mediumCards.Any())
            {
                return CreateResult(true, mediumCards, CardType.Single, PlayPurpose.Control, "出中等单牌");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult ExecuteResponsePlay(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            // 1. 控场时保持控制
            if (context.BiggestPlayer == args.CurrentCharacter)
            {
                var maintainControl = TryMaintainControl(cards, args, context);
                if (maintainControl.Success)
                {
                    return maintainControl;
                }
            }

            // 2. 夺回控制权
            var takeControl = TryTakeControl(cards, args, context);
            if (takeControl.Success)
            {
                return takeControl;
            }

            // 3. 需要时用炸弹
            if (ShouldUseControlBomb(cards, context))
            {
                var bomb = computerAI.FindBomb(cards, -1);
                if (bomb.Any())
                {
                    return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "使用炸弹夺取控制权");
                }
            }

            return CreateResult(false);
        }

        #region 控制牌型方法
        private StrategyExecutionResult FindControlCombination(List<Card> cards)
        {
            // 三带牌
            var threeWithExtra = TryFindThreeWithExtra(cards);
            if (threeWithExtra.Success)
            {
                return threeWithExtra;
            }

            // 连对
            var pairChain = computerAI.FindPairStraight(cards, -1, 6);
            if (pairChain.Any())
            {
                return CreateResult(true, pairChain, CardType.PairStraight, PlayPurpose.Control, "出连对控场");
            }

            // 飞机
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

            // 优先带对子
            var pair = computerAI.FindPair(remainingCards, -1);
            if (pair.Any())
            {
                var combination = threes.Concat(pair).ToList();
                return CreateResult(true, combination, CardType.ThreeWithPair, PlayPurpose.Control, "出三带二");
            }

            // 其次带单牌  
            var single = FindAppropriateExtra(remainingCards);
            if (single != null)
            {
                var combination = threes.Concat(new[] { single }).ToList();
                return CreateResult(true, combination, CardType.ThreeWithOne, PlayPurpose.Control, "出三带一");
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
                return CreateResult(true, plane, CardType.TripleStraight, PlayPurpose.Control, "出飞机");
            }
            return CreateResult(false);
        }

        private StrategyExecutionResult TryPlayChainCards(List<Card> cards)
        {
            // 顺子
            for (int i = 8; i >= 5; i--)
            {
                var straight = computerAI.FindStraight(cards, -1, i);
                if (straight.Any())
                {
                    return CreateResult(true, straight, CardType.Straight, PlayPurpose.Control, "出顺子");
                }
            }

            return CreateResult(false);
        }
        #endregion

        #region 应对策略方法
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
                return CreateResult(true, appropriateCards, args.CardType, PlayPurpose.Control, "维持控制");
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
                ? CreateResult(true, selectedCards, args.CardType, PlayPurpose.Control, "夺取控制")
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

        #region 策略判断方法
        private bool ShouldUseControlBomb(List<Card> cards, GameContext context) =>
            context.CurrentPhase == GamePhase.Middle &&
            cards.Count <= 10 &&
            context.PassCount >= 1;

        private bool ShouldUseStrongCard(ComputerSmartArgs args) =>
            args.RemainingCards[args.BiggestCharacter] <= 5;

        protected override float EvaluateCore(SituationAnalysis situation)
        {
            float score = 0f;

            // 中等手牌强度最适合
            if (situation.HandStrength == HandStrength.Normal)
                score += 0.4f;

            // 中局最适合控制
            if (situation.Phase == GamePhase.Middle)
                score += 0.3f;

            // 略占优势时最适合控制
            if (situation.State is SituationState.Balanced or SituationState.Advantageous)
                score += 0.3f;

            // 控场时加分
            if (situation.IsInControl)
                score += 0.2f;

            return Mathf.Clamp01(score);
        }
        #endregion
    }
}*/