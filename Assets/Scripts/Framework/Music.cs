using UnityEngine;
using static CardManager;

public static class Music
{
    private const string BG_PATH = "BG/";
    private const string CARDS_PATH = "Cards/";
    private const string DIALOGUE_PATH = "Dialogue/";
    private const string EFFECT_PATH = "Effect/";

    // 背景音乐
    public static string Welcome = BG_PATH + "Welcome";
    public static readonly string[] BG = { BG_PATH + "Normal", BG_PATH + "Normal2", BG_PATH + "Normal3", BG_PATH + "Exciting", BG_PATH + "Welcome", BG_PATH + "BACK_MUSIC1", BG_PATH + "BACK_MUSIC2" };

    //游戏音效
    public static string Alart = EFFECT_PATH + "Special_alert";
    public static string Bomb = EFFECT_PATH + "Special_Bomb";
    public static string Multiply = EFFECT_PATH + "Special_Multiply";
    public static string Plane = EFFECT_PATH + "Special_plane";
    public static string Remind = EFFECT_PATH + "Special_Remind";
    public static string Star = EFFECT_PATH + "Special_star";
    public static string Win = EFFECT_PATH + "Win";
    public static string Lose = EFFECT_PATH + "Lose";


    //按钮音效
    public static string Enter = EFFECT_PATH + "Enter";
    public static string Gain = EFFECT_PATH + "Gain";
    public static string Peep = EFFECT_PATH + "Peep";
    public static string Switch = EFFECT_PATH + "Switch";
    public static string Ok = EFFECT_PATH + "SpecOk";
    public static string CancelOrReturn = EFFECT_PATH + "SpecCancelOrReturn";
    public static string Disable = EFFECT_PATH + "SpecDisable";
    public static string Dispatch = EFFECT_PATH + "Special_Dispatch";





    // 斗地主按钮音效
    public static string[] Order = { CARDS_PATH + "Man_Order", CARDS_PATH + "Man_1_Order" };
    public static string[] NoOrder = { CARDS_PATH + "Man_NoOrder", CARDS_PATH + "Man_1_NoOrder"};
    public static string DisGrab = CARDS_PATH + "Man_NoRob";
    public static readonly string[] Grab = { CARDS_PATH + "Man_1_Rob", CARDS_PATH + "Man_Rob1", CARDS_PATH + "Man_Rob2",CARDS_PATH + "Man_Rob3" };
    public static readonly string[] PassCard = { CARDS_PATH + "Man_1_buyao1", CARDS_PATH + "Man_1_buyao2", CARDS_PATH + "Man_1_buyao3", CARDS_PATH + "Man_buyao2"  };
    public static readonly string[] PlayCard = { CARDS_PATH + "Man_1_dani1", CARDS_PATH + "Man_1_dani2", CARDS_PATH + "Man_1_dani3" };

    // 卡牌音效
    public static string DealCard = EFFECT_PATH + "Givecard";
    public static string Select = EFFECT_PATH + "SpecSelectCard";


    // 单张牌音效
    public static readonly string[] A = { CARDS_PATH + "Man_1_A", CARDS_PATH + "Man_A" };
    public static readonly string[] Two = { CARDS_PATH + "Man_1_2", CARDS_PATH + "Man_2" };
    public static readonly string[] Three = { CARDS_PATH + "Man_1_3", CARDS_PATH + "Man_3" };
    public static readonly string[] Four = { CARDS_PATH + "Man_1_4", CARDS_PATH + "Man_4" };
    public static readonly string[] Five = { CARDS_PATH + "Man_1_5", CARDS_PATH + "Man_5" };
    public static readonly string[] Six = { CARDS_PATH + "Man_1_6", CARDS_PATH + "Man_6" };
    public static readonly string[] Seven = { CARDS_PATH + "Man_1_7", CARDS_PATH + "Man_7" };
    public static readonly string[] Eight = { CARDS_PATH + "Man_1_8", CARDS_PATH + "Man_8" };
    public static readonly string[] Nine = { CARDS_PATH + "Man_1_9", CARDS_PATH + "Man_9" };
    public static readonly string[] Ten = { CARDS_PATH + "Man_1_10", CARDS_PATH + "Man_10" };
    public static readonly string[] J = { CARDS_PATH + "Man_1_J", CARDS_PATH + "Man_11" };
    public static readonly string[] Q = { CARDS_PATH + "Man_1_Q", CARDS_PATH + "Man_12" };
    public static readonly string[] K = { CARDS_PATH + "Man_1_K", CARDS_PATH + "Man_13" };
    public static readonly string[] SJoker = { CARDS_PATH + "Man_1_SJoker", CARDS_PATH + "Man_14" };
    public static readonly string[] LJoker = { CARDS_PATH + "Man_1_LJoker", CARDS_PATH + "Man_15" };

    public static readonly string[][] Single = {
    Three, Four, Five, Six, Seven, Eight, Nine, Ten, J, Q, K, A, Two, SJoker, LJoker
    };

