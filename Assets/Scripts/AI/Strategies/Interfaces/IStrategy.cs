/*using AI.Models;
using AI.Utils;
using System.Collections.Generic;

namespace AI.Strategies
{
    public interface IStrategy
    {
        // 策略基本属性
        StrategyType Type { get; }
        string Name { get; }

        // 核心方法
        bool Execute(List<Card> cards, ComputerSmartArgs args, GameContext context);
        float EvaluateSuitability(SituationAnalysis situation);

        // 策略条件判断 
        bool CanExecute(List<Card> cards, ComputerSmartArgs args);
        bool ShouldUseBomb(SituationAnalysis situation);
    }
}*/