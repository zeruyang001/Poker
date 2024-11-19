using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntegrationModel
{
    /// <summary>
    /// 底分
    /// </summary>
    public int BasePoint;

    /// <summary>
    /// 倍数
    /// </summary>
    public int Multiple;

    public int InitMultiple = 1;

    private int playerIntegration;

    private int computerLeftIntegration;

    private int computerRightIntegration;


    public event System.Action<int> OnBasePointChanged;
    public event System.Action<int> OnMultipleChanged;
    public static event System.Action<GameData> OnGameDateChanged;



    /// <summary>
    /// 积分
    /// </summary>
    public int Result
    {
        get
        {
            return (Multiple * BasePoint);
        }
    }

    /// <summary>
    /// 玩家持有豆
    /// </summary>
    public int PlayerIntegration
    {
        get
        {
            return playerIntegration;
        }

        set
        {
            if (value < 0)
                playerIntegration = 0;
            else
                playerIntegration = value;
        }
    }

    public int ComputerLeftIntegration
    {
        get
        {
            return computerLeftIntegration;
        }

        set
        {
            if (value < 0)
                computerLeftIntegration = 0;
            else
                computerLeftIntegration = value;
        }
    }

    public int ComputerRightIntegration
    {
        get
        {
            return computerRightIntegration;
        }

        set
        {
            if (value < 0)
                computerRightIntegration = 0;
            else
                computerRightIntegration = value;
        }
    }


    public void Init(GameData gameData)
    {
        InitBasePointAndMultiple();
        UpdateGameDate(gameData);
    }
    
    public void UpdateGameDate(GameData gameData)
    {
        playerIntegration = gameData.playerIntegration;
        computerLeftIntegration = gameData.computerLeftIntegration;
        computerRightIntegration= gameData.computerRightIntegration;
        OnGameDateChanged?.Invoke(gameData);
    }

    public void InitBasePointAndMultiple()
    {
        Multiple = InitMultiple;
        BasePoint = 100;
        OnMultipleChanged?.Invoke(InitMultiple);
    }

    public void UpdateBasePoint(int newBasePoint)
    {
        BasePoint = newBasePoint;
        OnBasePointChanged?.Invoke(BasePoint);
    }

    public void UpdateMultiple(int newMultiple)
    {
        Multiple = newMultiple;
        OnMultipleChanged?.Invoke(Multiple);
    }

    public void DoubleMultiple()
    {
        AudioManager.Instance.PlaySoundEffect(Music.Multiply);
        Multiple *= 2;
        OnMultipleChanged?.Invoke(Multiple);
    }

    internal void GameOver(GameOverArgs gameOverArgs)
    {
        int landlordChange = Result * 2;
        int farmerChange = Result;

        if (gameOverArgs.isLandlord)
        {
            // 玩家是地主
            if (gameOverArgs.PlayerWin)
            {
                PlayerIntegration += landlordChange;
                ComputerLeftIntegration -= farmerChange;
                ComputerRightIntegration -= farmerChange;
            }
            else
            {
                PlayerIntegration -= landlordChange;
                ComputerLeftIntegration += farmerChange;
                ComputerRightIntegration += farmerChange;
            }
        }
        else
        {
            // 玩家是农民
            if (gameOverArgs.PlayerWin)
            {
                PlayerIntegration += farmerChange;
                if (gameOverArgs.ComputerLeftWin)
                {
                    ComputerLeftIntegration += farmerChange;
                    ComputerRightIntegration -= landlordChange;
                }
                else
                {
                    ComputerLeftIntegration -= landlordChange;
                    ComputerRightIntegration += farmerChange;
                }
            }
            else
            {
                PlayerIntegration -= farmerChange;
                if (gameOverArgs.ComputerLeftWin)
                {
                    ComputerLeftIntegration += landlordChange;
                    ComputerRightIntegration -= farmerChange;
                }
                else
                {
                    ComputerLeftIntegration -= farmerChange;
                    ComputerRightIntegration += landlordChange;
                }
            }
        }

        // 确保积分不会变成负数，并为电脑玩家添加"资金补充"机制
        PlayerIntegration = Math.Max(PlayerIntegration, 0);

        System.Random random = new System.Random();

        if (ComputerLeftIntegration <= 0)
        {
            ComputerLeftIntegration = random.Next(20, 31) *100; // 2000 到 3000 之间的随机数
        }
        else
        {
            ComputerLeftIntegration = Math.Max(ComputerLeftIntegration, 0);
        }

        if (ComputerRightIntegration <= 0)
        {
            ComputerRightIntegration = random.Next(20, 31) * 100; // 2000 到 3000 之间的随机数
        }
        else
        {
            ComputerRightIntegration = Math.Max(ComputerRightIntegration, 0);
        }

        GameData gameData = new GameData
        {
            playerIntegration = PlayerIntegration,
            computerRightIntegration = ComputerRightIntegration,
            computerLeftIntegration = ComputerLeftIntegration
        };
        OnGameDateChanged?.Invoke(gameData);
    }
}
