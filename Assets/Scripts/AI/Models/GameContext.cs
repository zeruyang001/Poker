/*using System;
using System.Collections.Generic;
using UnityEngine;
using AI.Utils;
using static CardManager;
using System.Linq;

namespace AI.Models
{
    /// <summary>
    /// 游戏上下文 - 维护当前游戏状态
    /// </summary>
    public class GameContext : IDisposable
    {
        #region Properties
        /// <summary>
        /// 当前游戏阶段
        /// </summary>
        public GamePhase CurrentPhase { get; private set; }

        /// <summary>
        /// 连续过牌次数
        /// </summary>
        public int PassCount { get; private set; }

        /// <summary>
        /// 各玩家手牌数量
        /// </summary>
        public Dictionary<CharacterType, int> RemainingCards { get; private set; }

        /// <summary>
        /// 已出牌记录
        /// </summary>
        public Dictionary<CharacterType, List<Card>> PlayedCards { get; private set; }

        /// <summary>
        /// 上一手牌信息
        /// </summary>
        public LastPlayInfo LastPlay { get; private set; }

        /// <summary>
        /// 当前最大牌的玩家
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
        /// 重置游戏状态
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
        /// 更新游戏状态
        /// </summary>
        public void Update(List<Card> currentHand, ComputerSmartArgs args)
        {
            try
            {
                // 更新基本信息
                UpdateBasicInfo(args);

                // 更新阶段
                UpdateGamePhase(currentHand.Count);

                // 更新牌局信息
                UpdateCardInfo(args);

                // 更新上一手牌信息
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

            // 更新玩家手牌数
            foreach (var kvp in args.RemainingCards)
            {
                RemainingCards[kvp.Key] = kvp.Value;
            }
        }

        private void UpdateGamePhase(int cardCount)
        {
            // 根据手牌数量更新游戏阶段

            if (cardCount <= AIConstants.END_GAME_THRESHOLD)
                CurrentPhase = GamePhase.Endgame;
            else
                CurrentPhase = GamePhase.Middle; 
        }

        private void UpdateCardInfo(ComputerSmartArgs args)
        {
            // 记录出牌
            if (args.LastPlay?.CardType != CardType.Invalid)
            {
                if (!PlayedCards.ContainsKey(args.LastPlay.Character))
                {
                    PlayedCards[args.LastPlay.Character] = new List<Card>();
                }
                // TODO: 需要添加实际出的牌
            }

            // 更新过牌计数
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
        /// 检查是否是残局
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
        /// 获取指定玩家已出的牌
        /// </summary>
        public List<Card> GetPlayedCards(CharacterType character)
        {
            return PlayedCards.TryGetValue(character, out var cards) ? cards : new List<Card>();
        }

        /// <summary>
        /// 获取指定玩家的剩余牌数
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