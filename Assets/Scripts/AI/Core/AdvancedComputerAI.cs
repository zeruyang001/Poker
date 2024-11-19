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

    // 叫地主和抢地主的阈值
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
            Debug.LogError($"初始化 AdvancedComputerAI 失败: {e.Message}");
            throw;
        }
    }

    /// <summary>
    /// 初始化游戏状态（在确定地主后调用）
    /// </summary>
    public void InitializeGameState(List<Card> threeCards, Player self, Player leftPlayer, Player rightPlayer)
    {
        try
        {
            gameState.Initialize(threeCards, self, leftPlayer, rightPlayer);

            // 初始化其他依赖组件
            //decisionEngine.OnInitialized(gameState);
            //situationAnalyzer.OnInitialized(gameState);
            //strategyExecutor.OnInitialized(gameState);
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化游戏状态失败: {e.Message}");
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
            /*            // 1. 分析当前局势
                        var situation = situationAnalyzer.Analyze(gameState);
                        LogSituation(situation);

                        // 2. 确定策略
                        var strategy = decisionEngine.DetermineStrategy(situation);

                        // 3. 执行策略选牌
                        StrategyExecutionResult result = strategyExecutor.ExecuteStrategy(strategy, cards, args);

                        // 4. 如果选牌成功，更新选中的牌
                        if (result.Success)
                        {
                            selectedCards = result.SelectedCards;
                            currentType = result.SelectedType;
                            Debug.Log($"选牌成功: {result.SelectedType}, 数量: {result.SelectedCards.Count}");
                        }

                        return result.Success;*/
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"选牌错误: {e.Message}");
            return false;
        }
    }

    // 实际出牌后更新游戏状态
    public void OnCardsPlayed(CharacterType playerType, List<Card> playedCards, CardType playType, int weight)
    {
        try
        {
            gameState.OnCardsPlayed(playerType, playedCards, playType, weight);
        }
        catch (Exception e)
        {
            Debug.LogError($"更新游戏状态错误: {e.Message}");
        }
    }

    // 玩家过牌时更新状态
    public void OnPlayerPass(CharacterType playerType, CardType requiredType, int weight, int length)
    {
        gameState.OnPlayerPass(playerType, requiredType, weight, length);
    }
    #endregion

    #region 叫地主或抢地主阶段
    public override bool DecideCallOrGrab(List<Card> cards, bool isGrabbing)
    {
        try
        {
            // 计算当前手牌强度
            float handStrength = AICardAnalyzer.CalculateHandStrength(cards);

            // 根据是否是抢地主阶段选择不同的阈值
            float threshold = isGrabbing ? GRAB_STRENGTH_THRESHOLD : CALL_STRENGTH_THRESHOLD;

            // 加入随机因素，使AI行为不那么机械
            float randomVariation = UnityEngine.Random.Range(-RANDOM_FACTOR, RANDOM_FACTOR);

            // 计算决策概率
            float decisionProbability = (handStrength - threshold) / threshold + 0.5f + randomVariation;
            bool decision = UnityEngine.Random.value < Mathf.Clamp01(decisionProbability);

            Debug.Log($"AI决策 - 手牌强度: {handStrength}, 阈值: {threshold}, " +
                     $"决策概率: {decisionProbability}, 决定{(decision ? "叫/抢" : "不叫/不抢")}地主");

            return decision;
        }
        catch (Exception e)
        {
            Debug.LogError($"叫地主决策错误: {e.Message}");
            return false;
        }
    }
    #endregion

    #region Helper Methods
    private void LogSituation(SituationAnalysis situation)
    {
        if (Debug.isDebugBuild)
        {
            Debug.Log($"局势分析: 阶段={situation.Phase}, " +
                     $"手牌强度={situation.HandStrength}, " +
                     $"是否控场={situation.IsInControl}");
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