/*using System;
using System.Collections.Generic;
using System.Linq;
using AI.Core;
using AI.Models;
using AI.Utils;
using UnityEngine;
using static CardManager;

namespace AI.Strategies
{
    /// <summary>
    /// 配合策略 - 专注于与队友配合,基于出牌顺序和角色调整策略
    /// </summary>
    public class CooperativeStrategy : StrategyBase
    {
        #region Fields
        private readonly PlayerRelationship relationship;
        private readonly SituationAnalyzer situationAnalyzer;
        #endregion

        #region Properties
        public override StrategyType Type => StrategyType.Cooperative;
        public override string Name => "配合";
        #endregion

        public CooperativeStrategy(PlayerRelationship playerRelationship)
        {
            relationship = playerRelationship;
            situationAnalyzer = new SituationAnalyzer();
        }

        protected override StrategyExecutionResult ExecuteCore(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            try
            {
                var situation = situationAnalyzer.Analyze(context);
                return args.CardType == CardType.Invalid
                    ? ExecuteInitiativePlay(cards, context, situation)
                    : ExecuteResponsePlay(cards, args, context, situation);
            }
            catch (Exception e)
            {
                Debug.LogError($"执行配合策略失败: {e.Message}");
                return CreateResult(false);
            }
        }

        private StrategyExecutionResult ExecuteInitiativePlay(List<Card> cards, GameContext context, SituationAnalysis situation)
        {
            var nextPlayer = relationship.GetNextPlayer(relationship.Self);
            bool isNextPlayerPartner = relationship.IsPartner(nextPlayer);

            if (isNextPlayerPartner)
            {
                return HandlePartnerNext(cards, context, situation);
            }
            else
            {
                return HandleLandlordNext(cards, context, situation);
            }
        }

        private StrategyExecutionResult ExecuteResponsePlay(List<Card> cards, ComputerSmartArgs args, GameContext context, SituationAnalysis situation)
        {
            var nextPlayer = relationship.GetNextPlayer(relationship.Self);
            bool isNextPlayerPartner = relationship.IsPartner(nextPlayer);
            var currentPlayer = new Player(args.CurrentCharacter);

            if (relationship.IsPartner(currentPlayer))
            {
                return HandlePartnerPlay(cards, args, situation);
            }
            else
            {
                return HandleLandlordPlay(cards, args, context, situation, isNextPlayerPartner);
            }
        }

        #region Core Strategy Methods
        private StrategyExecutionResult HandlePartnerNext(List<Card> cards, GameContext context, SituationAnalysis situation)
        {
            if (IsPartnerInDanger(context))
            {
                var assistCards = FindAssistCards(cards);
                if (assistCards.Any())
                {
                    return CreateResult(true, assistCards, GetCardType(assistCards), PlayPurpose.Cooperate, "协助队友");
                }
            }

            var smallCards = FindSmallCards(cards);
            if (smallCards.Any())
            {
                return CreateResult(true, smallCards, GetCardType(smallCards), PlayPurpose.Cooperate, "出小牌让队友压制");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult HandleLandlordNext(List<Card> cards, GameContext context, SituationAnalysis situation)
        {
            if (situation.TeamStrength >= situation.OpponentStrength)
            {
                var pressureCards = FindPressureCards(cards);
                if (pressureCards.Any())
                {
                    return CreateResult(true, pressureCards, GetCardType(pressureCards), PlayPurpose.Control, "压制地主");
                }
            }

            var safeCards = FindSafePlay(cards);
            if (safeCards.Any())
            {
                return CreateResult(true, safeCards, GetCardType(safeCards), PlayPurpose.Control, "安全出牌");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult HandlePartnerPlay(List<Card> cards, ComputerSmartArgs args, SituationAnalysis situation)
        {
            if (IsStrongPlay(args) || situation.ShouldKeepKeyCards)
            {
                return CreateResult(false);
            }

            var supportCards = FindSupportCards(cards, args);
            if (supportCards.Any())
            {
                return CreateResult(true, supportCards, args.CardType, PlayPurpose.Cooperate, "配合队友");
            }

            return CreateResult(false);
        }

        private StrategyExecutionResult HandleLandlordPlay(List<Card> cards, ComputerSmartArgs args, GameContext context, SituationAnalysis situation, bool isNextPlayerPartner)
        {
            if (isNextPlayerPartner && IsPartnerInDanger(context))
            {
                var responseCards = FindResponseCards(cards, args);
                if (responseCards.Any())
                {
                    return CreateResult(true, responseCards, args.CardType, PlayPurpose.Control, "为队友解围");
                }
            }

            return CreateResult(false);
        }
        #endregion

        #region Helper Methods
        private bool IsPartnerInDanger(GameContext context)
        {
            var partner = relationship.GetPartner();
            return partner != null && context.GetRemainingCardCount(partner.characterType) <= 3;
        }

        private bool IsStrongPlay(ComputerSmartArgs args)
        {
            return args.Weight >= (int)Rank.Two ||
                   args.CardType is CardType.Bomb or CardType.JokerBomb;
        }

        private List<Card> FindAssistCards(List<Card> cards)
        {
            return cards.Where(c => c.rank <= Rank.Ten)
                       .OrderBy(c => c.rank)
                       .Take(1)
                       .ToList();
        }

        private List<Card> FindSmallCards(List<Card> cards)
        {
            return cards.Where(c => !IsKeyCard(c))
                       .OrderBy(c => c.rank)
                       .Take(1)
                       .ToList();
        }

        private List<Card> FindPressureCards(List<Card> cards)
        {
            // 先找炸弹
            var bomb = computerAI.FindBomb(cards, -1);
            if (bomb.Any()) return bomb;

            // 找大牌
            return cards.Where(c => c.rank >= Rank.Ace)
                       .OrderBy(c => c.rank)
                       .Take(1)
                       .ToList();
        }

        private List<Card> FindSafePlay(List<Card> cards)
        {
            return cards.Where(c => c.rank <= Rank.Queen)
                       .OrderBy(c => c.rank)
                       .Take(1)
                       .ToList();
        }

        private List<Card> FindSupportCards(List<Card> cards, ComputerSmartArgs args)
        {
            return cards.Where(c =>(int)c.rank > args.Weight && c.rank <= Rank.King)
                       .OrderBy(c => c.rank)
                       .Take(args.Length)
                       .ToList();
        }

        private List<Card> FindResponseCards(List<Card> cards, ComputerSmartArgs args)
        {
            return cards.Where(c => (int)c.rank > args.Weight)
                       .OrderBy(c => c.rank)
                       .Take(args.Length)
                       .ToList();
        }

        private bool IsKeyCard(Card card)
        {
            return card.rank >= Rank.Two ||
                   card.rank == Rank.SJoker ||
                   card.rank == Rank.LJoker;
        }
        #endregion

        protected override float EvaluateCore(SituationAnalysis situation)
        {
            float score = 0f;

            if (!relationship.IsLandlord)
            {
                score += situation.TeamStrength * 0.4f;

                if (situation.HandStrength == HandStrength.Normal)
                    score += 0.3f;

                if (!situation.IsInControl)
                    score += 0.3f;
            }

            return Mathf.Clamp01(score);
        }
    }
}*/