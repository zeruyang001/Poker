using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class Player : AIPlayer
{
    List<Card> tempCard = null;
    List<CardUI> tempUI = null;

    /// <summary>
    /// �ҵ�ѡ�е�����
    /// </summary>
    /// <returns>ѡ�е���</returns>
    public List<Card> FindSelectCard()
    {
        CardUI[] cardUIs = CreatePoint.GetComponentsInChildren<CardUI>();
        tempCard = new List<Card>();
        tempUI = new List<CardUI>();
        for (int i = 0; i < cardUIs.Length; i++)
        {
            if (cardUIs[i].IsSelected)
            {
                tempUI.Add(cardUIs[i]);
                tempCard.Add(cardUIs[i].Card);
            }
        }
        //****///
        CardManager.Sort(tempCard);
        return new List<Card>(tempCard); ;
    }

    /// <summary>
    /// ɾ������/�ɹ�����
    /// </summary>
    public void DestroySelectCard()
    {
        if (tempCard == null || tempUI == null)
            return;
        else
        {

            for (int i = 0; i < tempCard.Count; i++)
            {
                tempUI[i].Destroy();
                cardList.Remove(tempCard[i]);
            }

            SortCards();
            characterUI.SetRemain(CardCount);
        }
    }


        public Player(CharacterType type)
    {
        characterType = type;
    }

    public void RefreshAllCardsAppearance()
    {
        foreach (var cardUI in CreatePoint.GetComponentsInChildren<CardUI>())
        {
            cardUI.UpdateCardAppearance();
        }
        // ǿ��ˢ�²���
        LayoutRebuilder.ForceRebuildLayoutImmediate(CreatePoint as RectTransform);
    }
    // ������Ӹ���������еķ���
}