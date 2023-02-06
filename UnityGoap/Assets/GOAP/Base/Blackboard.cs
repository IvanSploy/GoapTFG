using System.Collections.Generic;

namespace GoapTFG.Base
{
    /// <summary>
    /// Represents the update info of the objects in the world.
    /// </summary>
    /// <typeparam name="TA">Vector3 class</typeparam>
    /// <typeparam name="TB">Object class</typeparam>
    public class Blackboard<TA, TB>
    {
        private List<MemoryFact<TA, TB>> _facts;
        
    }
    
    
}