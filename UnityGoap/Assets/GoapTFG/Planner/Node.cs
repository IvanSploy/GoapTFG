using System.Collections.Generic;
using GoapTFG.Base;

namespace GoapTFG.Planner
{
    /// <summary>
    /// Defines the Node used by the Planner Search.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">Value type</typeparam>
    public abstract class Node<TA, TB> : System.IComparable
    {
        //Properties
        public Node<TA, TB> Parent { get; set; }
        public Action<TA, TB> Action { get; set; }
        public int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        public List<Node<TA, TB>> Children { get; }
        public Goal<TA, TB> Goal { get; }
        
        //Fields
        protected readonly PropertyGroup<TA, TB> PropertyGroup;
        
        //Constructor
        protected Node(PropertyGroup<TA, TB> propertyGroup, Goal<TA, TB> goal)
        {
            PropertyGroup = propertyGroup;
            Goal = goal;
            Children = new List<Node<TA, TB>>();
            TotalCost = 0;
            ActionCount = 0;
            IsGoal = false;
        }

            //Used By Generator
        /// <summary>
        /// Applies an action to a Node and creates the Node that result.
        /// </summary>
        /// <param name="action">Action applied to the node.</param>
        /// <returns>Node result.</returns>
        public Node<TA, TB> ApplyAction(Action<TA, TB> action)
        {
            var pg = action.ApplyAction(PropertyGroup);
            return pg == null ? null : CreateChildNode(pg, Goal, action);
        }

        /// <summary>
        /// Do regressive apply for the current state and node.
        /// </summary>
        /// <param name="action">Action applied to the node.</param>
        /// <param name="goal">Goal to be modified</param>
        /// <param name="reached">If the node has no conflicts</param>
        /// <returns>Node result.</returns>
        public Node<TA, TB> ApplyRegressiveAction(Action<TA, TB> action, Goal<TA, TB> goal, out bool reached)
        {
            return CreateChildNode(action.ApplyRegresiveAction(PropertyGroup, ref goal, out reached), goal, action);
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="pg">Property Group</param>
        /// <param name="goal"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected abstract Node<TA, TB> CreateChildNode(PropertyGroup<TA, TB> pg, Goal<TA, TB> goal, Action<TA, TB> action);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="action">Action that leads to this node.</param>
        public void Update(Node<TA, TB> parent, Action<TA, TB> action)
        {
            //Se actualiza la accion de origen.
            Action = action;
            ActionCount = parent.ActionCount + 1;
            Update(parent);
        }
        
        /// <summary>
        /// Updates the Node to sets a new Parent and all the related info.
        /// </summary>
        /// <param name="parent"></param>
        public virtual void Update(Node<TA, TB> parent)
        {
            //Se define la relación padre hijo.
            Parent = parent;
            TotalCost = Action.GetCost();
            ActionCount = parent.ActionCount + 1;
        }

        public int CompareTo(object obj)
        {
            Node<TA, TB> objNode = (Node<TA, TB>)obj;
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

            Node<TA, TB> objNode = (Node<TA, TB>)obj;
            
            return PropertyGroup.Equals(objNode.PropertyGroup);
        }

        public override int GetHashCode()
        {
            return PropertyGroup.GetHashCode();
        }

        public override string ToString()
        {
            string text = "";
            if (Action == null) text += "Initial Node";
            else text += Action.Name;
            text += " | Costes: " + TotalCost + "\n";
            return text;
        }
        #endregion
    }
}