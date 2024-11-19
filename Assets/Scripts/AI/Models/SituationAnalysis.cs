using AI.Utils;
using static CardManager;
using System.Collections.Generic;
using UnityEngine;

public class SituationAnalysis
{
    #region 基础状态
    public GamePhase Phase { get; set; }    // 游戏阶段
    public Identity PlayerRole { get; set; } // 玩家身份（地主/农民）
    public SituationState State { get; set; }// 局势状态
    public bool IsLandlord => PlayerRole == Identity.Landlord;
    #endregion

    #region 牌力分析
    public float HandStrength { get; set; }      // 手牌强度(0-1)
    public int RemainingCards { get; set; }      // 剩余手牌数
    public int BombCount { get; set; }           // 炸弹数量
    public bool HasRocket { get; set; }          // 是否有王炸
    public Dictionary<CardType, int> CardTypeCounts { get; set; } // 各种牌型数量
    #endregion

    #region 局势评估
    public float TeamStrength { get; set; }      // 己方总体实力
    public float OpponentStrength { get; set; }  // 对方总体实力
    public bool IsInControl { get; set; }        // 是否控场
    public bool IsUnderPressure { get; set; }    // 是否被压制
    public int ConsecutivePassCount { get; set; }// 连续被动过牌次数

    // 对手信息
    public int OpponentMinCards { get; set; }    // 对手最少手牌数
    public int PartnerRemainingCards { get; set; }// 队友剩余手牌数（农民）
    #endregion

    #region 策略建议
    public StrategyType SuggestedStrategy { get; set; }
    public PlayPurpose Purpose { get; set; }
    public bool ShouldUseBomb { get; set; }
    public float RiskLevel { get; set; }         // 风险等级(0-1)
    public HashSet<Rank> KeyRanksToKeep { get; set; } // 需要保留的关键点数
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
        // ... 重置其他属性
    }

    // 添加局势评估方法
    public float EvaluateOverallSituation()
    {
        float score = 0f;

        // 基础牌力评分 (40%)
        score += HandStrength * 0.4f;

        // 控场评分 (20%)
        if (IsInControl)
            score += 0.2f;

        // 剩余牌数评分 (20%)
        float cardCountScore = IsLandlord ?
            (20f - RemainingCards) / 20f :  // 地主越少牌越好
            (RemainingCards - OpponentMinCards) * 0.1f; // 农民要和地主保持差距
        score += cardCountScore * 0.2f;

        // 关键牌型评分 (20%)
        if (HasRocket) score += 0.1f;
        score += BombCount * 0.05f;

        return Mathf.Clamp01(score);
    }
}