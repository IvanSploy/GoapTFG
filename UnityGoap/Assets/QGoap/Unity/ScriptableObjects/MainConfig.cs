using System;
using System.Collections.Generic;
using QGoap.Unity.Utils;
using UnityEngine;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MainConfig", menuName = "QGoap/Main Config", order = -11)]
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