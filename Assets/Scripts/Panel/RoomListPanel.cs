using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;

public class RoomListPanel : BasePanel
{
    #region UI Components
    // 个人信息UI组件
    private Image playerAvatar;
    private Text playerNameText;
    private Text playerIdText;
    private Text pointText;
    private Text beanText;

    // 房间列表UI组件
    private Button createButton;
    private Button refreshButton;
    private Button matchButton;
    private Button singlePlayerButton;
    private Transform content;
    private GameObject roomObj;
    private Text roomIdText;
    #endregion

    #region Default Avatar
    private static readonly string DEFAULT_AVATAR_PATH = "Avatars/default_avatar"; // 默认头像路径
    private Sprite defaultAvatarSprite;
    #endregion

    #region  Unity Lifecycle
    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        // 1. 初始化UI组件
        InitializeComponents();

        // 2. 加载默认头像
        LoadDefaultAvatar();

        // 3. 设置事件监听
        SetupListeners();

        // 4. 开始异步初始化
        _ = InitializeAsync();

        // 5. 刷新房间列表
        OnRefreshClick();

        // 播放背景音乐
        AudioManager.Instance.PlayRandomBackgroundMusic(Music.BG);
    }

    // 新增: 异步初始化方法
    private async Task InitializeAsync()
    {
        try
        {
            // 初始化玩家数据
            await InitializePlayerData();

            // 刷新房间列表
            OnRefreshClick();
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化失败: {e.Message}");
            ShowTip("初始化失败，请重试");
        }
    }

    private async Task InitializePlayerData()
    {
        // 初始化PlayerDataManager
        await PlayerDataManager.Instance.Initialize();

        // 首次更新UI显示
        UpdateUIDisplay(PlayerDataManager.Instance.LocalData);

        // 如果有头像URL，加载头像
        if (!string.IsNullOrEmpty(PlayerDataManager.Instance.LocalData.avatarUrl))
        {
            StartCoroutine(LoadAvatarFromUrl(PlayerDataManager.Instance.LocalData.avatarUrl));
        }

        // 注册数据更新事件
        PlayerDataManager.Instance.OnDataUpdated += UpdateUIDisplay;
    }

    public override void OnClose()
    {
        // 移除数据更新监听
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataUpdated -= UpdateUIDisplay;
        }

        // 移除网络消息监听
        NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);

        // 停止背景音乐
        AudioManager.Instance.StopBackgroundMusic();
    }
    #endregion

    #region Initialization Methods
    private void InitializeComponents()
    {
        // 初始化个人信息UI组件
        Transform playerInfoPanel = transform.Find("PlayerInfoPanel");
        playerAvatar = playerInfoPanel.Find("Avatar").GetComponent<Image>();
        playerNameText = playerInfoPanel.Find("NameText").GetComponent<Text>();
        playerIdText = playerInfoPanel.Find("ID/IdText").GetComponent<Text>();
        pointText = playerInfoPanel.Find("Point/PointText").GetComponent<Text>();
        beanText = playerInfoPanel.Find("Bean/BeanText").GetComponent<Text>();

        // 初始化房间列表UI组件
        createButton = transform.Find("Create/CreateButton").GetComponent<Button>();
        refreshButton = transform.Find("Create/RefreshButton").GetComponent<Button>();
        matchButton = transform.Find("Create/MatchButton").GetComponent<Button>();
        singlePlayerButton = transform.Find("Create/SinglePlayerButton").GetComponent<Button>();
        content = transform.Find("RoomList/Scroll View/Viewport/Content");
        roomObj = transform.Find("Room").gameObject;
        roomObj.SetActive(false);
    }
    private void LoadDefaultAvatar()
    {
        defaultAvatarSprite = Resources.Load<Sprite>(DEFAULT_AVATAR_PATH);
        if (defaultAvatarSprite == null)
        {
            Debug.LogWarning("无法加载默认头像!");
        }
        // 先设置默认头像
        if (playerAvatar != null && defaultAvatarSprite != null)
        {
            playerAvatar.sprite = defaultAvatarSprite;
        }
    }

    private void SetupListeners()
    {
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);

        // 按钮点击事件
        createButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnCreateClick();
        });

        refreshButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnRefreshClick();
        });

        singlePlayerButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnSinglePlayerClick();
        });
        matchButton.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnMatchClick();
        });
    }
    #endregion


    #region UI Update Methods
    private void UpdateUIDisplay(PlayerDataManager.PlayerData data)
    {
        if (data == null) return;
        playerNameText.text = data.playerName;
        playerIdText.text = data.playerId;
        pointText.text = data.point.ToString();
        beanText.text = data.beans.ToString();
    }

    private IEnumerator LoadAvatarFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) yield break;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                if (texture != null && playerAvatar != null)
                {
                    playerAvatar.sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                }
            }
            else
            {
                Debug.LogWarning($"加载头像失败: {www.error}");
                if (defaultAvatarSprite != null && playerAvatar != null)
                {
                    playerAvatar.sprite = defaultAvatarSprite;
                }
            }
        }
    }
    #endregion

    #region Room Management
    public void GenerateRoom(RoomInfo roomInfo)
    {
        GameObject o = Instantiate(roomObj, content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;

        Transform trans = o.transform;
        roomIdText = trans.Find("IdText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Button joinButton = trans.Find("JoinButton").GetComponent<Button>();

        roomIdText.text = roomInfo.id.ToString();
        countText.text = roomInfo.count.ToString();
        statusText.text = roomInfo.isPrepare ? "准备中" : "已开始";

        joinButton.onClick.AddListener(OnJoinClick);
    }

    private void ClearRoomList()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
    }
    #endregion

    #region Network Message Handlers
    public void OnMsgCreateRoom(MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        if (msg.result)
        {
            ShowTip("创建成功");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            ShowTip("创建失败");
        }
    }

    public void OnMsgGetRoomList(MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;

        ClearRoomList();

        if (msg.roomsInfo != null)
        {
            foreach (var room in msg.roomsInfo)
            {
                GenerateRoom(room);
            }
        }
    }

    public void OnMsgEnterRoom(MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        if (msg.result)
        {
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            ShowTip("进入房间失败");
        }
    }
    #endregion

    #region Button Handlers
    private void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }

    private void OnRefreshClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }

    private void OnJoinClick()
    {
        if (int.TryParse(roomIdText.text, out int roomId))
        {
            MsgEnterRoom msg = new MsgEnterRoom { id = roomId };
            NetManager.Send(msg);
        }
        else
        {
            ShowTip("房间号无效");
        }
    }

    // 新增 OnMatchClick 方法
    private void OnMatchClick()
    {
        if (PlayerDataManager.Instance?.LocalData == null)
        {
            ShowTip("玩家数据未初始化");
            return;
        }

        // 检查是否有足够的游戏豆
        if (PlayerDataManager.Instance.LocalData.beans < 1000)
        {
            ShowTip("游戏豆不足1000,无法开始匹配");
            return;
        }

        // 直接打开匹配面板
        PanelManager.Open<MatchRoomPanel>();
    }

    private void OnSinglePlayerClick()
    {
        Close();
        GameManager.Instance.StartSinglePlayerGame();
    }
    #endregion

    #region Helper Methods
    private void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }
    #endregion
}
