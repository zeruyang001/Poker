using UnityEngine;

public class WebSocketHelper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // 这些方法将被JavaScript调用
    public void OnOpen()
    {
        WebGLNetManager.Instance.OnWebSocketOpen();
    }

    public void OnMessage(string msg)
    {
        WebGLNetManager.Instance.OnWebSocketMessage(msg);
    }

    public void OnError(string error)
    {
        WebGLNetManager.Instance.OnWebSocketError(error);
    }

    public void OnClose(string reason = "")
    {
        WebGLNetManager.Instance.OnWebSocketClose(reason);
    }
}