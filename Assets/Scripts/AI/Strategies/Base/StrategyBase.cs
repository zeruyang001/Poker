/*
using AI.Utils;
using UnityEngine;
using static CardManager;
using System.Collections.Generic;
using System;

namespace AI.Strategies
{
    public struct StrategyExecutionResult
    {
        public bool Success { get; set; }
        public List<Card> SelectedCards { get; set; }
        public CardType SelectedType { get; set; }
        public PlayPurpose Purpose { get; set; }
        public string Description { get; set; }
    }

    public abstract class StrategyBase : IStrategy
    {
        protected readonly ComputerAI computerAI;

        public StrategyBase()
        {
            computerAI = new ComputerAI();
        }

        public abstract StrategyType Type { get; }
        public abstract string Name { get; }

        public virtual bool Execute(List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            if (!CanExecute(cards, args))
                return false;

            try
            {
                var result = ExecuteCore(cards, args, context);
                if (result.Success)
                {
                    OnStrategyExecuted(result);
                }
                return result.Success;
            }
            catch (Exception e)
            {
                Debug.LogError($"Strategy {Name} execution failed: {e}");
                return false;
            }
        }

        public virtual float EvaluateSuitability(SituationAnalysis situation)
        {
            if (!IsPhaseApplicable(situation.Phase))
                return 0f;
            return EvaluateCore(situation);
        }

        public virtual bool CanExecute(List<Card> cards, ComputerSmartArgs args)
        {
            return cards != null && cards.Count > 0 && args != null;
        }

        public virtual bool ShouldUseBomb(SituationAnalysis situation)
        {
            return situation.Phase == GamePhase.Endgame ||
                   situation.HasWinningChance ||
                   situation.NeedsToBlockWin;
        }

        // 核心策略实现
        protected abstract StrategyExecutionResult ExecuteCore(List<Card> cards, ComputerSmartArgs args, GameContext context);
        protected abstract float EvaluateCore(SituationAnalysis situation);

        // 辅助方法
        protected virtual bool IsPhaseApplicable(GamePhase phase)
        {
            return true;
        }

        protected virtual void OnStrategyExecuted(StrategyExecutionResult result)
        {
            Debug.Log($"{Name} strategy executed: {result.Description}");
        }

        protected StrategyExecutionResult CreateResult(
            bool success,
            List<Card> cards = null,
            CardType type = CardType.Invalid,
            PlayPurpose purpose = PlayPurpose.Control,
            string description = null)
        {
            return new StrategyExecutionResult
            {
                Success = success,
                SelectedCards = cards,
                SelectedType = type,
                Purpose = purpose,
                Description = description ?? $"{Name} strategy {(success ? "succeeded" : "failed")}"
            };
        }
    }
}*/