/*using System;
using System.Collections.Generic;
using AI.Models;
using AI.Strategies;
using AI.Utils;
using UnityEngine;
using static CardManager;

namespace AI.Core
{
    /// <summary>
    /// 策略执行器 - 负责执行策略和处理结果
    /// </summary>
    public class StrategyExecutor
    {
        #region Fields
        private readonly ComputerAI computerAI;
        //private readonly PlayerRelationship relationship;
        private List<Card> selectedCards;
        private CardType currentCardType;
        private IStrategy currentStrategy;
        #endregion

        #region Constructor
*//*        public StrategyExecutor(ComputerAI ai, PlayerRelationship playerRelationship)
        {
            computerAI = ai;
            relationship = playerRelationship;
            selectedCards = new List<Card>();
            currentCardType = CardType.Invalid;
        }*//*
        #endregion

        #region Public Methods
        /// <summary>
        /// 执行策略
        /// </summary>
        public StrategyExecutionResult ExecuteStrategy(IStrategy strategy, List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            try
            {
                currentStrategy = strategy;
                bool success = strategy.Execute(cards, args, context);

                // 转换策略执行结果
                var result = new StrategyExecutionResult
                {
                    Success = success,
                    SelectedCards = success ? new List<Card>(selectedCards) : new List<Card>(),
                    SelectedType = success ? currentCardType : CardType.Invalid,
                    Purpose = success ? PlayPurpose.Control : PlayPurpose.Control,
                    Description = success ? "策略执行成功" : "策略执行失败"
                };

                if (success)
                {
                    UpdateSelectedCards(result);
                    HandleSpecialCases(result.SelectedType);
                    LogStrategy(result);
                }

                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"策略执行失败: {e.Message}");
                Reset();
                return new StrategyExecutionResult
                {
                    Success = false,
                    SelectedCards = new List<Card>(),
                    SelectedType = CardType.Invalid,
                    Purpose = PlayPurpose.Control,
                    Description = $"策略执行失败: {e.Message}"
                };
            }
        }

        /// <summary>
        /// 获取当前选中的牌
        /// </summary>
        public List<Card> GetSelectedCards() => new List<Card>(selectedCards);

        /// <summary>
        /// 获取当前牌型
        /// </summary>
        public CardType GetCurrentType() => currentCardType;

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            selectedCards.Clear();
            currentCardType = CardType.Invalid;
            currentStrategy = null;
        }
        #endregion

        #region Private Methods
        private void UpdateSelectedCards(StrategyExecutionResult result)
        {
            selectedCards.Clear();
            selectedCards.AddRange(result.SelectedCards);
            currentCardType = result.SelectedType;

            // 同步到ComputerAI
            computerAI.selectedCards = selectedCards;
            computerAI.currentType = currentCardType;
        }

        private void HandleSpecialCases(CardType cardType)
        {
            // 处理炸弹和王炸的积分翻倍
            if (cardType == CardType.Bomb || cardType == CardType.JokerBomb)
            {
                Debug.Log($"触发 {cardType} 积分翻倍");
            }
        }

        private void LogStrategy(StrategyExecutionResult result)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"策略执行: {currentStrategy?.GetType().Name}" +
                         $", 牌型: {result.SelectedType}" +
                         $", 张数: {result.SelectedCards.Count}" +
                         $", 目的: {result.Purpose}");
            }
        }
        #endregion
    }
}*/