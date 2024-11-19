using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lean.Pool;
using System.Linq;
using UnityEngine.UI;

public class CharacterBase : MonoBehaviour
{
    public CharacterType characterType;
    public string ID;
    public List<Card> cardList = new List<Card>();

    public Transform createPoint;
    public Identity identity;
    public CharacterUI characterUI;

    public GameObject prefab;

    public float randomWaitTime;

    void Awake()
    {
        // 在 Awake 或 Start 中初始化
        //randomWaitTime = UnityEngine.Random.Range(1.0f, 3.0f);
    }

    /// <summary>
    /// 是否有牌
    /// </summary>
    public bool HasCard
    {
        get { { return cardList.Count != 0; } }

    }

    public int CardCount { get { return cardList.Count; } }

    public Transform CreatePoint
    {
        get
        {
            if (createPoint == null)
                createPoint = transform.Find("CreatePoint");
            return createPoint;
        }

    }

    /// <summary>
    /// 添加牌
    /// </summary>
    /// <param name="card">添加的牌</param>
    /// <param name="selected">要增高么</param>
    public virtual void AddCard(Card card, bool selected)
    {
        if (card == null)
        {
            Debug.LogError("Attempting to add a null card!");
            return;
        }

        if (cardList == null)
        {
            cardList = new List<Card>();
        }
        if (CreatePoint == null)
        {
            Debug.LogError("CreatePoint is null!");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("Card prefab is null!");
            return;
        }
        cardList.Add(card);
        //****//
        card.BelongTo = characterType;
        CreateCardUI(card, selected);
        if(characterType == CharacterType.HostPlayer) SortCards(); // 添加卡牌后进行排序
    }

    // 新增的 RemoveCard 方法
    public virtual void RemoveCard(Card card)
    {
        if (cardList.Remove(card))
        {
            // 如果卡牌成功从列表中移除，我们还需要移除对应的 UI
            RemoveCardUI(card);
        }
    }

    // 辅助方法：移除卡牌 UI
    private void RemoveCardUI(Card card)
    {
        CardUI[] cardUIs = CreatePoint.GetComponentsInChildren<CardUI>();
        foreach (CardUI cardUI in cardUIs)
        {
            if (cardUI.Card == card)
            {
                LeanPool.Despawn(cardUI.gameObject);
                break;
            }
        }
        // 重新排序剩余的卡牌 UI
        if(characterType == CharacterType.HostPlayer) SortCards(); // 添加卡牌后进行排序
    }



    /// <summary>
    /// 根据数据创建CardUI
    /// </summary>
    /// <param name="card">数据</param>
    /// <param name="index">索引</param>
    /// <param name="isSeleted">上升？</param>
    public void CreateCardUI(Card card, bool isSeleted)
    {
        if (prefab == null)
        {
            Debug.LogError($"Card prefab is null for {characterType}!");
            return;
        }

        //对象池生成
        GameObject go = LeanPool.Spawn(prefab);
        if (go == null)
        {
            Debug.LogError($"Failed to spawn card prefab for {characterType}!");
            return;
        }

        //设置位置和是否选中
        CardUI cardUI = go.GetComponent<CardUI>();
        if (cardUI == null)
        {
            Debug.LogError($"CardUI component not found on spawned prefab for {characterType}!");
            LeanPool.Despawn(go);
            return;
        }

        cardUI.Card = card;
        cardUI.IsSelected = isSeleted;
        cardUI.SetPosition(CreatePoint);
    }

    public void CreateCardUI(Card card, bool isSeleted, Transform Point)
    {
        if (prefab == null)
        {
            Debug.LogError($"Card prefab is null for {characterType}!");
            return;
        }

        //对象池生成
        GameObject go = LeanPool.Spawn(prefab);
        if (go == null)
        {
            Debug.LogError($"Failed to spawn card prefab for {characterType}!");
            return;
        }

        //设置位置和是否选中
        CardUI cardUI = go.GetComponent<CardUI>();
        if (cardUI == null)
        {
            Debug.LogError($"CardUI component not found on spawned prefab for {characterType}!");
            LeanPool.Despawn(go);
            return;
        }

        cardUI.Card = card;
        cardUI.IsSelected = isSeleted;
        cardUI.SetPosition(Point);
    }

    /// <summary>
    /// 出牌 
    /// </summary>
    /// <returns></returns>
    public virtual Card DealCard()
    {
        Card card = cardList[CardCount - 1];
        cardList.Remove(card);
        return card;
    }

    public void SortCards(bool asc = true)
    {
        CardManager.Sort(cardList, asc);
        SortCardUI();  // 添加一个参数来保持选中状态
    }

    private void SortCardUI()
    {
        for (int i = 0; i < cardList.Count; i++)
        {
            CardUI cardUI = GetCardUI(cardList[i]);
            if (cardUI != null)
            {
                cardUI.transform.SetSiblingIndex(i);
            }
        }
    }

    private CardUI GetCardUI(Card card)
    {
        CardUI[] cardUIs = CreatePoint.GetComponentsInChildren<CardUI>();
        return cardUIs.FirstOrDefault(ui => ui.Card == card);
    }

    public void ClearCards()
    {
        cardList.Clear();
        // 可能还需要更新UI
    }
}