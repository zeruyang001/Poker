using System;
// 豆子变化消息
public class MsgBeansChanged : MsgBase
{
    public MsgBeansChanged()
    {
        protoName = "MsgBeansChanged";
    }

    public int Amount { get; set; }  // 变化数量，正数为增加，负数为减少
    public int CurrentBeans { get; set; }  // 当前总豆子数
    public string Reason { get; set; }  // 变化原因
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
