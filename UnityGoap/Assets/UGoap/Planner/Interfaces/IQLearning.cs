using UGoap.Base;
using UGoap.Planner;

namespace UGoap.Learning
{
    public interface IQLearning
    {
        float UpdateQValue(int state, string action, float r, int newState);

        float GetQValue(int state, string action);

        int ParseToStateCode(GoapState goapState);

        int GetReward(Node startNode, Node finishNode);

        void DebugLearning();
    }
}