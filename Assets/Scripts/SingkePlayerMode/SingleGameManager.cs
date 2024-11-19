using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;
using static AIPlayer;
using static CardManager;

public class SingleGameManager : MonoBehaviour
{
    #region Singleton
    public static SingleGameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeModel();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Fields
    public GameStateTracker StateTracker { get; set; }
    public IntegrationModel integrationModel;
    private RoundModel roundModel;
    private DataManager dataManager;
    private CharacterType theLandlord;

    private CharacterPanel characterPanel;
    private InteractionPanel interactionPanel;
    private CharacterType currentPlayerType;
    private Player currentPlayer;
    #endregion

    #region Initialization
    private void InitializeModel()
    {
        StateTracker = GameStateTracker.Instance;
        integrationModel = new IntegrationModel();
        roundModel = gameObject.GetComponent<RoundModel>() ?? gameObject.AddComponent<RoundModel>();
        dataManager = new DataManager();
    }

    public void StartSingleGameManager()
    {
        PanelManager.Open<CharacterPanel>();
        characterPanel = FindObjectOfType<CharacterPanel>();
        PanelManager.Open<InteractionPanel>();
        interactionPanel = FindObjectOfType<InteractionPanel>();
        if (interactionPanel == null || characterPanel == null)
        {
            Debug.LogError("Failed to find InteractionPanel or CharacterPanel");
            return;
        }
        InitializeGame();
    }

    public void InitializeGame()
    {
        if (characterPanel == null)
        {
            Debug.LogError("characterPanel is null in InitializeGame");
            return;
        }
        integrationModel.Init(dataManager.GetData());
        StateTracker.StartNewGame();
        ShuffleDeck();
        roundModel.InitRound();
        characterPanel.InitCharacters();
        interactionPanel.InitButton();
    }

    public void ShuffleDeck()
    {
        CardManager.CreateDeck();
        CardManager.ShuffleDeck();
    }
    #endregion

    #region Game Flow
    public void OnDealComplete(List<Card> threeLandlordCards)
    {
        StateTracker.landlordCards = threeLandlordCards;
        StartCallingLandlord();
    }

    public void StartCallingLandlord()
    {
        GameManager.gameState = GameState.Calling;
        StateTracker.SetGameState(GameState.Calling);
        characterPanel.HideAllPlayerActions();
        currentPlayerType = (CharacterType)UnityEngine.Random.Range(0, 3);
        roundModel.StartCallingRound(currentPlayerType);
    }
    public void CallLandlord(bool call, CharacterType PlayerType)
    {
        currentPlayer = characterPanel.GetCharacterControl(PlayerType);
        currentPlayerType = PlayerType;
        currentPlayer.EndTurn();
        currentPlayer.HasCalledLandlord = call ? LandlordCallState.Called : LandlordCallState.NotCalled;

        if (call)
        {
            HandleCallLandlord();
        }
        else
        {
            HandleNotCallLandlord();
        }
    }
    private void HandleCallLandlord()
    {
        AudioManager.Instance.PlaySoundEffect(Music.Order);
        theLandlord = currentPlayerType;
        characterPanel.ShowPlayerAction(currentPlayerType, PlayerActionState.CallLandlord);
        integrationModel.DoubleMultiple();

        GameManager.gameState = GameState.Grabbing;
        StateTracker.SetGameState(GameState.Grabbing);
        GrabbingLandlord();
    }

    private void HandleNotCallLandlord()
    {
        AudioManager.Instance.PlaySoundEffect(Music.NoOrder);
        characterPanel.ShowPlayerAction(currentPlayerType, PlayerActionState.NotCall);
        NextPlayerTurn();
        roundModel.NextCallingTurn();
    }

    public void GrabbingLandlord()
    {
        do
        {
            NextPlayerTurn();
            if (currentPlayerType == theLandlord)
            {
                StartPlaying();
                return;
            }
        } while (IsPlayerIneligibleForGrabbing());

        roundModel.GrabbingRound(currentPlayerType);
    }

