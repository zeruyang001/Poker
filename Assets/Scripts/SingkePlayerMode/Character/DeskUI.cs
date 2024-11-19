using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeskUI : MonoBehaviour
{

    Transform showPoint;

    public Transform ShowPoint
    {
        get
        {
            if (showPoint == null)
                showPoint = transform.Find("ShowPoint").transform;
            return showPoint;
        }
    }

    public CanvasGroup ShowGroup { get { return ShowPoint.GetComponent<CanvasGroup>(); } }

    /// <summary>
    ///设置显示的地主牌
    /// </summary>
    /// <param name="card">显示卡片信息</param>
    /// <param name="index">索引</param>
    public void SetShowCard(Card card, int index)
    {
        Image[] showCards = ShowPoint.GetComponentsInChildren<Image>();
        showCards[index+1].sprite = Resources.Load<Sprite>("CardImages/" + card.GetName());
        SetAlpha(1);
    }

    public void SetAlpha(int i)
    {
        ShowGroup.alpha = i;
    }
}
