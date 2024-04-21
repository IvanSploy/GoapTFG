using UGoap.Base;

namespace UGoap.Learning
{
    public interface IQLearning
    {
        float UpdateQValue(int state, string action, float r, int newState);

        float GetQValue(int state, string action);

        int ParseToStateCode(GoapConditions goal);

        void DebugLearning();
    }
}