    // 对子音效
    public static readonly string[] Pair_A = { CARDS_PATH + "Man_1_dui1", CARDS_PATH + "Man_dui1" };
    public static readonly string[] Pair_Two = { CARDS_PATH + "Man_1_dui2", CARDS_PATH + "Man_dui2" };
    public static readonly string[] Pair_Three = { CARDS_PATH + "Man_1_dui3", CARDS_PATH + "Man_dui3" };
    public static readonly string[] Pair_Four = { CARDS_PATH + "Man_1_dui4", CARDS_PATH + "Man_dui4" };
    public static readonly string[] Pair_Five = { CARDS_PATH + "Man_1_dui5", CARDS_PATH + "Man_dui5" };
    public static readonly string[] Pair_Six = { CARDS_PATH + "Man_1_dui6", CARDS_PATH + "Man_dui6" };
    public static readonly string[] Pair_Seven = { CARDS_PATH + "Man_1_dui7", CARDS_PATH + "Man_dui7" };
    public static readonly string[] Pair_Eight = { CARDS_PATH + "Man_1_dui8", CARDS_PATH + "Man_dui8" };
    public static readonly string[] Pair_Nine = { CARDS_PATH + "Man_1_dui9", CARDS_PATH + "Man_dui9" };
    public static readonly string[] Pair_Ten = { CARDS_PATH + "Man_1_dui10", CARDS_PATH + "Man_dui10" };
    public static readonly string[] Pair_J = { CARDS_PATH + "Man_1_dui11", CARDS_PATH + "Man_dui11" };
    public static readonly string[] Pair_Q = { CARDS_PATH + "Man_1_dui12", CARDS_PATH + "Man_dui12" };
    public static readonly string[] Pair_K = { CARDS_PATH + "Man_1_dui13", CARDS_PATH + "Man_dui13" };

    public static readonly string[][] Pair = {
    Pair_Three, Pair_Four, Pair_Five, Pair_Six, Pair_Seven, Pair_Eight, Pair_Nine, Pair_Ten, Pair_J, Pair_Q, Pair_K, Pair_A, Pair_Two
    };

    // 三张音效
    public static readonly string[] Triple = {
        CARDS_PATH + "Man_triple3", CARDS_PATH + "Man_triple4", CARDS_PATH + "Man_triple5", CARDS_PATH + "Man_triple6",
        CARDS_PATH + "Man_triple7", CARDS_PATH + "Man_triple8", CARDS_PATH + "Man_triple9", CARDS_PATH + "Man_triple10",
        CARDS_PATH + "Man_tripleJ", CARDS_PATH + "Man_tripleQ", CARDS_PATH + "Man_tripleK", CARDS_PATH + "Man_tripleA",
        CARDS_PATH + "Man_triple2"
    };

    // 修改现有的和新增的卡牌类型音效数组
    public static readonly string[] ThreeWithOne = { CARDS_PATH + "Man_1_sandaiyi", CARDS_PATH + "Man_sandaiyi" };
    public static readonly string[] ThreeWithPair = { CARDS_PATH + "Man_1_sandaiyidui", CARDS_PATH + "Man_sandaiyidui" };
    public static readonly string[] Straight = { CARDS_PATH + "Man_1_shunzi", CARDS_PATH + "Man_shunzi" };
    public static readonly string[] PairStraight = { CARDS_PATH + "Man_1_liandui", CARDS_PATH + "Man_liandui" };
    public static readonly string[] TripleStraight = { CARDS_PATH + "Man_1_feiji", CARDS_PATH + "Man_feiji" };
    public static readonly string[] TripleStraightWithSingle = { CARDS_PATH + "Man_1_feiji", CARDS_PATH + "Man_feiji" };
    public static readonly string[] TripleStraightWithPair = { CARDS_PATH + "Man_1_feiji", CARDS_PATH + "Man_feiji" };
    public static readonly string[] FourBomb = { CARDS_PATH + "Man_1_zhadan", CARDS_PATH + "Man_zhadan" };
    public static readonly string[] JokerBomb = { CARDS_PATH + "Man_1_wangzha", CARDS_PATH + "Man_wangzha" };
    public static readonly string[] FourWithTwo = { CARDS_PATH + "Man_sidaier" };
    public static readonly string[] FourWithTwoPair = { CARDS_PATH + "Man_sidaierliangdui" };

    public static string GetCardTypeSound(CardType cardType, int Weight)
    {
        switch (cardType)
        {
            case CardType.Single:
                return RandomSound(Single[Weight]);
            case CardType.Pair:
                return RandomSound(Pair[Weight]);
            case CardType.Three:
                return Triple[Weight];
            case CardType.ThreeWithOne:
                return RandomSound(ThreeWithOne);
            case CardType.ThreeWithPair:
                return RandomSound(ThreeWithPair);
            case CardType.Straight:
                return RandomSound(Straight);
            case CardType.PairStraight:
                return RandomSound(PairStraight);
            case CardType.TripleStraight:
                return RandomSound(TripleStraight);
            case CardType.TripleStraightWithSingle:
                return RandomSound(TripleStraightWithSingle);
            case CardType.TripleStraightWithPair:
                return RandomSound(TripleStraightWithPair);
            case CardType.Bomb:
                return RandomSound(FourBomb);
            case CardType.JokerBomb:
                return RandomSound(JokerBomb);
            case CardType.FourWithTwo:
                return RandomSound(FourWithTwo);
            case CardType.FourWithTwoPair:
                return RandomSound(FourWithTwoPair);
            default:
                return RandomSound(PlayCard);
        }
    }

    // 修改 RandomSound 方法以接受单个字符串或字符串数组
    public static string RandomSound(string[] names)
    {
        if (names == null || names.Length == 0)
        {
            Debug.LogWarning("No sound names provided.");
            return string.Empty;
        }
        return names[UnityEngine.Random.Range(0, names.Length)];
    }
}