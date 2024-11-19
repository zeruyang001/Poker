public enum CharacterType
{
    HostPlayer,
    RightPlayer,
    LeftPlayer,
    Desk,
    Deck
}

/// <summary>
/// �Ƶ����
/// </summary>
public enum Identity
{
    Farmer,
    Landlord
}

public enum PlayerStatus
{
    Call,
    Rob,
    Play,
    /// <summary>
    /// ���ֽ׶� (11-20����)
    /// </summary>
    Opening,

    /// <summary>
    /// �оֽ׶� (3-10����)
    /// </summary>
    Middle,

    /// <summary>
    /// �оֽ׶� (2���Ƽ�����)
    /// </summary>
    Endgame,
}

public enum GameState
{
    Idle,           // ��Ϸ��ʼ״̬
    Preparing,      // ���׼����
    Matching,       // ƥ�����������
    WaitingForPlayers, // �ȴ�������Ҽ���
    Ready,          // ������Ҷ�׼���ã��ȴ���ʼ
    Dealing,        // ������
    Calling,        // �е����׶�
    Grabbing,       // �������׶�
    Playing,        // ������Ϸ��
    RoundEnd,       // ���ֽ���
    GameOver        // ��Ϸ�����������Ƕ����Ϸ�����ս�����
}

public enum PlayerActionState
{
    None,
    CallLandlord,
    NotCall,
    GrabLandlord,
    NotGrab,
    Double,
}