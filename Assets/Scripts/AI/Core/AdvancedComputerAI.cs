using AI.Core;
//using AI.Models;
//using AI.Strategies;
using AI.Utils;
using static CardManager;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AdvancedComputerAI : ComputerAI
{
    #region Dependencies
    //private readonly DecisionEngine decisionEngine;
    //private readonly SituationAnalyzer situationAnalyzer;
    //private readonly StrategyExecutor strategyExecutor;
    private readonly GameStateContext gameState;
    #endregion

    // �е���������������ֵ
    private const float CALL_STRENGTH_THRESHOLD = 200f;
    private const float GRAB_STRENGTH_THRESHOLD = 240f;
    private const float RANDOM_FACTOR = 0.2f;

    #region Constructor & Initialization
    public AdvancedComputerAI()
    {
        try
        {
            //decisionEngine = new DecisionEngine(this);
            //situationAnalyzer = new SituationAnalyzer();
            //strategyExecutor = new StrategyExecutor(this);
            gameState = new GameStateContext();
        }
        catch (Exception e)
        {
            Debug.LogError($"��ʼ�� AdvancedComputerAI ʧ��: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// ��ʼ����Ϸ״̬����ȷ����������ã�
    /// </summary>
    public void InitializeGameState(List<Card> threeCards, Player self, Player leftPlayer, Player rightPlayer)
    {
        try
        {
            gameState.Initialize(threeCards, self, leftPlayer, rightPlayer);

            // ��ʼ�������������
            //decisionEngine.OnInitialized(gameState);
            //situationAnalyzer.OnInitialized(gameState);
            //strategyExecutor.OnInitialized(gameState);
        }
        catch (Exception e)
        {
            Debug.LogError($"��ʼ����Ϸ״̬ʧ��: {e.Message}");
            throw;
        }
    }
    #endregion

    #region Core Logic
    public override bool SmartSelectCards(List<Card> cards, ComputerSmartArgs args)
    {
        if (cards == null || args == null) return false;

        try
        {
            /*            // 1. ������ǰ����
                        var situation = situationAnalyzer.Analyze(gameState);
                        LogSituation(situation);

                        // 2. ȷ������
                        var strategy = decisionEngine.DetermineStrategy(situation);

                        // 3. ִ�в���ѡ��
                        StrategyExecutionResult result = strategyExecutor.ExecuteStrategy(strategy, cards, args);

                        // 4. ���ѡ�Ƴɹ�������ѡ�е���
                        if (result.Success)
                        {
                            selectedCards = result.SelectedCards;
                            currentType = result.SelectedType;
                            Debug.Log($"ѡ�Ƴɹ�: {result.SelectedType}, ����: {result.SelectedCards.Count}");
                        }

                        return result.Success;*/
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"ѡ�ƴ���: {e.Message}");
            return false;
        }
    }

    // ʵ�ʳ��ƺ������Ϸ״̬
    public void OnCardsPlayed(CharacterType playerType, List<Card> playedCards, CardType playType, int weight)
    {
        try
        {
            gameState.OnCardsPlayed(playerType, playedCards, playType, weight);
        }
        catch (Exception e)
        {
            Debug.LogError($"������Ϸ״̬����: {e.Message}");
        }
    }

    // ��ҹ���ʱ����״̬
    public void OnPlayerPass(CharacterType playerType, CardType requiredType, int weight, int length)
    {
        gameState.OnPlayerPass(playerType, requiredType, weight, length);
    }
    #endregion

    #region �е������������׶�
    public override bool DecideCallOrGrab(List<Card> cards, bool isGrabbing)
    {
        try
        {
            // ���㵱ǰ����ǿ��
            float handStrength = AICardAnalyzer.CalculateHandStrength(cards);

            // �����Ƿ����������׶�ѡ��ͬ����ֵ
            float threshold = isGrabbing ? GRAB_STRENGTH_THRESHOLD : CALL_STRENGTH_THRESHOLD;

            // ����������أ�ʹAI��Ϊ����ô��е
            float randomVariation = UnityEngine.Random.Range(-RANDOM_FACTOR, RANDOM_FACTOR);

            // ������߸���
            float decisionProbability = (handStrength - threshold) / threshold + 0.5f + randomVariation;
            bool decision = UnityEngine.Random.value < Mathf.Clamp01(decisionProbability);

            Debug.Log($"AI���� - ����ǿ��: {handStrength}, ��ֵ: {threshold}, " +
                     $"���߸���: {decisionProbability}, ����{(decision ? "��/��" : "����/����")}����");

            return decision;
        }
        catch (Exception e)
        {
            Debug.LogError($"�е������ߴ���: {e.Message}");
            return false;
        }
    }
    #endregion

    #region Helper Methods
    private void LogSituation(SituationAnalysis situation)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log($"���Ʒ���: �׶�={situation.Phase}, " +
                     $"����ǿ��={situation.HandStrength}, " +
                     $"�Ƿ�س�={situation.IsInControl}");
        }
    }
    #endregion

    #region Cleanup
    public override void Reset()
    {
        base.Reset();
        gameState.Reset();
    }

    private void OnDestroy()
    {
        gameState?.Dispose();
    }
    #endregion
}