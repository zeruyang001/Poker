/*using static CardManager;
using System.Collections.Generic;
using System.Linq;
using System;
using AI.Utils;
using UnityEngine;

public class MCTSPlayer
{
    #region ���ò���
    private const int MAX_ITERATIONS = 100;       // ����������
    private const float TIME_LIMIT = 2.0f;         // ʱ������(��)
    private const int MAX_ROLLOUT_DEPTH = 10;      // ���ģ�����
    private const float EXPLORATION_PARAM = 1.414f; // UCT̽������
    private const float URGENCY_FACTOR = 0.8f;     // �����̶�����
    #endregion

    #region ���ķ���
    public CardCombination GetBestAction(GameStateContext context)
    {
        var rootState = new MCTSGameState(context);
        var rootNode = new MCTSNode(rootState);

        float endTime = Time.realtimeSinceStartup + TIME_LIMIT;
        int iterations = 0;

        try
        {
            // ��ѭ��
            while (ShouldContinueSearch(endTime, iterations))
            {
                MCTSNode selectedNode = SelectNode(rootNode);
                float value = SimulateAndBackpropagate(selectedNode);
                iterations++;
            }

            Debug.Log($"MCTS���: {iterations}�ε���");
            return SelectFinalAction(rootNode);
        }
        catch (Exception e)
        {
            Debug.LogError($"MCTS��������: {e.Message}");
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

    #region ģ�������
    private float SimulateAndBackpropagate(MCTSNode node)
    {
        // ִ�п���ģ��
        float value = Simulate(node.State);

        // ���򴫲����
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

            // �ۻ��ۿ۽���
            float immediateValue = EvaluateStateValue(currentState);
            accumulatedValue += immediateValue * discountFactor;
            discountFactor *= 0.95f;

            depth++;
        }

        // �ϲ�����״̬��ֵ���ۻ�����
        float finalStateValue = currentState.GetScore();
        return finalStateValue * 0.7f + accumulatedValue * 0.3f;
    }

    private CardCombination SelectSimulationAction(MCTSGameState state)
    {
        var actions = state.GetPossibleActions();
        if (actions.Count == 0) return null;

        // ���Ի�ϣ�
        // 80%����ʹ������ʽѡ��
        // 20%�������ѡ��
        if (UnityEngine.Random.value < 0.8f)
        {
            return SelectHeuristicAction(actions, state);
        }

        return actions[UnityEngine.Random.Range(0, actions.Count)];
    }

    private CardCombination SelectHeuristicAction(List<CardCombination> actions, MCTSGameState state)
    {
        // ����ÿ������������ʽ��ֵ
        var actionScores = actions.Select(action =>
        {
            float score = CalculateActionHeuristicValue(action, state);
            return (action, score);
        }).ToList();

        // ʹ��softmaxѡ��
        return SelectWithSoftmax(actionScores);
    }
    #endregion

    #region ������������
    private float CalculateUCTValue(MCTSNode node, float urgency)
    {
        if (node.Visits == 0) return float.MaxValue;

        float exploitation = node.MeanValue;
        float exploration = EXPLORATION_PARAM *
            Mathf.Sqrt(Mathf.Log(node.Parent.Visits) / node.Visits);

        // ���ݽ����̶ȵ���̽����
        exploration *= (1 - urgency);

        return exploitation + exploration;
    }

    private float CalculateUrgency(MCTSGameState state)
    {
        // ����ʣ�������͵�ǰ���Ƽ�������̶�
        float cardRatio = state.HandCards.Count / 20.0f;
        float baseUrgency = 1 - cardRatio;

        // ���Ƕ�������
        var minOpponentCards = state.RemainingCards.Values.Min();
        if (minOpponentCards <= 3)
        {
            baseUrgency += 0.2f;
        }

        // ���������������
        if (state.ConsecutivePassCount >= 2)
        {
            baseUrgency += 0.15f;
        }

        return Mathf.Clamp01(baseUrgency * URGENCY_FACTOR);
    }

    private float CalculateActionHeuristicValue(CardCombination action, MCTSGameState state)
    {
        float value = action.Value;  // ����������ֵ

        // ���ݲ�ͬ���������ֵ
        if (state.IsLandlord)
        {
            // �������Ե���
            if (state.HandCards.Count <= 5)
            {
                value *= 1.3f;  // �����ʱ��߳��Ƽ�ֵ
            }
        }
        else
        {
            // ũ����Ե���
            var landlordCards = state.RemainingCards[GetLandlordPlayer(state)];
            if (landlordCards <= 3)
            {
                value *= 1.2f;  // ���������ʱ��߼�ֵ
            }
        }

        // �������Ͷ������
        if (action.Type == CardType.Bomb || action.Type == CardType.JokerBomb)
        {
            value *= (1 + state.ConsecutivePassCount * 0.2f);  // ��ѹ��ʱ���ը����ֵ
        }

        return value;
    }

    private CardCombination SelectWithSoftmax(List<(CardCombination action, float score)> actionScores)
    {
        float temperature = 0.5f;  // �¶Ȳ���������ѡ��������

        // ����softmax����
        float maxScore = actionScores.Max(x => x.score);
        var probabilities = actionScores.Select(x =>
            Mathf.Exp((x.score - maxScore) / temperature)).ToList();

        float sum = probabilities.Sum();
        probabilities = probabilities.Select(p => p / sum).ToList();

        // ������ѡ��
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

        return actionScores[0].action;  // Ĭ�Ϸ��ص�һ��
    }
    #endregion

    #region ���ն���ѡ��
    private CardCombination SelectFinalAction(MCTSNode rootNode)
    {
        // ����Ϸ�����׶Σ�ѡ����ʴ������Ķ���
        if (rootNode.State.HandCards.Count <= 4)
        {
            return rootNode.Children
                .OrderByDescending(c => c.Visits)
                .First()
                .Action;
        }

        // �����ۺϿ��Ƿ��ʴ�����ƽ����ֵ
        return rootNode.Children
            .OrderByDescending(c =>
                c.Visits * 0.7f +
                c.MeanValue * 0.3f * MAX_ITERATIONS)
            .First()
            .Action;
    }

    private CardCombination GetSafeFallbackAction(MCTSGameState state)
    {
        // �ڷ�������ʱ�İ�ȫ���˲���
        var actions = state.GetPossibleActions();
        if (actions.Count == 0) return null;

        // ����ѡ��ȫ�ĳ���ѡ��
        return actions
            .OrderByDescending(a => a.Value)
            .ThenBy(a => a.Cards.Count)
            .First();
    }
    #endregion

    #region ���Ժ���־
    private void LogSearchStatistics(MCTSNode rootNode, int iterations)
    {
        if (!Debug.isDebugBuild) return;

        var bestChild = rootNode.Children
            .OrderByDescending(c => c.Visits)
            .First();

        Debug.Log($"MCTS�������:\n" +
                 $"��������: {iterations}\n" +
                 $"��Ѷ������ʴ���: {bestChild.Visits}\n" +
                 $"��Ѷ���ƽ����ֵ: {bestChild.MeanValue:F3}\n" +
                 $"��չ���ڵ���: {CountNodes(rootNode)}");
    }

    private int CountNodes(MCTSNode node)
    {
        return 1 + node.Children.Sum(CountNodes);
    }
    #endregion

    #region ��������
    private CharacterType GetLandlordPlayer(MCTSGameState state)
    {
        // ʵ�ָ�����Ϸ״̬��ȡ������ҵ��߼�
        return CharacterType.Player; // Ĭ�Ϸ���
    }

    private float EvaluateStateValue(MCTSGameState state)
    {
        // ����ʹ��״̬����������
        return state.GetScore();
    }
    #endregion
}*/