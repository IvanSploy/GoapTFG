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
        public GoapConditions Goal { get; set; }
        public Node Parent { get; set; }
        public List<Node> Children { get; }
        public IGoapAction PreviousAction { get; set; }
        public virtual int TotalCost { get; set; }
        public int ActionCount => Parent != null ? Parent.ActionCount + 1 : 0;

        public bool IsGoal(GoapState state) => !Goal.CheckConflict(state);

        //Fields
        protected readonly INodeGenerator NodeGenerator;
        protected readonly Func<GoapConditions,GoapState,int> CustomHeuristic;
        protected readonly IQLearning QLearning;

        public bool UseLearning => QLearning != null;
        
        //Constructor
        protected Node(INodeGenerator nodeGenerator, GoapConditions goal, Func<GoapConditions,GoapState,int> customHeuristic)
        {
            NodeGenerator = nodeGenerator;
            Goal = goal;
            CustomHeuristic = customHeuristic;
            Children = new List<Node>();
        }
        
        protected Node(INodeGenerator nodeGenerator, GoapConditions goal, IQLearning qLearning)
        {
            NodeGenerator = nodeGenerator;
            Goal = goal;
            QLearning = qLearning;
            Children = new List<Node>();
        }

            //Used By Generator
        /// <summary>
        /// Do mixed apply for the current state and node.
        /// </summary>
        /// <param name="currentGoapState">The current state of the research.</param>
        /// <param name="goapAction">PreviousAction applied to the node.</param>
        /// <returns>Node result and unchecked conditions.</returns>
        public Node ApplyAction(IGoapAction goapAction)
        {
            var newGoal = Goal.ApplyAction(goapAction);
            return newGoal == null ? null : CreateChildNode(newGoal, goapAction);
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="goapState">Property Group</param>
        /// <param name="goapGoal"></param>
        /// <param name="goapAction"></param>
        /// <param name="cost">Custom cost</param>
        /// <returns></returns>
        protected abstract Node CreateChildNode(GoapConditions goal, IGoapAction action);

        /// <summary>
        /// Update the info related to the parent and the action that leads to this node.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="goapAction">PreviousAction that leads to this node.</param>
        public void Update(Node parent, IGoapAction goapAction)
        {
            //Se actualiza la accion de origen y el objetivo.
            PreviousAction = goapAction;
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
            TotalCost = PreviousAction.GetCost(parent.Goal);
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
            
            return Goal.Equals(objNode.Goal);
        }

        public override int GetHashCode()
        {
            return Goal.GetHashCode();
        }
        #endregion
    }
}