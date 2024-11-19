using UnityEngine;
using UnityEngine.UI;

public class InteractionPanel : BasePanel
{
    public Button dealButton;
    public Button callButton;
    public Button notCallButton;
    public Button grabButton;
    public Button notGrabButton;
    public Button playButton;
    public Button passButton;
    public Button hintButton;

    private SingleGameManager singleGameManager;

    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
        RoundModel.PlayerHandler += RoundModel_PlayerHandler;
    }

    public override void OnShow(params object[] para)
    {
        FindComponents();
        SetupButtonListeners();
        singleGameManager = SingleGameManager.Instance;
    }

    private void FindComponents()
    {
        dealButton = gameObject.transform.Find("DealButton").GetComponent<Button>();
        callButton = gameObject.transform.Find("CallButton").GetComponent<Button>();
        notCallButton = gameObject.transform.Find("NotCallButton").GetComponent<Button>();
        grabButton = gameObject.transform.Find("GrabButton").GetComponent<Button>();
        notGrabButton = gameObject.transform.Find("NotGrabButton").GetComponent<Button>();
        playButton = gameObject.transform.Find("PlayButton").GetComponent<Button>();
        passButton = gameObject.transform.Find("PassButton").GetComponent<Button>();
        hintButton = gameObject.transform.Find("HintButton").GetComponent<Button>();
    }

    private void SetupButtonListeners()
    {
        dealButton.onClick.AddListener(() => {
            // 播放随机的"背景"音乐;
            AudioManager.Instance.PlayRandomBackgroundMusic(Music.BG);
            AudioManager.Instance.PlaySoundEffect(Music.Dispatch);
            OnDealClick();
        });
        callButton.onClick.AddListener(() => {
  
            OnCallClick();
        });
        notCallButton.onClick.AddListener(() => {
            
            OnNotCallClick();
        });
        grabButton.onClick.AddListener( () => {
            
            OnGrabClick();
        });
        notGrabButton.onClick.AddListener( () => {
            
            OnNotGrabClick();
        });
        playButton.onClick.AddListener( () => {
            OnPlayClick();
        });
        passButton.onClick.AddListener( () => {
            OnPassClick();
        });
        hintButton.onClick.AddListener( () => {
            AudioManager.Instance.PlaySoundEffect(Music.Ok);
            OnHintClick();
        });
    }



    public void DeactivateAll()
    {
        playButton.gameObject.SetActive(false);
        callButton.gameObject.SetActive(false);
        notCallButton.gameObject.SetActive(false);
        grabButton.gameObject.SetActive(false);
        notGrabButton.gameObject.SetActive(false);
        passButton.gameObject.SetActive(false);
        dealButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
    }

    public void ActivateStart()
    {
        DeactivateAll();
        dealButton.gameObject.SetActive(true);
    }

    public void ActivateCallButtons()
    { 
        DeactivateAll();
        callButton.gameObject.SetActive(true);
        notCallButton.gameObject.SetActive(true);

    }
    public void ActivateGrabAndNotGrab()
    {
        DeactivateAll();
        grabButton.gameObject.SetActive(true);
        notGrabButton.gameObject.SetActive(true);
    }

    public void ActivatePlayAndPass(bool isPassActive = true)
    {
        DeactivateAll();
        playButton.gameObject.SetActive(true);
        passButton.gameObject.SetActive(true);
        hintButton.gameObject.SetActive(true);
        passButton.interactable = isPassActive;
    }

    private void OnDealClick()
    {
        singleGameManager.RequestDeal();
        DeactivateAll();
    }

    private void OnCallClick()
    {
        singleGameManager.CallLandlord(true, CharacterType.HostPlayer);
        DeactivateAll();
    }

    private void OnNotCallClick()
    {
        singleGameManager.CallLandlord(false, CharacterType.HostPlayer);
        DeactivateAll();
    }

    private void OnGrabClick()
    {
        DeactivateAll();
        singleGameManager.GrabLandlord(true,CharacterType.HostPlayer);
    }

    private void OnNotGrabClick()
    {
        DeactivateAll();
        singleGameManager.GrabLandlord(false,CharacterType.HostPlayer);
    }

    private void OnPlayClick()
    {
        singleGameManager.RequestPlay();
    }

    public void OnPassClick()
    {
        DeactivateAll();
        singleGameManager.RequestPass();
    }

    private void OnHintClick()
    {
        singleGameManager.RequestHint();
    }

    public void OnCompleteDeal()
    {
        ActivateGrabAndNotGrab();
    }

    public void OnSuccessedPlay()
    {
        DeactivateAll();
    }

    public void InitButton()
    {
        DeactivateAll();
        ActivateStart();
    }

    /// <summary>
    /// 玩家出牌显示的UI
    /// </summary>
    /// <param name="canClick">可以按下</param>
    private void RoundModel_PlayerHandler(ComputerSmartArgs e)
    {
        switch (GameManager.gameState)
        {
            case GameState.Calling:
                ActivateCallButtons();
                break;
            case GameState.Grabbing:
                ActivateGrabAndNotGrab();
                break;
            case GameState.Playing:
                ActivatePlayAndPass(e.BiggestCharacter != CharacterType.HostPlayer);
                break;
                // 可以根据需要添加其他状态的处理
        }
    }
    public override void OnClose()
    {
        // 停止背景音乐
        AudioManager.Instance.StopBackgroundMusic();
        RoundModel.PlayerHandler -= RoundModel_PlayerHandler;
        base.OnClose();
    }


}