using AI.Utils;
using static CardManager;
using System.Collections.Generic;
using UnityEngine;

public class SituationAnalysis
{
    #region ����״̬
    public GamePhase Phase { get; set; }    // ��Ϸ�׶�
    public Identity PlayerRole { get; set; } // �����ݣ�����/ũ��
    public SituationState State { get; set; }// ����״̬
    public bool IsLandlord => PlayerRole == Identity.Landlord;
    #endregion

    #region ��������
    public float HandStrength { get; set; }      // ����ǿ��(0-1)
    public int RemainingCards { get; set; }      // ʣ��������
    public int BombCount { get; set; }           // ը������
    public bool HasRocket { get; set; }          // �Ƿ�����ը
    public Dictionary<CardType, int> CardTypeCounts { get; set; } // ������������
    #endregion

    #region ��������
    public float TeamStrength { get; set; }      // ��������ʵ��
    public float OpponentStrength { get; set; }  // �Է�����ʵ��
    public bool IsInControl { get; set; }        // �Ƿ�س�
    public bool IsUnderPressure { get; set; }    // �Ƿ�ѹ��
    public int ConsecutivePassCount { get; set; }// �����������ƴ���

    // ������Ϣ
    public int OpponentMinCards { get; set; }    // ��������������
    public int PartnerRemainingCards { get; set; }// ����ʣ����������ũ��
    #endregion

    #region ���Խ���
    public StrategyType SuggestedStrategy { get; set; }
    public PlayPurpose Purpose { get; set; }
    public bool ShouldUseBomb { get; set; }
    public float RiskLevel { get; set; }         // ���յȼ�(0-1)
    public HashSet<Rank> KeyRanksToKeep { get; set; } // ��Ҫ�����Ĺؼ�����
    #endregion

    public SituationAnalysis()
    {
        CardTypeCounts = new Dictionary<CardType, int>();
        KeyRanksToKeep = new HashSet<Rank>();
        Reset();
    }

    public void Reset()
    {
        Phase = GamePhase.Opening;
        State = SituationState.Balanced;
        HandStrength = 0.5f;
        TeamStrength = 0.5f;
        OpponentStrength = 0.5f;
        RiskLevel = 0f;
        // ... ������������
    }

    // ��Ӿ�����������
    public float EvaluateOverallSituation()
    {
        float score = 0f;

        // ������������ (40%)
        score += HandStrength * 0.4f;

        // �س����� (20%)
        if (IsInControl)
            score += 0.2f;

        // ʣ���������� (20%)
        float cardCountScore = IsLandlord ?
            (20f - RemainingCards) / 20f :  // ����Խ����Խ��
            (RemainingCards - OpponentMinCards) * 0.1f; // ũ��Ҫ�͵������ֲ��
        score += cardCountScore * 0.2f;

        // �ؼ��������� (20%)
        if (HasRocket) score += 0.1f;
        score += BombCount * 0.05f;

        return Mathf.Clamp01(score);
    }
}