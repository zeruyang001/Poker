public class MsgMatchGame : MsgBase
{
    public MsgMatchGame()
    {
        protoName = "MsgMatchGame";
    }
    // 客户端发送
    public string playerId;  // 玩家ID
    public int rankScore;    // 玩家分数,用于匹配
    
    // 服务器响应
    public bool result;      // 匹配结果
    public string roomId;    // 匹配成功后的房间ID
    public string[] playerList;  // 匹配成功的玩家列表(3个玩家)
    public string errorMsg;  // 错误信息
}
