using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Threading.Tasks;

public class LoginPanel : BasePanel
{
    private InputField idInput;
    private InputField pwInput;
    private Button loginButton;
    private Button registerButton;
    private GameObject loadingIndicator;
    private Toggle rememberMeToggle;

    private const float CONNECT_TIMEOUT = 5f; // 5�볬ʱ
    private bool isLoggingIn = false;  // ����:����Ƿ����ڵ�¼��
    private float loginStartTime;
    private Coroutine loginTimeoutCoroutine; // �������洢��ʱ���Э�̵�����

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
        // ȷ��Э�̱�ֹͣ
        if(loginTimeoutCoroutine != null)
        {
            StopCoroutine(loginTimeoutCoroutine);
            loginTimeoutCoroutine = null;
        }
        // ֻ��Ҫ���������ص�״̬
        isLoggingIn = false;
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
        AudioManager.Instance.StopBackgroundMusic();
    }

    /// <summary>
    /// �����¼����¼�
    /// </summary>
    private async void OnLoginClick()
    {
        // ������ڵ�¼��,ֱ�ӷ���
        if(isLoggingIn)
            return;

        // ������֤
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
        {
            ShowTip("�û��������벻��Ϊ��");
            return;
        }

        // ȷ������
        if(!await EnsureServerConnection())
            return;

        // ��ʼ��¼����
        StartLogin();
    }
    // ����:��ʼ��¼����
    private void StartLogin()
    {
        isLoggingIn = true;
        SetLoadingIndicatorActive(true);
        
        // ���͵�¼��Ϣ
        MsgLogin msgLogin = new MsgLogin 
        { 
            id = idInput.text, 
            pw = pwInput.text 
        };
        NetManager.Send(msgLogin);

        loginStartTime = Time.time;
        // ����Э������
        loginTimeoutCoroutine = StartCoroutine(CheckLoginTimeout());
    }

    private IEnumerator CheckLoginTimeout()
    {
        while (Time.time - loginStartTime < CONNECT_TIMEOUT)
        {
            yield return null;
        }
        if (isLoggingIn)
        {
            Debug.LogWarning("��¼��ʱ,δ�յ���Ӧ");
            OnLoginFailed("��¼��ʱ,������");
        }
    }

        // ����:�����¼ʧ��
    private void OnLoginFailed(string message)
    {
        isLoggingIn = false;
        // ֹͣ��ʱ���
        if(loginTimeoutCoroutine != null)
        {
            StopCoroutine(loginTimeoutCoroutine);
            loginTimeoutCoroutine = null;
        }
        SetLoadingIndicatorActive(false);
        ShowTip(message);
        NetManager.Close(); // �ر�����,�Ա��´�����
    }

    /// <summary>
    /// ����ע�����¼�
    /// </summary>
    private async void OnRegisterClick()
    {
        // ȷ������
        if(!await EnsureServerConnection())
            return;

        // ��ע�����
        PanelManager.Open<RegisterPanel>();
    }

    private void OnMsgLogin(MsgBase msgBase)
    {
        Debug.Log("OnMsgLogin called");
        SetLoadingIndicatorActive(false);
        MsgLogin msg = msgBase as MsgLogin;
        Debug.Log($"Login result: {msg.result}");

        // ֹͣ��ʱ���
        if(loginTimeoutCoroutine != null)
        {
            StopCoroutine(loginTimeoutCoroutine);
            loginTimeoutCoroutine = null;
        }

        // ���õ�¼״̬
        isLoggingIn = false;
        
        if (msg.result)
        {
            SaveCredentials();
            ShowTip("��¼�ɹ�");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetupPlayers(msg.id, "", "");
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
            // ��ʾ����Ĵ�����Ϣ
            string errorMsg = string.IsNullOrEmpty(msg.errorMsg) ? 
                            "��¼ʧ��,�����û���������" : msg.errorMsg;
            OnLoginFailed(errorMsg);
        }
    }

    private void OnConnectSucc(string err)
    {
        Debug.Log("���ӳɹ������ڵ�¼...");
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

    /// <summary>
    /// ȷ���������������
    /// </summary>
    private async Task<bool> EnsureServerConnection(bool showLoading = true)
    {
        // ����Ѿ�����,ֱ�ӷ���true
        if(NetManager.IsConnected)
            return true;

        try
        {
            if(showLoading)
                SetLoadingIndicatorActive(true);

            // ���֮ǰ������,�ȶϿ�
            NetManager.Close();
            
            // ����������
            NetManager.Connect("127.0.0.1", 8888);

            // �ȴ����ӳɹ���ʱ
            float startTime = Time.time;
            while(!NetManager.IsConnected && Time.time - startTime < CONNECT_TIMEOUT)
            {
                await Task.Delay(100);
            }

            if(!NetManager.IsConnected)
            {
                ShowTip("�޷����ӵ�������,�������������");
                return false;
            }

            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"���ӷ�����ʧ��: {ex.Message}");
            ShowTip("���ӷ�����ʱ��������");
            return false;
        }
        finally
        {
            if(showLoading)
                SetLoadingIndicatorActive(false);
        }
    }
}