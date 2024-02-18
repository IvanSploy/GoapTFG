using System.IO;
using UGoap.Unity.ScriptableObjects;
using UnityEditor;
using UnityEngine;

namespace UGoap.Unity.CodeGenerator
{
    public static class PropertyGenerator
    {
        private const string CodeDirectory = "Assets/UGoap/Unity/Properties/";
        private const string CodeKeysName = "UGoapProperties.cs";
        private const string CodeValuesName = "UGoapPropertyValues.cs";
        private const string ReferenceKeysPath = "Assets/UGoap/Unity/Properties/UGoapPropertiesReference.txt";
        private const string ReferenceValuesPath = "Assets/UGoap/Unity/Properties/UGoapPropertyValuesReference.txt";

        public static void GenerateKeys(UGoapConfig config)
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
            
            string keys = "";
            foreach (var propertyKey in config.PropertyKeys)
            {
                keys += propertyKey + ",\n";
            }
            code = code.Replace("[propertyKeys]", keys);
            
            string types = "";
            for (var i = 0; i < config.PropertyKeys.Count; i++)
            {
                var propertyKey = config.PropertyKeys[i];
                var propertyType = config.PropertyTypes[i];
                types += "{ PropertyKey." + propertyKey + ", PropertyType." + propertyType + " },\n";
            }

            code = code.Replace("[propertyTypes]", types);
            
            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.Write(code);
            }
        }
        
        public static void GenerateValues(UGoapConfig config)
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
            
            string customEnums = "";
            foreach (var customEnum in config.Enums)
            {
                string enums = ", new [] { ";
                foreach (var enumName in customEnum.EnumValues)
                {
                    enums += "\"" + enumName + "\", ";
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