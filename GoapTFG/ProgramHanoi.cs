using System;
using System.Collections.Generic;
using System.Diagnostics;
using GoapTFG.Base;
using GoapTFG.Planner;

namespace GoapTFG
{
    internal class Program
    {
        public const int PIECES = 5;
        public const int RODS = 3;

        public const string S_PIECE = "PIECE";
        public const string S_ROD = "ROD";

        public static readonly List<string> Hanoi = new List<string>();

        public static void Main(string[] args)
        {
            //INICIALIZACION VARIABLES HANOI
            const int HANOI = RODS + PIECES;
            for (var i = 1; i <= PIECES; i++) Hanoi.Add(S_PIECE + i);
            for (var i = 1; i <= RODS; i++) Hanoi.Add(S_ROD + i);

            //SITUACION INICIAL
            PropertyGroup<string, object> initialProperties = new PropertyGroup<string, object>();
            
            //Las piezas están encima de otras y del primer rod.
            for (var i = 0; i < PIECES; i++) initialProperties.Set("ON(" + Hanoi[i] + "," + Hanoi[i + 1] + ")", true);

            //La primera pieza está libre.
            initialProperties.Set("CLEAR(" + Hanoi[0] + ")", true);

            //El resto de rods menos el primero están libres.
            for (var i = PIECES + 1; i < HANOI; i++) initialProperties.Set("CLEAR(" + Hanoi[i] + ")", true);

            //SITUACION OBJETIVA
            PropertyGroup<string, object> goalProperties = new PropertyGroup<string, object>();
            
            //NO Es implicitamente necesario debido a las restricciones de las acciones.
            //Las piezas están encima de otras. 
            //for (var i = 0; i < PIECES - 1; i++) goalProperties.Set("ON(" + Hanoi[i] + "," + Hanoi[i + 1] + ")", true);

            //La ultima pieza debe estar sobre la ultima varilla.
            goalProperties.Set("ON(" + Hanoi[PIECES - 1] + "," + Hanoi[HANOI - 1] + ")", true);

            //La primera pieza está libre
            goalProperties.Set("CLEAR(" + Hanoi[0] + ")", true);

            //Las primeras varillas están libres.
            for (var i = PIECES; i < HANOI - 1; i++) goalProperties.Set("CLEAR(" + Hanoi[i] + ")", true);

            Console.Out.WriteLine(initialProperties);
            Console.Out.WriteLine(goalProperties);
            
            Goal<string, object> goal = new Goal<string, object>(goalProperties, 0);
            NodeGenerator<string, object> planner = new NodeGenerator<string, object>();

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
                        
                        actions.Add(new Base.Action<string, object>("MOVE(" + Hanoi[i] + "," + Hanoi[j] + "," + Hanoi[k] + ")", preCond, effects));
                    }
                }
            }
            
            //USO DE UN AJENTE.
            Agent<string, object> agent = new Agent<string, object>(planner, goal, actions);
            Stopwatch timer = new Stopwatch();
            timer.Start();
            agent.CreatePlan(initialProperties);
            timer.Stop();
            Console.WriteLine("Tiempo total: " + timer.ElapsedMilliseconds + "\n");
            agent.DoPlan();
            Console.WriteLine("Total de acciones del plan: " + agent.Count());
        }
    }
}