using UnityEngine;
using UnityEngine.UI;
using System;

public class RegisterPanel : BasePanel
{
    private InputField idInput;
    private InputField nicknameInput; // �����ǳ������
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
        nicknameInput = gameObject.transform.Find("NicknameInput").GetComponent<InputField>(); // �����ǳ���������
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
        // ������
        if(string.IsNullOrEmpty(idInput.text) || 
           string.IsNullOrEmpty(pwInput.text) ||
           string.IsNullOrEmpty(nicknameInput.text))
        {
            ShowTip("�û�����������ǳƲ���Ϊ��");
            return;
        }

        if(pwInput.text != repInput.text)
        {
            ShowTip("�������벻һ��");
            return;
        }

        // ����ע����Ϣ,�����ǳ��ֶ�
        MsgRegister msg = new MsgRegister
        {
            id = idInput.text,
            pw = pwInput.text,
            nickname = nicknameInput.text, // �����ǳ��ֶ�
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
            ShowTip("ע��ɹ�");
            AudioManager.Instance.PlaySoundEffect(Music.Ok);
            Close();
        }
        else 
        {
            ShowTip(msg.error_msg ?? "ע��ʧ��"); // ��ʾ���������Ϣ
            AudioManager.Instance.PlaySoundEffect(Music.Disable);
        }
    }

    private void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }
}