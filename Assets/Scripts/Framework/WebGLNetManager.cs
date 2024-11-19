using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class WebGLNetManager
{
    private static WebGLNetManager _instance;
    public static WebGLNetManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WebGLNetManager();
            }
            return _instance;
        }
    }

    private ByteArray byteArray;
    private Queue<ByteArray> writeQueue;
    private bool isConnecting;
    private bool isClosing;
    private List<MsgBase> msgList = new List<MsgBase>();
    private int msgCount = 0;
    private const int processMsgCount = 10;

    public bool isUsePing = true;
    public int pingInterval = 30;
    private float lastPingTime = 0;
    private float lastPongTime = 0;

    private bool isWebSocketConnected = false;
    public bool IsConnected => isWebSocketConnected;

    [DllImport("__Internal")]
    private static extern void WebSocketConnect(string url);
    [DllImport("__Internal")]
    private static extern void WebSocketSend(string message);
    [DllImport("__Internal")]
    private static extern void WebSocketClose();

    private const int MAX_RECONNECT_ATTEMPTS = 3;
    private int reconnectAttempts = 0;
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    public delegate void EventListener(string err);
    public delegate void MsgListener(MsgBase msgBase);
    private Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    private Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    private string lastConnectedIp;
    private int lastConnectedPort;

    private WebGLNetManager() { }

    public void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    public void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }

    private void FireEvent(NetEvent netEvent, string err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    public void AddMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
        }
    }

    public void RemoveMsgListener(string msgName, MsgListener listener)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if (msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    private void FireMsg(string msgName, MsgBase msgBase)
    {
        if (msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }
    }

    public void Connect(string ip, int port)
    {
        lastConnectedIp = ip;
        lastConnectedPort = port;
        if (isConnecting)
        {
            Debug.Log("���������У������ظ�����");
            return;
        }

        Init();
        isConnecting = true;

        string url = $"ws://{ip}:{port}";
        WebSocketConnect(url);

        Debug.Log($"��ʼ���ӵ������� {ip}:{port}");
    }

    private void Init()
    {
        byteArray = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
        msgList.Clear();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!msgListeners.ContainsKey("MsgPong"))
            AddMsgListener("MsgPong", OnMsgPong);
    }

    public void Close()
    {
        if (isConnecting) return;

        if (writeQueue.Count > 0)
            isClosing = true;
        else
        {
            WebSocketClose();
        }
    }

    public void Send(MsgBase msg)
    {
        if (!isWebSocketConnected)
        {
            Debug.LogWarning("WebSocketδ���ӣ�������������...");
            Connect(lastConnectedIp, lastConnectedPort);
            return;
        }
        if (isConnecting || isClosing) return;

        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1]  = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        string sendString = Convert.ToBase64String(sendBytes);
        WebSocketSend(sendString);
        Debug.Log("��ʼ���͵������� "+ msg.protoName);
    }

    private void OnReceiveData()
    {
        // ȷ�������������ֽڱ�ʾ��Ϣ����
        if (byteArray.Length <= 2) return;

        int readIndex = byteArray.readIndex;
        byte[] bytes = byteArray.bytes;

        // ������Ϣ�峤��
        short bodyLength = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        Debug.Log($"��Ϣ�峤��: {bodyLength}");
        if (byteArray.Length < bodyLength + 2) return;

        byteArray.readIndex += 2;

        // ����Э����
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);
        Debug.Log($"��������Э����: {protoName}");

        if (string.IsNullOrEmpty(protoName))
        {
            Debug.LogError("����Э����ʧ��");
            return;
        }

        byteArray.readIndex += nameCount;

        // ������Ϣ��
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyCount);

        if (msgBase != null)
        {
            byteArray.readIndex += bodyCount;
            byteArray.MoveBytes();

            lock (msgList)
            {
                msgList.Add(msgBase);
            }
            msgCount++;

            Debug.Log($"���յ���Ϣ: {protoName}");
        }
        else
        {
            Debug.LogError($"������Ϣʧ��: {protoName}");
        }

        // ����������ݣ���������
        if (byteArray.Length > 2)
        {
            OnReceiveData();
        }
    }

    private void MsgUpdate()
    {
        if (msgCount == 0)
            return;
        for (int i = 0; i < processMsgCount; i++)
        {
            MsgBase msgBase = null;
            lock (msgList)
            {
                if (msgCount > 0)
                {
                    msgBase = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            if (msgBase != null)
            {
                FireMsg(msgBase.protoName, msgBase);
            }
            else
            {
                break;
            }
        }
    }

    private void PingUpdate()
    {
        if (!isUsePing)
            return;
        if (Time.time - lastPingTime > pingInterval)
        {
            MsgPing msg = new MsgPing();
            Send(msg);
            lastPingTime = Time.time;
        }
        if (Time.time - lastPongTime > pingInterval * 4)
        {
            Close();
        }
    }

    public void Update()
    {
        MsgUpdate();
        PingUpdate();
    }

    private void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }

    public void SaveLastConnection(string ip, int port)
    {
        PlayerPrefs.SetString("LastIP", ip);
        PlayerPrefs.SetInt("LastPort", port);
        PlayerPrefs.Save();
    }

    public bool CheckConnection()
    {
        if (!isWebSocketConnected)
        {
            Debug.LogWarning("Connection lost. Attempting to reconnect...");
            string lastIP = PlayerPrefs.GetString("LastIP", "127.0.0.1");
            int lastPort = PlayerPrefs.GetInt("LastPort", 8889);
            Connect(lastIP, lastPort);
            return false;
        }
        return true;
    }

    public void Reconnect()
    {
        Close();
        string lastIP = PlayerPrefs.GetString("LastIP", "127.0.0.1");
        int lastPort = PlayerPrefs.GetInt("LastPort", 8889);
        Connect(lastIP, lastPort);
    }

    public void OnWebSocketOpen()
    {
        Debug.Log("WebSocket ���ӳɹ�");
        isConnecting = false;
        isWebSocketConnected = true;
        FireEvent(NetEvent.ConnectSucc, "");
    }

    public void OnWebSocketMessage(string msg)
    {
        Debug.Log($"�յ�WebSocket��Ϣ: {msg.Substring(0, Math.Min(msg.Length, 100))}..."); // ֻ��ӡǰ100���ַ�
        try
        {
            byte[] bytes = Convert.FromBase64String(msg);
            byteArray.WriteBytes(bytes);
            OnReceiveData();
        }
        catch (Exception e)
        {
            Debug.LogError($"WebSocket ��Ϣ����ʧ��: {e.Message}");
        }
    }

    private bool IsBase64String(string s)
    {
        s = s.Trim();
        return (s.Length % 4 == 0) && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", System.Text.RegularExpressions.RegexOptions.None);
    }

    public void OnWebSocketClose(string reason = "Unknown reason")
    {
        Debug.LogWarning($"WebSocket���ӹر�: {reason}");
        isWebSocketConnected = false;

        if (reconnectAttempts < MAX_RECONNECT_ATTEMPTS)
        {
            reconnectAttempts++;
            Debug.Log($"������������ (���� {reconnectAttempts}/{MAX_RECONNECT_ATTEMPTS})");
            Connect(lastConnectedIp, lastConnectedPort);
        }
        else
        {
            FireEvent(NetEvent.Close, reason);
        }
    }

    public void OnWebSocketError(string error)
    {
        Debug.LogError($"WebSocket ����: {error}");
        FireEvent(NetEvent.ConnectFail, error);
    }
}
