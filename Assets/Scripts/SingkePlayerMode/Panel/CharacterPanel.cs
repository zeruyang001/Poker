using AI.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using static CardManager;

public class CharacterPanel : BasePanel
{
    #region Fields
    public Player hostPlayer;
    public DeskControl deskControl;
    public Player leftPlayer;
    public Player rightPlayer;
    [SerializeField] private GameInfoDisplay gameInfoDisplay;
    [SerializeField] private PlayerActionDisplayManager actionDisplayManager;

    private SingleGameManager singleGameManager;
    private ComputerSmartArgs smartArgs = new ComputerSmartArgs();
    #endregion

    #region Initialization
    public override void OnInit()
    {
        base.OnInit();
        InitializeEventListeners();
    }

    public override void OnShow(params object[] para)
    {
        InitializePlayers();
        InitializeGameManager();
    }

    private void InitializeEventListeners()
    {
        RoundModel.ComputerHandler += RoundModel_ComputerHandler;
        RoundModel.PlayerHandler += RoundModel_PlayerHandler;
        IntegrationModel.OnGameDateChanged += UpdateIntegration;
    }

    private void InitializeGameManager()
    {
        singleGameManager = SingleGameManager.Instance;
        gameInfoDisplay = gameObject.transform.Find("GameInfo").GetComponent<GameInfoDisplay>();
        gameInfoDisplay.ResetDisplay();
    }

    private void InitializePlayers()
    {
        hostPlayer = InitializePlayer("Player", CharacterType.HostPlayer);
        leftPlayer = InitializePlayer("ComputerLeft", CharacterType.LeftPlayer);
        rightPlayer = InitializePlayer("ComputerRight", CharacterType.RightPlayer);
        deskControl = InitializeDeskControl("Desk");
    }

    private Player InitializePlayer(string name, CharacterType type)
    {
        GameObject playerObject = gameObject.transform.Find(name).gameObject;
        Player player = playerObject.GetComponent<Player>() ?? playerObject.AddComponent<Player>();
        player.characterType = type;
        player.Identity = Identity.Farmer;
        return player;
    }

    private DeskControl InitializeDeskControl(string name)
    {
        GameObject deskObject = gameObject.transform.Find(name).gameObject;
        DeskControl desk = deskObject.GetComponent<DeskControl>() ?? deskObject.AddComponent<DeskControl>();
        desk.characterType = CharacterType.Desk;
        return desk;
    }
    #endregion

    #region Game Flow
    public void InitCharacters()
    {
        ResetAllPlayers();
        HideAllPlayerActions();
    }

    private void ResetAllPlayers()
    {
        Lean.Pool.LeanPool.DespawnAll();
        ResetPlayer(hostPlayer);
        ResetPlayer(leftPlayer);
        ResetPlayer(rightPlayer);
        deskControl?.Clear();
    }

    private void ResetPlayer(Player player)
    {
        player?.ClearCards();
        player.characterUI.SetRemain(0);
        player.Identity = Identity.Farmer;
        player.ResetLandlordCall();
    }

    public void RequestDeal()
    {
        StartCoroutine(DealCard());
    }

    private IEnumerator DealCard()
    {
        yield return DealInitialCards();
        FinalizeDeal();
    }

