/*using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AI.Models;
using AI.Strategies;
using AI.Utils;
using UnityEngine;
// ����Ӧ����ȷָ��ʹ�� UnityEngine.Debug
using Debug = UnityEngine.Debug;
using static CardManager;

namespace AI.Core
{
    /// <summary>
    /// �������� - ����ѡ���ִ�в���
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
                // ��������ʵ��
                strategies[StrategyType.Aggressive] = new AggressiveStrategy();
                strategies[StrategyType.Conservative] = new ConservativeStrategy();
                strategies[StrategyType.Control] = new ControlStrategy();
                strategies[StrategyType.Cooperative] = new CooperativeStrategy(relationship);
            }
            catch (Exception e)
            {
                Debug.LogError($"���Գ�ʼ��ʧ��: {e.Message}");
                throw;
            }
        }
        #endregion

        #region Strategy Selection
        /// <summary>
        /// ���ݾ���ѡ����Ѳ���
        /// </summary>
        public IStrategy DetermineStrategy(SituationAnalysis situation)
        {
            try
            {
                // ��ȡ���п��ò��Ե�����
                var rankedStrategies = strategies.Values
                    .Select(strategy => new
                    {
                        Strategy = strategy,
                        Score = EvaluateStrategyScore(strategy, situation)
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ToList();

                // ����������ߵĲ���
                return rankedStrategies.FirstOrDefault()?.Strategy ?? strategies[StrategyType.Conservative];
            }
            catch (Exception e)
            {
                Debug.LogError($"Error determining strategy: {e.Message}");
                return strategies[StrategyType.Conservative];
            }
        }

        /// <summary>
        /// �������Ե������Ե÷�
        /// </summary>
        private float EvaluateStrategyScore(IStrategy strategy, SituationAnalysis situation)
        {
            // ��������������
            float baseScore = strategy.EvaluateSuitability(situation);
            if (baseScore <= 0) return 0;

            // ���ݽ�����Լ�Ȩ
            if (strategy.Type == situation.SuggestedStrategy)
            {
                baseScore *= 1.5f;
            }

            // �����������
            baseScore = AdjustStrategyScore(baseScore, strategy.Type, situation);

            return baseScore;
        }

        /// <summary>
        /// ������������������Ե÷�
        /// </summary>
        private float AdjustStrategyScore(float baseScore, StrategyType strategyType, SituationAnalysis situation)
        {
            float adjustedScore = baseScore;

            // ������Ϸ�׶ε���
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

            // ���ݾ���״̬����
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