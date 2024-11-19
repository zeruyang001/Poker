using UnityEngine;
using UnityEngine.UI;

public class RegisterPanel : BasePanel
{
    private InputField idInput;
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
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
        {
            ShowTip("�û��������벻��Ϊ��");
            return;
        }
        if (pwInput.text != repInput.text)
        {
            ShowTip("�������벻һ��");
            return;
        }

        MsgRegister msg = new MsgRegister
        {
            id = idInput.text,
            pw = pwInput.text
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
        ShowTip(msg.result ? "ע��ɹ�" : "ע��ʧ��");
        if (msg.result)
        {
            Close();
        }
    }

    private void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }
}