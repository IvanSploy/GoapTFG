using System;
using System.Collections.Generic;
using UGoap.Unity.CodeGenerator;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Config", menuName = "Goap Items/Config", order = 0)]
    public class UGoapConfig : ScriptableObject
    {
        public List<string> PropertyKeys;
        
        public List<PropertyType> PropertyTypes;
        public List<EnumConfig> Enums;
        
        
        [ContextMenu("GenerateKeys")]
        public void GenerateKeys()
        {
            PropertyGenerator.GenerateKeys(this);
        }
        
        [ContextMenu("GenerateValues")]
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