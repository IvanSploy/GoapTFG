using System;
using System.Collections.Generic;
using GoapHanoi.Core;

namespace GoapHanoi
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            State<string, bool> stateA = new State<string, bool>();
            stateA.Set("puertaAbierta", true);
            
            State<string, bool> stateB = new State<string, bool>();
            stateB.Set("puertaAbierta", false);

            Core.Action<string, bool> actionA = new Core.Action<string, bool>("cerrar",stateA, stateB);
            Core.Action<string, bool> actionB = new Core.Action<string, bool>("abrir",stateB, stateA);
            
            actionA.CheckApplyAction(stateA);
            actionA.CheckApplyAction(stateB);
            actionB.CheckApplyAction(stateA);
            actionB.CheckApplyAction(stateB);
        }
    }
}