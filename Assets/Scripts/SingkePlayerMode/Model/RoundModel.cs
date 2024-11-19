using UnityEngine;
using System;
using static CardManager;

/// <summary>
/// 管理游戏回合的模型
/// </summary>
public class RoundModel : MonoBehaviour
{
    #region Fields

    public static event Action<ComputerSmartArgs> PlayerHandler;
    public static event Action<ComputerSmartArgs> ComputerHandler;

    private SingleGameManager singleGameManager;
    private GameStateTracker stateTracker;

    // 回合状态
    private int currentWeight;
    private int currentLength;
    private CardType currentType;
    private CharacterType biggestCharacter;
    private CharacterType currentCharacter;
    private CharacterType startCallingCharacter;
    #endregion


    #region Properties
    public int CurrentWeight
    {
        get => currentWeight;
        set => currentWeight = value;
    }

    public int CurrentLength
    {
        get => currentLength;
        set => currentLength = value;
    }

    public CharacterType BiggestCharacter
    {
        get => biggestCharacter;
        set => biggestCharacter = value;
    }

    public CharacterType CurrentCharacter
    {
        get => currentCharacter;
        set => currentCharacter = value;
    }

    public CardType CurrentType
    {
        get => currentType;
        set => currentType = value;
    }
    #endregion

    #region Initialization
    private void Start()
    {
        singleGameManager = SingleGameManager.Instance;
        stateTracker = singleGameManager.StateTracker;
        InitRound();
    }
    public void InitRound()
    {
        CurrentType = CardType.Invalid;
        CurrentWeight = -1;
        CurrentLength = -1;
        BiggestCharacter = CharacterType.Desk;
        CurrentCharacter = CharacterType.Desk;

    }
    #endregion

    #region Game Flow
    /// <summary>
    /// 开始新的叫地主回合
    /// </summary>
    /// <param name="startPlayer">开始叫地主的玩家</param>
    public void StartCallingRound(CharacterType startPlayer)
    {
        CurrentCharacter = startPlayer;
        startCallingCharacter = startPlayer;
        InvokePlayerAction(startPlayer);
    }

    public void NextCallingTurn()
    {
        currentCharacter = (CharacterType)(((int)currentCharacter + 1) % 3);
        if (currentCharacter == startCallingCharacter)
        {
            singleGameManager.RestartGame();
        }
        else
        {
            InvokePlayerAction(currentCharacter);
        }
    }

    public void GrabbingRound(CharacterType startPlayer)
    {
        CurrentCharacter = startPlayer;
        InvokePlayerAction(startPlayer);
    }

    public void StartRound(CharacterType cType)
    {
        biggestCharacter = cType;
        currentCharacter = cType;
        stateTracker.BiggestCharacter = biggestCharacter;
        stateTracker.CurrentCharacter = currentCharacter;
        InvokePlayerAction(cType);
    }

    public void Pass(CharacterType character)
    {
        NextTurn();
    }

    public void NextTurn()
    {
        currentCharacter++;
        if (currentCharacter == CharacterType.Desk || currentCharacter == CharacterType.Deck)
            currentCharacter = CharacterType.HostPlayer;
        InvokePlayerAction(currentCharacter);
    }
    private void InvokePlayerAction(CharacterType cType)
    {
        var args = CreateSmartArgs();
        stateTracker.RoundCount++;
        if (cType == CharacterType.HostPlayer)
            PlayerHandler?.Invoke(args);
        else
            ComputerHandler?.Invoke(args);


    }

    private ComputerSmartArgs CreateSmartArgs()
    {
        return new ComputerSmartArgs
        {
            // 基础出牌信息
            PlayCardArgs = new PlayCardArgs
            {
                CardType = this.CurrentType,
                Length = this.CurrentLength,
                Weight = this.CurrentWeight,
                CharacterType = this.CurrentCharacter
            },

            // 角色信息
            BiggestCharacter = this.BiggestCharacter,
        };
     }
    #endregion

    #region Card Actions
    public void PlayCard(PlayCardArgs args, IntegrationModel integrationModel)
    {
        if (!singleGameManager) return;

        UpdateRoundData(args);

        PlayCardSound(args.CardType, args.Weight);
        PlayCardAnimation(args.CardType);
        UpdateScore(args.CardType, integrationModel);

        if (singleGameManager.IsGameOver(args.CharacterType))
            singleGameManager.StartCoroutine(singleGameManager.DelayedGameOver(args.CharacterType));
        else
            NextTurn();
    }

    internal void TryPlayCards(PlayCardArgs e)
    {
        if (e.CharacterType != CharacterType.HostPlayer) return;

        if (IsValidPlay(e))
        {
            singleGameManager.SuccessedPlay(e);
        }
        else
        {
            Debug.Log("重新选择");
        }
    }
    #endregion

    #region Helper Methods

    private void PlayCardSound(CardType cardType, int weight)
    {
        string soundEffect = Music.GetCardTypeSound(cardType, weight);
        AudioManager.Instance.PlaySoundEffect(soundEffect);
    }

    private void UpdateRoundData(PlayCardArgs e)
    {
        BiggestCharacter = e.CharacterType;
        CurrentLength = e.Length;
        CurrentWeight = e.Weight;
        CurrentType = e.CardType;
    }

    private void PlayCardAnimation(CardType cardType)
    {
        if (cardType == CardType.Straight || cardType == CardType.PairStraight ||
            cardType == CardType.TripleStraight || cardType == CardType.TripleStraightWithSingle ||
            cardType == CardType.TripleStraightWithPair || cardType == CardType.Bomb ||
            cardType == CardType.JokerBomb)
        {
            AnimationManager.Instance.PlayAnimation(cardType, BiggestCharacter);
        }
    }

    private void UpdateScore(CardType cardType, IntegrationModel integrationModel)
    {
        if (cardType == CardType.Bomb || cardType == CardType.JokerBomb)
        {
            integrationModel.DoubleMultiple();
        }
    }

    private bool IsValidPlay(PlayCardArgs e)
    {
        return biggestCharacter == CharacterType.Desk ||
               (e.CardType == CurrentType && e.Length == CurrentLength && e.Weight > CurrentWeight) ||
               (e.CardType == CardType.Bomb && CurrentType != CardType.Bomb) ||
               e.CardType == CardType.JokerBomb ||
               e.CharacterType == BiggestCharacter;
    }
    #endregion

}