    private bool IsPlayerIneligibleForGrabbing()
    {
        Player player = characterPanel.GetCharacterControl(currentPlayerType);
        return player.HasCalledLandlord == LandlordCallState.NotCalled || player.HasGrabbedLandlord != LandlordCallState.Initial;
    }
    public void GrabLandlord(bool grab, CharacterType PlayerType)
    {
        currentPlayerType = PlayerType;
        currentPlayer = characterPanel.GetCharacterControl(currentPlayerType);
        currentPlayer.HasGrabbedLandlord = grab ? LandlordCallState.Called : LandlordCallState.NotCalled;
        currentPlayer.EndTurn();

        if (grab)
        {
            HandleGrabLandlord();
        }
        else
        {
            HandleNotGrabLandlord();
        }

        GrabbingLandlord();
    }

    private void HandleGrabLandlord()
    {
        AudioManager.Instance.PlaySoundEffect(Music.Grab);
        characterPanel.ShowPlayerAction(currentPlayerType, PlayerActionState.GrabLandlord);
        theLandlord = currentPlayerType;
        integrationModel.DoubleMultiple();
        currentPlayer.HasGrabbedLandlord = LandlordCallState.Called;
    }

    private void HandleNotGrabLandlord()
    {
        AudioManager.Instance.PlaySoundEffect(Music.DisGrab);
        characterPanel.ShowPlayerAction(currentPlayerType, PlayerActionState.NotGrab);
        currentPlayer.HasGrabbedLandlord = LandlordCallState.NotCalled;
    }

    private void NextPlayerTurn()
    {
        currentPlayerType = (CharacterType)(((int)currentPlayerType + 1) % 3);
    }

    private void StartPlaying()
    {
        GameManager.gameState = GameState.Playing;
        StateTracker.SetGameState(GameState.Playing);
        StateTracker.SetLandlord(theLandlord);
        characterPanel.HideAllPlayerActions(); // ������ƽ׶�ʱ����������ʾ
        characterPanel.GrabLandlord(theLandlord);
        roundModel.StartRound(theLandlord);
    }
    #endregion

    #region Game Actions
    public void RestartGame()
    {
        // ����ϴ�Ʋ�����
        InitializeGame();
        RequestDeal();
    }


    public void RequestDeal()
    {
        if (characterPanel != null)
        {
            characterPanel.RequestDeal();
            GameManager.gameState = GameState.Dealing;
            StateTracker.SetGameState(GameState.Dealing);
        }
        else
        {
            Debug.LogError("CharacterPanel not found!");
        }
    }

    public void RequestPlay()
    {
        characterPanel.RequestPlay();
    }

    public void RequestPass()
    {
        characterPanel.RequestPass();
    }
    public void RequestHint()
    {
        characterPanel.RequestHint();
    }

    public void PlayCard(PlayCardArgs args, List<Card> playCards)
    {
        StateTracker.RecordPlayedCards(args,playCards);
        roundModel.PlayCard(args, integrationModel);
    }

    internal void TryPlayCards(PlayCardArgs e)
    {
        roundModel.TryPlayCards(e);
    }

    public void SuccessedPlay(PlayCardArgs e)
    {
        interactionPanel.OnSuccessedPlay();
        characterPanel.OnSuccessedPlay(e);
    }

    public void PlayerPass()
    {
        // �������Pass���߼�
        interactionPanel.OnPassClick();
    }

    #endregion

