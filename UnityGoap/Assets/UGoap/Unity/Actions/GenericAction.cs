using System;
using System.Collections;
using System.Threading.Tasks;
using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [Serializable]
    public class GenericAction : GoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;

        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }
        
        public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent agent)
        {
            return true;
        }

        public override async Task<GoapState> Execute(GoapState goapState, IGoapAgent agent)
        {
            await Task.Delay(_waitSeconds);
            return goapState;
        }
    }
}