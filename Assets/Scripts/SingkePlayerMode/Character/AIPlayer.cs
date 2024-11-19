using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using static CardManager;
using System.Linq;
using AI.Utils;
using AI.Core;

public class AIPlayer : CharacterBase
{
    #region Fields
    private const float TurnDuration = 20f;
    private const float CallDuration = 10f;
    public CanvasGroup Group;
    public TimerUI TimerUI;
    private AILevel currentLevel = AILevel.Advanced;
    public ComputerAI computerAI;

    private bool hasPlayedReminder = false;
    public enum LandlordCallState
    {
        Initial = 0,
        NotCalled = 1,
        Called = 2
    }

    public LandlordCallState HasCalledLandlord { get; set; }
    public LandlordCallState HasGrabbedLandlord { get; set; }
    #endregion


    #region Properties
    public ComputerAI ComputerAI
    {
        get => computerAI;
        private set
        {
            computerAI = value;
            if (computerAI is AdvancedComputerAI advancedAI)
            {
                // 初始化高级AI的特殊属性
               // InitializeAdvancedAI(advancedAI);
            }
        }
    }
    public List<Card> SelectCards => ComputerAI.selectedCards;
    public CardType CurrType => ComputerAI.currentType;
    public Identity Identity
    {
        get => identity;
        set
        {
            identity = value;
            characterUI.SetIdentity(value);
        }
    }
    #endregion

    #region Unity Lifecycle
    protected virtual void Start()
    {
        InitializeAI();
        SetupTimer();
    }
    #endregion

    #region Initialization
    private void InitializeAI()
    {
        prefab = prefab ?? Resources.Load<GameObject>("CardUI");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load card prefab for {characterType}!");
        }
        ComputerAI = GetComponent<ComputerAI>();
        //ComputerAI = CreateAI(currentLevel);
        if (ComputerAI == null)
        {
            Debug.LogError("未找到 ComputerAI 组件");
            ComputerAI = gameObject.AddComponent<ComputerAI>();
        }
    }

    private void SetupTimer()
    {
        if (TimerUI != null)
        {
            TimerUI.OnTimerUpdate += OnTimerUpdate;
            TimerUI.OnTimeUp += HandleTimeUp;
        }
    }
    #endregion


    #region Card Management
    public override void AddCard(Card card, bool selected = false)
    {
        base.AddCard(card, selected);
        characterUI.SetRemain(CardCount);
    }

    public override Card DealCard()
    {
        Card card = base.DealCard();
        characterUI.SetRemain(CardCount);
        return card;
    }

    /// <summary>
    /// 智能选牌（适配新的ComputerSmartArgs）
    /// </summary>
    public bool SmartSelectCards(ComputerSmartArgs args)
    {
        if (ComputerAI.SmartSelectCards(cardList, args))
        {
            DestroyCards(SelectCards);
            return true;
        }
        PassAction();
        return false;
    }

    /// <summary>
    /// 智能提示（适配新的ComputerSmartArgs）
    /// </summary>
    public bool SmartHintCards(ComputerSmartArgs args)
    {
        if (ComputerAI.SmartSelectCards(cardList, args))
        {
            ShowHintCards(SelectCards);
            return true;
        }
        return false;
    }

    private void DestroyCards(List<Card> cardsToDestroy)
    {
        var cardsToRemove = CreatePoint.GetComponentsInChildren<CardUI>()
            .Where(cardUI => cardsToDestroy.Contains(cardUI.Card))
            .ToList();

        foreach (var cardUI in cardsToRemove)
        {
            cardUI.Destroy();
            cardList.Remove(cardUI.Card);
        }
        characterUI.SetRemain(CardCount);
    }

    private void ShowHintCards(List<Card> cardsToHint)
    {
        foreach (var cardUI in CreatePoint.GetComponentsInChildren<CardUI>())
        {
            cardUI.IsSelected = cardsToHint.Contains(cardUI.Card);
        }
        characterUI.SetRemain(CardCount);
    }
    #endregion


    #region Turn Management
    public void StartTurn(bool isCallPhase = false)
    {
        float duration = isCallPhase ? CallDuration : TurnDuration;
        TimerUI.StartTimer(duration);
        hasPlayedReminder = false;

        if (characterType == CharacterType.HostPlayer)
        {
            AudioManager.Instance.PlaySoundEffect(Music.Ok);
        }
    }
    private void OnTimerUpdate(float remainingTime)
    {
        if (remainingTime <= 5f)
        {
            if (!hasPlayedReminder)
            {
                AudioManager.Instance.PlaySoundEffect(Music.Remind);
                hasPlayedReminder = true;
            }

            // 假设 TimerUI 有一个 SetTimerColor 方法来改变颜色
            TimerUI.SetTimerColor(Color.red);
        }
        else
        {
            // 重置颜色和提醒状态
            TimerUI.SetTimerColor(Color.black);
            hasPlayedReminder = false;
        }
    }

    public void EndTurn()
    {
        TimerUI.StopTimer();
    }

    private void HandleTimeUp()
    {
        switch (GameManager.gameState)
        {
            case GameState.Calling:
                AutoCall();
                break;
            case GameState.Grabbing:
                AutoGrab();
                break;
            case GameState.Playing:
                AutoPlayPass();
                break;
            default:
                Debug.LogWarning($"Unexpected game state in HandleTimeUp: {GameManager.gameState}");
                break;
        }
    }

    private void AutoCall() =>
        SingleGameManager.Instance.CallLandlord(characterType == CharacterType.HostPlayer ? false : DecideCallOrGrab(false), characterType);

    private void AutoGrab() =>
        SingleGameManager.Instance.GrabLandlord(characterType == CharacterType.HostPlayer ? false : DecideCallOrGrab(true), characterType);

    private void AutoPlayPass()
    {
        if (characterType == CharacterType.HostPlayer)
        {
            SingleGameManager.Instance.PlayerPass();
        }
        else
        {
            SingleGameManager.Instance.HandlePass(characterType);
            PassAction();
        }
    }
    #endregion

    #region AI Decision Making
    public bool DecideCallOrGrab(bool isGrabbing)
    {
        // 获取当前游戏状态用于决策
        ComputerSmartArgs currentState = SingleGameManager.Instance.GetCurrentGameState();
        return ComputerAI.DecideCallOrGrab(cardList, isGrabbing);
    }

    public void ResetLandlordCall()
    {
        HasCalledLandlord = LandlordCallState.Initial;
        HasGrabbedLandlord = LandlordCallState.Initial;
    }
    #endregion

    #region UI Management
    public void PassAction()
    {
        EndTurn();
        Group.alpha = 1;
        StartCoroutine(Pass());
    }

    private IEnumerator Pass()
    {
        yield return new WaitForSeconds(2f);
        Group.alpha = 0;
    }
    #endregion

    #region Helper Methods
    public  ComputerAI CreateAI(AILevel level)
    {
        switch (level)
        {
            case AILevel.Advanced:
                return new AdvancedComputerAI();
            case AILevel.Basic:
            default:
                return new ComputerAI();
        }
    }
    #endregion

    #region Cleanup
    protected virtual void OnDestroy()
    {
        if (TimerUI != null)
        {
            TimerUI.OnTimeUp -= HandleTimeUp;
            TimerUI.OnTimerUpdate -= OnTimerUpdate;
            TimerUI.RemoveAllListeners();
        }
    }
    #endregion
}
