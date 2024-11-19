using System.Collections.Generic;
using UnityEngine;

public class PlayerActionDisplayManager : MonoBehaviour
{
    [System.Serializable]
    private class PlayerActionInfo
    {
        public CharacterType characterType;
        public Transform displayPoint;
        public PlayerActionDisplay actionDisplay;
    }

    [SerializeField] private PlayerActionInfo[] playerActionInfos;
    [SerializeField] private PlayerActionDisplay actionDisplayPrefab;
    private Dictionary<CharacterType, PlayerActionInfo> actionInfoDict;

    private void Awake()
    {
        InitializeActionDisplays();
    }

    private void InitializeActionDisplays()
    {
        actionInfoDict = new Dictionary<CharacterType, PlayerActionInfo>();
        foreach (var info in playerActionInfos)
        {
            if (info.actionDisplay == null)
            {
                info.actionDisplay = Instantiate(actionDisplayPrefab, info.displayPoint);
            }
            info.actionDisplay.SetState(PlayerActionState.None); // 初始化时隐藏所有提示
            actionInfoDict[info.characterType] = info;
        }
    }

    public void HideAllPlayerActions()
    {
        foreach (var info in actionInfoDict.Values)
        {
            info.actionDisplay.SetState(PlayerActionState.None);
        }
    }

    public void ShowPlayerAction(CharacterType character, PlayerActionState state)
    {
        if (actionInfoDict.TryGetValue(character, out PlayerActionInfo info))
        {
            info.actionDisplay.SetState(state);
        }
        else
        {
            Debug.LogError($"No action display found for character type: {character}");
        }
    }

    public void HidePlayerAction(CharacterType character)
    {
        if (actionInfoDict.TryGetValue(character, out PlayerActionInfo info))
        {
            info.actionDisplay.SetState(PlayerActionState.None);
        }
    }
}