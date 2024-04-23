using UGoap.Base;

namespace UGoap.Learning
{
    public interface IQLearning
    {
        //Properties
        bool UseStatePrediction { get; }
        
        //Methods
        float Apply(int state, string action, float r, int newState);
        float Get(int state, string action);
        int ParseToStateCode(GoapConditions goal);
        int ParseToStateCode(GoapState state);
        void DebugLearning();
    }
}