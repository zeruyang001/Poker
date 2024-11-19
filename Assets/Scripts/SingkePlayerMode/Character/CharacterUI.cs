using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ������� ���� ͷ�� ������ʾ
/// </summary>
public class CharacterUI : MonoBehaviour
{

    public Image head;
    public Text score;
    public Text remain;

    public void SetIdentity(Identity identity)
    {
        switch (identity)
        {
            case Identity.Farmer:
                head.sprite = Resources.Load<Sprite>("Pokers/Role_Farmer");
                break;
            case Identity.Landlord:
                head.sprite = Resources.Load<Sprite>("Pokers/Role_Landlord");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// ���û���
    /// </summary>
    /// <param name="score"></param>
    public void SetIntergation(int score)
    {
        this.score.text = score.ToString();
    }

    public void SetRemain(int number)
    {
        remain.text = "ʣ������ : " + number.ToString();
    }
}
