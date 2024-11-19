using System.Collections.Generic;
using static CardManager;

/// <summary>
/// 出牌决策参数类
/// </summary>
public class ComputerSmartArgs
{
    #region 出牌信息
    /// <summary>
    /// 出牌类型 出牌权重 出牌长度 出牌角色
    /// </summary>
    public PlayCardArgs PlayCardArgs { get; set; }
    #endregion

    #region 角色信息
    /// <summary>
    /// 当前最大牌的角色
    /// </summary>
    public CharacterType BiggestCharacter { get; set; }

    /// <summary>
    /// 角色剩余手牌数量
    /// </summary>
    public int RemainingCards { get; set; }

    /// <summary>
    /// 角色出的牌
    /// </summary>
    public List<Card> PlayCards { get; set; }
    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    public ComputerSmartArgs()
    {
        Reset();
    }

    public void Reset()
    {
        PlayCardArgs = new PlayCardArgs
        {
            CardType = default(CardType),
            CharacterType = default(CharacterType),
            Length = 0,
            Weight = 0
        };

        BiggestCharacter = CharacterType.Desk;
        RemainingCards = 0;
        PlayCards = new List<Card>();  // 初始化一个空的牌列表
    }

    public ComputerSmartArgs Clone()
    {
        return new ComputerSmartArgs
        {
            PlayCardArgs = new PlayCardArgs
            {
                CardType = this.PlayCardArgs.CardType,
                CharacterType = this.PlayCardArgs.CharacterType,
                Length = this.PlayCardArgs.Length,
                Weight = this.PlayCardArgs.Weight
            },
            BiggestCharacter = this.BiggestCharacter,
            RemainingCards = this.RemainingCards,
            PlayCards = new List<Card>(this.PlayCards) // 深拷贝手牌列表
        };
    }
}

