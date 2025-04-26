using System.Collections;
using System.Collections.Generic;
using static QGoap.Base.PropertyManager;

namespace QGoap.Base
{
    public class Goal : IEnumerable<KeyValuePair<PropertyManager.PropertyKey, Condition>>
    {
        //Properties
        public ConditionGroup ConditionGroup { get; }
        public string Name { get; }
        
        //Constructors
        public Goal(string name, ConditionGroup conditionGroup)
        {
            ConditionGroup = new ConditionGroup(conditionGroup);
            Name = name;
        }
        
        //GOAP Utilites
        public bool IsEmpty() => ConditionGroup.IsEmpty();
        public bool IsGoal (State state) => !ConditionGroup.HasConflict(state);
        public bool Has(PropertyManager.PropertyKey key) => ConditionGroup.Has(key);
        public Condition Get(PropertyManager.PropertyKey key) => ConditionGroup.Get(key);

        //Operators
        public Condition this[PropertyManager.PropertyKey key] => ConditionGroup[key];

        public override string ToString()
        {
            return "Goal: " + Name + "\n" + ConditionGroup;
        }

        public IEnumerator<KeyValuePair<PropertyManager.PropertyKey, Condition>> GetEnumerator()
        {
            return ConditionGroup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ConditionGroup.GetEnumerator();
        }
    }
}