using System.IO;
using GoapTFG.UGoap.ScriptableObjects;
using UnityEditor;

namespace GoapTFG.UGoap.CodeGenerator
{
    public static class EnumGenerator
    {
        private const string GeneratedCodePath = "Assets/GoapTFG/Unity/CodeGenerator/Enums/";
        private const string GeneratedNamespace = "GoapTFG.Unity.CodeGenerator.Enums";
        private const string ScriptableObjectsPath = "Assets/GoapItems";
        
        [MenuItem("GoapTFG/GenerateGoals")]
        public static void CreateGoalEnum()
        {
            var guids = AssetDatabase.FindAssets("t: goalscriptableobject", new[]{ ScriptableObjectsPath });

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                UGoapGoal obj = AssetDatabase.LoadAssetAtPath<UGoapGoal>(path);
                guids[i] = obj.name.Replace(" ", "");
            }

            GenerateEnum("GoalName", guids);
        }
        
        private static void GenerateEnum(string enumName, string[] enumEntries)
        {
            if(!Directory.Exists(GeneratedCodePath)) Directory.CreateDirectory(GeneratedCodePath);
            
            string filePathAndName = GeneratedCodePath + enumName + ".cs";
            
            using (StreamWriter streamWriter = new StreamWriter(filePathAndName))
            {
                streamWriter.WriteLine("namespace " + GeneratedNamespace);
                streamWriter.WriteLine("{");
                streamWriter.WriteLine("\tpublic enum " + enumName);
                streamWriter.WriteLine("\t{");
                for (int i = 0; i < enumEntries.Length; i++)
                {
                    streamWriter.WriteLine("\t" + "\t" + enumEntries[i] + ",");
                }

                streamWriter.WriteLine("\t}");
                streamWriter.WriteLine("}");
            }
            AssetDatabase.Refresh();
        }
    }
}