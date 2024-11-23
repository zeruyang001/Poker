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

    private const float CONNECT_TIMEOUT = 5f; // 5秒超时
    private bool isLoggingIn = false;  // 新增:标记是否正在登录中
    private float loginStartTime;
    private Coroutine loginTimeoutCoroutine; // 新增：存储超时检查协程的引用

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

        // 播放登录界面背景音乐
        AudioManager.Instance.PlayBackgroundMusic(Music.Welcome);

        // 为按钮添加点击音效
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
        // 确保协程被停止
        if(loginTimeoutCoroutine != null)
        {
            StopCoroutine(loginTimeoutCoroutine);
            loginTimeoutCoroutine = null;
        }
        // 只需要清理面板相关的状态
        isLoggingIn = false;
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectSucc, OnConnectSucc);
        NetManager.RemoveEventListener(NetManager.NetEvent.ConnectFail, OnConnectFail);
        NetManager.RemoveMsgListener("MsgLogin", OnMsgLogin);
        AudioManager.Instance.StopBackgroundMusic();
    }

    /// <summary>
    /// 处理登录点击事件
    /// </summary>
    private async void OnLoginClick()
    {
        // 如果正在登录中,直接返回
        if(isLoggingIn)
            return;

        // 输入验证
        if (string.IsNullOrEmpty(idInput.text) || string.IsNullOrEmpty(pwInput.text))
        {
            ShowTip("用户名和密码不能为空");
            return;
        }

        // 确保连接
        if(!await EnsureServerConnection())
            return;

        // 开始登录流程
        StartLogin();
    }
    // 新增:开始登录流程
    private void StartLogin()
    {
        isLoggingIn = true;
        SetLoadingIndicatorActive(true);
        
        // 发送登录消息
        MsgLogin msgLogin = new MsgLogin 
        { 
            id = idInput.text, 
            pw = pwInput.text 
        };
        NetManager.Send(msgLogin);

        loginStartTime = Time.time;
        // 保存协程引用
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
            Debug.LogWarning("登录超时,未收到响应");
            OnLoginFailed("登录超时,请重试");
        }
    }

        // 新增:处理登录失败
    private void OnLoginFailed(string message)
    {
        isLoggingIn = false;
        // 停止超时检查
        if(loginTimeoutCoroutine != null)
        {
            StopCoroutine(loginTimeoutCoroutine);
            loginTimeoutCoroutine = null;
        }
        SetLoadingIndicatorActive(false);
        ShowTip(message);
        NetManager.Close(); // 关闭连接,以便下次重试
    }

    /// <summary>
    /// 处理注册点击事件
    /// </summary>
    private async void OnRegisterClick()
    {
        // 确保连接
        if(!await EnsureServerConnection())
            return;

        // 打开注册面板
        PanelManager.Open<RegisterPanel>();
    }

    private void OnMsgLogin(MsgBase msgBase)
    {
        Debug.Log("OnMsgLogin called");
        SetLoadingIndicatorActive(false);
        MsgLogin msg = msgBase as MsgLogin;
        Debug.Log($"Login result: {msg.result}");

        // 停止超时检查
        if(loginTimeoutCoroutine != null)
        {
            StopCoroutine(loginTimeoutCoroutine);
            loginTimeoutCoroutine = null;
        }

        // 重置登录状态
        isLoggingIn = false;
        
        if (msg.result)
        {
            SaveCredentials();
            ShowTip("登录成功");

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
            // 显示具体的错误信息
            string errorMsg = string.IsNullOrEmpty(msg.errorMsg) ? 
                            "登录失败,请检查用户名和密码" : msg.errorMsg;
            OnLoginFailed(errorMsg);
        }
    }

    private void OnConnectSucc(string err)
    {
        Debug.Log("连接成功，正在登录...");
    }

    private void OnConnectFail(string err)
    {
        SetLoadingIndicatorActive(false);
        ShowTip($"连接失败: {err}");
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
    /// 确保与服务器的连接
    /// </summary>
    private async Task<bool> EnsureServerConnection(bool showLoading = true)
    {
        // 如果已经连接,直接返回true
        if(NetManager.IsConnected)
            return true;

        try
        {
            if(showLoading)
                SetLoadingIndicatorActive(true);

            // 如果之前有连接,先断开
            NetManager.Close();
            
            // 尝试新连接
            NetManager.Connect("127.0.0.1", 8888);

            // 等待连接成功或超时
            float startTime = Time.time;
            while(!NetManager.IsConnected && Time.time - startTime < CONNECT_TIMEOUT)
            {
                await Task.Delay(100);
            }

            if(!NetManager.IsConnected)
            {
                ShowTip("无法连接到服务器,请检查网络后重试");
                return false;
            }

            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"连接服务器失败: {ex.Message}");
            ShowTip("连接服务器时发生错误");
            return false;
        }
        finally
        {
            if(showLoading)
                SetLoadingIndicatorActive(false);
        }
    }
}