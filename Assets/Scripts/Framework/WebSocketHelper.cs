using UnityEngine;

public class WebSocketHelper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    // ��Щ��������JavaScript����
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