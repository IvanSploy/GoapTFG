using System;
using System.Linq;
using UnityEngine;

namespace UGoap.Learning
{
    public static class QGoapExtensions
    {
        public static string FindMax(this GoapQLearning qLearning, int state, string search)
        {
            if (qLearning.Values.TryGetValue(state, out var values))
            {
                try
                {
                    var pair = values
                        .Where(pair => pair.Key.Contains(search, StringComparison.CurrentCultureIgnoreCase))
                        .Aggregate((maxPair, pair) => pair.Value > maxPair.Value ? pair : maxPair);
                    return pair.Key;
                }
                catch (InvalidOperationException e)
                {
                    Debug.LogWarning(e.Message);
                    return null;
                }
            }

            return null;
        }
    }
}