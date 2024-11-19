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
    /// ����ִ���� - ����ִ�в��Ժʹ�����
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
        /// ִ�в���
        /// </summary>
        public StrategyExecutionResult ExecuteStrategy(IStrategy strategy, List<Card> cards, ComputerSmartArgs args, GameContext context)
        {
            try
            {
                currentStrategy = strategy;
                bool success = strategy.Execute(cards, args, context);

                // ת������ִ�н��
                var result = new StrategyExecutionResult
                {
                    Success = success,
                    SelectedCards = success ? new List<Card>(selectedCards) : new List<Card>(),
                    SelectedType = success ? currentCardType : CardType.Invalid,
                    Purpose = success ? PlayPurpose.Control : PlayPurpose.Control,
                    Description = success ? "����ִ�гɹ�" : "����ִ��ʧ��"
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
                Debug.LogError($"����ִ��ʧ��: {e.Message}");
                Reset();
                return new StrategyExecutionResult
                {
                    Success = false,
                    SelectedCards = new List<Card>(),
                    SelectedType = CardType.Invalid,
                    Purpose = PlayPurpose.Control,
                    Description = $"����ִ��ʧ��: {e.Message}"
                };
            }
        }

        /// <summary>
        /// ��ȡ��ǰѡ�е���
        /// </summary>
        public List<Card> GetSelectedCards() => new List<Card>(selectedCards);

        /// <summary>
        /// ��ȡ��ǰ����
        /// </summary>
        public CardType GetCurrentType() => currentCardType;

        /// <summary>
        /// ����״̬
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

            // ͬ����ComputerAI
            computerAI.selectedCards = selectedCards;
            computerAI.currentType = currentCardType;
        }

        private void HandleSpecialCases(CardType cardType)
        {
            // ����ը������ը�Ļ��ַ���
            if (cardType == CardType.Bomb || cardType == CardType.JokerBomb)
            {
                Debug.Log($"���� {cardType} ���ַ���");
            }
        }

        private void LogStrategy(StrategyExecutionResult result)
        {
            if (Debug.isDebugBuild)
            {
                Debug.Log($"����ִ��: {currentStrategy?.GetType().Name}" +
                         $", ����: {result.SelectedType}" +
                         $", ����: {result.SelectedCards.Count}" +
                         $", Ŀ��: {result.Purpose}");
            }
        }
        #endregion
    }
}*/