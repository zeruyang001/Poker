// 新增的玩家数据更新消息类 
public class MsgUpdatePlayerData : MsgBase 
{
    public MsgUpdatePlayerData()
    {
        protoName = "MsgUpdatePlayerData";
    }

    // 发送给服务器的数据
    public string playerId;        // 玩家ID
    public int point;             // 商城积分
    public int beans;             // 当前游戏豆
    public long lastUpdateTime;    // 客户端的最后更新时间

    // 服务器返回的数据
    public bool success;          // 更新是否成功
    public bool hasConflict;      // 是否存在数据冲突
    public string errorMsg;       // 错误信息
}