using System.IO;
using UGoap.Unity.ScriptableObjects;
using UnityEditor;

namespace UGoap.Unity.CodeGenerator
{
    public static class EnumGenerator
    {
        private const string GeneratedCodePath = "Assets/GoapTFG/Unity/CodeGenerator/Enums/";
        private const string GeneratedNamespace = "GoapTFG.Unity.CodeGenerator.Enums";
        private const string ScriptableObjectsPath = "Assets/GoapItems";
        
        [MenuItem("UGoap/GenerateGoals")]
        public static void CreateGoalEnum()
        {
            var guids = AssetDatabase.FindAssets("t: goalscriptableobject", new[]{ ScriptableObjectsPath });

            for (var i = 0; i < guids.Length; i++)
            {
                var guid = guids[i];
                var path = AssetDatabase.GUIDToAssetPath(guid);
                UGoapPriorityGoal obj = AssetDatabase.LoadAssetAtPath<UGoapPriorityGoal>(path);
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