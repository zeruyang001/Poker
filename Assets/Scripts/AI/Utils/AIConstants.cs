using AI.Core;

namespace AI.Utils
{
    /// <summary>
    /// AI策略相关常量
    /// </summary>
    public static class AIConstants
    {
        #region 策略阈值
        /// <summary>
        /// 进攻策略阈值
        /// </summary>
        public const float AGGRESSIVE_THRESHOLD = 0.7f;

        /// <summary>
        /// 保守策略阈值
        /// </summary>
        public const float CONSERVATIVE_THRESHOLD = 0.3f;

        /// <summary>
        /// 炸弹使用阈值
        /// </summary>
        public const float BOMB_THRESHOLD = 0.6f;

        /// <summary>
        /// 王炸使用阈值
        /// </summary>
        public const float ROCKET_THRESHOLD = 0.8f;

        public const float GRAB_THRESHOLD = 0.8f;
        public const float CALL_THRESHOLD = 0.55f;
        #endregion

        #region 牌值权重
        /// <summary>
        /// 普通牌权重
        /// </summary>
        public const float NORMAL_CARD_WEIGHT = 1.0f;

        /// <summary>
        /// 大牌权重(A及以上)
        /// </summary>
        public const float BIG_CARD_WEIGHT = 1.5f;

        /// <summary>
        /// 炸弹权重
        /// </summary>
        public const float BOMB_WEIGHT = 2.0f;

        /// <summary>
        /// 王炸权重
        /// </summary>
        public const float ROCKET_WEIGHT = 3.0f;
        #endregion

        #region 牌型加权
        /// <summary>
        /// 顺子加权
        /// </summary>
        public const float STRAIGHT_BONUS = 0.15f;

        /// <summary>
        /// 连对加权
        /// </summary>
        public const float PAIR_STRAIGHT_BONUS = 0.2f;

        /// <summary>
        /// 飞机加权
        /// </summary>
        public const float PLANE_BONUS = 0.25f;
        #endregion

        #region 游戏阶段判定
        /// <summary>
        /// 中局牌数阈值
        /// </summary>
        public const int MIDDLE_GAME_THRESHOLD = 10;

        /// <summary>
        /// 残局牌数阈值
        /// </summary>
        public const int END_GAME_THRESHOLD = 5;
        #endregion

        #region 局势评估
        /// <summary>
        /// 绝对优势阈值
        /// </summary>
        public const float DOMINANT_THRESHOLD = 0.8f;

        /// <summary>
        /// 相对优势阈值
        /// </summary>
        public const float ADVANTAGE_THRESHOLD = 0.6f;

        /// <summary>
        /// 劣势阈值
        /// </summary>
        public const float DISADVANTAGE_THRESHOLD = 0.4f;

        /// <summary>
        /// 绝对劣势阈值
        /// </summary>
        public const float CRITICAL_THRESHOLD = 0.2f;
        #endregion

        #region Helper Methods
        public static ComputerAI CreateAI(AILevel level)
        {
            switch (level)
            {
                case AILevel.Advanced:
                    return new AdvancedComputerAI();
                case AILevel.Basic:
                default:
                    return new ComputerAI();
            }
        }
        /// <summary>
        /// 获取手牌强度等级
        /// </summary>
        public static HandStrength GetHandStrength(float strengthValue)
        {
            if (strengthValue >= DOMINANT_THRESHOLD) return HandStrength.VeryStrong;
            if (strengthValue >= ADVANTAGE_THRESHOLD) return HandStrength.Strong;
            if (strengthValue >= CONSERVATIVE_THRESHOLD) return HandStrength.Normal;
            if (strengthValue >= CRITICAL_THRESHOLD) return HandStrength.Weak;
            return HandStrength.VeryWeak;
        }

        /// <summary>
        /// 获取局势状态
        /// </summary>
        public static SituationState GetSituationState(float situationValue)
        {
            if (situationValue >= DOMINANT_THRESHOLD) return SituationState.Dominant;
            if (situationValue >= ADVANTAGE_THRESHOLD) return SituationState.Advantageous;
            if (situationValue >= DISADVANTAGE_THRESHOLD) return SituationState.Balanced;
            if (situationValue >= CRITICAL_THRESHOLD) return SituationState.Disadvantageous;
            return SituationState.Critical;
        }

        /// <summary>
        /// 获取游戏阶段
        /// </summary>
        public static GamePhase GetGamePhase(int cardCount)
        {
            if (cardCount > MIDDLE_GAME_THRESHOLD) return GamePhase.Opening;
            if (cardCount > END_GAME_THRESHOLD) return GamePhase.Middle;
            return GamePhase.Endgame;
        }

        /// <summary>
        /// 获取卡牌权重
        /// </summary>
        public static float GetCardWeight(Card card)
        {
            if (card.rank == Rank.SJoker || card.rank == Rank.LJoker)
                return ROCKET_WEIGHT;
            if ((int)card.rank >= (int)Rank.Two)
                return BIG_CARD_WEIGHT;
            return NORMAL_CARD_WEIGHT;
        }
        #endregion
    }
}
