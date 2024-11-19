using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 保留原有的公共静态字段
    public static string id = "";
    public static bool isHost;
    public static List<Card> cards = new List<Card>();
    public static List<Card> threeCards = new List<Card>();
    public static PlayerStatus status = PlayerStatus.Call;
    public static string leftId = "";
    public static string rightId = "";
    public static GameObject leftObj;
    public static GameObject rightObj;
    public static GameObject playerObj;
    public static bool isLandLord = false;
    public static GameObject threeCardsObj;
    public static bool isPressing;
    public static List<Card> selectCard = new List<Card>();
    public static bool canNotPlay;

    public static GameState gameState;

    private Transform root;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager instance created");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 新增 SetupPlayers 方法
    public void SetupPlayers(string currentPlayerId, string leftPlayerId, string rightPlayerId)
    {
        id = currentPlayerId;
        leftId = leftPlayerId;
        rightId = rightPlayerId;
    }

    private void Start()
    {
        NetManager.AddEventListener(NetManager.NetEvent.Close, OnConnectClose);
        NetManager.AddMsgListener("MsgKick", OnMsgKick);
        PanelManager.Init();
        PanelManager.Open<LoginPanel>();

        root = GameObject.Find("Root").transform;
    }

    private void Update()
    {
        NetManager.Update();
    }

    public void OnConnectClose(string err)
    {
        PanelManager.Open<TipPanel>("断开连接");
    }

    public void OnMsgKick(MsgBase msgBase)
    {
        root.GetComponent<BasePanel>().Close();
        PanelManager.Open<TipPanel>("被踢下线");
        PanelManager.Open<LoginPanel>();
    }

    public static void SyncDestroy(string id)
    {
        GameObject objToDestroy = null;
        if (leftId == id)
            objToDestroy = leftObj;
        else if (rightId == id)
            objToDestroy = rightObj;
        else if (GameManager.id == id)
            objToDestroy = playerObj;

        if (objToDestroy != null)
        {
            for (int i = objToDestroy.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(objToDestroy.transform.GetChild(i).gameObject);
            }
        }
    }

    public static void SyncGenerate(string id, string name)
    {
        GameObject resource = Resources.Load<GameObject>(name);
        GameObject parentObj = null;

        if (leftId == id)
            parentObj = leftObj;
        else if (rightId == id)
            parentObj = rightObj;
        else if (GameManager.id == id)
            parentObj = playerObj;

        if (parentObj != null)
        {
            GameObject go = Instantiate(resource, Vector3.zero, Quaternion.identity);
            go.transform.SetParent(parentObj.transform, false);
        }
    }

    public static void SyncGenerateCard(string id, string name)
    {
        name = "Card/" + name;
        Sprite sprite = Resources.Load<Sprite>(name);
        GameObject parentObj = null;

        if (leftId == id)
            parentObj = leftObj;
        else if (rightId == id)
            parentObj = rightObj;
        else if (GameManager.id == id)
            parentObj = playerObj;

        if (parentObj != null)
        {
            GameObject go = new GameObject(name);
            Image image = go.AddComponent<Image>();
            image.SetNativeSize();
            go.transform.localScale = new Vector3(0.7f, 0.7f);
            image.sprite = sprite;
            go.transform.SetParent(parentObj.transform, false);
        }
    }

    public static void SyncCardCount(string id, int count)
    {
        GameObject parentObj = null;

        if (leftId == id)
            parentObj = leftObj;
        else if (rightId == id)
            parentObj = rightObj;

        if (parentObj != null)
        {
            Text text = parentObj.transform.parent.Find("CardImage/Text").GetComponent<Text>();
            text.text = (int.Parse(text.text) - count).ToString();
        }
    }

    #region SinglePlayerMode
    public void StartSinglePlayerGame()
    {
        SingleGameManager.Instance.StartSingleGameManager();
    }
    #endregion
}