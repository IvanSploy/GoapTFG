using System;

namespace LUGoap.Base
{
    public static class Random
    {
        private static readonly System.Random _random = new(DateTime.Now.Millisecond);
        
        public static float Next()
        {
            lock (_random)
            {
                return (float)_random.NextDouble();
            }
        }
        
        public static float Range(int inclusiveMin, int exclusiveMax)
        {
            lock (_random)
            {
                return _random.Next(inclusiveMin, exclusiveMax);
            }
        }
        
        public static float Range(float inclusiveMin, float exclusiveMax)
        {
            lock (_random)
            {
                return (float)(_random.NextDouble() * (exclusiveMax - inclusiveMin) + inclusiveMin);
            }
        }
        
        public static int RangeToInt(float inclusiveMin, float inclusiveMax)
        {
            lock (_random)
            {
                return (int)Math.Round(_random.NextDouble() * (inclusiveMax - inclusiveMin) + inclusiveMin);
            }
        }
        
        public static int RangeToInt(int inclusiveMin, int inclusiveMax)
        {
            lock (_random)
            {
                return (int)Math.Round(_random.NextDouble() * (float)(inclusiveMax - inclusiveMin) + inclusiveMin);
            }
        }
    }
}