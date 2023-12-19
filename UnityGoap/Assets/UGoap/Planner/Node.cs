using System;
using System.Collections.Generic;
using System.Linq;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Defines the Node used by the Planner Search.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class Node<TKey, TValue> : System.IComparable
    {
        //Properties
        public Node<TKey, TValue> Parent { get; set; }      
        public List<Node<TKey, TValue>> Children { get; }
        public IGoapAction<TKey, TValue> Action { get; set; }
        public GoapGoal<TKey, TValue> Goal { get; set; }
        public int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        
        //Fields
        public readonly PropertyGroup<TKey, TValue> State;
        protected readonly INodeGenerator<TKey, TValue> Generator;
        
        //Constructor
        protected Node(PropertyGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goal, INodeGenerator<TKey, TValue> generator)
        {
            State = state;
            Goal = goal;
            Generator = generator;
            Children = new List<Node<TKey, TValue>>();
            TotalCost = 0;
            ActionCount = 0;
            IsGoal = false;
        }

            //Used By Generator
        /// <summary>
        /// Applies an action to a Node and creates the Node that result.
        /// </summary>
        /// <param name="goapAction">Action applied to the node.</param>
        /// <returns>Node result.</returns>
        public Node<TKey, TValue> ApplyAction(IGoapAction<TKey, TValue> goapAction)
        {
            var pg = goapAction.ApplyAction(new GoapStateInfo<TKey, TValue>(State, Goal));
            return pg == null ? null : CreateChildNode(pg, Goal, goapAction);
        }

        /// <summary>
        /// Do regressive apply for the current state and node.
        /// </summary>
        /// <param name="goapAction">Action applied to the node.</param>
        /// <param name="goapGoal">Goal to be modified</param>
        /// <param name="reached">If the node has no conflicts</param>
        /// <returns>Node result.</returns>
        public Node<TKey, TValue> ApplyRegressiveAction(IGoapAction<TKey, TValue> goapAction,
            GoapGoal<TKey, TValue> goapGoal, out bool reached)
        {
            var stateCreated = goapAction.ApplyRegressiveAction(new GoapStateInfo<TKey, TValue>(State, goapGoal), out reached);
            return stateCreated == null ? null : CreateChildNode(stateCreated.WorldState, stateCreated.CurrentGoal, goapAction);
        }

        /// <summary>
        /// Do regressive apply for the current state and node.
        /// </summary>
        /// <param name="currentState">The current state of the research.</param>
        /// <param name="goapAction">Action applied to the node.</param>
        /// <returns>Node result and unchecked conditions.</returns>
        public Node<TKey, TValue> ApplyMixedAction(PropertyGroup<TKey, TValue> currentState,
            IGoapAction<TKey, TValue> goapAction)
        {
            var recursiveResult = CheckMixedGoal(currentState, goapAction);
            Node<TKey, TValue> child = null;
            if(recursiveResult.goal != null) child = CreateChildNode(recursiveResult.finalState, recursiveResult.goal, goapAction);
            if (child != null && !recursiveResult.goal.IsEmpty()) child.IsGoal = false;
            if (child is { IsGoal: true } && !recursiveResult.valid) return null;
            return child;
        }
        
        public (PropertyGroup<TKey, TValue> finalState, GoapGoal<TKey, TValue> goal, bool valid) CheckMixedGoal(
            PropertyGroup<TKey, TValue> currentState, IGoapAction<TKey, TValue> goapAction)
        {
            (PropertyGroup<TKey, TValue> finalState, GoapGoal<TKey, TValue> goal, bool valid) result;
            var actionResult = goapAction.ApplyMixedAction(currentState, Goal);

            PropertyGroup<TKey, TValue> mergeConditions;
            
            //Nodes
            if (Parent != null)
            {
                var parentResult = Parent.CheckMixedGoal(actionResult.state, Action);

                //Action invalid, path not valid.
                if (parentResult.goal == null) 
                    return (null, null, false);
                
                mergeConditions = PropertyGroup<TKey, TValue>.Merge(
                    actionResult.goal.GetState(), parentResult.goal.GetState());
                
                result.finalState = parentResult.finalState;
                result.valid = parentResult.valid && actionResult.valid; //check action validate.
            }
            //Final node
            else
            {
                //Check main goal
                mergeConditions = PropertyGroup<TKey, TValue>.Merge(
                    actionResult.goal.GetState(), Goal.GetConflicts(actionResult.state));

                result.finalState = actionResult.state;
                result.valid = actionResult.valid; //Check action validate.
            }
            
            if (mergeConditions != null)
            {
                result.goal = new GoapGoal<TKey, TValue>(actionResult.goal.Name, mergeConditions,
                    actionResult.goal.PriorityLevel);
            }
            //Action invalid, path not valid.
            else return (null, null, false);
            
            return result;
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="pg">Property Group</param>
        /// <param name="goapGoal"></param>
        /// <param name="goapAction"></param>
        /// <returns></returns>
        protected abstract Node<TKey, TValue> CreateChildNode(PropertyGroup<TKey, TValue> pg, GoapGoal<TKey, TValue> goapGoal, IGoapAction<TKey, TValue> goapAction);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">Action that leads to this node.</param>
        public void Update(Node<TKey, TValue> parent, IGoapAction<TKey, TValue> goapAction)
        {
            //Se actualiza la accion de origen y el objetivo.
            Action = goapAction;
            ActionCount = parent.ActionCount + 1;
            Update(parent);
        }
        
        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goal"></param>
        /// <param name="goapAction">Action that leads to this node.</param>
        public void Update(Node<TKey, TValue> parent, GoapGoal<TKey, TValue> goal, IGoapAction<TKey, TValue> goapAction)
        {
            //Se actualiza la accion de origen y el objetivo.
            Action = goapAction;
            ActionCount = parent.ActionCount + 1;
            Goal = goal;
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
            TotalCost = Action.GetCost(new GoapStateInfo<TKey, TValue>(State, Goal));
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
        
        public TValue this[TKey key] => State[key];

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            Node<TKey, TValue> objNode = (Node<TKey, TValue>)obj;
            
            return State.Equals(objNode.State);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }
        #endregion
    }
}