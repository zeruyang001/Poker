/*using System;
using System.Collections.Generic;
using UnityEngine;
using AI.Utils;
using static CardManager;
using System.Linq;

namespace AI.Models
{
    /// <summary>
    /// ��Ϸ������ - ά����ǰ��Ϸ״̬
    /// </summary>
    public class GameContext : IDisposable
    {
        #region Properties
        /// <summary>
        /// ��ǰ��Ϸ�׶�
        /// </summary>
        public GamePhase CurrentPhase { get; private set; }

        /// <summary>
        /// �������ƴ���
        /// </summary>
        public int PassCount { get; private set; }

        /// <summary>
        /// �������������
        /// </summary>
        public Dictionary<CharacterType, int> RemainingCards { get; private set; }

        /// <summary>
        /// �ѳ��Ƽ�¼
        /// </summary>
        public Dictionary<CharacterType, List<Card>> PlayedCards { get; private set; }

        /// <summary>
        /// ��һ������Ϣ
        /// </summary>
        public LastPlayInfo LastPlay { get; private set; }

        /// <summary>
        /// ��ǰ����Ƶ����
        /// </summary>
        public CharacterType BiggestPlayer { get; private set; }
        #endregion

        #region Constructor
        public GameContext()
        {
            RemainingCards = new Dictionary<CharacterType, int>();
            PlayedCards = new Dictionary<CharacterType, List<Card>>();
            LastPlay = new LastPlayInfo();
            Reset();
        }
        #endregion

        #region State Management
        /// <summary>
        /// ������Ϸ״̬
        /// </summary>
        public void Reset()
        {
            CurrentPhase = GamePhase.Opening;
            PassCount = 0;
            BiggestPlayer = CharacterType.Desk;

            RemainingCards.Clear();
            foreach (CharacterType type in Enum.GetValues(typeof(CharacterType)))
            {
                if (type != CharacterType.Desk && type != CharacterType.Deck)
                {
                    RemainingCards[type] = 0;
                }
            }

            PlayedCards.Clear();
            LastPlay.Reset();
        }

        /// <summary>
        /// ������Ϸ״̬
        /// </summary>
        public void Update(List<Card> currentHand, ComputerSmartArgs args)
        {
            try
            {
                // ���»�����Ϣ
                UpdateBasicInfo(args);

                // ���½׶�
                UpdateGamePhase(currentHand.Count);

                // �����ƾ���Ϣ
                UpdateCardInfo(args);

                // ������һ������Ϣ
                UpdateLastPlayInfo(args);

            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to update GameContext: {e.Message}");
            }
        }

        private void UpdateBasicInfo(ComputerSmartArgs args)
        {
            BiggestPlayer = args.BiggestCharacter;

            // �������������
            foreach (var kvp in args.RemainingCards)
            {
                RemainingCards[kvp.Key] = kvp.Value;
            }
        }

        private void UpdateGamePhase(int cardCount)
        {
            // ������������������Ϸ�׶�

            if (cardCount <= AIConstants.END_GAME_THRESHOLD)
                CurrentPhase = GamePhase.Endgame;
            else
                CurrentPhase = GamePhase.Middle; 
        }

        private void UpdateCardInfo(ComputerSmartArgs args)
        {
            // ��¼����
            if (args.LastPlay?.CardType != CardType.Invalid)
            {
                if (!PlayedCards.ContainsKey(args.LastPlay.Character))
                {
                    PlayedCards[args.LastPlay.Character] = new List<Card>();
                }
                // TODO: ��Ҫ���ʵ�ʳ�����
            }

            // ���¹��Ƽ���
            if (args.LastPlay?.CardType == CardType.Invalid)
            {
                PassCount++;
            }
            else
            {
                PassCount = 0;
            }
        }

        private void UpdateLastPlayInfo(ComputerSmartArgs args)
        {
            if (args.LastPlay != null)
            {
                LastPlay.Character = args.LastPlay.Character;
                LastPlay.CardType = args.LastPlay.CardType;
                LastPlay.Weight = args.LastPlay.Weight;
                LastPlay.Length = args.LastPlay.Length;
                LastPlay.BombCount = args.LastPlay.BombCount;
                LastPlay.RocketPlayed = args.LastPlay.RocketPlayed;
            }
        }
        #endregion

        #region Game State Queries
        /// <summary>
        /// ����Ƿ��ǲо�
        /// </summary>
        public bool IsEndgame(CharacterType character = CharacterType.Desk)
        {
            if (character == CharacterType.Desk)
            {
                return RemainingCards.Values.Any(count => count <= AIConstants.END_GAME_THRESHOLD);
            }
            return RemainingCards.TryGetValue(character, out int count) && count <= AIConstants.END_GAME_THRESHOLD;
        }

        /// <summary>
        /// ��ȡָ������ѳ�����
        /// </summary>
        public List<Card> GetPlayedCards(CharacterType character)
        {
            return PlayedCards.TryGetValue(character, out var cards) ? cards : new List<Card>();
        }

        /// <summary>
        /// ��ȡָ����ҵ�ʣ������
        /// </summary>
        public int GetRemainingCardCount(CharacterType character)
        {
            return RemainingCards.TryGetValue(character, out int count) ? count : 0;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            RemainingCards?.Clear();
            PlayedCards?.Clear();
            LastPlay = null;
        }
        #endregion
    }
}*/