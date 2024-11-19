// 成就解锁消息
using System;
using System.Collections.Generic;

public class MsgAchievementUnlocked : MsgBase
{
    public MsgAchievementUnlocked()
    {
        protoName = "MsgAchievementUnlocked";
    }

    public string AchievementId { get; set; }  // 成就ID
    public string AchievementName { get; set; }  // 成就名称
    public string Description { get; set; }  // 成就描述
    public string IconUrl { get; set; }  // 成就图标URL
    public Dictionary<string, int> Rewards { get; set; }  // 奖励内容
    public int Rarity { get; set; }  // 成就稀有度（百分比）
    public long UnlockTime { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}