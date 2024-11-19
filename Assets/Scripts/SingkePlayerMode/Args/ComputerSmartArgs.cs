using System.Collections.Generic;
using static CardManager;

/// <summary>
/// ���ƾ��߲�����
/// </summary>
public class ComputerSmartArgs
{
    #region ������Ϣ
    /// <summary>
    /// �������� ����Ȩ�� ���Ƴ��� ���ƽ�ɫ
    /// </summary>
    public PlayCardArgs PlayCardArgs { get; set; }
    #endregion

    #region ��ɫ��Ϣ
    /// <summary>
    /// ��ǰ����ƵĽ�ɫ
    /// </summary>
    public CharacterType BiggestCharacter { get; set; }

    /// <summary>
    /// ��ɫʣ����������
    /// </summary>
    public int RemainingCards { get; set; }

    /// <summary>
    /// ��ɫ������
    /// </summary>
    public List<Card> PlayCards { get; set; }
    #endregion

    /// <summary>
    /// ���캯��
    /// </summary>
    public ComputerSmartArgs()
    {
        Reset();
    }

    public void Reset()
    {
        PlayCardArgs = new PlayCardArgs
        {
            CardType = default(CardType),
            CharacterType = default(CharacterType),
            Length = 0,
            Weight = 0
        };

        BiggestCharacter = CharacterType.Desk;
        RemainingCards = 0;
        PlayCards = new List<Card>();  // ��ʼ��һ���յ����б�
    }

    public ComputerSmartArgs Clone()
    {
        return new ComputerSmartArgs
        {
            PlayCardArgs = new PlayCardArgs
            {
                CardType = this.PlayCardArgs.CardType,
                CharacterType = this.PlayCardArgs.CharacterType,
                Length = this.PlayCardArgs.Length,
                Weight = this.PlayCardArgs.Weight
            },
            BiggestCharacter = this.BiggestCharacter,
            RemainingCards = this.RemainingCards,
            PlayCards = new List<Card>(this.PlayCards) // ��������б�
        };
    }
}

