namespace AI.Utils
{
    /// <summary>
    /// ��Ϸ�׶�
    /// </summary>
    public enum GamePhase
    {
        /// <summary>
        /// ���ֽ׶� (11-20����)
        /// </summary>
        Opening,

        /// <summary>
        /// �оֽ׶� (5-10����)
        /// </summary>
        Middle,

        /// <summary>
        /// �оֽ׶� (5���Ƽ�����)
        /// </summary>
        Endgame
    }

    public enum StrategyType
    {
        /// <summary>
        /// �������� - ����ʹ�ô���/ը��,׷����ٻ�ʤ
        /// </summary>
        Aggressive,

        /// <summary>
        /// ���ز��� - ���ȳ�С��,�����ؼ���
        /// </summary>  
        Conservative,

        /// <summary>
        /// ���Ʋ��� - �����ƿس��ƽ���,ά�ֳ���Ȩ
        /// </summary>
        Control,

        /// <summary>
        /// ��ϲ��� - �����Эͬ,֧�ֶ��ѳ���
        /// </summary>
        Cooperative
    }

    /// <summary>
    /// ����ǿ��
    /// </summary>
    public enum HandStrength
    {
        /// <summary>
        /// �ǳ��� - �޴���/ը��
        /// </summary>
        VeryWeak,

        /// <summary>
        /// ���� - ����������
        /// </summary>
        Weak,

        /// <summary>
        /// һ�� - ��������
        /// </summary>
        Normal,

        /// <summary>
        /// ��ǿ - ���Ŵ���/��ը��
        /// </summary>
        Strong,

        /// <summary>
        /// �ǳ�ǿ - ���ը��/��ը
        /// </summary>
        VeryStrong
    }

    /// <summary>
    /// ����Ŀ��
    /// </summary>
    public enum PlayPurpose
    {
        /// <summary>
        /// �س� - ���ֳ���Ȩ
        /// </summary>
        Control,

        /// <summary>
        /// ˦�� - ����С��
        /// </summary>
        Discard,

        /// <summary>
        /// ��̽ - ��̽��������
        /// </summary>
        Probe,

        /// <summary>
        /// ѹ�� - ѹ�ƶ��ִ���
        /// </summary>
        Suppress,

        /// <summary>
        /// ��� - ��϶��ѳ���
        /// </summary>
        Cooperate
    }

    /// <summary>
    /// ����״̬
    /// </summary>
    public enum SituationState
    {
        /// <summary>
        /// ��������
        /// </summary>
        Dominant,

        /// <summary>
        /// �������
        /// </summary>
        Advantageous,

        /// <summary>
        /// ����
        /// </summary>
        Balanced,

        /// <summary>
        /// �������
        /// </summary>
        Disadvantageous,

        /// <summary>
        /// ��������
        /// </summary>
        Critical
    }

    public enum AILevel
    {
        Basic,
        Advanced
    }
}