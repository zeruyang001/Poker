using UnityEngine;
using System;

public static class NetManager
{
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }

    public delegate void EventListener(string err);
    public delegate void MsgListener(MsgBase msgBase);

#if UNITY_WEBGL && !UNITY_EDITOR
    private static WebGLNetManager Instance => WebGLNetManager.Instance;
#else
    private static AndroidNetManager Instance => AndroidNetManager.Instance;
#endif

    public static bool IsConnected => Instance.IsConnected;

    public static void Connect(string ip, int port) => Instance.Connect(ip, port);
    public static void Close() => Instance.Close();
    public static void Send(MsgBase msg) => Instance.Send(msg);

    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Instance.AddEventListener((WebGLNetManager.NetEvent)netEvent, 
            (WebGLNetManager.EventListener)Delegate.CreateDelegate(typeof(WebGLNetManager.EventListener), listener.Target, listener.Method));
#else
        Instance.AddEventListener((AndroidNetManager.NetEvent)netEvent,
            (AndroidNetManager.EventListener)Delegate.CreateDelegate(typeof(AndroidNetManager.EventListener), listener.Target, listener.Method));
#endif
    }

    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Instance.RemoveEventListener((WebGLNetManager.NetEvent)netEvent, 
            (WebGLNetManager.EventListener)Delegate.CreateDelegate(typeof(WebGLNetManager.EventListener), listener.Target, listener.Method));
#else
        Instance.RemoveEventListener((AndroidNetManager.NetEvent)netEvent,
            (AndroidNetManager.EventListener)Delegate.CreateDelegate(typeof(AndroidNetManager.EventListener), listener.Target, listener.Method));
#endif
    }

    public static void AddMsgListener(string msgName, MsgListener listener)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Instance.AddMsgListener(msgName, 
            (WebGLNetManager.MsgListener)Delegate.CreateDelegate(typeof(WebGLNetManager.MsgListener), listener.Target, listener.Method));
#else
        Instance.AddMsgListener(msgName,
            (AndroidNetManager.MsgListener)Delegate.CreateDelegate(typeof(AndroidNetManager.MsgListener), listener.Target, listener.Method));
#endif
    }

    public static void RemoveMsgListener(string msgName, MsgListener listener)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        Instance.RemoveMsgListener(msgName, 
            (WebGLNetManager.MsgListener)Delegate.CreateDelegate(typeof(WebGLNetManager.MsgListener), listener.Target, listener.Method));
#else
        Instance.RemoveMsgListener(msgName,
            (AndroidNetManager.MsgListener)Delegate.CreateDelegate(typeof(AndroidNetManager.MsgListener), listener.Target, listener.Method));
#endif
    }

    public static void Update() => Instance.Update();

    public static void Reconnect() => Instance.Reconnect();
    public static void SaveLastConnection(string ip, int port) => Instance.SaveLastConnection(ip, port);
    public static bool CheckConnection() => Instance.CheckConnection();
}
