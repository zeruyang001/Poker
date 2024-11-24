
using System;
using UnityEngine;
using UnityEngine.UI;

public class MatchRoomPanel : BasePanel
{
    // UI组件
    private Text matchingText;
    private Image loadingIcon;
    private Button startButton;
    private Button cancelButton;
    private float rotateSpeed = 90f; // 每秒旋转90度

    // 匹配状态
    private bool isMatching = false;
    private float matchingTime = 0;
    private const float MAX_MATCH_TIME = 60f; // 最大匹配时间

    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        // 初始化组件
        matchingText = transform.Find("MatchingText").GetComponent<Text>();
        startButton = transform.Find("StartButton").GetComponent<Button>();
        loadingIcon = transform.Find("LoadingIcon").GetComponent<Image>();
        cancelButton = transform.Find("CancelButton").GetComponent<Button>();

        // 设置初始UI状态
        matchingText.text = "点击开始按钮开始匹配";
        loadingIcon.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);

        // 添加事件监听
        startButton.onClick.AddListener(OnStartClick);
        cancelButton.onClick.AddListener(OnCancelClick);
        NetManager.AddMsgListener("MsgMatchGame", OnMsgMatchGame);
    }

    public override void OnClose()
    {
        isMatching = false;
        NetManager.RemoveMsgListener("MsgMatchGame", OnMsgMatchGame);
    }

    private void Update()
    {
        if (isMatching)
        {
            // 更新等待时间显示
            matchingTime += Time.deltaTime;
            matchingText.text = $"正在匹配中...({(int)matchingTime}s)";

            // 旋转加载图标
            loadingIcon.transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);

            // 检查是否超时
            if (matchingTime >= MAX_MATCH_TIME)
            {
                OnMatchTimeout();
            }
        }
    }
    // 开始按钮点击事件
    private void OnStartClick()
    {
        if (!isMatching)
        {
            // 播放点击音效
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            StartMatching();

            // 更新UI状态
            startButton.gameObject.SetActive(false);
            loadingIcon.gameObject.SetActive(true);
        }
    }

    private void StartMatching()
    {
        isMatching = true;
        matchingTime = 0;

        // 发送匹配请求
        MsgMatchGame msg = new MsgMatchGame();
        msg.playerId = PlayerDataManager.Instance.LocalData.playerId;
        NetManager.Send(msg);
    }

    private void OnCancelClick()
    {
        AudioManager.Instance.PlaySoundEffect(Music.CancelOrReturn);
        if (isMatching)
        {
            // TODO: 发送取消匹配消息到服务器
            isMatching = false;
            loadingIcon.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            matchingText.text = "点击开始按钮开始匹配";
        }
        else
        {
            PanelManager.Open<TipPanel>("当前未在匹配中");
            PanelManager.Open<RoomListPanel>();
            Close();
        }
    }

    private void OnMsgMatchGame(MsgBase msgBase)
    {
        MsgMatchGame msg = msgBase as MsgMatchGame;
        if (msg == null) return;

        if (msg.result)
        {
            // 匹配成功
            isMatching = false;
            AudioManager.Instance.PlaySoundEffect(Music.Ok);
            // 打开匹配游戏面板而不是普通房间面板
            //PanelManager.Open<MatchBattlePanel>();
            //PanelManager.Open<MatchActionPanel>();
            Close();
        }
        else
        {
            // 匹配失败
            ShowTip(msg.errorMsg);
            isMatching = false;
            loadingIcon.gameObject.SetActive(false);
            startButton.gameObject.SetActive(true);
            matchingText.text = "点击开始按钮开始匹配";
        }
    }

    private void OnMatchTimeout()
    {
        isMatching = false;
        loadingIcon.gameObject.SetActive(false);
        startButton.gameObject.SetActive(true);
        matchingText.text = "点击开始按钮开始匹配";
        ShowTip("匹配超时,请重试");
    }

    private void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }
}