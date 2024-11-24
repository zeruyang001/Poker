public class MsgGetBasePlayerInfo : MsgBase
{
    public MsgGetBasePlayerInfo()
    {
        protoName = "MsgGetBasePlayerInfo";
    }

    // 玩家基本信息
    public string playerId;     // 玩家ID 
    public string playerName;   // 玩家名字
    public string avatarUrl;    // 头像URL
    public int point;          // 商城积分
    public int beans;          // 游戏豆
    public long lastUpdateTime; // 最后更新时间(毫秒时间戳)
}