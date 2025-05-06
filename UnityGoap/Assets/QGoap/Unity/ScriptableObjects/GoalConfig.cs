using System.Collections.Generic;
using QGoap.Base;
using UnityEngine;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "QGoap/Goal")]
    public class GoalConfig : ScriptableObject
    {
        [HideInInspector] public List<ConditionProperty> Properties;
        
        public Goal Create()
        {
            ConditionGroup group = new();
            group.ApplyProperties(Properties);
            return new Goal(name, group);
        }
    }
}
