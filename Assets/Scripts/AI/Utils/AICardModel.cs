using System.Collections.Generic;
using static CardManager;

namespace AI.Utils
{
    /// <summary>
    /// 卡牌分析结果数据类
    /// </summary>
    public class CardAnalysis
    {
        public float HandStrength { get; set; }
        public int BombCount { get; set; }
        public bool HasRocket { get; set; }
        public List<CardCombination> PotentialCombinations { get; set; }
        public Dictionary<CardType, int> CardTypeCounts { get; set; }
    }

    /// <summary>
    /// 代表一个卡牌组合
    /// </summary>
    public class CardCombination
    {
        public List<Card> Cards { get; set; }
        public CardType Type { get; set; }
        public float Value { get; set; }
        public bool IsKey = false;
    }
}
