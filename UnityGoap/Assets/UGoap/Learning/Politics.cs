using System;
using System.Linq;
using UnityEngine;

namespace UGoap.Learning
{
    public static class Politics
    {
        public static string FindMax(this LearningConfig learningConfig, int state, string search)
        {
            if (learningConfig.Values.TryGetValue(state, out var values))
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