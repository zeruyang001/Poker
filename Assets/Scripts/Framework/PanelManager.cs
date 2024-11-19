using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PanelManager
{
    /// <summary>
    /// �㼶
    /// </summary>
    public enum Layer
    {
        Panel,
        Tip
    }
    /// <summary>
    /// �㼶�б�
    /// </summary>
    private static Dictionary<Layer,Transform> layers=new Dictionary<Layer,Transform>();
    /// <summary>
    /// ����б�
    /// </summary>
    private static Dictionary<string,BasePanel> panels=new Dictionary<string,BasePanel>();
    /// <summary>
    /// ��Ŀ¼
    /// </summary>
    private static Transform root;
    /// <summary>
    /// ����
    /// </summary>
    private static Transform canvas;
    /// <summary>
    /// ��ʼ��
    /// </summary>
    public static void Init()
    {
        root = GameObject.Find("Root").transform;
        canvas = root.Find("Canvas");
        layers.Add(Layer.Panel, canvas.Find("Panel"));
        layers.Add(Layer.Tip, canvas.Find("Tip"));
    }
    /// <summary>
    /// �����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="para"></param>
    public static void Open<T>(params object[] para) where T : BasePanel
    {
        string name = typeof(T).ToString();
        if (panels.ContainsKey(name))
            return;

        // ����Ԥ����
        GameObject panelGo = Resources.Load<GameObject>(name);
        if (panelGo == null)
        {
            Debug.LogError($"Failed to load panel prefab: {name}");
            return;
        }

        // ʵ����Ԥ����
        GameObject instance = GameObject.Instantiate(panelGo);
        T panel = instance.GetComponent<T>();

        if (panel == null)
        {
            Debug.LogError($"Panel component not found on prefab: {name}");
            GameObject.Destroy(instance);
            return;
        }

        panel.OnInit();
        Transform layer = layers[panel.layer];
        instance.transform.SetParent(layer, false);
        panels.Add(name, panel);
        panel.OnShow(para);
    }
    /// <summary>
    /// �ر����
    /// </summary>
    /// <param name="name"></param>
    public static void Close(string name)
    {
        if (!panels.ContainsKey(name))
            return;

        BasePanel panel = panels[name];
        panel.OnClose();
        panels.Remove(name);
        GameObject.Destroy(panel.gameObject);
    }
}
