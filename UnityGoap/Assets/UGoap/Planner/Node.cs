using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;

namespace UGoap.Planner
{
    /// <summary>
    /// Defines the Node used by the Planner Search.
    /// </summary>
    public abstract class Node : IComparable
    {
        //Properties
        public Node Parent { get; set; }      
        public List<Node> Children { get; }
        public IGoapAction ParentAction { get; set; }
        public GoapGoal Goal { get; set; }
        public virtual int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        
        //Fields
        public readonly GoapState State;
        protected readonly Func<GoapGoal,GoapState,int> CustomHeuristic;
        protected readonly IQLearning QLearning;

        public bool UseLearning => QLearning != null;
        
        //Constructor
        protected Node(GoapState state, GoapGoal goal, Func<GoapGoal,GoapState,int> customHeuristic)
        {
            State = state;
            Goal = goal;
            CustomHeuristic = customHeuristic;
            Children = new List<Node>();
            ActionCount = 0;
        }
        
        protected Node(GoapState state, GoapGoal goal, IQLearning qLearning)
        {
            State = state;
            Goal = goal;
            QLearning = qLearning;
            Children = new List<Node>();
            ActionCount = 0;
        }

            //Used By Generator
        /// <summary>
        /// Do mixed apply for the current state and node.
        /// </summary>
        /// <param name="currentGoapState">The current state of the research.</param>
        /// <param name="goapAction">ParentAction applied to the node.</param>
        /// <returns>Node result and unchecked conditions.</returns>
        public Node ApplyAction(GoapState currentGoapState,
            IGoapAction goapAction)
        {
            var recursiveResult = CheckGoal(currentGoapState, goapAction);
            return recursiveResult.goal == null ? null : CreateChildNode(recursiveResult.finalState, recursiveResult.goal,
                goapAction, recursiveResult.cost);
        }
        
        public (GoapState finalState, GoapGoal goal, int cost) CheckGoal(
            GoapState currentGoapState, IGoapAction goapAction)
        {
            (GoapState finalState, GoapGoal goal, int cost) result;
            var actionResult = goapAction.ApplyAction(
                new GoapStateInfo(currentGoapState,
                    Goal,
                    State));

            if (actionResult.State == null) return (null, null, -1);
            
            GoapConditions mergeConditions;
            
            //Nodes
            if (Parent != null)
            {
                var parentResult = Parent.CheckGoal(actionResult.State, ParentAction);

                //ParentAction invalid, path not valid.
                if (parentResult.goal == null) 
                    return (null, null, -1);
                
                mergeConditions = GoapConditions.Merge(
                    actionResult.Goal, 
                    parentResult.goal);
                
                result.finalState = parentResult.finalState;
                result.cost = goapAction.GetCost(currentGoapState, Goal) + parentResult.cost;
            }
            //Final node
            else
            {
                //Check main goal
                mergeConditions = GoapConditions.Merge(
                    actionResult.Goal,
                    Goal.GetConflicts(actionResult.State));

                result.finalState = actionResult.State;
                result.cost = goapAction.GetCost(currentGoapState, Goal);
            }
            
            if (mergeConditions != null)
            {
                result.goal = new GoapGoal(actionResult.Goal.Name, mergeConditions,
                    actionResult.Goal.PriorityLevel);
            }
            //ParentAction invalid, path not valid.
            else return (null, null, -1);
            
            return result;
        }
        
        public int GetUpdatedCost(GoapState currentState, IGoapAction goapAction)
        {
            int cost;
            var actionResult = goapAction.ApplyAction(new GoapStateInfo(currentState, Goal, State));
            
            //Nodes
            if (Parent != null)
            {
                var parentCost = Parent.GetUpdatedCost(actionResult.State, ParentAction);
                cost = ParentAction.GetCost(currentState, Goal) + parentCost;
            }
            //Final node
            else
            {
                cost = goapAction.GetCost(currentState, Goal);
            }
            
            return cost;
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="goapState">Property Group</param>
        /// <param name="goapGoal"></param>
        /// <param name="goapAction"></param>
        /// <param name="cost">Custom cost</param>
        /// <returns></returns>
        protected abstract Node CreateChildNode(GoapState goapState, GoapGoal goapGoal,
            IGoapAction goapAction, int cost = -1);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">ParentAction that leads to this node.</param>
        public void Update(Node parent, IGoapAction goapAction)
        {
            //Se actualiza la accion de origen y el objetivo.
            ParentAction = goapAction;
            ActionCount = parent.ActionCount + 1;
            Update(parent);
        }

        /// <summary>
        /// Updates the Node to sets a new Parent and all the related info.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="customHeuristic"></param>
        /// <param name="learning"></param>
        public virtual void Update(Node parent)
        {
            //Se define la relación padre hijo.
            Parent = parent;
            TotalCost = ParentAction.GetCost(parent.State, parent.Goal);
            ActionCount = parent.ActionCount + 1;
        }

        public int CompareTo(object obj)
        {
            Node objNode = (Node)obj;
            //En inserciones se comprobará que no se trata del mismo nodo aunque tengan el mismo coste.
            //En búsquedas se encontrará al comprobar que se trata del mismo nodo.
            if (Equals(objNode)) return 0;
            
            //Comprobado que no se trata del mismo nodo se ordena por coste, ignorando si tienen el mismo coste,
            //directamente se predomina el de la izquierda.
            var result = TotalCost.CompareTo(objNode.TotalCost);
            return result == 0 ? -1 : result;
        }

        #region Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            Node objNode = (Node)obj;
            
            return State.Equals(objNode.State) && Goal.Equals(objNode.Goal);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }
        #endregion
    }
}