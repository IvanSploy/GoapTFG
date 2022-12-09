using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;

namespace GoapTFG
{
    internal class Program
    {
        public const int PIECES = 5;
        public const int RODS = 3;

        public static readonly string[] Hanoi =
        {
            "XS",
            "S",
            "M",
            "L",
            "XL",
            "Rod1",
            "Rod2",
            "Rod3"
        };
        
        public static void Main(string[] args)
        {
            /*PropertyGroup<string, bool> propertyGroupA = new PropertyGroup<string, bool>();
            propertyGroupA.Set("puertaAbierta", true);
            
            PropertyGroup<string, bool> propertyGroupB = new PropertyGroup<string, bool>();
            propertyGroupB.Set("puertaAbierta", false);

            Base.Action<string, bool> actionA = new Base.Action<string, bool>("cerrar",propertyGroupA, propertyGroupB);
            Base.Action<string, bool> actionB = new Base.Action<string, bool>("abrir",propertyGroupB, propertyGroupA);
            
            actionA.CheckApplyAction(propertyGroupA);
            actionA.CheckApplyAction(propertyGroupB);
            actionB.CheckApplyAction(propertyGroupA);
            actionB.CheckApplyAction(propertyGroupB);
            */

            //Hanoi variables
            const int HANOI = RODS + PIECES;
            
            //Se define la situación inicial.
            PropertyGroup<string, object> initialProperties = new PropertyGroup<string, object>();
            initialProperties.Set("ON(" + Hanoi[0] + "," + Hanoi[1] + ")", true);
            initialProperties.Set("ON(" + Hanoi[1] + "," + Hanoi[2] + ")", true);
            initialProperties.Set("ON(" + Hanoi[2] + "," + Hanoi[3] + ")", true);
            initialProperties.Set("ON(" + Hanoi[3] + "," + Hanoi[4] + ")", true);
            initialProperties.Set("ON(" + Hanoi[4] + "," + Hanoi[5] + ")", true);
            initialProperties.Set("CLEAR(" + Hanoi[0] + ")", true);
            initialProperties.Set("CLEAR(" + Hanoi[6] + ")", true);
            initialProperties.Set("CLEAR(" + Hanoi[7] + ")", true);
            
            //Se define la situación objetivo
            PropertyGroup<string, object> goalProperties = new PropertyGroup<string, object>();
            goalProperties.Set("ON(" + Hanoi[0] + "," + Hanoi[1] + ")", true);
            goalProperties.Set("ON(" + Hanoi[1] + "," + Hanoi[2] + ")", true);
            goalProperties.Set("ON(" + Hanoi[2] + "," + Hanoi[3] + ")", true);
            goalProperties.Set("ON(" + Hanoi[3] + "," + Hanoi[4] + ")", true);
            goalProperties.Set("ON(" + Hanoi[4] + "," + Hanoi[7] + ")", true);
            goalProperties.Set("CLEAR(" + Hanoi[0] + ")", true);
            goalProperties.Set("CLEAR(" + Hanoi[5] + ")", true);
            goalProperties.Set("CLEAR(" + Hanoi[6] + ")", true);
            
            Console.Out.WriteLine(initialProperties);
            Console.Out.WriteLine(goalProperties);
            
            Goal<string, object> goal = new Goal<string, object>(goalProperties, 0);
            NodeGenerator<string, object> planner = new NodeGenerator<string, object>(initialProperties, goal);
            Agent<string, object> agent = new Agent<string, object>(planner);
            
            //Acciones disponibles por el agente.
            List<Base.Action<string, object>> actions = new List<Base.Action<string, object>>();

            //Move(b,x,y)
            //b -> piece to be move.
            //x -> piece under b.
            //y -> piece where b is going to be moved
            for (int i = 0; i < PIECES; i++) //Rods cannot be moved.
            {
                for (int j = 0; j < HANOI; j++) 
                {
                    if(i == j) continue; //i, j, k must be different.
                    for (int k = i + 1; k < HANOI; k++) //The piece has to be smaller than the piece over to be moved.
                    {
                        if(j == k) continue; //i, j, k must be different.
                        PropertyGroup<string, object> preCond = new PropertyGroup<string, object>();
                        PropertyGroup<string, object> effects = new PropertyGroup<string, object>();
                        preCond.Set("CLEAR(" + Hanoi[i] + ")", true);
                        preCond.Set("ON(" + Hanoi[i] + "," + Hanoi[j] + ")", true);
                        preCond.Set("CLEAR(" + Hanoi[k] + ")", true);
                        
                        effects.Set("CLEAR(" + Hanoi[j] + ")", true);
                        effects.Set("ON(" + Hanoi[i] + "," + Hanoi[k] + ")", true);
                        effects.Set("CLEAR(" + Hanoi[k] + ")", false);
                        effects.Set("ON(" + Hanoi[i] + "," + Hanoi[j] + ")", false);
                        
                        actions.Add(new Base.Action<string, object>("MOVE(" + i + "," + j + "," + k + ")", preCond, effects));
                    }
                }
            }
            
            agent.AddActions(actions);
            agent.GetPlan();
            agent.DoPlan();
        }
    }
}