using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AI.Utils;
using static AI.Utils.AICardFinder;
using static AI.Utils.AICardAnalyzer;
using static CardManager;

/// <summary>
/// 电脑出牌
/// </summary>
public class ComputerAI:MonoBehaviour
{
    /// <summary>
    /// 当前选中的牌
    /// </summary>
    public List<Card> selectedCards;
    public CardType currentType { get; set; } = CardType.Invalid;

    private void Start()
    {
        selectedCards = new List<Card>();
        currentType = CardType.Invalid;
    }

    #region Public Methods
    /// <summary>
    /// 智能选牌（新版）
    /// </summary>
    public virtual bool SmartSelectCards(List<Card> cards, ComputerSmartArgs args)
    {
        return SmartSelectCards(cards, args.PlayCardArgs.CardType, args.PlayCardArgs.Weight, args.PlayCardArgs.Length,
                              args.BiggestCharacter == args.PlayCardArgs.CharacterType);
    }
    public bool SmartSelectCards(List<Card> cards, CardType cardType, int rank, int length, bool isBiggest)
    {
        Sort(cards); // 升序排列
        cardType = isBiggest ? CardType.Invalid : cardType;
        currentType = cardType;
        selectedCards.Clear();

        switch (cardType)
        {
            case CardType.Invalid:
                //随机出牌
                selectedCards = FindSmartestCards(cards);
                break;
            case CardType.Single:
                selectedCards = FindSingle(cards, rank);
                break;
            case CardType.Pair:
                selectedCards = FindPair(cards, rank);
                break;
            case CardType.Three:
                selectedCards = FindThree(cards, rank);
                break;
            case CardType.ThreeWithOne:
                selectedCards = FindThreeWithOne(cards, rank);
                break;
            case CardType.ThreeWithPair:
                selectedCards = FindThreeAndDouble(cards, rank);
                break;
            case CardType.Straight:
                selectedCards = FindStraight(cards, rank, length);
                break;
            case CardType.PairStraight:
                selectedCards = FindPairStraight(cards, rank, length);
                break;
            case CardType.TripleStraight:
                selectedCards = FindTripleStraight(cards, rank, length);
                break;
            case CardType.TripleStraightWithSingle:
                selectedCards = FindTripleStraightWithSingle(cards, rank, length);
                break;
            case CardType.TripleStraightWithPair:
                selectedCards = FindTripleStraightWithPair(cards, rank, length);
                break;
            case CardType.Bomb:
                selectedCards = FindBomb(cards, rank);
                break;
            case CardType.JokerBomb:
                selectedCards = FindJokerBomb(cards);
                break;
            case CardType.FourWithTwo:
                selectedCards = FindFourWithTwo(cards, rank);
                break;
            case CardType.FourWithTwoPair:
                selectedCards = FindFourWithTwoPairs(cards, rank);
                break;
            default:
                break;
        }

        // 尝试使用炸弹
        if (selectedCards.Count == 0 && cardType != CardType.Bomb && cardType != CardType.JokerBomb)
        {
            TryUseBomb(cards);
        }

        return selectedCards.Count > 0;
    }

    /// <summary>
    /// 决定是否叫地主/抢地主
    /// </summary>
    public virtual bool DecideCallOrGrab(List<Card> cards, bool isGrabbing)
    {
        float handQuality = EvaluateHandQuality(cards);
        float threshold = isGrabbing ? AIConstants.GRAB_THRESHOLD : AIConstants.CALL_THRESHOLD;
        float adjustedProbability = Mathf.Clamp01((handQuality - threshold) + 0.5f);
        return UnityEngine.Random.value < adjustedProbability;
    }
    #endregion

    /// <summary>
    /// 自己随便出牌
    /// </summary>
    /// <param name="cards"></param>
    /// <returns></returns>
    private List<Card> FindSmartestCards(List<Card> cards)
    {
        CardType[] typesToTry = { CardType.Straight, CardType.ThreeWithPair, CardType.ThreeWithOne, CardType.Three, CardType.Pair, CardType.Single };

        foreach (var type in typesToTry)
        {
            var result = FindCardsByType(cards, type);
            if (result.Count > 0)
            {
                currentType = type;
                return result;
            }
        }

        return new List<Card>();
    }

    private List<Card> FindCardsByType(List<Card> cards, CardType type)
    {
        switch (type)
        {
            case CardType.Straight:
                return FindLongestStraight(cards);
            case CardType.ThreeWithPair:
            case CardType.ThreeWithOne:
            case CardType.Three:
                return FindThreeWithExtra(cards, type);
            case CardType.Pair:
                return FindPair(cards, -1);
            case CardType.Single:
                return cards.Take(1).ToList();
            default:
                return new List<Card>();
        }
    }

    #region Protected Utility Methods
    /// <summary>
    /// 尝试使用炸弹
    /// </summary>
    protected void TryUseBomb(List<Card> cards)
    {
        selectedCards = FindBomb(cards, -1);
        if (selectedCards.Count > 0)
        {
            currentType = CardType.Bomb;
        }
        else
        {
            selectedCards = FindJokerBomb(cards);
            if (selectedCards.Count > 0)
            {
                currentType = CardType.JokerBomb;
            }
        }
    }
    #endregion

    public virtual void Reset()
    {
        selectedCards.Clear();
        currentType = CardType.Invalid;
    }
}
