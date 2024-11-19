using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Lean.Pool;

public class CardUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    [SerializeField] private static GameObject prefab;
    [SerializeField] private Image cardImage;
    [SerializeField] private Color selectedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float selectionHeight = 30f;

    public CharacterType characterType;
    private Card card;
    private bool isSelected;
    private RectTransform rectTransform;

    public Card Card
    {
        get => card;
        set
        {
            card = value;
            SetImage();
        }
    }

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            UpdateCardAppearance();
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cardImage = GetComponent<Image>();
    }

    public static CardUI Create(Card card, bool isSeleted = false)
    {
        GameObject cardObject = LeanPool.Spawn(prefab);
        CardUI cardUI = cardObject.GetComponent<CardUI>();
        cardUI.Card = card;
        cardUI.isSelected = isSeleted;
        return cardUI;
    }

    /// <summary>
    /// ����ͼƬ
    /// </summary>
    public void SetImage()
    {
        if (card == null) return;
        if (card.BelongTo == CharacterType.HostPlayer || card.BelongTo == CharacterType.Desk)
        {
            cardImage.sprite = Resources.Load<Sprite>($"CardImages/{card.GetName()}");
        }
        else //���� ��ʾ����
        {
            cardImage.sprite = Resources.Load<Sprite>("CardImages/FixedBack");
        }
    }

    /// <summary>
    ///��һ�ε�����
    /// </summary>
    public void SetBackImage()
    {
        cardImage.sprite = Resources.Load<Sprite>("CardImages/CardBack");

    }

    /// <summary>
    /// ����Ϊλ���Լ�ƫ��
    /// </summary>
    /// <param name="parent">������</param>
    /// <param name="index">����������</param>

    public void SetPosition(Transform parent)
    {
        if (parent == null) return;
        transform.SetParent(parent, false);

        UpdateCardAppearance();
    }



    public void UpdateCardAppearance()
    {
        cardImage.color = isSelected ? selectedColor : normalColor;

        // �����Ҫѡ��ʱ����Ч��
        if (card.BelongTo == CharacterType.HostPlayer || card.BelongTo == CharacterType.Desk)
        {
            Vector3 localPosition = transform.localPosition;
            localPosition.y = isSelected ? selectionHeight : 0f;
            transform.localPosition = localPosition;
        }
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        ToggleSelect();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Input.GetMouseButton(0))
        {
            ToggleSelect();
        }
    }

    private void ToggleSelect()
    {
        // ֻ������ҵ���
        if (card.BelongTo == CharacterType.HostPlayer)
        {
            IsSelected = !IsSelected;
            UpdateCardAppearance();
            AudioManager.Instance.PlaySoundEffect(Music.Select);
        }

    }

    /// <summary>
    /// ��ʼ������
    /// </summary>
    public void OnSpawn()
    {
        cardImage = GetComponent<Image>();
    }

    /// <summary>
    /// ��������
    /// </summary>
    public void OnDespawn()
    {
        isSelected = false;
        cardImage.sprite = null;
        card = null;
        name = null;
    }

    /// <summary>
    /// ����
    /// </summary>
    public void Destroy()
    {
        LeanPool.Despawn(gameObject);
    }
}