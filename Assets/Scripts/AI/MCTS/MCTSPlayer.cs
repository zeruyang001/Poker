/*using static CardManager;
using System.Collections.Generic;
using System.Linq;
using System;
using AI.Utils;
using UnityEngine;

public class MCTSPlayer
{
    #region 配置参数
    private const int MAX_ITERATIONS = 100;       // 最大迭代次数
    private const float TIME_LIMIT = 2.0f;         // 时间限制(秒)
    private const int MAX_ROLLOUT_DEPTH = 10;      // 最大模拟深度
    private const float EXPLORATION_PARAM = 1.414f; // UCT探索参数
    private const float URGENCY_FACTOR = 0.8f;     // 紧急程度因子
    #endregion

    #region 核心方法
    public CardCombination GetBestAction(GameStateContext context)
    {
        var rootState = new MCTSGameState(context);
        var rootNode = new MCTSNode(rootState);

        float endTime = Time.realtimeSinceStartup + TIME_LIMIT;
        int iterations = 0;

        try
        {
            // 主循环
            while (ShouldContinueSearch(endTime, iterations))
            {
                MCTSNode selectedNode = SelectNode(rootNode);
                float value = SimulateAndBackpropagate(selectedNode);
                iterations++;
            }

            Debug.Log($"MCTS完成: {iterations}次迭代");
            return SelectFinalAction(rootNode);
        }
        catch (Exception e)
        {
            Debug.LogError($"MCTS搜索错误: {e.Message}");
            return GetSafeFallbackAction(rootState);
        }
    }

    private bool ShouldContinueSearch(float endTime, int iterations)
    {
        if (iterations >= MAX_ITERATIONS) return false;
        if (Time.realtimeSinceStartup >= endTime) return false;
        return true;
    }

    private MCTSNode SelectNode(MCTSNode node)
    {
        while (!node.State.IsTerminal())
        {
            if (!node.IsFullyExpanded)
            {
                return node.Expand();
            }
            node = SelectBestChild(node);
        }
        return node;
    }

    private MCTSNode SelectBestChild(MCTSNode node)
    {
        float urgency = CalculateUrgency(node.State);
        return node.Children.OrderByDescending(c =>
            CalculateUCTValue(c, urgency)).First();
    }
    #endregion

    #region 模拟和评估
    private float SimulateAndBackpropagate(MCTSNode node)
    {
        // 执行快速模拟
        float value = Simulate(node.State);

        // 反向传播结果
        node.Backpropagate(value);

        return value;
    }

    private float Simulate(MCTSGameState state)
    {
        var currentState = new MCTSGameState(state);
        int depth = 0;
        float accumulatedValue = 0;
        float discountFactor = 1.0f;

        while (!currentState.IsTerminal() && depth < MAX_ROLLOUT_DEPTH)
        {
            var action = SelectSimulationAction(currentState);
            if (action == null) break;

            currentState = currentState.ApplyAction(action);

            // 累积折扣奖励
            float immediateValue = EvaluateStateValue(currentState);
            accumulatedValue += immediateValue * discountFactor;
            discountFactor *= 0.95f;

            depth++;
        }

        // 合并最终状态价值和累积奖励
        float finalStateValue = currentState.GetScore();
        return finalStateValue * 0.7f + accumulatedValue * 0.3f;
    }

    private CardCombination SelectSimulationAction(MCTSGameState state)
    {
        var actions = state.GetPossibleActions();
        if (actions.Count == 0) return null;

        // 策略混合：
        // 80%概率使用启发式选择
        // 20%概率随机选择
        if (UnityEngine.Random.value < 0.8f)
        {
            return SelectHeuristicAction(actions, state);
        }

        return actions[UnityEngine.Random.Range(0, actions.Count)];
    }

    private CardCombination SelectHeuristicAction(List<CardCombination> actions, MCTSGameState state)
    {
        // 计算每个动作的启发式价值
        var actionScores = actions.Select(action =>
        {
            float score = CalculateActionHeuristicValue(action, state);
            return (action, score);
        }).ToList();

        // 使用softmax选择
        return SelectWithSoftmax(actionScores);
    }
    #endregion

    #region 评估辅助方法
    private float CalculateUCTValue(MCTSNode node, float urgency)
    {
        if (node.Visits == 0) return float.MaxValue;

        float exploitation = node.MeanValue;
        float exploration = EXPLORATION_PARAM *
            Mathf.Sqrt(Mathf.Log(node.Parent.Visits) / node.Visits);

        // 根据紧急程度调整探索项
        exploration *= (1 - urgency);

        return exploitation + exploration;
    }

    private float CalculateUrgency(MCTSGameState state)
    {
        // 基于剩余牌数和当前局势计算紧急程度
        float cardRatio = state.HandCards.Count / 20.0f;
        float baseUrgency = 1 - cardRatio;

        // 考虑对手牌数
        var minOpponentCards = state.RemainingCards.Values.Min();
        if (minOpponentCards <= 3)
        {
            baseUrgency += 0.2f;
        }

        // 考虑连续被动情况
        if (state.ConsecutivePassCount >= 2)
        {
            baseUrgency += 0.15f;
        }

        return Mathf.Clamp01(baseUrgency * URGENCY_FACTOR);
    }

    private float CalculateActionHeuristicValue(CardCombination action, MCTSGameState state)
    {
        float value = action.Value;  // 基础动作价值

        // 根据不同情况调整价值
        if (state.IsLandlord)
        {
            // 地主策略调整
            if (state.HandCards.Count <= 5)
            {
                value *= 1.3f;  // 快出完时提高出牌价值
            }
        }
        else
        {
            // 农民策略调整
            var landlordCards = state.RemainingCards[GetLandlordPlayer(state)];
            if (landlordCards <= 3)
            {
                value *= 1.2f;  // 地主快出完时提高价值
            }
        }

        // 特殊牌型额外调整
        if (action.Type == CardType.Bomb || action.Type == CardType.JokerBomb)
        {
            value *= (1 + state.ConsecutivePassCount * 0.2f);  // 被压制时提高炸弹价值
        }

        return value;
    }

    private CardCombination SelectWithSoftmax(List<(CardCombination action, float score)> actionScores)
    {
        float temperature = 0.5f;  // 温度参数，控制选择的随机性

        // 计算softmax概率
        float maxScore = actionScores.Max(x => x.score);
        var probabilities = actionScores.Select(x =>
            Mathf.Exp((x.score - maxScore) / temperature)).ToList();

        float sum = probabilities.Sum();
        probabilities = probabilities.Select(p => p / sum).ToList();

        // 按概率选择
        float random = UnityEngine.Random.value;
        float cumulative = 0;
        for (int i = 0; i < actionScores.Count; i++)
        {
            cumulative += probabilities[i];
            if (random <= cumulative)
            {
                return actionScores[i].action;
            }
        }

        return actionScores[0].action;  // 默认返回第一个
    }
    #endregion

    #region 最终动作选择
    private CardCombination SelectFinalAction(MCTSNode rootNode)
    {
        // 在游戏结束阶段，选择访问次数最多的动作
        if (rootNode.State.HandCards.Count <= 4)
        {
            return rootNode.Children
                .OrderByDescending(c => c.Visits)
                .First()
                .Action;
        }

        // 否则综合考虑访问次数和平均价值
        return rootNode.Children
            .OrderByDescending(c =>
                c.Visits * 0.7f +
                c.MeanValue * 0.3f * MAX_ITERATIONS)
            .First()
            .Action;
    }

    private CardCombination GetSafeFallbackAction(MCTSGameState state)
    {
        // 在发生错误时的安全回退策略
        var actions = state.GetPossibleActions();
        if (actions.Count == 0) return null;

        // 优先选择安全的出牌选项
        return actions
            .OrderByDescending(a => a.Value)
            .ThenBy(a => a.Cards.Count)
            .First();
    }
    #endregion

    #region 调试和日志
    private void LogSearchStatistics(MCTSNode rootNode, int iterations)
    {
        if (!Debug.isDebugBuild) return;

        var bestChild = rootNode.Children
            .OrderByDescending(c => c.Visits)
            .First();

        Debug.Log($"MCTS搜索完成:\n" +
                 $"迭代次数: {iterations}\n" +
                 $"最佳动作访问次数: {bestChild.Visits}\n" +
                 $"最佳动作平均价值: {bestChild.MeanValue:F3}\n" +
                 $"总展开节点数: {CountNodes(rootNode)}");
    }

    private int CountNodes(MCTSNode node)
    {
        return 1 + node.Children.Sum(CountNodes);
    }
    #endregion

    #region 辅助方法
    private CharacterType GetLandlordPlayer(MCTSGameState state)
    {
        // 实现根据游戏状态获取地主玩家的逻辑
        return CharacterType.Player; // 默认返回
    }

    private float EvaluateStateValue(MCTSGameState state)
    {
        // 可以使用状态的内置评估
        return state.GetScore();
    }
    #endregion
}*/