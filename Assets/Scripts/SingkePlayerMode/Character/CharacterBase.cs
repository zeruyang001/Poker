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
        // �� Awake �� Start �г�ʼ��
        //randomWaitTime = UnityEngine.Random.Range(1.0f, 3.0f);
    }

    /// <summary>
    /// �Ƿ�����
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
    /// �����
    /// </summary>
    /// <param name="card">��ӵ���</param>
    /// <param name="selected">Ҫ����ô</param>
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
        if(characterType == CharacterType.HostPlayer) SortCards(); // ��ӿ��ƺ��������
    }

    // ������ RemoveCard ����
    public virtual void RemoveCard(Card card)
    {
        if (cardList.Remove(card))
        {
            // ������Ƴɹ����б����Ƴ������ǻ���Ҫ�Ƴ���Ӧ�� UI
            RemoveCardUI(card);
        }
    }

    // �����������Ƴ����� UI
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
        // ��������ʣ��Ŀ��� UI
        if(characterType == CharacterType.HostPlayer) SortCards(); // ��ӿ��ƺ��������
    }



    /// <summary>
    /// �������ݴ���CardUI
    /// </summary>
    /// <param name="card">����</param>
    /// <param name="index">����</param>
    /// <param name="isSeleted">������</param>
    public void CreateCardUI(Card card, bool isSeleted)
    {
        if (prefab == null)
        {
            Debug.LogError($"Card prefab is null for {characterType}!");
            return;
        }

        //���������
        GameObject go = LeanPool.Spawn(prefab);
        if (go == null)
        {
            Debug.LogError($"Failed to spawn card prefab for {characterType}!");
            return;
        }

        //����λ�ú��Ƿ�ѡ��
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

        //���������
        GameObject go = LeanPool.Spawn(prefab);
        if (go == null)
        {
            Debug.LogError($"Failed to spawn card prefab for {characterType}!");
            return;
        }

        //����λ�ú��Ƿ�ѡ��
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
    /// ���� 
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
        SortCardUI();  // ���һ������������ѡ��״̬
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
        // ���ܻ���Ҫ����UI
    }
}