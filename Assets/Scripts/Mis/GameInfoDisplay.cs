using UnityEngine;
using TMPro;
using System.Collections;

public class GameInfoDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI multipleText;
    // 可以在这里添加更多游戏信息的文本字段

    private Color initialColor = Color.green;
    private Color finalColor = Color.red;
    private int initialMultiple;
    private Coroutine currentAnimation;

    private void Start()
    {
        IntegrationModel integrationModel = SingleGameManager.Instance.integrationModel;
        initialMultiple = integrationModel.InitMultiple;

        integrationModel.OnBasePointChanged += UpdateScoreText;
        integrationModel.OnMultipleChanged += UpdateMultipleText;

        UpdateScoreText(integrationModel.BasePoint);
        UpdateMultipleText(integrationModel.InitMultiple);
    }

    private void OnDestroy()
    {
        if (SingleGameManager.Instance != null)
        {
            IntegrationModel integrationModel = SingleGameManager.Instance.integrationModel;
            integrationModel.OnBasePointChanged -= UpdateScoreText;
            integrationModel.OnMultipleChanged -= UpdateMultipleText;
        }
    }

    private void UpdateScoreText(int score)
    {
        scoreText.text = $"{score}";
    }

    private void UpdateMultipleText(int multiple)
    {
        multipleText.text = $" {multiple}";

        // 如果有正在进行的动画，先停止它
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // 开始新的动画
        currentAnimation = StartCoroutine(TextChangeEffect(multipleText, multiple));
    }

    private IEnumerator TextChangeEffect(TextMeshProUGUI text, int multiple)
    {
        Vector3 originalScale = text.transform.localScale;
        Color originalColor = text.color;
        float animationTime = 1f; // 动画总时长
        float halfTime = animationTime / 2f;

        // 放大并变红
        for (float t = 0; t < halfTime; t += Time.deltaTime)
        {
            float progress = t / halfTime;
            text.transform.localScale = Vector3.Lerp(originalScale, originalScale * 3f, progress);
            text.color = Color.Lerp(originalColor, finalColor, progress);
            yield return null;
        }

        // 缩小并变绿
        for (float t = 0; t < halfTime; t += Time.deltaTime)
        {
            float progress = t / halfTime;
            text.transform.localScale = Vector3.Lerp(originalScale * 3f, originalScale, progress);
            text.color = Color.Lerp(finalColor, initialColor, progress);
            yield return null;
        }

        // 确保最终状态正确
        text.transform.localScale = originalScale;
        text.color = originalColor;
        currentAnimation = null;
    }

    public void ResetDisplay()
    {
        IntegrationModel integrationModel = SingleGameManager.Instance.integrationModel;
        initialMultiple = integrationModel.InitMultiple;
        UpdateScoreText(integrationModel.BasePoint);
        UpdateMultipleText(integrationModel.Multiple);
    }

    // 可以在这里添加更多方法来更新其他游戏信息
}