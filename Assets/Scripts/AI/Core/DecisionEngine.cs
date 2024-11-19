/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AI.Models;
using AI.Strategies;
using AI.Utils;
using UnityEngine;
// 顶部应该明确指定使用 UnityEngine.Debug
using Debug = UnityEngine.Debug;
using static CardManager;

namespace AI.Core
{
    /// <summary>
    /// 决策引擎 - 负责选择和执行策略
    /// </summary>
    public class DecisionEngine
    {
        #region Fields
        private readonly Dictionary<StrategyType, IStrategy> strategies;
        private readonly ComputerAI aiController;
        private PlayerRelationship relationship;
        #endregion

        #region Constructor
        public DecisionEngine(ComputerAI controller)
        {
            aiController = controller;
            strategies = new Dictionary<StrategyType, IStrategy>();
        }
        #endregion

        #region Initialization
        public void OnInitialized(PlayerRelationship playerRelationship)
        {
            relationship = playerRelationship ?? throw new ArgumentNullException(nameof(playerRelationship));
            InitializeStrategies();
        }

        private void InitializeStrategies()
        {
            try
            {
                // 创建策略实例
                strategies[StrategyType.Aggressive] = new AggressiveStrategy();
                strategies[StrategyType.Conservative] = new ConservativeStrategy();
                strategies[StrategyType.Control] = new ControlStrategy();
                strategies[StrategyType.Cooperative] = new CooperativeStrategy(relationship);
            }
            catch (Exception e)
            {
                Debug.LogError($"策略初始化失败: {e.Message}");
                throw;
            }
        }
        #endregion

        #region Strategy Selection
        /// <summary>
        /// 根据局势选择最佳策略
        /// </summary>
        public IStrategy DetermineStrategy(SituationAnalysis situation)
        {
            try
            {
                // 获取所有可用策略的评分
                var rankedStrategies = strategies.Values
                    .Select(strategy => new
                    {
                        Strategy = strategy,
                        Score = EvaluateStrategyScore(strategy, situation)
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ToList();

                // 返回评分最高的策略
                return rankedStrategies.FirstOrDefault()?.Strategy ?? strategies[StrategyType.Conservative];
            }
            catch (Exception e)
            {
                Debug.LogError($"Error determining strategy: {e.Message}");
                return strategies[StrategyType.Conservative];
            }
        }

        /// <summary>
        /// 评估策略的适用性得分
        /// </summary>
        private float EvaluateStrategyScore(IStrategy strategy, SituationAnalysis situation)
        {
            // 基础适用性评分
            float baseScore = strategy.EvaluateSuitability(situation);
            if (baseScore <= 0) return 0;

            // 根据建议策略加权
            if (strategy.Type == situation.SuggestedStrategy)
            {
                baseScore *= 1.5f;
            }

            // 特殊情况调整
            baseScore = AdjustStrategyScore(baseScore, strategy.Type, situation);

            return baseScore;
        }

        /// <summary>
        /// 根据特殊情况调整策略得分
        /// </summary>
        private float AdjustStrategyScore(float baseScore, StrategyType strategyType, SituationAnalysis situation)
        {
            float adjustedScore = baseScore;

            // 根据游戏阶段调整
            switch (situation.Phase)
            {
                case GamePhase.Opening:
                    if (strategyType == StrategyType.Conservative)
                        adjustedScore *= 1.2f;
                    break;
                case GamePhase.Endgame:
                    if (strategyType == StrategyType.Aggressive)
                        adjustedScore *= 1.3f;
                    break;
            }

            // 根据局势状态调整
            if (situation.IsUnderPressure && strategyType == StrategyType.Conservative)
                adjustedScore *= 1.2f;
            if (situation.HasWinningChance && strategyType == StrategyType.Aggressive)
                adjustedScore *= 1.5f;
            if (situation.NeedsToBlockWin && strategyType == StrategyType.Aggressive)
                adjustedScore *= 1.4f;

            return adjustedScore;
        }
        #endregion
    }
}*/