using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoToAction", menuName = "Goap Items/Actions/GoToAction", order = 3)]
    public class GoToAction : GoapActionSO
    {
        public GoToAction() : base() { }

        protected override void PerformedActions(PropertyGroup<PropertyList, object> worldState)
        { }
    }
}