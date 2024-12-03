using System;
using System.Collections.Generic;
using UGoap.Base;

namespace UGoap.Planner
{
    /// <summary>
    /// Defines the Node used by the Planner Search.
    /// </summary>
    public abstract class Node : IComparable
    {
        //Properties
        public GoapState InitialState { get; private set; }
        public GoapConditions Goal { get; private set; }
        public Node Parent { get; private set; }
        public GoapAction PreviousAction { get; private set; }
        public GoapActionInfo PreviousActionInfo { get; private set; }
        public virtual int TotalCost { get; private set; }
        public GoapSettings Settings { get; private set; }

        public int ActionCount => Parent != null ? Parent.ActionCount + 1 : 0;
        public bool IsGoal(GoapState state) => !Goal.CheckConflict(state);

        //Fields
        public readonly List<Node> Children = new();
        protected INodeGenerator _nodeGenerator;
        
        //Methods
        public void Setup(INodeGenerator nodeGenerator, GoapState initialState, GoapConditions goal)
        {
            _nodeGenerator = nodeGenerator;
            InitialState = initialState;
            Goal = goal;
            Parent = null;
            PreviousAction = null;
            PreviousActionInfo = new GoapActionInfo();
            Children.Clear();
            CreateSettings();
        }
        
        private void CreateSettings()
        {
            Settings = new GoapSettings
            {
                InitialState = InitialState,
                Goal = Goal,
            };
        }

            //Used By Generator
        /// <summary>
        /// Do mixed apply for the current state and node.
        /// </summary>
        /// <param name="action">PreviousAction applied to the node.</param>
        /// <returns>Node result and unchecked conditions.</returns>
        public Node ApplyAction(GoapAction action)
        {
           //Apply action
           var effects = action.GetEffects(Settings);
           var resultGoal = Goal.ApplyEffects(effects);
           if (resultGoal == null) return null;
           
           //Merge new conflicts.
           var conditions = action.GetPreconditions(Settings);
           resultGoal = resultGoal.Merge(conditions);

           //Store action dynamic info.
           var actionInfo = new GoapActionInfo
           {
               Conditions = conditions,
               Effects = effects,
           };

           return resultGoal == null ? null : CreateChild(resultGoal, action, actionInfo);
        }
        
        private Node CreateChild(GoapConditions goal, GoapAction action, GoapActionInfo actionInfo)
        {
            var child = _nodeGenerator.CreateNode(InitialState, goal);
            child.Update(this, action, actionInfo);
            Children.Add(child);
            return child;
        }

        /// <summary>
        /// Apply the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">PreviousAction that leads to this node.</param>
        public void Update(Node parent, GoapAction action, GoapActionInfo actionInfo)
        {
            //Se actualiza la accion de origen y el objetivo.
            PreviousAction = action;
            PreviousActionInfo = actionInfo;
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

        public virtual void UpdateCost() => TotalCost = PreviousAction.GetCost(Parent.Goal);

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