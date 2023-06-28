using System.Collections.Generic;
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
        public IGoapAction<TKey, TValue> GoapAction { get; set; }
        public int TotalCost { get; set; }
        public int ActionCount { get; set; }
        public bool IsGoal { get; set; }
        public List<Node<TKey, TValue>> Children { get; }
        public GoapGoal<TKey, TValue> GoapGoal { get; }
        
        //Fields
        protected readonly PropertyGroup<TKey, TValue> PropertyGroup;
        
        //Constructor
        protected Node(PropertyGroup<TKey, TValue> propertyGroup, GoapGoal<TKey, TValue> goapGoal)
        {
            PropertyGroup = propertyGroup;
            GoapGoal = goapGoal;
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
            var pg = goapAction.ApplyAction(new GoapStateInfo<TKey, TValue>(PropertyGroup, GoapGoal));
            return pg == null ? null : CreateChildNode(pg, GoapGoal, goapAction);
        }

        /// <summary>
        /// Do regressive apply for the current state and node.
        /// </summary>
        /// <param name="goapAction">Action applied to the node.</param>
        /// <param name="goapGoal">Goal to be modified</param>
        /// <param name="reached">If the node has no conflicts</param>
        /// <returns>Node result.</returns>
        public Node<TKey, TValue> ApplyRegressiveAction(IGoapAction<TKey, TValue> goapAction, GoapGoal<TKey, TValue> goapGoal, out bool reached)
        {
            var stateCreated = goapAction.ApplyRegressiveAction(new GoapStateInfo<TKey, TValue>(PropertyGroup, goapGoal), out reached);
            return stateCreated == null ? null : CreateChildNode(stateCreated.WorldState, stateCreated.CurrentGoal, goapAction);
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
            //Se actualiza la accion de origen.
            GoapAction = goapAction;
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
            TotalCost = GoapAction.GetCost(new GoapStateInfo<TKey, TValue>(PropertyGroup, GoapGoal));
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