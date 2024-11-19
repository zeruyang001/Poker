namespace AI.Utils
{
    /// <summary>
    /// 游戏阶段
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// 开局阶段 (11-20张牌)
        /// </summary>
        Opening,

        /// <summary>
        /// 中局阶段 (5-10张牌)
        /// </summary>
        Middle,

        /// <summary>
        /// 残局阶段 (5张牌及以下)
        /// </summary>
        Endgame
    }

    public enum StrategyType
    {
        /// <summary>
        /// 进攻策略 - 优先使用大牌/炸弹,追求快速获胜
        /// </summary>
        Aggressive,

        /// <summary>
        /// 保守策略 - 优先出小牌,保留关键牌
        /// </summary>  
        Conservative,

        /// <summary>
        /// 控制策略 - 主动掌控出牌节奏,维持出牌权
        /// </summary>
        Control,

        /// <summary>
        /// 配合策略 - 与队友协同,支持队友出牌
        /// </summary>
        Cooperative
    }

    /// <summary>
    /// 手牌强度
    /// </summary>
    public enum HandStrength
    {
        /// <summary>
        /// 非常弱 - 无大牌/炸弹
        /// </summary>
        VeryWeak,

        /// <summary>
        /// 较弱 - 有少量大牌
        /// </summary>
        Weak,

        /// <summary>
        /// 一般 - 均衡牌力
        /// </summary>
        Normal,

        /// <summary>
        /// 较强 - 多张大牌/有炸弹
        /// </summary>
        Strong,

        /// <summary>
        /// 非常强 - 多个炸弹/王炸
        /// </summary>
        VeryStrong
    }

    /// <summary>
    /// 出牌目的
    /// </summary>
    public enum PlayPurpose
    {
        /// <summary>
        /// 控场 - 保持出牌权
        /// </summary>
        Control,

        /// <summary>
        /// 甩牌 - 出掉小牌
        /// </summary>
        Discard,

        /// <summary>
        /// 试探 - 试探对手牌力
        /// </summary>
        Probe,

        /// <summary>
        /// 压制 - 压制对手大牌
        /// </summary>
        Suppress,

        /// <summary>
        /// 配合 - 配合队友出牌
        /// </summary>
        Cooperate
    }

    /// <summary>
    /// 局势状态
    /// </summary>
    public enum SituationState
    {
        /// <summary>
        /// 绝对优势
        /// </summary>
        Dominant,

        /// <summary>
        /// 相对优势
        /// </summary>
        Advantageous,

        /// <summary>
        /// 均势
        /// </summary>
        Balanced,

        /// <summary>
        /// 相对劣势
        /// </summary>
        Disadvantageous,

        /// <summary>
        /// 绝对劣势
        /// </summary>
        Critical
    }

    public enum AILevel
    {
        Basic,
        Advanced
    }
}