using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanel : BasePanel
{
    private Button startButton;
    private Button prepareButton;
    private Button closeButton;
    private Transform content;
    private GameObject playerObj;


    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }
    public override void OnShow(params object[] para)
    {
        startButton = gameObject.transform.Find("StartButton").GetComponent<Button>();
        prepareButton = gameObject.transform.Find("PrepareButton").GetComponent<Button>();
        closeButton = gameObject.transform.Find("CloseButton").GetComponent<Button>();
        content = gameObject.transform.Find("PlayerList/Scroll View/Viewport/Content");
        playerObj = gameObject.transform.Find("Player").gameObject;


        playerObj.SetActive(false);

        startButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnStartClick();
        });
        closeButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.CancelOrReturn);
            OnCloseClick();
        });
        prepareButton.onClick.AddListener( () => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnPrepareClick();
        });

        NetManager.AddMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.AddMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.AddMsgListener("MsgPrepare", OnMsgPrepare);
        NetManager.AddMsgListener("MsgStartBattle", OnMsgStartBattle);


        MsgGetRoomInfo msgGetRoomInfo = new MsgGetRoomInfo();
        NetManager.Send(msgGetRoomInfo);

        // ���������"����"����
        AudioManager.Instance.PlayRandomBackgroundMusic(Music.BG);
    }

    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgGetRoomInfo", OnMsgGetRoomInfo);
        NetManager.RemoveMsgListener("MsgLeaveRoom", OnMsgLeaveRoom);
        NetManager.RemoveMsgListener("MsgPrepare", OnMsgPrepare);
        NetManager.RemoveMsgListener("MsgStartBattle", OnMsgStartBattle);

        // ֹͣ��������
        AudioManager.Instance.StopBackgroundMusic();
    }
    public void OnStartClick()
    {
        MsgStartBattle msgStartBattle = new MsgStartBattle();
        NetManager.Send(msgStartBattle);
    }
    public void OnPrepareClick()
    {
        MsgPrepare msgPrepare = new MsgPrepare();
        NetManager.Send(msgPrepare);
    }
    public void OnCloseClick()
    {
        MsgLeaveRoom msgLeaveRoom = new MsgLeaveRoom();
        NetManager.Send(msgLeaveRoom);
    }
    public void OnMsgGetRoomInfo(MsgBase msgBase)
    {
        MsgGetRoomInfo msg = msgBase as MsgGetRoomInfo;
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }
        if (msg.playersInfo == null)
            return;
        for (int i = 0; i < msg.playersInfo.Length; i++)
        {
            GeneratePlayerInfo(msg.playersInfo[i]);
        }
    }
    public void GeneratePlayerInfo(PlayerInfo playerInfo)
    {
        GameObject go = Instantiate(playerObj);
        go.transform.SetParent(content);
        go.SetActive(true);
        go.transform.localScale = Vector3.one;

        Transform trans = go.transform;
        Text idText = trans.Find("IdText").GetComponent<Text>();
        Text beanText = trans.Find("BeanText").GetComponent<Text>();
        Text statusText = trans.Find("StatusText").GetComponent<Text>();
        Text hostText = trans.Find("HostText").GetComponent<Text>();

        idText.text = playerInfo.id;
        beanText.text = playerInfo.bean.ToString();
        if (playerInfo.isPrepare)
        {
            statusText.text = "��׼��";
        }
        else
        {
            statusText.text = "δ׼��";
        }
        if (playerInfo.isHost)
        {
            hostText.gameObject.SetActive(true);
        }
        else
        {
            hostText.gameObject.SetActive(false);
        }

        if (playerInfo.id == GameManager.id)
        {
            GameManager.isHost = playerInfo.isHost;
            if (GameManager.isHost)
            {
                startButton.gameObject.SetActive(true);
                prepareButton.gameObject.SetActive(false);
            }
            else
            {
                startButton.gameObject.SetActive(false);
                prepareButton.gameObject.SetActive(true);
            }
        }
    }

    public void OnMsgLeaveRoom(MsgBase msgBase)
    {
        MsgLeaveRoom msg = msgBase as MsgLeaveRoom;
        if (msg.result)
        {
            PanelManager.Open<TipPanel>("�˳�����");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
        else
        {
            PanelManager.Open<TipPanel>("�˳�����ʧ��");
        }
    }

    public void OnMsgPrepare(MsgBase msgBase)
    {
        MsgPrepare msg = msgBase as MsgPrepare;
        if (!msg.isPrepare)
            return;
        MsgGetRoomInfo msgGetRoomInfo = new MsgGetRoomInfo();
        NetManager.Send(msgGetRoomInfo);
    }

    public void OnMsgStartBattle(MsgBase msgBase)
    {
        MsgStartBattle msg = msgBase as MsgStartBattle;
        switch (msg.result)
        {
            case 0:
                PanelManager.Open<BattlePanel>();
                Close();
                break;
            case 1:
                PanelManager.Open<TipPanel>("�����������ˣ��޷���ʼ");
                break;
            case 2:
                PanelManager.Open<TipPanel>("�����δ׼��,�޷���ʼ");
                break;
            case 3:
                PanelManager.Open<TipPanel>("���ڵ�ǰ����");
                break;
            default:
                PanelManager.Open<TipPanel>("δ֪����");
                break;
        }
    }

}
