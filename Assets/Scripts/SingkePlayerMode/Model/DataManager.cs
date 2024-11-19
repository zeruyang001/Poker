using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using UnityEngine;
using System.Threading.Tasks;

public class DataManager
{
    public static string DataPath = Application.persistentDataPath + "/score.xml";
    // 数据路径
    [SerializeField] private string dataPath = DataPath;

    // 异步保存数据
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
            Debug.LogError("保存数据时发生错误: " + e.Message);
        }
    }

    // 外部调用的异步保存数据方法
    public void SaveData(GameData data)
    {
        SaveDataAsync(data).ConfigureAwait(false);
        Debug.Log("储存数据");
    }

    // 获取数据
    public GameData GetData()
    {
        const int maxRetries = 3;
        const int retryDelay = 100; // milliseconds

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // 尝试读取文件的代码
                using (FileStream stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    XmlSerializer xml = new XmlSerializer(typeof(GameData));
                    return (GameData)xml.Deserialize(stream);
                }
            }
            catch (IOException e)
            {
                if (i == maxRetries - 1) // 如果是最后一次尝试
                {
                    Debug.LogError("读取数据时发生错误: " + e.Message);
                    return CreateDefaultGameData();
                }
                System.Threading.Thread.Sleep(retryDelay);
            }
            catch (XmlException e)
            {
                Debug.LogError("XML文件格式错误: " + e.Message);
                return CreateDefaultGameData();
            }
        }

        return CreateDefaultGameData(); // 如果所有尝试都失败
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
