using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;

public class AndroidNetManager
{
    private static AndroidNetManager _instance;
    public static AndroidNetManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new AndroidNetManager();
            }
            return _instance;
        }
    }

    private Socket socket;
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

    public bool IsConnected { get; private set; }

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

    private AndroidNetManager() { }

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
        if (IsConnected)
        {
            Debug.LogWarning("已经连接到服务器，无需重复连接");
            return;
        }
        if (isConnecting)
        {
            Debug.LogWarning("正在连接中，请勿重复连接");
            return;
        }
        Init();
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
        Debug.Log($"开始连接到服务器 {ip}:{port}");
    }

    private void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("连接成功");
            isConnecting = false;
            IsConnected = true;
            FireEvent(NetEvent.ConnectSucc, "");
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback, socket);
        }
        catch (SocketException ex)
        {
            Debug.LogError($"连接失败: {ex}");
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
            IsConnected = false;
        }
    }

    private void Init()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        byteArray = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
        IsConnected = false;
        msgList.Clear();
        msgCount = 0;
        lastPingTime = Time.time;
        lastPongTime = Time.time;

        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListener("MsgPong", OnMsgPong);
        }
    }

    public void Close()
    {
        if (socket == null)
            return;

        try
        {
            if (socket.Connected)
            {
                socket.Shutdown(SocketShutdown.Both);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error during socket shutdown: {e.Message}");
        }
        finally
        {
            socket.Close();
            socket = null;
            IsConnected = false;
            FireEvent(NetEvent.Close, "Connection closed");
        }
    }

    public void Send(MsgBase msg)
    {
        if (!IsConnected)
        {
            Debug.LogError("Socket未连接，无法发送消息");
            return;
        }
        if (isConnecting)
        {
            Debug.LogError("正在连接中，无法发送消息");
            return;
        }
        if (isClosing)
        {
            Debug.LogError("正在关闭连接，无法发送消息");
            return;
        }

        byte[] nameBytes = MsgBase.EncodeName(msg);
        byte[] bodyBytes = MsgBase.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[len + 2];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock (writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }

    // SendCallback, ReceiveCallback, OnReceiveData methods remain unchanged
    private void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            if (socket == null || !socket.Connected)
            {
                Debug.LogWarning("Socket is null or not connected in SendCallback");
                return;
            }
            int count = socket.EndSend(ar);

            ByteArray ba = null;
            lock (writeQueue)
            {
                if (writeQueue.Count > 0)
                {
                    ba = writeQueue.First();
                }
            }

            if (ba != null)
            {
                ba.readIndex += count;
                if (ba.Length == 0)
                {
                    lock (writeQueue)
                    {
                        writeQueue.Dequeue();
                        if (writeQueue.Count > 0)
                        {
                            ba = writeQueue.First();
                        }
                        else
                        {
                            ba = null;
                        }
                    }
                }
                if (ba != null)
                {
                    socket.BeginSend(ba.bytes, ba.readIndex, ba.Length, 0, SendCallback, socket);
                }
                else if (isClosing)
                {
                    socket.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"发送消息失败: {ex}");
            Close();
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = ar.AsyncState as Socket;
            if (socket == null || !socket.Connected)
            {
                Debug.LogWarning("Socket is null or not connected in ReceiveCallback");
                Close();
                return;
            }
            int count = socket.EndReceive(ar);
            if (count == 0)
            {
                Close();
                return;
            }
            byteArray.writeIndex += count;
            OnReceiveData();
            if (byteArray.Remain < 8)
            {
                byteArray.MoveBytes();
                byteArray.ReSize(byteArray.Length * 2);
            }
            socket.BeginReceive(byteArray.bytes, byteArray.writeIndex, byteArray.Remain, 0, ReceiveCallback, socket);
        }
        catch (ObjectDisposedException)
        {
            Debug.LogWarning("Attempted to use a disposed socket in ReceiveCallback");
            Close();
        }
        catch (Exception ex)
        {
            Debug.LogError($"接收消息失败: {ex}");
            Close();
        }
    }

    private void OnReceiveData()
    {
        if (byteArray.Length <= 2)
            return;
        int readIndex = byteArray.readIndex;
        byte[] bytes = byteArray.bytes;
        short bodyLength = (short)(bytes[readIndex + 1] * 256 + bytes[readIndex]);
        if (byteArray.Length < bodyLength + 2)
            return;

        byteArray.readIndex += 2;
        int nameCount = 0;
        string protoName = MsgBase.DecodeName(byteArray.bytes, byteArray.readIndex, out nameCount);

        if (protoName == "")
        {
            Debug.LogError("解析协议名失败");
            return;
        }
        byteArray.readIndex += nameCount;
        int bodyCount = bodyLength - nameCount;
        MsgBase msgBase = MsgBase.Decode(protoName, byteArray.bytes, byteArray.readIndex, bodyCount);
        byteArray.readIndex += bodyCount;
        byteArray.MoveBytes();
        lock (msgList)
        {
            msgList.Add(msgBase);
        }
        msgCount++;
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

    // MsgUpdate, PingUpdate, OnMsgPong methods remain unchanged

    public void Reconnect()
    {
        Close();
        Connect(PlayerPrefs.GetString("LastIP", "127.0.0.1"), PlayerPrefs.GetInt("LastPort", 8889));
    }

    public void SaveLastConnection(string ip, int port)
    {
        // 在Android环境下保存上次连接的IP和端口号
        PlayerPrefs.SetString("LastIP", ip);
        PlayerPrefs.SetInt("LastPort", port);
        PlayerPrefs.Save();
    }

    public bool CheckConnection()
    {
        // 在Android环境下检查连接状态,并尝试重新连接
        if (!IsConnected)
        {
            Debug.LogWarning("Connection lost. Attempting to reconnect...");
            string lastIP = PlayerPrefs.GetString("LastIP", "127.0.0.1");
            int lastPort = PlayerPrefs.GetInt("LastPort", 8889);
            Connect(lastIP, lastPort);
            return false;
        }
        return true;
    }
}