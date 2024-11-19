// 匹配成功消息
public class MsgMatchSuccess : MsgBase
{
    public MsgMatchSuccess()
    {
        protoName = "MsgMatchSuccess";
    }
    public string[] playerIds;  // 匹配成功的玩家ID数组
    public int roomId;          // 房间ID
}
