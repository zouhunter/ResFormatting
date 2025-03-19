using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Weli.ResFormat
{
    public class AssetPostImporter : AssetPostprocessor
    {
        List<AssetAuditor.AssetRule> _assetRules;

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
                        AssetAuditor.FixRule(assetPath, AssetAuditor.AssetType.Model, rule);
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
                        AssetAuditor.FixRule(assetPath, AssetAuditor.AssetType.Texture, rule);
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
                        AssetAuditor.FixRule(assetPath, AssetAuditor.AssetType.Audio, rule);
                    }
                }
            }
        }
    }
}