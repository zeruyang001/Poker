using System.Collections.Generic;
using static CardManager;

namespace AI.Utils
{
    /// <summary>
    /// ���Ʒ������������
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
    /// ����һ���������
    /// </summary>
    public class CardCombination
    {
        public List<Card> Cards { get; set; }
        public CardType Type { get; set; }
        public float Value { get; set; }
        public bool IsKey = false;
    }
}
