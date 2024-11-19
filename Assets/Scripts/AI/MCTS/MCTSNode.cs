/*using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Utils;
using static CardManager;


public class MCTSNode
{
    #region ����
    public MCTSGameState State { get; private set; }
    public CardCombination Action { get; private set; }
    public MCTSNode Parent { get; private set; }
    public List<MCTSNode> Children { get; private set; }

    // MCTSͳ������
    public int Visits { get; private set; }
    public float TotalValue { get; private set; }
    public float MeanValue => Visits > 0 ? TotalValue / Visits : 0;

    // UCB���
    private const float ExplorationConstant = 1.414f;
    public bool IsFullyExpanded => UntriedActions.Count == 0;

    private List<CardCombination> UntriedActions;
    #endregion

    #region ���캯��
    public MCTSNode(MCTSGameState state, CardCombination action = null, MCTSNode parent = null)
    {
        State = state;
        Action = action;
        Parent = parent;
        Children = new List<MCTSNode>();
        UntriedActions = state.GetPossibleActions();
    }
    #endregion

    #region ���ķ���
    public MCTSNode SelectChild()
    {
        return Children.OrderByDescending(c => c.GetUCBValue()).First();
    }

    public MCTSNode Expand()
    {
        if (UntriedActions.Count == 0) return null;

        // ���ѡ��һ��δ���ԵĶ���
        var actionIndex = Random.Range(0, UntriedActions.Count);
        var action = UntriedActions[actionIndex];
        UntriedActions.RemoveAt(actionIndex);

        // ������״̬�ͽڵ�
        var nextState = State.ApplyAction(action);
        var childNode = new MCTSNode(nextState, action, this);
        Children.Add(childNode);

        return childNode;
    }

    public float Rollout()
    {
        var currentState = new MCTSGameState(State);  // ���Ƶ�ǰ״̬
        int maxDepth = 20;  // ��ֹ����ѭ��
        int depth = 0;

        while (!currentState.IsTerminal() && depth < maxDepth)
        {
            var actions = currentState.GetPossibleActions();
            if (actions.Count == 0) break;

            // ʹ������ʽѡ����
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
            value = -value;  // �Կ���Ϸ�з�ת��ֵ
        }
    }
    #endregion

    #region ��������
    private float GetUCBValue()
    {
        if (Visits == 0) return float.MaxValue;
        var exploitation = MeanValue;
        var exploration = Mathf.Sqrt(Mathf.Log(Parent.Visits) / Visits);
        return exploitation + ExplorationConstant * exploration;
    }

    private CardCombination SelectRolloutAction(List<CardCombination> actions, MCTSGameState state)
    {
        // ����ʽѡ��
        if (Random.value < 0.8f)  // 80%����ʹ������ʽ
        {
            // ����ֵ�������ѡ��ǰ��������֮һ
            var sortedActions = actions.OrderByDescending(a => a.Value).ToList();
            int selectRange = Mathf.Min(3, sortedActions.Count);
            return sortedActions[Random.Range(0, selectRange)];
        }

        // 20%������ȫ���ѡ��
        return actions[Random.Range(0, actions.Count)];
    }

    private float EvaluateRolloutResult(MCTSGameState finalState, int depth)
    {
        float score = finalState.GetScore();

        // ������ȵ�������
        float depthPenalty = depth * 0.01f;  // ���Խ�󣬷���Խ��
        score -= depthPenalty;

        // �����������������������
        if (!finalState.IsTerminal())
        {
            //score += finalState.StateValue * 0.5f;
        }

        return score;
    }

    public CardCombination GetBestAction()
    {
        // ��Ϸ����ʱֱ�ӷ�����߷��ʴ����Ķ���
        if (State.IsTerminal())
        {
            return Children.OrderByDescending(c => c.Visits).First().Action;
        }

        // �����Ƿ��ʴ�����ƽ����ֵ�ļ�Ȩ��
        return Children.OrderByDescending(c =>
            c.Visits * 0.7f + c.MeanValue * 0.3f).First().Action;
    }
    #endregion
}
*/