    private IEnumerator DealInitialCards()
    {
        for (int i = 0; i < 17; i++)
        {
            AddCard(CharacterType.HostPlayer, CardManager.DrawCard(), false, ShowPoint.Player);
            AddCard(CharacterType.LeftPlayer, CardManager.DrawCard(), false, ShowPoint.ComputerLeft);
            AddCard(CharacterType.RightPlayer, CardManager.DrawCard(), false, ShowPoint.ComputerRight);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void FinalizeDeal()
    {
        for (int i = 0; i < 3; i++)
        {
            AddCard(CharacterType.Desk, CardManager.DrawCard(), false, ShowPoint.Desk);
        }
        SetBackImageForDeskCards();
        SingleGameManager.Instance.OnDealComplete(deskControl.cardList);
    }

    private void SetBackImageForDeskCards()
    {
        CardUI[] cardUIs = deskControl.CreatePoint.GetComponentsInChildren<CardUI>();
        foreach (var ui in cardUIs)
            ui.SetBackImage();
    }

    public void AddCard(CharacterType cType, Card card, bool isSelect, ShowPoint pos)
    {
        switch (cType)
        {
            case CharacterType.HostPlayer:
                hostPlayer.AddCard(card, isSelect);
                break;
            case CharacterType.RightPlayer:
                rightPlayer.AddCard(card, isSelect);
                break;
            case CharacterType.LeftPlayer:
                leftPlayer.AddCard(card, isSelect);
                break;
            case CharacterType.Desk:
                deskControl.AddCard(card, isSelect, pos);
                break;
        }
    }

    public void GrabLandlord(CharacterType character)
    {
        // 先处理地主牌
        DealThreeCard(character);

        // 更新身份
        UpdatePlayerIdentity(character, Identity.Landlord);

        // 初始化AI的玩家关系
        InitializeAIRelationships();

        // 清理地主牌显示
        deskControl.Clear(ShowPoint.Desk);
        hostPlayer.RefreshAllCardsAppearance();
    }

    private void UpdatePlayerIdentity(CharacterType characterType, Identity identity)
    {
        GetCharacterControl(characterType).Identity = identity;
    }

    private void DealThreeCard(CharacterType cType)
    {
        AudioManager.Instance.PlaySoundEffect(Music.DealCard);
        for (int i = 0; i < 3; i++)
        {
            Card card = deskControl.DealCard();
            AddCard(cType, card, false, ShowPoint.Player);
            deskControl.SetShowCard(card, i);
        }
    }

    public void CompleteDeal()
    {
        leftPlayer.SortCards();
        rightPlayer.SortCards();
    }

    public void UpdateIntegration(GameData gameData)
    {
        hostPlayer.characterUI.SetIntergation(gameData.playerIntegration);
        leftPlayer.characterUI.SetIntergation(gameData.computerLeftIntegration);
        rightPlayer.characterUI.SetIntergation(gameData.computerRightIntegration);
    }
    #endregion

    #region AI Logic
    private IEnumerator ComputerPlay(ComputerSmartArgs args)
    {
        Player computerPlayer = GetCharacterControl(args.PlayCardArgs.CharacterType);
        computerPlayer.StartTurn();
        yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 3.0f));

        deskControl.Clear(GetShowPointForCharacter(args.PlayCardArgs.CharacterType));

        // 使用新的 ComputerSmartArgs 结构
        bool canPlay = computerPlayer.SmartSelectCards(args);
        if (canPlay)
        {
            ProcessComputerCardPlay(computerPlayer, args.PlayCardArgs.CharacterType);
        }
        else
        {
            // 处理过牌
            singleGameManager.HandlePass(computerPlayer.characterType);
            ShowPassAction(args.PlayCardArgs.CharacterType);
        }
    }

    private void ShowPassAction(CharacterType character)
    {
        // 显示过牌动画或提示
        //ShowPlayerAction(character, PlayerActionState.Pass);
    }

    private void ProcessComputerCardPlay(Player computerPlayer, CharacterType characterType)
    {
        List<Card> cardList = computerPlayer.SelectCards;
        CardType currentType = computerPlayer.CurrType;

        // 移动牌到桌面
        foreach (var card in cardList)
        {
            deskControl.AddCard(card, false, GetShowPointForCharacter(characterType));
        }

        // 结束回合并提交出牌
        computerPlayer.EndTurn();
        PlayCardArgs AIplayCardArgs = new PlayCardArgs
        {
            CardType = currentType,
            Length = cardList.Count,
            CharacterType = characterType,
            Weight = CardManager.GetWeight(cardList, currentType)
        };

        singleGameManager.PlayCard(AIplayCardArgs, cardList);
    }

