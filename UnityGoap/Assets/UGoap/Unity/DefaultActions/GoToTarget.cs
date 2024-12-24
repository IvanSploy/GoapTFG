using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity.ScriptableObjects;
using static UGoap.Base.PropertyManager;

namespace UGoap.Unity.Action
{
    [CreateAssetMenu(fileName = "GoTo", menuName = "UGoap/Actions/GoTo")]
    public class GoToTarget : ActionConfig<GoToTargetAction>
    {
        public PropertyKey TargetKey;
        public int SpeedFactor = 1;
        public string ExcludedLocation = "none";
        
        protected override GoToTargetAction Install(GoToTargetAction targetAction)
        {
            targetAction.TargetKey = TargetKey;
            targetAction.SpeedFactor = SpeedFactor;
            targetAction.ExcludedLocation = ExcludedLocation;
            return targetAction;
        }
    }
    
    public class GoToTargetAction : Base.Action
    {
        public PropertyKey TargetKey;
        public int SpeedFactor;
        public string ExcludedLocation;
        
        protected override Conditions GetProceduralConditions(ActionSettings settings)
        {
            var condition = new Conditions();
            condition.Set(TargetKey, BaseTypes.ConditionType.NotEqual, ExcludedLocation);
            return condition;
        }
        
        protected override Effects GetProceduralEffects(ActionSettings settings)
        {
            Effects proceduralEffects = new Effects();
            string target = ExcludedLocation;
            var targetList = settings.Goal.TryGetOrDefault(TargetKey, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == BaseTypes.ConditionType.Equal);
                if(condition != null) target = (string)condition.Value;
            }
            proceduralEffects[TargetKey] = new EffectValue(target, BaseTypes.EffectType.Set);
            return proceduralEffects;
        }
        
        public override int GetCost(Conditions goal)
        {
            if (!goal.Has(TargetKey)) return 50 / SpeedFactor;
            
            var target = (string) goal[TargetKey].First(condition => condition.ConditionType == BaseTypes.ConditionType.Equal).Value;

            var pos = WorkingMemoryManager.Get(target).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(Vector3.zero, pos) / SpeedFactor));
            return cost;
        }
        
        protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
        {
            if (iAgent is not UGoapAgent agent) return false;
            
            return true;
        }

        protected override async Task<State> OnExecute(State state, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            if (iAgent is not UGoapAgent agent) return null;

            var targetName = state.TryGetOrDefault(TargetKey, "None");
            UEntity targetEntity = WorkingMemoryManager.Get(targetName).Object;
            var target = targetEntity.transform.position;
            
            var t = agent.transform;

            target.y = t.position.y;
            var rotationTarget = Quaternion.LookRotation(target - t.position, Vector3.up);
            
            bool reached = false;

            var speed = agent.Speed * SpeedFactor;
            
            //Rotate
            while (!reached)
            {
                if (token.IsCancellationRequested)
                {
                    return state;
                }
                
                t.rotation = Quaternion.RotateTowards(t.rotation, rotationTarget, speed * 45 * Time.deltaTime );
                
                if (Vector3.Distance(t.eulerAngles, rotationTarget.eulerAngles) < float.Epsilon)
                {
                    reached = true;
                }
                await Task.Yield();
            }

            reached = false;
            
            //Move
            while (!reached)
            {
                if (token.IsCancellationRequested)
                {
                    return state;
                }
                var p = t.position;
                target.y = p.y;
                t.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);
                target.y = p.y;
                if (Vector3.Distance(t.position, target) < float.Epsilon)
                {
                    reached = true;
                }
                await Task.Yield();
            }

            return state;
        }
    }
}