using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
        public GoapState InitialState { get; }
        public GoapConditions Goal { get; }
        public Node Parent { get; private set; }
        public List<Node> Children { get; private set; }
        public GoapAction PreviousAction { get; private set; }
        public GoapActionInfo PreviousActionInfo { get; private set; }
        public virtual int TotalCost { get; private set; }
        public int ActionCount => Parent != null ? Parent.ActionCount + 1 : 0;
        public GoapSettings Settings { get; private set; }

        public bool IsGoal(GoapState state) => !Goal.CheckConflict(state);
        public bool UseLearning => LearningConfig != null;

        //Fields
        protected readonly INodeGenerator NodeGenerator;
        protected readonly Func<GoapConditions,GoapState,int> CustomHeuristic;
        protected readonly ILearningConfig LearningConfig;
        
        //Constructor
        protected Node(INodeGenerator nodeGenerator, GoapState initialState, GoapConditions goal, Func<GoapConditions,GoapState,int> customHeuristic)
        {
            NodeGenerator = nodeGenerator;
            InitialState = initialState;
            Goal = goal;
            CustomHeuristic = customHeuristic;
            Children = new List<Node>();
            CreateSettings();
        }
        
        protected Node(INodeGenerator nodeGenerator, GoapState initialState, GoapConditions goal, ILearningConfig learningConfig)
        {
            NodeGenerator = nodeGenerator;
            InitialState = initialState;
            Goal = goal;
            this.LearningConfig = learningConfig;
            Children = new List<Node>();
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
        /// <param name="currentGoapState">The current state of the research.</param>
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

           return resultGoal == null ? null : CreateChildNode(resultGoal, action, actionInfo);
        }
        
        private bool CheckAction(GoapState state, IGoapAgent agent)
        {
            if (!PreviousActionInfo.Conditions.CheckConflict(state))
            {
                bool valid = PreviousAction.Validate(state, PreviousActionInfo, agent);
                if (!valid)
                {
                    DebugRecord.AddRecord("La acción no ha podido completarse, plan detenido :(");
                }
                return valid;
            }
            
            DebugRecord.AddRecord("El agente no cumple con las precondiciones necesarias, plan detenido :(");
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        /// <summary>
        /// To execute an action related to a certain node.
        /// </summary>
        /// <param name="goapAction"></param>
        /// <param name="state"></param>
        /// <param name="agent"></param>
        /// <returns></returns>
        public Task<GoapState> ExecuteAction(GoapState state, IGoapAgent agent, CancellationToken token)
        {
            if (!CheckAction(state, agent))
                return null;

            state += PreviousActionInfo.Effects;
            return PreviousAction.Execute(state, agent, token);
        }

        /// <summary>
        /// Performs the creation of a new Node based on an existent PG.
        /// </summary>
        /// <param name="goapState">Property Group</param>
        /// <param name="goapGoal"></param>
        /// <param name="goapAction"></param>
        /// <param name="cost">Custom cost</param>
        /// <returns></returns>
        protected abstract Node CreateChildNode(GoapConditions goal, GoapAction action, GoapActionInfo actionInfo);

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