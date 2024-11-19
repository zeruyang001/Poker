using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoginPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginButton;
    private Button registerButton;
    private GameObject loadingIndicator;
    private Toggle rememberMeToggle;

    private const float LOGIN_TIMEOUT = 10f; // 10�볬ʱ
    private float loginStartTime;

    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        idInput = gameObject.transform.Find("IdInput").GetComponent<InputField>();
        pwInput = gameObject.transform.Find("PwInput").GetComponent<InputField>();
        loginButton = gameObject.transform.Find("LoginButton").GetComponent<Button>();
        registerButton = gameObject.transform.Find("RegisterButton").GetComponent<Button>();
        loadingIndicator = gameObject.transform.Find("LoadingIndicator").gameObject;
        rememberMeToggle = gameObject.transform.Find("RememberMeToggle").GetComponent<Toggle>();
        rememberMeToggle.isOn = true;

        NetManager.AddEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.AddEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.AddMsgListener("MsgLogin", OnMsgLogin);

        LoadSavedCredentials();
        SetLoadingIndicatorActive(false);

        // ���ŵ�¼���汳������
        AudioManager.Instance.PlayBackgroundMusic(Music.Welcome);

        // Ϊ��ť��ӵ����Ч
        loginButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Enter);
            OnLoginClick();
        });
        registerButton.onClick.AddListener(() => {
            AudioManager.Instance.PlaySoundEffect(Music.Gain);
            OnRegisterClick();
        });

    }
    private void LoadSavedCredentials()
    {
        idInput.text = PlayerPrefs.GetString("LastLoginId", "");
        pwInput.text = PlayerPrefs.GetString("LastLoginPw", "");
        rememberMeToggle.isOn = PlayerPrefs.GetInt("RememberMe", 0) == 1;
    }

    private void SaveCredentials()
    {
        if (rememberMeToggle.isOn)
        {
            PlayerPrefs.SetString("LastLoginId", idInput.text);
            PlayerPrefs.SetString("LastLoginPw", pwInput.text);
            PlayerPrefs.SetInt("RememberMe", 1);
        }
        else
        {
            PlayerPrefs.DeleteKey("LastLoginId");
            PlayerPrefs.DeleteKey("LastLoginPw");
            PlayerPrefs.SetInt("RememberMe", 0);
        }
        PlayerPrefs.Save();
    }
    public override void OnClose()
    {
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
        // ֹͣ��������
        AudioManager.Instance.StopBackgroundMusic();
    }

    private void OnLoginClick()
    {
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
        {
            ShowTip("�û��������벻��Ϊ��");
            return;
        }

        SetLoadingIndicatorActive(true);
        NetManager.Connect("127.0.0.1", 8888);
        loginStartTime = Time.time;
        StartCoroutine(CheckLoginTimeout());
    }

    private IEnumerator CheckLoginTimeout()
    {
        while (Time.time - loginStartTime < LOGIN_TIMEOUT)
        {
            yield return null;
        }

        if (NetManager.IsConnected)
        {
            Debug.LogWarning("��¼��ʱ��δ�յ���Ӧ");
            SetLoadingIndicatorActive(false);
            ShowTip("��¼��ʱ��������");
        }
    }

    private void OnRegisterClick()
    {
        PanelManager.Open<RegisterPanel>();
    }

    private void OnMsgLogin(MsgBase msgBase)
    {
        Debug.Log("OnMsgLogin called");
        SetLoadingIndicatorActive(false);
        MsgLogin msg = msgBase as MsgLogin;
        Debug.Log($"Login result: {msg.result}");
        if (msg.result)
        {
            SaveCredentials();
            ShowTip("��¼�ɹ�");

            // ȷ�� GameManager ʵ������
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetupPlayers(msg.id, "", ""); // Set up current player, left and right IDs will be set later
            }
            else
            {
                Debug.LogError("GameManager instance not found!");
            }

            PanelManager.Open<RoomListPanel>();
            Close();
        }
        else
        {
            ShowTip("��¼ʧ�ܣ������û���������");
        }
    }

    private void OnConnectSucc(string err)
    {
        Debug.Log("���ӳɹ������ڵ�¼...");
        MsgLogin msgLogin = new MsgLogin { id = idInput.text, pw = pwInput.text };
        NetManager.Send(msgLogin);
    }

    private void OnConnectFail(string err)
    {
        SetLoadingIndicatorActive(false);
        ShowTip($"����ʧ��: {err}");
    }

    private void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }

    private void SetLoadingIndicatorActive(bool active)
    {
        loadingIndicator.SetActive(active);
        loginButton.interactable = !active;
        registerButton.interactable = !active;
    }
}