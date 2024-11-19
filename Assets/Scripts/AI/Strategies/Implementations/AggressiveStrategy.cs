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
    /// 进攻策略 - 优先使用大牌和炸弹控制局势
    /// </summary>
    public class AggressiveStrategy : StrategyBase
    {
        private new readonly ComputerAI computerAI;

        public AggressiveStrategy()
        {
            computerAI = new ComputerAI();
        }

        public override StrategyType Type => StrategyType.Aggressive;
        public override string Name => "进攻";

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
                Debug.LogError($"执行进攻策略失败: {e.Message}");
                return CreateResult(false);
            }
        }

        private StrategyExecutionResult ExecuteInitiativePlay(List<Card> cards, GameContext context)
        {
            // 1. 尝试直接获胜
            if (CanWinDirectly(cards))
            {
                return CreateResult(true, cards, GetCardType(cards), PlayPurpose.Control, "一次性出完获胜");
            }

            // 2. 使用炸弹或王炸
            var bombPlay = TryPlayBomb(cards, context);
            if (bombPlay.Success)
            {
                return bombPlay;
            }

            // 3. 优先大牌组合
            var highValuePlay = TryPlayHighValueCards(cards);
            if (highValuePlay.Success)
            {
                return highValuePlay;
            }

            // 4. 出最大单牌
            var biggestSingle = FindBiggestSingle(cards);
            if (biggestSingle.Any())
            {
                return CreateResult(true, biggestSingle, CardType.Single, PlayPurpose.Control, "出最大单牌");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult ExecuteResponsePlay(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            // 1. 用大牌压制 
            if (ShouldBeatWithBigCards(args, context))
            {
                var response = TryBeatWithBiggerCards(cards, args);
                if (response.Success)
                {
                    return response;
                }
            }

            // 2. 使用炸弹压制
            if (ShouldUseBombToRespond(args, context))
            {
                var bombResponse = TryPlayBombToRespond(cards, args);
                if (bombResponse.Success)
                {
                    return bombResponse;
                }
            }

            return CreateResult(false);
        }

        #region 出牌辅助方法
        private bool CanWinDirectly(List<Card> cards)
        {
            return cards.Count <= 4 && CardManager.CanPop(cards, out _);
        }

        private StrategyExecutionResult TryPlayBomb(List<Card> cards, GameContext context)
        {
            if (!ShouldUseBombInitiatively(context))
            {
                return CreateResult(false);
            }

            var bomb = computerAI.FindBomb(cards, -1);
            if (bomb.Any())
            {
                return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "主动使用炸弹");
            }

            var rocket = computerAI.FindJokerBomb(cards);
            if (rocket.Any())
            {
                return CreateResult(true, rocket, CardType.JokerBomb, PlayPurpose.Control, "主动使用王炸");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult TryPlayBombToRespond(List<Card> cards, ComputerSmartArgs args)
        {
            if (args.CardType == CardType.Bomb)
            {
                var biggerBomb = computerAI.FindBomb(cards, args.Weight);
                if (biggerBomb.Any())
                {
                    return CreateResult(true, biggerBomb, CardType.Bomb, PlayPurpose.Control, "使用更大炸弹");
                }
            }
            else
            {
                var bomb = computerAI.FindBomb(cards, -1);
                if (bomb.Any())
                {
                    return CreateResult(true, bomb, CardType.Bomb, PlayPurpose.Control, "使用炸弹压制");
                }
            }

            var rocket = computerAI.FindJokerBomb(cards);
            if (rocket.Any())
            {
                return CreateResult(true, rocket, CardType.JokerBomb, PlayPurpose.Control, "使用王炸压制");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult TryPlayHighValueCards(List<Card> cards)
        {
            // 修正 Func 参数类型错误
            var tryMethods = new List<(CardType type, Func<List<Card>, int, List<Card>> finder)>
            {
                (CardType.ThreeWithPair, computerAI.FindThreeAndDouble),
                (CardType.ThreeWithOne, computerAI.FindThreeWithOne),
                (CardType.Three, computerAI.FindThree),
                (CardType.Pair, computerAI.FindPair)
            };

            // 单独处理需要额外参数的方法
            if (TryFindTripleStraight(cards, out var tripleStraight))
            {
                return CreateResult(true, tripleStraight, CardType.TripleStraight, PlayPurpose.Control, "出飞机");
            }

            if (TryFindStraight(cards, out var straight))
            {
                return CreateResult(true, straight, CardType.Straight, PlayPurpose.Control, "出顺子");
            }

            foreach (var (type, finder) in tryMethods)
            {
                var result = finder(cards, -1);
                if (result.Any())
                {
                    return CreateResult(true, result, type, PlayPurpose.Control, $"出{type}");
                }
            }

            return CreateResult(false);
        }
        private bool TryFindTripleStraight(List<Card> cards, out List<Card> result)
        {
            result = computerAI.FindTripleStraight(cards, -1, 6);
            return result.Any();
        }

        private bool TryFindStraight(List<Card> cards, out List<Card> result)
        {
            result = computerAI.FindStraight(cards, -1, 5);
            return result.Any();
        }

        private List<Card> FindBiggestSingle(List<Card> cards)
        {
            var biggest = cards.OrderByDescending(c => c.rank).FirstOrDefault();
            return biggest != null ? new List<Card> { biggest } : new List<Card>();
        }

        private StrategyExecutionResult TryBeatWithBiggerCards(List<Card> cards, ComputerSmartArgs args)
        {
            List<Card> selectedCards = null;

            switch (args.CardType)
            {
                case CardType.Single:
                    selectedCards = computerAI.FindSingle(cards, args.Weight);
                    break;
                case CardType.Pair:
                    selectedCards = computerAI.FindPair(cards, args.Weight);
                    break;
                case CardType.Three:
                    selectedCards = computerAI.FindThree(cards, args.Weight);
                    break;
                case CardType.ThreeWithOne:
                    selectedCards = computerAI.FindThreeWithOne(cards, args.Weight);
                    break;
                case CardType.ThreeWithPair:
                    selectedCards = computerAI.FindThreeAndDouble(cards, args.Weight);
                    break;
                case CardType.Straight:
                    selectedCards = computerAI.FindStraight(cards, args.Weight, args.Length);
                    break;
                case CardType.PairStraight:
                    selectedCards = computerAI.FindPairStraight(cards, args.Weight, args.Length);
                    break;
            }

            if (selectedCards?.Any() == true)
            {
                return CreateResult(true, selectedCards, args.CardType, PlayPurpose.Control, "压制对手出牌");
            }

            return CreateResult(false);
        }

        #endregion

        #region 策略判断方法
        private bool ShouldUseBombInitiatively(GameContext context)
        {
            return context.CurrentPhase == GamePhase.Endgame ||
                   context.PassCount >= 2;
        }

        private bool ShouldUseBombToRespond(ComputerSmartArgs args, GameContext context)
        {
            return context.CurrentPhase == GamePhase.Endgame ||
                   args.Weight >= (int)Rank.Two ||
                   context.PassCount >= 2;
        }

        private bool ShouldBeatWithBigCards(ComputerSmartArgs args, GameContext context)
        {
            return context.IsEndgame() || args.Weight >= (int)Rank.Jack;
        }
        #endregion

        protected override float EvaluateCore(SituationAnalysis situation)
        {
            float score = 0f;

            // 手牌强度评分
            if (situation.HandStrength >= HandStrength.Strong)
                score += 0.3f;

            // 游戏阶段评分
            if (situation.Phase == GamePhase.Endgame)
                score += 0.3f;

            // 局势评分
            if (situation.State == SituationState.Dominant)
                score += 0.4f;

            // 特殊情况评分
            if (situation.HasWinningChance)
                score += 0.5f;
            if (situation.NeedsToBlockWin)
                score += 0.4f;

            return Mathf.Clamp01(score);
        }

        private CardType GetCardType(List<Card> cards)
        {
            CardType type;
            return CardManager.CanPop(cards, out type) ? type : CardType.Invalid;
        }
    }
}*/