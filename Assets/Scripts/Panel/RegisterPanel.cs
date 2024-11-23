using UnityEngine;
using UnityEngine.UI;
using System;

public class RegisterPanel : BasePanel
{
    private InputField idInput;
    private InputField nicknameInput; // 新增昵称输入框
    private InputField pwInput;
    private InputField repInput;
    private Button registerButton;
    private Button closeButton;

    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        FindComponents();
        AddListeners();
        NetManager.AddMsgListener("MsgRegister", OnMsgRegister);
    }

    public override void OnClose()
    {
        NetManager.RemoveMsgListener("MsgRegister", OnMsgRegister);
    }

    private void FindComponents()
    {
        idInput = gameObject.transform.Find("IdInput").GetComponent<InputField>();
        nicknameInput = gameObject.transform.Find("NicknameInput").GetComponent<InputField>(); // 新增昵称输入框查找
        pwInput = gameObject.transform.Find("PwInput").GetComponent<InputField>();
        repInput = gameObject.transform.Find("RepInput").GetComponent<InputField>();
        registerButton = gameObject.transform.Find("RegisterButton").GetComponent<Button>();
        closeButton = gameObject.transform.Find("CloseButton").GetComponent<Button>();
    }

    private void AddListeners()
    {
        registerButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Gain);
            OnRegisterClick();
        });
        closeButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.CancelOrReturn);
            OnCloseClick();
        });
    }

    private void OnRegisterClick()
    {
        // 输入检查
        if(string.IsNullOrEmpty(idInput.text) || 
           string.IsNullOrEmpty(pwInput.text) ||
           string.IsNullOrEmpty(nicknameInput.text))
        {
            ShowTip("用户名、密码和昵称不能为空");
            return;
        }

        if(pwInput.text != repInput.text)
        {
            ShowTip("两次密码不一致");
            return;
        }

        // 发送注册消息,增加昵称字段
        MsgRegister msg = new MsgRegister
        {
            id = idInput.text,
            pw = pwInput.text,
            nickname = nicknameInput.text, // 新增昵称字段
        };

        NetManager.Send(msg); 
    }

    private void OnCloseClick()
    {
        Close();
    }

    private void OnMsgRegister(MsgBase msgBase)
    {
        MsgRegister msg = msgBase as MsgRegister;
        if(msg.result)
        {
            ShowTip("注册成功");
            AudioManager.Instance.PlaySoundEffect(Music.Ok);
            Close();
        }
        else 
        {
            ShowTip(msg.error_msg ?? "注册失败"); // 显示具体错误信息
            AudioManager.Instance.PlaySoundEffect(Music.Disable);
        }
    }

    private void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }
}