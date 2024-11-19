// 积分变化消息
using System;

public class MsgPointsChanged : MsgBase
{
    public MsgPointsChanged()
    {
        protoName = "MsgPointsChanged";
    }

    public int Amount { get; set; }  // 变化数量，正数为增加，负数为减少
    public int CurrentPoints { get; set; }  // 当前总积分
    public string Reason { get; set; }  // 变化原因
    public bool IsStoreSync { get; set; }  // 是否是从商城同步的积分
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}