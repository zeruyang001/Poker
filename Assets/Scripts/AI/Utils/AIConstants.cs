using AI.Core;

namespace AI.Utils
{
    /// <summary>
    /// AI������س���
    /// </summary>
    public static class AIConstants
    {
        #region ������ֵ
        /// <summary>
        /// ����������ֵ
        /// </summary>
        public const float AGGRESSIVE_THRESHOLD = 0.7f;

        /// <summary>
        /// ���ز�����ֵ
        /// </summary>
        public const float CONSERVATIVE_THRESHOLD = 0.3f;

        /// <summary>
        /// ը��ʹ����ֵ
        /// </summary>
        public const float BOMB_THRESHOLD = 0.6f;

        /// <summary>
        /// ��ըʹ����ֵ
        /// </summary>
        public const float ROCKET_THRESHOLD = 0.8f;

        public const float GRAB_THRESHOLD = 0.8f;
        public const float CALL_THRESHOLD = 0.55f;
        #endregion

        #region ��ֵȨ��
        /// <summary>
        /// ��ͨ��Ȩ��
        /// </summary>
        public const float NORMAL_CARD_WEIGHT = 1.0f;

        /// <summary>
        /// ����Ȩ��(A������)
        /// </summary>
        public const float BIG_CARD_WEIGHT = 1.5f;

        /// <summary>
        /// ը��Ȩ��
        /// </summary>
        public const float BOMB_WEIGHT = 2.0f;

        /// <summary>
        /// ��ըȨ��
        /// </summary>
        public const float ROCKET_WEIGHT = 3.0f;
        #endregion

        #region ���ͼ�Ȩ
        /// <summary>
        /// ˳�Ӽ�Ȩ
        /// </summary>
        public const float STRAIGHT_BONUS = 0.15f;

        /// <summary>
        /// ���Լ�Ȩ
        /// </summary>
        public const float PAIR_STRAIGHT_BONUS = 0.2f;

        /// <summary>
        /// �ɻ���Ȩ
        /// </summary>
        public const float PLANE_BONUS = 0.25f;
        #endregion

        #region ��Ϸ�׶��ж�
        /// <summary>
        /// �о�������ֵ
        /// </summary>
        public const int MIDDLE_GAME_THRESHOLD = 10;

        /// <summary>
        /// �о�������ֵ
        /// </summary>
        public const int END_GAME_THRESHOLD = 5;
        #endregion

        #region ��������
        /// <summary>
        /// ����������ֵ
        /// </summary>
        public const float DOMINANT_THRESHOLD = 0.8f;

        /// <summary>
        /// ���������ֵ
        /// </summary>
        public const float ADVANTAGE_THRESHOLD = 0.6f;

        /// <summary>
        /// ������ֵ
        /// </summary>
        public const float DISADVANTAGE_THRESHOLD = 0.4f;

        /// <summary>
        /// ����������ֵ
        /// </summary>
        public const float CRITICAL_THRESHOLD = 0.2f;
        #endregion

        #region Helper Methods
        public static ComputerAI CreateAI(AILevel level)
        {
            switch (level)
            {
                case AILevel.Advanced:
                    return new AdvancedComputerAI();
                case AILevel.Basic:
                default:
                    return new ComputerAI();
            }
        }
        /// <summary>
        /// ��ȡ����ǿ�ȵȼ�
        /// </summary>
        public static HandStrength GetHandStrength(float strengthValue)
        {
            if (strengthValue >= DOMINANT_THRESHOLD) return HandStrength.VeryStrong;
            if (strengthValue >= ADVANTAGE_THRESHOLD) return HandStrength.Strong;
            if (strengthValue >= CONSERVATIVE_THRESHOLD) return HandStrength.Normal;
            if (strengthValue >= CRITICAL_THRESHOLD) return HandStrength.Weak;
            return HandStrength.VeryWeak;
        }

        /// <summary>
        /// ��ȡ����״̬
        /// </summary>
        public static SituationState GetSituationState(float situationValue)
        {
            if (situationValue >= DOMINANT_THRESHOLD) return SituationState.Dominant;
            if (situationValue >= ADVANTAGE_THRESHOLD) return SituationState.Advantageous;
            if (situationValue >= DISADVANTAGE_THRESHOLD) return SituationState.Balanced;
            if (situationValue >= CRITICAL_THRESHOLD) return SituationState.Disadvantageous;
            return SituationState.Critical;
        }

        /// <summary>
        /// ��ȡ��Ϸ�׶�
        /// </summary>
        public static GamePhase GetGamePhase(int cardCount)
        {
            if (cardCount > MIDDLE_GAME_THRESHOLD) return GamePhase.Opening;
            if (cardCount > END_GAME_THRESHOLD) return GamePhase.Middle;
            return GamePhase.Endgame;
        }

        /// <summary>
        /// ��ȡ����Ȩ��
        /// </summary>
        public static float GetCardWeight(Card card)
        {
            if (card.rank == Rank.SJoker || card.rank == Rank.LJoker)
                return ROCKET_WEIGHT;
            if ((int)card.rank >= (int)Rank.Two)
                return BIG_CARD_WEIGHT;
            return NORMAL_CARD_WEIGHT;
        }
        #endregion
    }
}
