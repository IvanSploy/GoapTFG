using System;
using QGoap.Base;

namespace QGoap.Unity
{
    public static class GoapHeuristics
    {
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<ConditionGroup, State, int> GetCustomHeuristic()
        {
            return (conditions, worldState) =>
            {
                var heuristic = 0;
                foreach (var conditionPair in conditions)
                {
                    heuristic += conditionPair.Value.GetDistance(worldState.TryGetOrDefault(conditionPair.Key));
                }

                return heuristic;
            };
        }
    }
}
