using System.Collections.Generic;
using System.IO;
using QGoap.Base;
using QGoap.Unity.ScriptableObjects;
using UnityEngine;

namespace QGoap.Unity.Utils
{
    public static class PropertyGenerator
    {
        private const string CodeDirectory = "Assets/QGoap/Base/Properties/";
        private const string CodeKeysName = "Properties.cs";
        private const string CodeValuesName = "PropertyEnums.cs";
        private const string ReferenceKeysPath = "Assets/QGoap/Base/Properties/PropertiesReference.txt";
        private const string ReferenceValuesPath = "Assets/QGoap/Base/Properties/PropertyEnumsReference.txt";

        public static void GenerateProperties(PropertyConfig[] properties)
        {
            if (!File.Exists(ReferenceKeysPath))
            {
                Debug.LogError("Reference file does not exist, code not generated.");
                return;
            }
            
            if(!Directory.Exists(CodeDirectory)) Directory.CreateDirectory(CodeDirectory);
            string filePathAndName = CodeDirectory + CodeKeysName;

            string code = "";
            using(StreamReader streamReader = new StreamReader(ReferenceKeysPath))
            {
                code += streamReader.ReadToEnd();
            }
            
            var addedKeys = new HashSet<string>();
            string keys = "";
            string types = "";
            foreach (var property in properties)
            {
                if (!addedKeys.Add(property.Key)) continue;
                keys += property.Key.Replace(" ", string.Empty) + ",\n";
                types += "{ PropertyKey." + property.Key.Replace(" ", string.Empty) + ", PropertyType." + property.Type + " },\n";
            }
            
            code = code.Replace("[propertyKeys]", keys);
            code = code.Replace("[propertyTypes]", types);
            
            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.Write(code);
            }
        }
        
        public static void GenerateEnums(EnumConfig[] enumConfigs)
        {
            if (!File.Exists(ReferenceValuesPath))
            {
                Debug.LogError("Reference file does not exist, code not generated.");
                return;
            }
            
            if(!Directory.Exists(CodeDirectory)) Directory.CreateDirectory(CodeDirectory);
            string filePathAndName = CodeDirectory + CodeValuesName;

            string code = "";
            using(StreamReader streamReader = new StreamReader(ReferenceValuesPath))
            {
                code += streamReader.ReadToEnd();
            }
            
            var addedKeys = new HashSet<PropertyManager.PropertyKey>();
            string customEnums = "";
            foreach (var customEnum in enumConfigs)
            {
                if(!addedKeys.Add(customEnum.EnumType)) continue;
                string enums = ", new [] { ";
                foreach (var enumName in customEnum.EnumValues)
                {
                    enums += "\"" + enumName.Replace(" ", string.Empty) + "\", ";
                }
                enums += "}";
                customEnums += "{ PropertyKey." + customEnum.EnumType + enums + "},\n";
            }
            code = code.Replace("[enumNames]", customEnums);
            
            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.Write(code);
            }
        }
    }
}