    #region Game Over
    public IEnumerator DelayedGameOver(CharacterType winnerType)
    {
        yield return new WaitForSeconds(1f);

        // ��ȡӮ�����
        Identity winnerIdentity = characterPanel.GetCharacterControl(winnerType).Identity;
        GameOverArgs gameOverArgs = new GameOverArgs();

        // ���Ӯ���ǵ���
        if (winnerIdentity == Identity.Landlord)
        {
            gameOverArgs.isLandlord = true;
            // ֻ��Ӯ�һ�ʤ
            gameOverArgs.ComputerRightWin = winnerType == CharacterType.RightPlayer;
            gameOverArgs.ComputerLeftWin = winnerType == CharacterType.LeftPlayer;
            gameOverArgs.PlayerWin = winnerType == CharacterType.HostPlayer;
        }
        // ���Ӯ����ũ��
        else
        {
            gameOverArgs.isLandlord = false;
            // ����ũ�񶼻�ʤ
            foreach (CharacterType type in new[] { CharacterType.HostPlayer, CharacterType.LeftPlayer, CharacterType.RightPlayer })
            {
                if (characterPanel.GetCharacterControl(type).Identity == Identity.Farmer)
                {
                    switch (type)
                    {
                        case CharacterType.HostPlayer:
                            gameOverArgs.PlayerWin = true;
                            break;
                        case CharacterType.LeftPlayer:
                            gameOverArgs.ComputerLeftWin = true;
                            break;
                        case CharacterType.RightPlayer:
                            gameOverArgs.ComputerRightWin = true;
                            break;
                    }
                }
            }
        }

        GameOver(gameOverArgs);
    }

    public void GameOver(GameOverArgs gameOverArgs)
    {
        integrationModel.GameOver(gameOverArgs);

        // �洢����
        SaveGameData();

        // �����Ϸ�������
        PanelManager.Open<GameOverPanel>(gameOverArgs);
    }

    private void SaveGameData()
    {
        GameData data = new GameData()
        {
            computerLeftIntegration = integrationModel.ComputerLeftIntegration,
            computerRightIntegration = integrationModel.ComputerRightIntegration,
            playerIntegration = integrationModel.PlayerIntegration,
        };
        dataManager.SaveData(data);
    }

    public bool IsGameOver(CharacterType characterType)
    {
        Player player = characterPanel.GetCharacterControl(characterType);
        return !player.HasCard;
    }
    #endregion

    #region Card Count Management
    /// <summary>
    /// ��ȡָ����ɫ��ʣ������
    /// </summary>
    public int GetPlayerCardCount(CharacterType characterType)
    {
        var player = characterPanel.GetCharacterControl(characterType);
        return player != null ? player.cardList.Count : 0;
    }

    /// <summary>
    /// ��ȡ������ҵ�ʣ������
    /// </summary>
    public Dictionary<CharacterType, int> GetAllPlayersCardCount()
    {
        return new Dictionary<CharacterType, int>
        {
            { CharacterType.HostPlayer, GetPlayerCardCount(CharacterType.HostPlayer) },
            { CharacterType.LeftPlayer, GetPlayerCardCount(CharacterType.LeftPlayer) },
            { CharacterType.RightPlayer, GetPlayerCardCount(CharacterType.RightPlayer) }
        };
    }
    #endregion

    #region Game State Tracking
    public void HandlePass(CharacterType characterType)
    {
        StateTracker.RecordPass(characterType);
        AudioManager.Instance.PlaySoundEffect(Music.PassCard);
        roundModel.NextTurn();
    }
    #endregion

    #region Game State Helper Methods
    /// <summary>
    /// ��ȡ��Ϸ��ǰ�غ���
    /// </summary>

    /// <summary>
    /// ��ȡ������ɫ
    /// </summary>
    public CharacterType GetLandlordCharacter() => theLandlord;

    /// <summary>
    /// ���ָ������Ƿ��ǵ���
    /// </summary>
    public bool IsLandlord(CharacterType characterType) => characterType == theLandlord;

    /// <summary>
    /// ��ȡ��ǰ��������Ϸ״̬
    /// </summary>
    public ComputerSmartArgs GetCurrentGameState()
    {
        return new ComputerSmartArgs
        {
            // ����������Ϣ��RoundModelά��
            PlayCardArgs = new PlayCardArgs
            {
                CardType = roundModel.CurrentType,
                CharacterType = roundModel.CurrentCharacter,
                Length = roundModel.CurrentLength,
                Weight = roundModel.CurrentWeight,
            },
            // ��ɫ��Ϣ
            BiggestCharacter = roundModel.BiggestCharacter,
        };
    }
    #endregion
    // Additional methods as needed...
}