using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class PlayerDataManager : MonoBehaviour 
{
    #region Singleton
    private static PlayerDataManager instance;
    public static PlayerDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("PlayerDataManager");
                instance = go.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    #endregion

    #region Events
    // 数据更新事件
    public event Action<PlayerData> OnDataUpdated;
    public event Action<int> OnBeansChanged;
    public event Action<int> OnScoreChanged;
    #endregion

    #region Data
    [Serializable]
    public class PlayerData
    {
        public string playerId;     // 玩家ID
        public string playerName;   // 玩家名字
        public string avatarUrl;    // 头像URL
        public int point;           // 积分
        public int beans;           // 游戏豆
        public long lastUpdateTime; // 最后更新时间戳
    }

    private PlayerData _localData;
    public PlayerData LocalData => _localData;

    // 数据是否已初始化
    private bool _isInitialized = false;
    // 是否正在同步
    private bool _isSyncing = false;
    // 同步冷却时间(ms)
    private const int SYNC_COOLDOWN = 1000;
    // 上次同步时间
    private long _lastSyncTime = 0;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLocalData();
    }

    private void OnEnable()
    {
        // 注册服务器消息监听
        NetManager.AddMsgListener("MsgGetBasePlayerInfo", OnMsgGetBasePlayerInfo);
        NetManager.AddMsgListener("MsgUpdatePlayerData", OnMsgUpdatePlayerData);
    }

    private void OnDisable()
    {
        // 移除消息监听
        NetManager.RemoveMsgListener("MsgGetBasePlayerInfo", OnMsgGetBasePlayerInfo);
        NetManager.RemoveMsgListener("MsgUpdatePlayerData", OnMsgUpdatePlayerData);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveLocalData();
        }
    }

    private void OnApplicationQuit()
    {
        SaveLocalData();
    }
    #endregion

    #region Public Methods
    public async Task<bool> Initialize()
    {
        if (_isInitialized) return true;

        try
        {
            // 请求服务器数据
            await RequestServerData();
            _isInitialized = true;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化玩家数据失败: {e.Message}");
            return false;
        }
    }

    // 更新beans并同步到服务器
    public async Task UpdateBeans(int newBeans, string reason = "")
    {
        if (_localData == null) return;

        int oldBeans = _localData.beans;
        _localData.beans = newBeans;
        OnBeansChanged?.Invoke(newBeans);

        // 更新本地时间戳
        _localData.lastUpdateTime = GetTimestamp();
        SaveLocalData();

        // 同步到服务器
        await SyncToServer();

        Debug.Log($"Beans更新: {oldBeans} -> {newBeans}, 原因: {reason}");
    }

    // 更新积分并同步到服务器
    public async Task UpdatePoint(int newScore, string reason = "")
    {
        if (_localData == null) return;

        int oldScore = _localData.point;
        _localData.point = newScore;
        OnScoreChanged?.Invoke(newScore);

        // 更新本地时间戳
        _localData.lastUpdateTime = GetTimestamp();
        SaveLocalData();

        // 同步到服务器
        await SyncToServer();

        Debug.Log($"Score更新: {oldScore} -> {newScore}, 原因: {reason}");
    }

    // 强制从服务器刷新数据
    public async Task ForceRefresh()
    {
        await RequestServerData();
    }
    #endregion

    #region Private Methods
    private void LoadLocalData()
    {
        string json = PlayerPrefs.GetString("PlayerData", "");
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                _localData = JsonUtility.FromJson<PlayerData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"加载本地数据失败: {e.Message}");
                _localData = new PlayerData();
            }
        }
        else
        {
            _localData = new PlayerData();
        }
    }

    private void SaveLocalData()
    {
        if (_localData == null) return;

        try
        {
            string json = JsonUtility.ToJson(_localData);
            PlayerPrefs.SetString("PlayerData", json);
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"保存本地数据失败: {e.Message}");
        }
    }

    private async Task RequestServerData()
    {
        MsgGetBasePlayerInfo msg = new MsgGetBasePlayerInfo();
        NetManager.Send(msg);

        // 等待服务器响应
        await Task.Delay(100);
    }

    private async Task SyncToServer()
    {
        if (_isSyncing) return;

        // 检查同步冷却
        long now = GetTimestamp();
        if (now - _lastSyncTime < SYNC_COOLDOWN)
        {
            await Task.Delay(SYNC_COOLDOWN);
        }

        _isSyncing = true;
        try
        {
            // 创建同步消息
            var msg = new MsgUpdatePlayerData
            {
                playerId = _localData.playerId,
                point = _localData.point,
                beans = _localData.beans,
                lastUpdateTime = _localData.lastUpdateTime
            };

            // 发送到服务器
            NetManager.Send(msg);
            _lastSyncTime = GetTimestamp();
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private void OnMsgGetBasePlayerInfo(MsgBase msgBase)
    {
        var msg = msgBase as MsgGetBasePlayerInfo;
        if (msg == null) return;

        bool needUpdate = false;

        // 只有服务器数据比本地新才更新
        if (_localData.lastUpdateTime < msg.lastUpdateTime)
        {
            _localData.playerId = msg.playerId;
            _localData.playerName = msg.playerName;
            _localData.point = msg.point;
            _localData.beans = msg.beans;
            _localData.avatarUrl = msg.avatarUrl;
            _localData.lastUpdateTime = msg.lastUpdateTime;
            needUpdate = true;
        }

        if (needUpdate)
        {
            SaveLocalData();
            OnDataUpdated?.Invoke(_localData);
        }
    }

    private void OnMsgUpdatePlayerData(MsgBase msgBase)
    {
        var msg = msgBase as MsgUpdatePlayerData;
        if (msg == null) return;

        // 处理服务器的响应，可能包含冲突解决
        if (msg.hasConflict)
        {
            // 如果有冲突，以服务器数据为准
            _localData.point = msg.point;
            _localData.beans = msg.beans;
            _localData.lastUpdateTime = msg.lastUpdateTime;
            SaveLocalData();
            OnDataUpdated?.Invoke(_localData);
        }
    }

    private long GetTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    #endregion
}