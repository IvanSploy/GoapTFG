using System;
using System.Collections.Generic;
using UGoap.Unity.Utils;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Config", menuName = "UGoap/Config", order = 0)]
    public class MainConfig : ScriptableObject
    {
        public List<string> PropertyKeys;
        
        public List<PropertyType> PropertyTypes;
        public List<EnumConfig> Enums;
        
        
        [ContextMenu("GenerateProperties")]
        public void GenerateKeys()
        {
            PropertyGenerator.GenerateKeys(this);
        }
        
        [ContextMenu("GenerateEnums")]
        public void GenerateValues()
        {
            PropertyGenerator.GenerateValues(this);
        }
    }

    [Serializable]
    public struct EnumConfig
    {
        public PropertyKey EnumType;
        public List<string> EnumValues;
    }
}