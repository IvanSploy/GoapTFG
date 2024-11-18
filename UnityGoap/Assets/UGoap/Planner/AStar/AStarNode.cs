using System;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planner
{
    public class AStarNode : Node
    {
        //Properties
        public int HCost { get; set; }
        public int GCost { get; set; }

        public override int TotalCost => GCost + HCost;

        //Constructor
        public AStarNode(INodeGenerator nodeGenerator, GoapState initialState, GoapConditions goal, Func<GoapConditions,GoapState,int> heuristic) :
            base(nodeGenerator, initialState, goal, heuristic)
        {
            GCost = 0;
            HCost = 0;
        }
        
        public AStarNode(INodeGenerator nodeGenerator, GoapState initialState, GoapConditions goal, ILearningConfig learningConfig) :
            base(nodeGenerator, initialState, goal, learningConfig)
        {
            GCost = 0;
            HCost = 0;
        }

        protected override Node CreateChildNode(GoapConditions goal, GoapAction action, GoapActionInfo actionInfo)
        {
            var aStarNode = UseLearning ? new AStarNode(NodeGenerator, InitialState, goal, LearningConfig)
                : new AStarNode(NodeGenerator, InitialState, goal, CustomHeuristic);
            aStarNode.Update(this, action, actionInfo);
            Children.Add(aStarNode);
            return aStarNode;
        }

        public override void Update(Node parent)
        {
            base.Update(parent);
            
            AStarNode asnParent = (AStarNode) parent;
            HCost = UseLearning ? GetLearning() : GetHeuristic(NodeGenerator.InitialState);
            GCost = PreviousAction.GetCost(parent.Goal) + asnParent.GCost;
        }

        /// <summary>
        /// Retrieves the value of the heuristic applied to this node.
        /// </summary>
        /// <returns>Heuristic cost.</returns>
        public int GetHeuristic(GoapState initialState)
        {
            return CustomHeuristic?.Invoke(Goal, initialState) ?? Goal.CountConflicts(initialState);
        }

        public int GetLearning()
        {
            var state = LearningConfig.GetLearningStateCode(InitialState, Goal);
            return -(int)LearningConfig.Get(state, PreviousAction.Name);
        }

        #region Overrides
        
        public override string ToString()
        {
            string text = "";
            if (PreviousAction == null) text += "Initial Node";
            else text += PreviousAction.Name;
            text += " | Costes: " + GCost + " | " + HCost + " | " + TotalCost + "\n";
            text += " | Objetivo: " + Goal + "\n";
            return text;
        }
        #endregion
    }
}