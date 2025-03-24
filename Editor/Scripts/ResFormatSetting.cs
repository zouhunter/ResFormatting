using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UFrame.ResFormat
{
    [FilePath("ProjectSettings/ResFormatSetting.asset", FilePathAttribute.Location.ProjectFolder)]
    public class ResFormatSetting : UnityEditor.ScriptableSingleton<ResFormatSetting>
    {
         public  string proxyAssetDir;
        public  string proxyTexturePath;
        public  string proxyModelPath;
        public  string proxyAudioPath;

        public List<AssetAuditor.AssetRule> assetRules = new List<AssetAuditor.AssetRule>();
        public void AddRule(AssetAuditor.AssetRule rule)
        {
            if (string.IsNullOrEmpty(rule.RuleName))
            {
                Debug.LogError("RuleName is null or empty");
                return;
            }
            var index = assetRules.FindIndex(x => x.RuleName == rule.RuleName);
            if (index < 0)
            {
                assetRules.Add(rule);
            }
            else
            {
                assetRules[index] = rule;
            }
            Save();
        }
        public void DeleteRole(string ruleName)
        {
            assetRules.RemoveAll(x => x.RuleName == ruleName);
            Save();
        }
        public static void Save()
        {
            EditorUtility.SetDirty(instance);
            instance.Save(true);
        }
    }
}