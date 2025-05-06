using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Learning;

namespace QGoap.Planning
{
    public class Plan
    {
        public IAgent _agent;
        public NodeAction First { get; private set; }
        public NodeAction Current { get; private set; }
        public Stack<NodeAction> ExecutedActions { get; } = new();
        public IEntity CurrentEntity { get; private set; }
        public bool IsCompleted { get; set; }
        
        private readonly Stack<NodeAction> _nodes = new();
        private CancellationTokenSource _cancellationTokenSource;
        private readonly Stopwatch _stopwatch = new();

        public NodeAction? Previous => ExecutedActions.Count > 0 ? ExecutedActions.Peek() : null;
        public NodeAction? Next => _nodes.Count > 0 ? _nodes.Peek() : null;
        public int Count => _nodes.Count;

        public Plan(IAgent agent, Node finalNode)
        {
            _agent = agent;
            
            //Get nodes
            Stack<NodeAction> aux = new();
            while (finalNode.Parent != null)
            {
                aux.Push(finalNode.ActionData);
                finalNode = finalNode.Parent;
                //Debug.Log("Estado: " + nodeGoal.State + "| Goal: " + nodeGoal.Goal);
            }

            //Stores the actions in order.
            while (aux.Count > 0)
            {
                _nodes.Push(aux.Pop());
            }
        }
        
        public async Task<EffectGroup> ExecuteNext()
        {
            if (Current.Action != null) ExecutedActions.Push(Current);

            Current = _nodes.Pop();
            if (First.Action == null) First = Current;

            _stopwatch.Start();
            try
            {
                return await ExecuteCurrent();
            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        public void Finish(State state)
        {
            DebugRecord.Record(state != null ? state.ToString() : "Plan failed.");
            if (!IsCompleted && state != null && Count == 0) IsCompleted = true;
            ApplyReward(state);
            _stopwatch.Stop();
        }
        
        public void ApplyReward(State state)
        {
            if (_agent is not ILearningAgent { Learning: not null } learningAgent) return;
            
            if (state != null)
            {
                var reward = -((int)Math.Round(_stopwatch.ElapsedMilliseconds / 1000f) + 1);

                var nextLearningCode = Current.GlobalLearningCode;
                if (Previous.HasValue) nextLearningCode = Previous.Value.GlobalLearningCode;

                //Default update learning.
                learningAgent.Learning.Update(Current.GlobalLearningCode, Current.Action.Name,
                    reward, nextLearningCode);

                //Plan succeed
                if (Count == 0)
                {
                    learningAgent.Learning.Update(First.GlobalLearningCode,
                        First.Action.Name, learningAgent.Learning.SucceedReward);

                    if (learningAgent.ApplyRewardsToLocal)
                    {
                        foreach (var node in ExecutedActions)
                        {
                            if (node.Action is LearningAction learningAction)
                            {
                                learningAction.ApplyReward(node.LocalLearningCode, node.Parameters, learningAgent.Learning.SucceedReward);
                            }
                        }
                    }
                }
            }
            //Plan fail
            else
            {
                learningAgent.Learning.Update(Current.GlobalLearningCode,
                    Current.Action.Name, learningAgent.Learning.FailReward, 0);

                if (learningAgent.ApplyRewardsToLocal)
                {
                    foreach (var node in ExecutedActions)
                    {
                        if (node.Action is LearningAction learningAction)
                        {
                            learningAction.ApplyReward(node.LocalLearningCode, node.Parameters, learningAgent.Learning.FailReward);
                        }
                    }
                }
            }
        }

        public bool VerifyCurrent()
        {
            var isConflict = Current.ConditionGroup.HasConflict(_agent.CurrentState);
            if (isConflict) return false;

            var state = _agent.CurrentState + Current.EffectGroup;
            foreach (var nextAction in _nodes)
            {
                isConflict = nextAction.ConditionGroup.HasConflict(state);
                if (isConflict) return false;
                state += nextAction.EffectGroup;
            }

            return !_agent.CurrentGoal.ConditionGroup.HasConflict(state);
        }
        
        private Task<EffectGroup> ExecuteCurrent()
        {
            if (!CurrentIsValid()) return Task.FromResult<EffectGroup>(null);

            _cancellationTokenSource = new CancellationTokenSource();
            return Current.Action.Execute(Current.EffectGroup, Current.Parameters, _cancellationTokenSource.Token);
        }
        
        private bool CurrentIsValid()
        {
            if (!Current.ConditionGroup.HasConflict(_agent.CurrentState))
            {
                var finalState = _agent.CurrentState + Current.EffectGroup;
                bool valid = Current.Action.Validate(finalState, Current.Parameters);
                if (!valid)
                {
                    DebugRecord.Record("[GOAP] Plan detenido. La acción no ha podido completarse.");
                }
                return valid;
            }
            
            DebugRecord.Record("[GOAP] Plan detenido. El agente no cumple con las precondiciones necesarias.");
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        public void Interrupt()
        {
            _cancellationTokenSource?.Cancel();
        }
    }
}