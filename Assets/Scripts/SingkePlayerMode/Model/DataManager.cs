using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;
using System.Threading.Tasks;

public class DataManager
{
    public static string DataPath = Application.persistentDataPath + "/score.xml";
    // ����·��
    [SerializeField] private string dataPath = DataPath;

    // �첽��������
    public async Task SaveDataAsync(GameData data)
    {
        try
        {
            using (Stream stream = new FileStream(dataPath, FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xml = new XmlSerializer(data.GetType());
                using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8))
                {
                    await Task.Run(() => xml.Serialize(sw, data));
                }
            }
        }
        catch (IOException e)
        {
            Debug.LogError("��������ʱ��������: " + e.Message);
        }
    }

    // �ⲿ���õ��첽�������ݷ���
    public void SaveData(GameData data)
    {
        SaveDataAsync(data).ConfigureAwait(false);
        Debug.Log("��������");
    }

    // ��ȡ����
    public GameData GetData()
    {
        const int maxRetries = 3;
        const int retryDelay = 100; // milliseconds

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // ���Զ�ȡ�ļ��Ĵ���
                using (FileStream stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(GameData));
                    return (GameData)xml.Deserialize(stream);
                }
            }
            catch (IOException e)
            {
                if (i == maxRetries - 1) // ��������һ�γ���
                {
                    Debug.LogError("��ȡ����ʱ��������: " + e.Message);
                    return CreateDefaultGameData();
                }
                System.Threading.Thread.Sleep(retryDelay);
            }
            catch (XmlException e)
            {
                Debug.LogError("XML�ļ���ʽ����: " + e.Message);
                return CreateDefaultGameData();
            }
        }

        return CreateDefaultGameData(); // ������г��Զ�ʧ��
    }

    private GameData CreateDefaultGameData()
    {
        System.Random random = new System.Random();
        return new GameData
        {
            playerIntegration = random.Next(30) * 100,
            computerLeftIntegration = random.Next(30) * 100,
            computerRightIntegration = random.Next(30) * 100
        };
    }
}
