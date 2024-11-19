/*using AI.Models;
using AI.Utils;
using System;

public class SituationAnalyzer
{
    #region ���ķ�������
    public SituationAnalysis Analyze(GameContext context)
    {
        var analysis = new SituationAnalysis();

        try
        {
            // 1. �����������
            AnalyzeBasicSituation(analysis, context);

            // 2. ��������ǿ��
            AnalyzeHandStrength(analysis, context);

            // 3. ��������
            AnalyzeGameSituation(analysis, context);

            // 4. ȷ������
            DetermineStrategy(analysis, context);

            // 5. ��������
            EvaluateRisks(analysis, context);

            return analysis;
        }
        catch (Exception e)
        {
            Debug.LogError($"���Ʒ�������: {e.Message}");
            return analysis;
        }
    }

    private void AnalyzeBasicSituation(SituationAnalysis analysis, GameContext context)
    {
        // ȷ����Ϸ�׶�
        analysis.Phase = DetermineGamePhase(context);

        // ��¼�����ݺͽ�ɫ
        analysis.PlayerRole = context.SelfIdentity;

        // ���������������
        CountRemainingCards(analysis, context);
    }

    private void AnalyzeHandStrength(SituationAnalysis analysis, GameContext context)
    {
        var handCards = context.GetPlayerCards(context.Self.characterType);

        // ͳ����������
        analysis.BombCount = CountBombs(handCards);
        analysis.HasRocket = HasRocket(handCards);

        // �������ֿ��ܵ��������
        AnalyzeCardCombinations(analysis, handCards);

        // ������������ǿ��
        analysis.HandStrength = CalculateHandStrength(handCards, analysis);
    }

    private void AnalyzeGameSituation(SituationAnalysis analysis, GameContext context)
    {
        // �����س�״̬
        analysis.IsInControl = IsPlayerInControl(context);

        // �������ʵ��
        CalculateTeamStrengths(analysis, context);

        // ����ѹ��״̬
        AnalyzePressureSituation(analysis, context);

        // ���¾���״̬
        UpdateSituationState(analysis);
    }
    #endregion

    #region ������������
    private GamePhase DetermineGamePhase(GameContext context)
    {
        var cardCount = context.GetRemainingCardCount(context.Self.characterType);

        if (cardCount > 16)
            return GamePhase.Opening;
        else if (cardCount > 8)
            return GamePhase.Middle;
        else
            return GamePhase.Endgame;
    }

    private void CalculateTeamStrengths(SituationAnalysis analysis, GameContext context)
    {
        if (analysis.IsLandlord)
        {
            // ������ʵ������
            analysis.TeamStrength = CalculateLandlordStrength(context);
            analysis.OpponentStrength = CalculateFarmersStrength(context);
        }
        else
        {
            // ũ��ʵ������
            analysis.TeamStrength = CalculateFarmersStrength(context);
            analysis.OpponentStrength = CalculateLandlordStrength(context);
        }
    }
    #endregion
}*/