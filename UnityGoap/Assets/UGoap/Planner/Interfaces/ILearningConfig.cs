using UGoap.Base;
using UGoap.Planner;

namespace UGoap.Learning
{
    public enum LearningType
    {
        State,
        Goal,
        Both
    }
    
    public interface ILearningConfig
    {
        //Properties
        LearningType Type { get; }
        
        //Methods
        float Apply(int state, string action, float r, int newState);
        float Get(int state, string action);
        void UpdateLearning(Node node, GoapState initialState, float reward);
        int GetLearningStateCode(GoapState state, GoapConditions goal);
    }
}