using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UFrame.ResFormat
{
    public class AssetPostImporter : AssetPostprocessor
    {
        List<AssetAuditor.AssetRule> _assetRules;
        private HashSet<string> _importingPaths = new HashSet<string>();

        void Init()
        {
            if (_assetRules == null)
            {
                _assetRules = new List<AssetAuditor.AssetRule>(ResFormatSetting.instance.assetRules);
            }
        }

        void OnPostprocessModel(GameObject model)
        {
            Init();

            if (_assetRules != null)
            {
                foreach (var rule in _assetRules)
                {
                    if (rule.assetType == AssetAuditor.AssetType.Model && rule.autoImport)
                    {
                        var succcess = AssetAuditor.FixRule(assetPath, AssetAuditor.AssetType.Model, rule);
                        if (succcess)
                            TryReimport();
                    }
                }
            }
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            Init();
            if (_assetRules != null)
            {
                foreach (var rule in _assetRules)
                {
                    if (rule.assetType == AssetAuditor.AssetType.Texture && rule.autoImport)
                    {
                        var succcess = AssetAuditor.FixRule(assetPath, AssetAuditor.AssetType.Texture, rule);
                        if (succcess)
                            TryReimport();
                    }
                }
            }
        }
        void OnPostprocessAudio(AudioClip clip)
        {

            Init();

            if (_assetRules != null)
            {
                foreach (var rule in _assetRules)
                {
                    if (rule.assetType == AssetAuditor.AssetType.Audio && rule.autoImport)
                    {
                        var succcess = AssetAuditor.FixRule(assetPath, AssetAuditor.AssetType.Audio, rule);
                        if (succcess)
                            TryReimport();
                    }
                }
            }
        }
        
        private void TryReimport()
        {
            if (_importingPaths.Contains(assetPath))
                  return;
            _importingPaths.Add(assetPath);
            EditorApplication.delayCall += () =>
            {
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.Default);
            };
        }

    }
}