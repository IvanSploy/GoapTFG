using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;

namespace UGoap.Planner
{
    public class Plan
    {
        public GoapState InitialState { get; private set; }
        public PlanNode Current { get; private set; }
        public Stack<PlanNode> ExecutedActions { get; } = new();
        public IGoapEntity CurrentEntity { get; private set; }
        public bool IsDone { get; set; }
        
        private readonly Stack<PlanNode> _nodes = new();
        private readonly IGoapAgent _agent;
        private CancellationTokenSource _cancellationTokenSource;

        public PlanNode Next => _nodes.Peek();

        //Constructor
        public Plan(GoapState initialState, IGoapAgent agent, Node finalNode)
        {
            InitialState = initialState;
            _agent = agent;
            
            //Get nodes
            Stack<PlanNode> aux = new();
            while (finalNode.Parent != null)
            {
                var planAction = new PlanNode(finalNode.Parent.Goal, finalNode.PreviousAction, finalNode.PreviousActionInfo);
                aux.Push(planAction);
                
                finalNode = finalNode.Parent;
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
            }

            //Stores the actions in order.
            while (aux.Count > 0)
            {
                _nodes.Push(aux.Pop());
            }
        }
        
        //Accesors
        public int Count => _nodes.Count;
        
        //Methods
        public Task<GoapState> ExecuteNext(GoapState currentState)
        {
            if (Count == 0)
            {
                IsDone = true;
                return null;
            }
            
            Current = _nodes.Pop();
            ExecutedActions.Push(Current);

            _cancellationTokenSource = new CancellationTokenSource();
            
            var result = Current.ExecuteAction(currentState, _agent, _cancellationTokenSource.Token);
            if (result == null) return null;
            
            result.ContinueWith(goapState =>
            {
                if (goapState.Result != null) DebugRecord.AddRecord(goapState.ToString());
            });
            
            return result;
        }

        public void Interrupt()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}