public enum CharacterType
{
    HostPlayer,
    RightPlayer,
    LeftPlayer,
    Desk,
    Deck
}

/// <summary>
/// 牌的身份
/// </summary>
public enum Identity
{
    Farmer,
    Landlord
}

public enum PlayerStatus
{
    Call,
    Rob,
    Play,
    /// <summary>
    /// 开局阶段 (11-20张牌)
    /// </summary>
    Opening,

    /// <summary>
    /// 中局阶段 (3-10张牌)
    /// </summary>
    Middle,

    /// <summary>
    /// 残局阶段 (2张牌及以下)
    /// </summary>
    Endgame,
}

public enum GameState
{
    Idle,           // 游戏初始状态
    Preparing,      // 玩家准备中
    Matching,       // 匹配其他玩家中
    WaitingForPlayers, // 等待其他玩家加入
    Ready,          // 所有玩家都准备好，等待开始
    Dealing,        // 发牌中
    Calling,        // 叫地主阶段
    Grabbing,       // 抢地主阶段
    Playing,        // 正在游戏中
    RoundEnd,       // 单局结束
    GameOver        // 游戏结束（可能是多局游戏的最终结束）
}

public enum PlayerActionState
{
    None,
    CallLandlord,
    NotCall,
    GrabLandlord,
    NotGrab,
    Double,
}