using UnityEngine;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity.ScriptableObjects;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Unity.Action
{
    [CreateAssetMenu(fileName = "GoTo", menuName = "LUGoap/Actions/GoTo")]
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
            if (iAgent is not Agent agent) return false;
            
            return true;
        }

        protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            if (iAgent is not Agent agent) return null;

            var targetName = (string)effects.TryGetOrDefault(TargetKey, "None").Value;
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
                if (token.IsCancellationRequested) return null;
                
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
                if (token.IsCancellationRequested) return null;
                    
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

            return effects;
        }
    }
}