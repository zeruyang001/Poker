using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
        #region UI Components
    // 个人信息UI组件
    private Image playerAvatar; // 玩家头像
    private Text playerNameText; // 玩家名字
    private Text playerIdText;  // 玩家ID
    private Text scoreText;    // 积分
    private Text beanText;     // 游戏豆
    
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

    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        InitializeComponents();
        SetupListeners();
        LoadDefaultAvatar();
        FetchAndDisplayPlayerInfo();
        RefreshRoomList();
    }
    private void InitializeComponents()
    {
        // 初始化个人信息UI组件
        Transform playerInfoPanel = transform.Find("PlayerInfoPanel");
        playerAvatar = playerInfoPanel.Find("Avatar").GetComponent<Image>();
        playerNameText = playerInfoPanel.Find("NameText").GetComponent<Text>();
        playerIdText = playerInfoPanel.Find("ID/IdText").GetComponent<Text>();
        scoreText = playerInfoPanel.Find("Score/ScoreText").GetComponent<Text>();
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

        private void SetupListeners()
    {
        NetManager.AddMsgListener("MsgGetBasePlayerInfo", OnMsgGetBasePlayerInfo);
        NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);

        // 按钮点击事件
        createButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnCreateClick();
        });

        refreshButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnRefreshClick();
        });

        singlePlayerButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnSinglePlayerClick();
        });
    }

    public override void OnShow(params object[] para)
    {
        //Ѱ�����
        playerIdText.text = GameManager.id;

        NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);

        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);

        // Ϊ��ť���ӵ����Ч
        createButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnCreateClick();
        });

        // Ϊ��ť���ӵ����Ч
        refreshButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnRefreshClick();
        });

        // Ϊ��ť���ӵ����Ч
        singlePlayerButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnSinglePlayerClick();
        });
    }
    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.RemoveMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.RemoveMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.RemoveMsgListener("MsgGetRoomList", OnMsgGetRoomList);
    }
    public void OnCreateClick()
    {
        MsgCreateRoom msg = new MsgCreateRoom();
        NetManager.Send(msg);
    }
    public void OnRefreshClick()
    {
        MsgGetRoomList msg = new MsgGetRoomList();
        NetManager.Send(msg);
    }
    private void OnMsgGetPlayerInfo(MsgBase msgBase)
    {
        MsgGetBasePlayerInfo msg = new MsgGetBasePlayerInfo();
        if (msg != null)
        {
            // 更新UI显示
            playerNameText.text = msg.playerName;
            playerIdText.text = $"ID: {msg.playerId}";
            scoreText.text = $"积分: {msg.score}";
            beanText.text = $"游戏豆: {msg.beans}";

            // 处理头像
            if (!string.IsNullOrEmpty(msg.avatarUrl))
            {
                // TODO: 加载服务器头像的逻辑
                StartCoroutine(LoadAvatarFromUrl(msg.avatarUrl));
            }
        }
    }

    public void OnMsgGetAchieve(MsgBase msgBase)
    {
        MsgGetAchieve msg = msgBase as MsgGetAchieve;
        beanText.text = msg.bean.ToString();
    }
    public void OnMsgCreateRoom(MsgBase msgBase)
    {
        MsgCreateRoom msg = msgBase as MsgCreateRoom;
        if (msg.result)
        {
            PanelManager.Open<TipPanel>("�����ɹ�");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("����ʧ��");
        }
    }
    public void OnMsgGetRoomList(MsgBase msgBase)
    {
        MsgGetRoomList msg = msgBase as MsgGetRoomList;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        if (msg.rooms == null)
            return;
        for (int i = 0; i < msg.rooms.Length; i++)
        {
            GenerateRoom(msg.rooms[i]);
        }

    }
    public void OnMsgEnterRoom(MsgBase msgBase)
    {
        MsgEnterRoom msg = msgBase as MsgEnterRoom;
        if (msg.result)
        {
            Debug.Log("Attempting to open RoomListPanel");
            PanelManager.Open<RoomPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("���뷿��ʧ��");
        }
    }

    public void GenerateRoom(RoomInfo roomInfo)
    {
        GameObject o = Instantiate(roomObj);
        o.transform.SetParent(content);
        o.SetActive(true);
        o.transform.localScale = Vector3.one;


        Transform trans = o.transform;
        roomIdText=trans.Find("IdText").GetComponent<Text>();
        Text countText = trans.Find("CountText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Button joinButton = trans.Find("JoinButton").GetComponent<Button>();

        roomIdText.text = roomInfo.id.ToString();
        countText.text = roomInfo.count.ToString();
        if (roomInfo.isPrepare)
        {
            statusText.text = "׼����";
        }
        else
        {
            statusText.text = "�ѿ�ʼ";
        }
        joinButton.onClick.AddListener(OnJoinClick);
    }
    public void OnJoinClick()
    {
        MsgEnterRoom msgEnterRoom = new MsgEnterRoom();
        msgEnterRoom.id = int.Parse(roomIdText.text);
        NetManager.Send(msgEnterRoom);
    }

    private void OnSinglePlayerClick()
    {
        Close();
        // ֱ�ӽ��뵥��ģʽ
        GameManager.Instance.StartSinglePlayerGame();
        
    }
}
