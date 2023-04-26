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
        public GoapAction<TA, TB> GoapAction { get; set; }
        public int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        public List<Node<TA, TB>> Children { get; }
        public GoapGoal<TA, TB> GoapGoal { get; }
        
        //Fields
        protected readonly PropertyGroup<TA, TB> PropertyGroup;
        
        //Constructor
        protected Node(PropertyGroup<TA, TB> propertyGroup, GoapGoal<TA, TB> goapGoal)
        {
            PropertyGroup = propertyGroup;
            GoapGoal = goapGoal;
            Children = new List<Node<TA, TB>>();
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
        public Node<TA, TB> ApplyAction(GoapAction<TA, TB> goapAction)
        {
            var pg = goapAction.ApplyAction(PropertyGroup);
            return pg == null ? null : CreateChildNode(pg, GoapGoal, goapAction);
        }

        /// <summary>
        /// Do regressive apply for the current state and node.
        /// </summary>
        /// <param name="goapAction">Action applied to the node.</param>
        /// <param name="goapGoal">Goal to be modified</param>
        /// <param name="reached">If the node has no conflicts</param>
        /// <returns>Node result.</returns>
        public Node<TA, TB> ApplyRegressiveAction(GoapAction<TA, TB> goapAction, GoapGoal<TA, TB> goapGoal, out bool reached)
        {
            var pg = goapAction.ApplyRegressiveAction(PropertyGroup, ref goapGoal, out reached);
            return pg == null ? null : CreateChildNode(pg, goapGoal, goapAction);
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="pg">Property Group</param>
        /// <param name="goapGoal"></param>
        /// <param name="goapAction"></param>
        /// <returns></returns>
        protected abstract Node<TA, TB> CreateChildNode(PropertyGroup<TA, TB> pg, GoapGoal<TA, TB> goapGoal, GoapAction<TA, TB> goapAction);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">Action that leads to this node.</param>
        public void Update(Node<TA, TB> parent, GoapAction<TA, TB> goapAction)
        {
            //Se actualiza la accion de origen.
            GoapAction = goapAction;
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
            TotalCost = GoapAction.GetCost();
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
            if (GoapAction == null) text += "Initial Node";
            else text += GoapAction.Name;
            text += " | Costes: " + TotalCost + "\n";
            return text;
        }
        #endregion
    }
}