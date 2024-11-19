// 任务完成消息
using System;
using System.Collections.Generic;
public class MsgMissionComplete : MsgBase
{
    public MsgMissionComplete()
    {
        protoName = "MsgMissionComplete";
    }

    public string MissionId { get; set; }  // 任务ID
    public string MissionName { get; set; }  // 任务名称
    public int RewardBeans { get; set; }  // 奖励豆子数量
    public int RewardPoints { get; set; }  // 奖励积分数量
    public Dictionary<string, int> ExtraRewards { get; set; }  // 额外奖励
    public bool IsDailyMission { get; set; }  // 是否是每日任务
    public long CompleteTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}