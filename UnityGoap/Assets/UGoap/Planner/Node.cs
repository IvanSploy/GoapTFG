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
        public IGoapAction<TKey, TValue> Action { get; set; }
        public EffectGroup<TKey, TValue> ProceduralEffects { get; set; }
        public GoapGoal<TKey, TValue> Goal { get; set; }
        public int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        public bool CanBeGoal { get; set; } = true;
        
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
            (var state, var proceduralEffects) =
                goapAction.ApplyAction(new GoapStateInfo<TKey, TValue>(State, Goal));
            return state == null ? null : CreateChildNode(state, Goal, goapAction, proceduralEffects);
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
            return stateCreated == null ? null : CreateChildNode(stateCreated.State, stateCreated.Goal, goapAction, stateCreated.ProceduralEffects);
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
            if(recursiveResult.goal != null) child = CreateChildNode(recursiveResult.finalState, recursiveResult.goal, goapAction, recursiveResult.proceduralEffects);
            if (child != null)
            {
                if (!recursiveResult.goal.IsEmpty())
                {
                    child.CanBeGoal = false;
                    child.IsGoal = false;
                }
                else
                {
                    child.CanBeGoal = true;
                    child.IsGoal = true;
                }
            }
            if (child is { IsGoal: true } && !recursiveResult.valid) return null;
            return child;
        }
        
        public (PropertyGroup<TKey, TValue> finalState, GoapGoal<TKey, TValue> goal, EffectGroup<TKey, TValue> proceduralEffects, bool valid) CheckMixedGoal(
            PropertyGroup<TKey, TValue> currentState, IGoapAction<TKey, TValue> goapAction)
        {
            (PropertyGroup<TKey, TValue> finalState, GoapGoal<TKey, TValue> goal, EffectGroup<TKey, TValue> proceduralEffects, bool valid) result;
            var actionResult = goapAction.ApplyMixedAction(currentState, Goal);
            var resultInfo = actionResult.stateInfo;
            
            ConditionGroup<TKey, TValue> mergeConditions;
            
            //Nodes
            if (Parent != null)
            {
                var parentResult = Parent.CheckMixedGoal(resultInfo.State, Action);

                //Action invalid, path not valid.
                if (parentResult.goal == null) 
                    return (null, null, null, false);
                
                mergeConditions = ConditionGroup<TKey, TValue>.Merge(
                    resultInfo.Goal, 
                    parentResult.goal);
                
                result.finalState = parentResult.finalState;
                result.valid = parentResult.valid && actionResult.valid; //check action validate.
            }
            //Final node
            else
            {
                //Check main goal
                mergeConditions = ConditionGroup<TKey, TValue>.Merge(
                    resultInfo.Goal,
                    Goal.GetConflicts(resultInfo.State));

                result.finalState = resultInfo.State;
                result.valid = actionResult.valid; //Check action validate.
            }
            
            if (mergeConditions != null)
            {
                result.goal = new GoapGoal<TKey, TValue>(resultInfo.Goal.Name, mergeConditions,
                    resultInfo.Goal.PriorityLevel);
            }
            //Action invalid, path not valid.
            else return (null, null, null, false);
            
            result.proceduralEffects = actionResult.stateInfo.ProceduralEffects;
            
            return result;
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="state">Property Group</param>
        /// <param name="goapGoal"></param>
        /// <param name="goapAction"></param>
        /// <param name="proceduralEffects"></param>
        /// <returns></returns>
        protected abstract Node<TKey, TValue> CreateChildNode(PropertyGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goapGoal,
            IGoapAction<TKey, TValue> goapAction, EffectGroup<TKey, TValue> proceduralEffects);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">Action that leads to this node.</param>
        /// <param name="proceduralEffects"></param>
        public void Update(Node<TKey, TValue> parent, IGoapAction<TKey, TValue> goapAction,
            EffectGroup<TKey, TValue> proceduralEffects)
        {
            //Se actualiza la accion de origen y el objetivo.
            Action = goapAction;
            ProceduralEffects = proceduralEffects;
            ActionCount = parent.ActionCount + 1;
            Update(parent);
        }

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goal"></param>
        /// <param name="goapAction">Action that leads to this node.</param>
        /// <param name="proceduralEffects"></param>
        public void Update(Node<TKey, TValue> parent, GoapGoal<TKey, TValue> goal, IGoapAction<TKey, TValue> goapAction,
            EffectGroup<TKey, TValue> proceduralEffects)
        {
            //Se actualiza la accion de origen y el objetivo.
            Action = goapAction;
            ProceduralEffects = proceduralEffects;
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
            TotalCost = Action.GetCost(new GoapStateInfo<TKey, TValue>(parent.State, parent.Goal));
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