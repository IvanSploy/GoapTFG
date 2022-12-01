namespace GoapHanoi.Planner
{
    //Handles the GOAP planification and is who realices the actions.
    public class Agent<TA, TB>
    {
        private NodeGenerator<TA, TB> _planner;

        public Agent(NodeGenerator<TA, TB> planner)
        {
            _planner = planner;
        }

        public void OnUpdate()
        {
            
        }
    }
}