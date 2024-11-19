using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    public PanelManager.Layer layer = PanelManager.Layer.Panel;

    public virtual void OnInit() { }
    public virtual void OnShow(params object[] para) { }
    public virtual void OnClose() { }

    public void Close()
    {
        string name = GetType().ToString();
        PanelManager.Close(name);
    }

    public void ShowTip(string message)
    {
        PanelManager.Open<TipPanel>(message);
    }
}