    private IEnumerator AICallLandlord(CharacterType aiPlayer)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 4f));
        Player ai = GetCharacterControl(aiPlayer);

        // 获取当前游戏状态用于AI决策
        ComputerSmartArgs args = singleGameManager.GetCurrentGameState();
        bool shouldCall = ai.ComputerAI.DecideCallOrGrab(ai.cardList, false);
        singleGameManager.CallLandlord(shouldCall, aiPlayer);
    }

    private IEnumerator AIGrabLandlord(CharacterType aiPlayer)
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 4f));
        Player ai = GetCharacterControl(aiPlayer);

        // 获取当前游戏状态用于AI决策
        ComputerSmartArgs args = singleGameManager.GetCurrentGameState();
        bool shouldGrab = ai.ComputerAI.DecideCallOrGrab(ai.cardList, true);
        singleGameManager.GrabLandlord(shouldGrab, aiPlayer);
    }
    #endregion

    #region Player Actions
    public void RequestPlay()
    {
        List<Card> cardList = hostPlayer.FindSelectCard();
        if (CardManager.CanPop(cardList, out CardType cardType))
        {
            PlayCardArgs e = new PlayCardArgs()
            {
                CardType = cardType,
                CharacterType = CharacterType.HostPlayer,
                Length = cardList.Count,
                Weight = CardManager.GetWeight(cardList, cardType)
            };

            singleGameManager.TryPlayCards(e);
        }
        else
        {
            ShowTip("牌不对");
            Debug.Log("牌不对！");
        }
    }

    public void RequestPass()
    {
        // 更新为使用新的过牌处理
        singleGameManager.HandlePass(hostPlayer.characterType);
        hostPlayer.PassAction();
    }

    public void OnSuccessedPlay(PlayCardArgs playerArgs)
    {
        hostPlayer.EndTurn();
        List<Card> cardList = hostPlayer.FindSelectCard();
        deskControl.Clear(ShowPoint.Player);
        foreach (var card in cardList)
            deskControl.AddCard(card, false, ShowPoint.Player);

        hostPlayer.DestroySelectCard();

        singleGameManager.PlayCard(playerArgs, cardList);
    }

    public void RequestHint()
    {
        ComputerSmartArgs currentState = singleGameManager.GetCurrentGameState();
        bool can = hostPlayer.SmartHintCards(currentState);
        if (!can)
        {
            ShowTip("没能大过上家牌");
        }
    }
    #endregion

    #region Event Handlers
        private void RoundModel_PlayerHandler(ComputerSmartArgs args)
    {
        HandlePlayerTurn(args);
    }

    private void RoundModel_ComputerHandler(ComputerSmartArgs args)
    {
        HandleGameStateChange(GameManager.gameState, args);
    }

    private void HandlePlayerTurn(ComputerSmartArgs args)
    {
        switch (GameManager.gameState)
        {
            case GameState.Calling:
            case GameState.Grabbing:
                hostPlayer.StartTurn(true);
                break;
            case GameState.Playing:
                HandlePlayingState(args);
                break;
            default:
                Debug.LogWarning($"Unhandled game state in HandlePlayerTurn: {GameManager.gameState}");
                break;
        }
    }
    private void HandlePlayingState(ComputerSmartArgs args)
    {
        if (args.PlayCardArgs.CharacterType == CharacterType.HostPlayer)
        {
            PreparePlayerTurn(args);
        }
        else
        {
            StartCoroutine(ComputerPlay(args));
        }
    }

    private void PreparePlayerTurn(ComputerSmartArgs args)
    {
        hostPlayer.StartTurn();
        deskControl.Clear(ShowPoint.Player);

        // 更新本地状态
        UpdateLocalGameState(args);
    }

    private void UpdateLocalGameState(ComputerSmartArgs args)
    {
        if (args != null)
        {
            smartArgs = args.Clone();
        }
        else
        {
            Debug.LogWarning("Received null ComputerSmartArgs in PreparePlayerTurn");
            smartArgs = new ComputerSmartArgs();
        }
    }

    private void HandleLandlordSelection(ComputerSmartArgs args)
    {
        if (args.PlayCardArgs.CharacterType != CharacterType.HostPlayer)
        {
            StartCoroutine(GameManager.gameState == GameState.Calling
                ? AICallLandlord(args.PlayCardArgs.CharacterType)
                : AIGrabLandlord(args.PlayCardArgs.CharacterType));
        }
    }

    public void HandleGameStateChange(GameState newState, ComputerSmartArgs args)
    {
        Player currentPlayer = GetCharacterControl(args.PlayCardArgs.CharacterType);
        switch (newState)
        {
            case GameState.Calling:
            case GameState.Grabbing:
                currentPlayer.StartTurn(true);
                HandleLandlordSelection(args);
                break;
            case GameState.Playing:
                HandlePlayingState(args);
                break;
            default:
                Debug.LogWarning($"Unhandled game state in HandleGameStateChange: {newState}");
                break;
        }
    }
    #endregion

    #region UI Methods
    public void ShowPlayerAction(CharacterType character, PlayerActionState state)
    {
        actionDisplayManager.ShowPlayerAction(character, state);
    }

    public void HidePlayerAction(CharacterType character)
    {
        actionDisplayManager.HidePlayerAction(character);
    }

    public void HideAllPlayerActions()
    {
        actionDisplayManager.HideAllPlayerActions();
    }
    #endregion

    #region Utility Methods
    public Player GetCharacterControl(CharacterType characterType)
    {
        switch (characterType)
        {
            case CharacterType.HostPlayer:
                return hostPlayer;
            case CharacterType.LeftPlayer:
                return leftPlayer;
            case CharacterType.RightPlayer:
                return rightPlayer;
            default:
                Debug.LogError($"Unsupported CharacterType: {characterType}");
                return null;
        }
    }

    private ShowPoint GetShowPointForCharacter(CharacterType characterType)
    {
        switch (characterType)
        {
            case CharacterType.HostPlayer: return ShowPoint.Player;
            case CharacterType.LeftPlayer: return ShowPoint.ComputerLeft;
            case CharacterType.RightPlayer: return ShowPoint.ComputerRight;
            default: throw new ArgumentException("Invalid character type");
        }
    }
    #endregion

    #region AI Help Methods
    private void InitializeAIRelationships()
    {
        InitializeAIPlayerRelationships(hostPlayer);
        // 对每个AI玩家初始化关系
        InitializeAIPlayerRelationships(leftPlayer);
        InitializeAIPlayerRelationships(rightPlayer);
    }

    private void InitializeAIPlayerRelationships(Player aiPlayer)
    {
        if (aiPlayer.ComputerAI is AdvancedComputerAI advancedAI)
        {

            // 根据AI的位置确定其左右玩家
            (Player leftPlayer, Player rightPlayer) = GetAdjacentPlayers(aiPlayer.characterType);

            // 初始化关系
/*            advancedAI.InitializePlayerRelationships(
                deskControl.cardList,
                aiPlayer,
                leftPlayer,
                rightPlayer
            );*/
        }
    }

    /// <summary>
    /// 获取指定玩家位置的左右玩家
    /// </summary>
    private (Player leftPlayer, Player rightPlayer) GetAdjacentPlayers(CharacterType currentPosition)
    {
        Player leftPlayer, rightPlayer;

        switch (currentPosition)
        {
            case CharacterType.HostPlayer:
                leftPlayer = this.leftPlayer;
                rightPlayer = this.rightPlayer;
                break;

            case CharacterType.LeftPlayer:
                leftPlayer = this.rightPlayer;
                rightPlayer = hostPlayer;
                break;

            case CharacterType.RightPlayer:
                leftPlayer = hostPlayer;
                rightPlayer = this.leftPlayer;
                break;

            default:
                Debug.LogError($"Invalid player position: {currentPosition}");
                return (null, null);
        }

        return (leftPlayer, rightPlayer);
    }
    #endregion

    #region Cleanup
    public override void OnClose()
    {
        RemoveEventListeners();
        base.OnClose();
    }

    private void RemoveEventListeners()
    {
        RoundModel.ComputerHandler -= RoundModel_ComputerHandler;
        RoundModel.PlayerHandler -= RoundModel_PlayerHandler;
        IntegrationModel.OnGameDateChanged -= UpdateIntegration;
    }
    #endregion
}