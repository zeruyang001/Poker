using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TimerUI : MonoBehaviour
{
    [SerializeField] private Image timerFillImage;
    [SerializeField] private TextMeshProUGUI timerText;

    public event Action OnTimeUp;
    public event Action<float> OnTimerUpdate;

    private float duration;
    private float remainingTime;
    private bool isRunning;
    private bool hasFiveSecondsEventFired = false;

    public float RemainingTime => remainingTime;
    public bool IsRunning => isRunning;

    public void StartTimer(float duration)
    {
        if (duration <= 0)
        {
            Debug.LogError("Timer duration must be greater than 0.");
            return;
        }

        this.duration = duration;
        remainingTime = duration;
        isRunning = true;
        gameObject.SetActive(true);
        UpdateTimerDisplay();
        OnTimerUpdate?.Invoke(remainingTime);
    }

    public void StopTimer()
    {
        isRunning = false;
        remainingTime = 0;
        gameObject.SetActive(false);
    }

    public void PauseTimer() => isRunning = false;
    public void ResumeTimer() => isRunning = true;

    public void AddTime(float additionalTime)
    {
        remainingTime = Mathf.Min(remainingTime + additionalTime, duration);
        UpdateTimerDisplay();
    }

    private void Update()
    {
        if (!isRunning) return;
        remainingTime -= Time.deltaTime;
        UpdateTimerDisplay();

        if (!hasFiveSecondsEventFired && remainingTime <= 5f)
        {
            OnTimerUpdate?.Invoke(remainingTime);
            hasFiveSecondsEventFired = true;
        }

        if (remainingTime <= 0)
        {
            TimeUp();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (duration > 0)
        {
            timerFillImage.fillAmount = remainingTime / duration;
        }
        timerText.text = Mathf.CeilToInt(remainingTime).ToString();
    }

    private void TimeUp()
    {
        isRunning = false;
        remainingTime = 0;
        UpdateTimerDisplay();
        OnTimeUp?.Invoke();
    }

    public void ResetTimer()
    {
        hasFiveSecondsEventFired = false;  // ÷ÿ÷√±Í÷æ
        remainingTime = duration;
        UpdateTimerDisplay();
    }

    public void RemoveAllListeners()
    {
        OnTimeUp = null;
        OnTimerUpdate = null;
    }

    public void SetTimerColor(Color color)
    {
        if (timerText != null)
        {
            timerText.color = color;
        }
    }
}