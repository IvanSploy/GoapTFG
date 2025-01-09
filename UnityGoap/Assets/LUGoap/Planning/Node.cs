using System;
using System.Collections.Generic;
using LUGoap.Base;
using LUGoap.Learning;
using Action = LUGoap.Base.Action;

namespace LUGoap.Planning
{
    /// <summary>
    /// Defines the Node used by the Planner Search.
    /// </summary>
    public abstract class Node : IComparable
    {
        //Properties
        public State InitialState { get; private set; }
        public Conditions Goal { get; private set; }
        public Node Parent { get; private set; }
        public NodeAction ActionData { get; private set; }
        public virtual int TotalCost { get; private set; }

        public Action PreviousAction => ActionData.Action;
        public int ActionCount => Parent != null ? Parent.ActionCount + 1 : 0;
        public bool IsGoal(State state) => !Goal.CheckConflict(state);

        //Fields
        public readonly List<Node> Children = new();
        protected INodeGenerator _nodeGenerator;
        
        //Methods
        public void Setup(INodeGenerator nodeGenerator, State initialState, Conditions goal)
        {
            _nodeGenerator = nodeGenerator;
            InitialState = initialState;
            Goal = goal;
            Parent = null;
            ActionData = new NodeAction();
            Children.Clear();
        }
        
        //Used By Generator
        /// <summary>
        /// Do mixed apply for the current state and node.
        /// </summary>
        /// <param name="action">PreviousAction applied to the node.</param>
        /// <param name="settings">Parameters created for the action.</param>
        /// <returns>Node result and unchecked conditions.</returns>
        public Node ApplyAction(Action action, ActionSettings settings)
        {
           //Apply action
           var effects = action.GetEffects(settings);
           var resultGoal = Goal.ApplyEffects(effects);
           if (resultGoal == null) return null;
           
           //Merge new conflicts.
           var conditions = action.GetPreconditions(settings);
           resultGoal = resultGoal.Merge(conditions);

           //Store action data.
           var actionInfo = new NodeAction
           {
               Action = action,
               Conditions = conditions,
               Effects = effects,
               Parameters = settings.Parameters,
               LearningCode = settings.LearningCode,
           };

           if (action is LearningAction learningAction)
           {
               actionInfo.ActionLearningCode = learningAction.GetLearningCode(InitialState, Goal);
           }

           return resultGoal == null ? null : CreateChild(resultGoal, actionInfo);
        }
        
        private Node CreateChild(Conditions goal, NodeAction action)
        {
            var child = _nodeGenerator.CreateNode(InitialState, goal);
            child.Update(this, action);
            Children.Add(child);
            return child;
        }

        /// <summary>
        /// Apply the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="action">PreviousAction that leads to this node.</param>
        public void Update(Node parent, NodeAction action)
        {
            //Se actualiza la accion de origen y el objetivo.
            ActionData = action;
            SetParent(parent);
        }

        /// <summary>
        /// Updates the Node to sets a new Parent and all the related info.
        /// </summary>
        /// <param name="parent"></param>
        public void SetParent(Node parent)
        {
            //Se define la relación padre hijo.
            Parent = parent;
            UpdateCost();
        }

        public virtual void UpdateCost() => TotalCost = PreviousAction.GetCost(Parent.Goal) + Parent.TotalCost;

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
            
            return Goal.Equals(objNode.Goal);
        }

        public override int GetHashCode()
        {
            return Goal.GetHashCode();
        }
        #endregion
    }
}