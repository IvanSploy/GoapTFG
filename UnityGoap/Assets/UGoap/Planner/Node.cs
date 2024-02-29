using System;
using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    /// <summary>
    /// Defines the Node used by the Planner Search.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class Node<TKey, TValue> : IComparable
    {
        //Properties
        public Node<TKey, TValue> Parent { get; set; }      
        public List<Node<TKey, TValue>> Children { get; }
        public IGoapAction<TKey, TValue> ParentAction { get; set; }
        public GoapGoal<TKey, TValue> Goal { get; set; }
        public virtual int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        
        //Fields
        public readonly GoapState<TKey, TValue> State;
        protected readonly INodeGenerator<TKey, TValue> Generator;
        
        //Constructor
        protected Node(GoapState<TKey, TValue> state, GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> generator)
        {
            State = state;
            Goal = goal;
            Generator = generator;
            Children = new List<Node<TKey, TValue>>();
            ActionCount = 0;
        }

            //Used By Generator
        /// <summary>
        /// Do mixed apply for the current state and node.
        /// </summary>
        /// <param name="currentGoapState">The current state of the research.</param>
        /// <param name="goapAction">ParentAction applied to the node.</param>
        /// <returns>Node result and unchecked conditions.</returns>
        public Node<TKey, TValue> ApplyAction(GoapState<TKey, TValue> currentGoapState,
            IGoapAction<TKey, TValue> goapAction)
        {
            var recursiveResult = CheckGoal(currentGoapState, goapAction);
            return recursiveResult.goal == null ? null : CreateChildNode(recursiveResult.finalState, recursiveResult.goal,
                goapAction, recursiveResult.cost);
        }
        
        public (GoapState<TKey, TValue> finalState, GoapGoal<TKey, TValue> goal, int cost) CheckGoal(
            GoapState<TKey, TValue> currentGoapState, IGoapAction<TKey, TValue> goapAction)
        {
            (GoapState<TKey, TValue> finalState, GoapGoal<TKey, TValue> goal, int cost) result;
            var actionResult = goapAction.ApplyAction(
                new GoapStateInfo<TKey, TValue>(currentGoapState,
                    Goal,
                    State));

            if (actionResult.State == null) return (null, null, -1);
            
            GoapConditions<TKey, TValue> mergeConditions;
            
            //Nodes
            if (Parent != null)
            {
                var parentResult = Parent.CheckGoal(actionResult.State, ParentAction);

                //ParentAction invalid, path not valid.
                if (parentResult.goal == null) 
                    return (null, null, -1);
                
                mergeConditions = GoapConditions<TKey, TValue>.Merge(
                    actionResult.Goal, 
                    parentResult.goal);
                
                result.finalState = parentResult.finalState;
                result.cost = goapAction.GetCost(currentGoapState, Goal) + parentResult.cost;
            }
            //Final node
            else
            {
                //Check main goal
                mergeConditions = GoapConditions<TKey, TValue>.Merge(
                    actionResult.Goal,
                    Goal.GetConflicts(actionResult.State));

                result.finalState = actionResult.State;
                result.cost = goapAction.GetCost(currentGoapState, Goal);
            }
            
            if (mergeConditions != null)
            {
                result.goal = new GoapGoal<TKey, TValue>(actionResult.Goal.Name, mergeConditions,
                    actionResult.Goal.PriorityLevel);
            }
            //ParentAction invalid, path not valid.
            else return (null, null, -1);
            
            return result;
        }
        
        public int GetUpdatedCost(GoapState<TKey, TValue> currentState, IGoapAction<TKey, TValue> goapAction)
        {
            int cost;
            var actionResult = goapAction.ApplyAction(new GoapStateInfo<TKey, TValue>(currentState, Goal, State));
            
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
        protected abstract Node<TKey, TValue> CreateChildNode(GoapState<TKey, TValue> goapState, GoapGoal<TKey, TValue> goapGoal,
            IGoapAction<TKey, TValue> goapAction, int cost = -1);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">ParentAction that leads to this node.</param>
        public void Update(Node<TKey, TValue> parent, IGoapAction<TKey, TValue> goapAction)
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
        public virtual void Update(Node<TKey, TValue> parent)
        {
            //Se define la relación padre hijo.
            Parent = parent;
            TotalCost = ParentAction.GetCost(parent.State, parent.Goal);
            ActionCount = parent.ActionCount + 1;
        }

        public int CompareTo(object obj)
        {
            Node<TKey, TValue> objNode = (Node<TKey, TValue>)obj;
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

            Node<TKey, TValue> objNode = (Node<TKey, TValue>)obj;
            
            return State.Equals(objNode.State) && Goal.Equals(objNode.Goal);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }
        #endregion
    }
}