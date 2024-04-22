using UGoap.Base;

namespace UGoap.Learning
{
    public interface IQLearning
    {
        float Apply(int state, string action, float r, int newState);

        float Get(int state, string action);

        int ParseToStateCode(GoapConditions goal);

        void DebugLearning();
    }
}