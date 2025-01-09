using System;
using System.Collections.Generic;
using LUGoap.Unity.Utils;
using UnityEngine;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Config", menuName = "LUGoap/Config", order = -10)]
    public class MainConfig : ScriptableObject
    {
        public PropertyConfig[] Properties;
        public EnumConfig[] Enums;
        
        [ContextMenu("Generate Properties")]
        public void GenerateProperties()
        {
            PropertyGenerator.GenerateProperties(Properties);
        }
        
        [ContextMenu("Generate Enums")]
        public void GenerateEnums()
        {
            PropertyGenerator.GenerateEnums(Enums);
        }
    }

    [Serializable]
    public struct PropertyConfig
    {
        public string Key;
        public PropertyType Type;
    }
    
    [Serializable]
    public struct EnumConfig
    {
        public PropertyKey EnumType;
        public List<string> EnumValues;
    }
}