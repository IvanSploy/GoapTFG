using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;

namespace UGoap.Planner
{
    public class Plan
    {
        public GoapState InitialState { get; private set; }
        public Node CurrentNode { get; private set; }
        public Stack<Node> ExecutedNodes { get; } = new();
        public IGoapEntity CurrentEntity { get; private set; }
        public bool IsDone { get; private set; }
        
        private readonly Stack<Node> _nodes = new();
        private readonly IGoapAgent _agent;

        private CancellationTokenSource _cancellationTokenSource;

        //Constructor
        public Plan(GoapState initialState, IGoapAgent agent, Node finalNode)
        {
            InitialState = initialState;
            _agent = agent;
            
            //Get nodes
            Stack<Node> aux = new();
            while (finalNode.Parent != null)
            {
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
                aux.Push(finalNode);
                finalNode = finalNode.Parent;
            }

            //Stores the nodes in order.
            while (aux.Count > 0)
            {
                _nodes.Push(aux.Pop());
            }
        }
        
        //Accesors
        public int Count => _nodes.Count;
        
        //Methods
        public async Task<GoapState> ExecuteNext(GoapState currentState)
        {
            if (Count == 0)
            {
                IsDone = true;
                return null;
            }
            
            CurrentNode = _nodes.Pop();
            ExecutedNodes.Push(CurrentNode);

            _cancellationTokenSource = new CancellationTokenSource();
            
            var result = await Task.Run(() =>
            {
                var task = CurrentNode.ExecuteAction(currentState, _agent, _cancellationTokenSource.Token);
                while (!task.IsCompleted) Task.Yield();
                return task.Result;
            });
            
            if (currentState != null) 
                DebugRecord.AddRecord(currentState.ToString());
            
            return result;
        }

        public void Interrupt(bool goalReached)
        {
            IsDone = goalReached;
            _cancellationTokenSource.Cancel();
        }
    }
}