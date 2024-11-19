/*using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Utils;
using static CardManager;


public class MCTSNode
{
    #region 属性
    public MCTSGameState State { get; private set; }
    public CardCombination Action { get; private set; }
    public MCTSNode Parent { get; private set; }
    public List<MCTSNode> Children { get; private set; }

    // MCTS统计数据
    public int Visits { get; private set; }
    public float TotalValue { get; private set; }
    public float MeanValue => Visits > 0 ? TotalValue / Visits : 0;

    // UCB相关
    private const float ExplorationConstant = 1.414f;
    public bool IsFullyExpanded => UntriedActions.Count == 0;

    private List<CardCombination> UntriedActions;
    #endregion

    #region 构造函数
    public MCTSNode(MCTSGameState state, CardCombination action = null, MCTSNode parent = null)
    {
        State = state;
        Action = action;
        Parent = parent;
        Children = new List<MCTSNode>();
        UntriedActions = state.GetPossibleActions();
    }
    #endregion

    #region 核心方法
    public MCTSNode SelectChild()
    {
        return Children.OrderByDescending(c => c.GetUCBValue()).First();
    }

    public MCTSNode Expand()
    {
        if (UntriedActions.Count == 0) return null;

        // 随机选择一个未尝试的动作
        var actionIndex = Random.Range(0, UntriedActions.Count);
        var action = UntriedActions[actionIndex];
        UntriedActions.RemoveAt(actionIndex);

        // 创建新状态和节点
        var nextState = State.ApplyAction(action);
        var childNode = new MCTSNode(nextState, action, this);
        Children.Add(childNode);

        return childNode;
    }

    public float Rollout()
    {
        var currentState = new MCTSGameState(State);  // 复制当前状态
        int maxDepth = 20;  // 防止无限循环
        int depth = 0;

        while (!currentState.IsTerminal() && depth < maxDepth)
        {
            var actions = currentState.GetPossibleActions();
            if (actions.Count == 0) break;

            // 使用启发式选择动作
            var action = SelectRolloutAction(actions, currentState);
            currentState = currentState.ApplyAction(action);
            depth++;
        }

        return EvaluateRolloutResult(currentState, depth);
    }

    public void Backpropagate(float value)
    {
        MCTSNode current = this;
        while (current != null)
        {
            current.Visits++;
            current.TotalValue += value;
            current = current.Parent;
            value = -value;  // 对抗游戏中翻转价值
        }
    }
    #endregion

    #region 辅助方法
    private float GetUCBValue()
    {
        if (Visits == 0) return float.MaxValue;
        var exploitation = MeanValue;
        var exploration = Mathf.Sqrt(Mathf.Log(Parent.Visits) / Visits);
        return exploitation + ExplorationConstant * exploration;
    }

    private CardCombination SelectRolloutAction(List<CardCombination> actions, MCTSGameState state)
    {
        // 启发式选择
        if (Random.value < 0.8f)  // 80%概率使用启发式
        {
            // 按价值排序并随机选择前几个动作之一
            var sortedActions = actions.OrderByDescending(a => a.Value).ToList();
            int selectRange = Mathf.Min(3, sortedActions.Count);
            return sortedActions[Random.Range(0, selectRange)];
        }

        // 20%概率完全随机选择
        return actions[Random.Range(0, actions.Count)];
    }

    private float EvaluateRolloutResult(MCTSGameState finalState, int depth)
    {
        float score = finalState.GetScore();

        // 根据深度调整分数
        float depthPenalty = depth * 0.01f;  // 深度越大，分数越低
        score -= depthPenalty;

        // 根据最终手牌情况调整分数
        if (!finalState.IsTerminal())
        {
            //score += finalState.StateValue * 0.5f;
        }

        return score;
    }

    public CardCombination GetBestAction()
    {
        // 游戏结束时直接返回最高访问次数的动作
        if (State.IsTerminal())
        {
            return Children.OrderByDescending(c => c.Visits).First().Action;
        }

        // 否则考虑访问次数和平均价值的加权和
        return Children.OrderByDescending(c =>
            c.Visits * 0.7f + c.MeanValue * 0.3f).First().Action;
    }
    #endregion
}
*/