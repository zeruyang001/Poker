using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomListPanel : BasePanel
{
    private Text idText;
    private Text beanText;
    private Button createButton;
    private Button refreshButton;
    private Button singlePlayerButton;
    private Transform content;
    private GameObject roomObj;

    private Text roomIdText;
    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        //Ѱ�����
        idText = gameObject.transform.Find("Head/IdText").GetComponent<Text>();
        beanText = gameObject.transform.Find("Head/BeanText").GetComponent<Text>();
        createButton = gameObject.transform.Find("Create/CreateButton").GetComponent<Button>();
        refreshButton = gameObject.transform.Find("Create/RefreshButton").GetComponent<Button>();
        singlePlayerButton = gameObject.transform.Find("Create/SinglePlayerButton").GetComponent<Button>();
        content = gameObject.transform.Find("RoomList/Scroll View/Viewport/Content");
        roomObj = gameObject.transform.Find("Room").gameObject;


        roomObj.SetActive(false);
        idText.text = GameManager.id;

        NetManager.AddMsgListener("MsgGetAchieve", OnMsgGetAchieve);
        NetManager.AddMsgListener("MsgCreateRoom", OnMsgCreateRoom);
        NetManager.AddMsgListener("MsgEnterRoom", OnMsgEnterRoom);
        NetManager.AddMsgListener("MsgGetRoomList", OnMsgGetRoomList);

        MsgGetAchieve msgGetAchieve = new MsgGetAchieve();
        NetManager.Send(msgGetAchieve);
        MsgGetRoomList msgGetRoomList = new MsgGetRoomList();
        NetManager.Send(msgGetRoomList);

        // Ϊ��ť��ӵ����Ч
        createButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnCreateClick();
        });

        // Ϊ��ť��ӵ����Ч
        refreshButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnRefreshClick();
        });

        // Ϊ��ť��ӵ����Ч
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
