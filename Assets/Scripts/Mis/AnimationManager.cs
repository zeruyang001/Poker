using UnityEngine;
using System.Collections;
using static CardManager;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance { get; private set; }

    [SerializeField] private Canvas animationCanvas;
    [SerializeField] private RectTransform playerAnimationPoint;
    [SerializeField] private RectTransform leftComputerAnimationPoint;
    [SerializeField] private RectTransform rightComputerAnimationPoint;

    [SerializeField] private GameObject straightPrefab;
    [SerializeField] private GameObject doubleStraightPrefab;
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject rocketPrefab;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayAnimation(CardType cardType, CharacterType characterType)
    {
        GameObject prefab = GetPrefabForCardType(cardType);
        if (prefab != null)
        {
            RectTransform animationPoint = GetAnimationPointForCharacter(characterType);
            GameObject animationObject = Instantiate(prefab, animationCanvas.transform);
            RectTransform rectTransform = animationObject.GetComponent<RectTransform>();

            rectTransform.anchorMin = animationPoint.anchorMin;
            rectTransform.anchorMax = animationPoint.anchorMax;
            rectTransform.anchoredPosition = animationPoint.anchoredPosition;

            Destroy(animationObject, 1f); // 2秒后销毁动画对象
        }
    }

    public void PlayAnimation_All(CardType cardType, CharacterType characterType)
    {
        GameObject prefab = GetPrefabForCardType(cardType);
        if (prefab != null)
        {
            RectTransform animationPoint = GetAnimationPointForCharacter(characterType);
            GameObject animationObject = Instantiate(prefab, animationCanvas.transform);
            RectTransform rectTransform = animationObject.GetComponent<RectTransform>();

            rectTransform.anchorMin = animationPoint.anchorMin;
            rectTransform.anchorMax = animationPoint.anchorMax;
            rectTransform.anchoredPosition = animationPoint.anchoredPosition;
            rectTransform.sizeDelta = animationPoint.sizeDelta;

            StartCoroutine(MoveAnimation(rectTransform));
        }
    }

    private IEnumerator MoveAnimation(RectTransform rectTransform)
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = startPosition + new Vector2(100, 100); // 移动到右上方100像素
        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rectTransform.anchoredPosition = endPosition;
        Destroy(rectTransform.gameObject, 0.5f); // 动画结束后再等待0.5秒销毁
    }



    private GameObject GetPrefabForCardType(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.Straight:
                AudioManager.Instance.PlaySoundEffect(Music.Star);
                return straightPrefab;
            case CardType.PairStraight:
                AudioManager.Instance.PlaySoundEffect(Music.Star);
                return doubleStraightPrefab;
            case CardType.TripleStraight:
            case CardType.TripleStraightWithSingle:
            case CardType.TripleStraightWithPair:
                AudioManager.Instance.PlaySoundEffect(Music.Plane);
                return planePrefab;
            case CardType.Bomb:
                AudioManager.Instance.PlaySoundEffect(Music.Bomb);
                return bombPrefab;
            case CardType.JokerBomb:
                AudioManager.Instance.PlaySoundEffect(Music.Bomb);
                return rocketPrefab;
            default:
                return null;
        }
    }

    private RectTransform GetAnimationPointForCharacter(CharacterType characterType)
    {
        switch (characterType)
        {
            case CharacterType.HostPlayer:
                return playerAnimationPoint;
            case CharacterType.LeftPlayer:
                return leftComputerAnimationPoint;
            case CharacterType.RightPlayer:
                return rightComputerAnimationPoint;
            default:
                return playerAnimationPoint; // 默认使用玩家位置
        }
    }
}