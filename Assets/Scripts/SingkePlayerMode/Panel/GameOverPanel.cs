using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    [SerializeField] private Image showImg;
    [SerializeField] private List<Sprite> showList;
    [SerializeField] private Button restartButton;
    private SingleGameManager singleGameManager;

    public override void OnInit()
    {
        layer = PanelManager.Layer.Panel;
    }

    public override void OnShow(params object[] para)
    {
        if (showImg == null || restartButton == null)
        {
            Debug.LogError("Failed to find required components in GameOverPanel!");
            return;
        }

        restartButton.onClick.AddListener(OnRestartClick);
        singleGameManager = SingleGameManager.Instance;

        if (para.Length > 0 && para[0] is GameOverArgs gameOverArgs)
        {
            SetupDisplay(gameOverArgs.isLandlord, gameOverArgs.PlayerWin);
        }
        else
        {
            Debug.LogError("Invalid parameters passed to GameOverPanel!");
        }
        
    }

    private void SetupDisplay(bool isLandlord, bool isWin)
    {
        AudioManager.Instance.StopBackgroundMusic();
        if (showList.Count < 4)
        {
            Debug.LogError("Not enough sprites in showList!");
            return;
        }
        string music = isWin ? Music.Win : Music.Lose;
        AudioManager.Instance.PlaySoundEffect(music);
        if (isLandlord)
        {
            showImg.sprite = isWin ? showList[0] : showList[1];
        }
        else
        {
            showImg.sprite = isWin ? showList[2] : showList[3];
        }
    }

    private void OnRestartClick()
    {
        singleGameManager.InitializeGame();
        Close();
    }

    public override void OnClose()
    {
        base.OnClose();
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(OnRestartClick);
        }
    }
}