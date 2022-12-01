using System;
using System.Collections.Generic;
using GoapHanoi.Base;

namespace GoapHanoi
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            PropertyGroup<string, bool> propertyGroupA = new PropertyGroup<string, bool>();
            propertyGroupA.Set("puertaAbierta", true);
            
            PropertyGroup<string, bool> propertyGroupB = new PropertyGroup<string, bool>();
            propertyGroupB.Set("puertaAbierta", false);

            Base.Action<string, bool> actionA = new Base.Action<string, bool>("cerrar",propertyGroupA, propertyGroupB);
            Base.Action<string, bool> actionB = new Base.Action<string, bool>("abrir",propertyGroupB, propertyGroupA);
            
            actionA.CheckApplyAction(propertyGroupA);
            actionA.CheckApplyAction(propertyGroupB);
            actionB.CheckApplyAction(propertyGroupA);
            actionB.CheckApplyAction(propertyGroupB);
            
            Console.Out.Write(propertyGroupA + propertyGroupB);
            Console.Out.Write(propertyGroupB + propertyGroupA);
        }
    }
}