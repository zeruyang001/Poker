// 匹配消息
public class MsgMatch : MsgBase
{
    public MsgMatch()
    {
        protoName = "MsgMatch";
    }
    public bool result;  // true: 开始匹配, false: 取消匹配
}
