using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Weli.ResFormat
{
    public class AssetAuditorNewRuleWindow : EditorWindow
    {
        private static List<AssetAuditor.AssetRule> assetRules;
        private static AssetAuditor.AssetRule newRule;

        private static string currentAsset;

        static int selected = -1;
        private static string[] affectedAssets;
        private static AssetAuditorNewRuleWindow window;

        private static Vector2 scrollPosition;

        [MenuItem("Window/ResFormatting/New Format Rule")]
        public static void ShowWindow()
        {
            window = GetWindow<AssetAuditorNewRuleWindow>();
            window.Show();
            window.titleContent = new GUIContent("新建规则");

            if (!AssetDatabase.IsValidFolder(AssetAuditorPreferences.ProxyAssetsDirectory))
            {
                string folder = AssetAuditorPreferences.ProxyAssetsDirectory.Split(Path.DirectorySeparatorChar).Last();

                string dir = AssetAuditorPreferences.ProxyAssetsDirectory.Substring(0,
                    AssetAuditorPreferences.ProxyAssetsDirectory.Length - folder.Length);
                
                
                AssetDatabase.CreateFolder(dir, folder);
            }
            UpdateExistingRules();
            scrollPosition = Vector2.zero;

            AssetAuditor.queueComplete += AffectedAssetSearchComplete;

        }

        private static void AffectedAssetSearchComplete()
        {
            affectedAssets = AssetAuditor.GetAffectedAssets();
            window?.Repaint();
            AssetAuditor.queueComplete -= AffectedAssetSearchComplete;
        }


        void OnGUI()
        {
            DoNewRuleGUI();
            UpdateSelectedAsset();
            UpdateExistingRules();
        }


        private static void UpdateSelectedAsset()
        {
            if (selected == -1) return;
            Selection.activeObject =
                AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(assetRules[selected].AssetGuid),
                    typeof(Object));
        }

        
        private static void UpdateExistingRules()
        {
            // clear current list
            assetRules = ResFormatSetting.instance.assetRules;
            int i = 0;

            // make sure that the current asset is selected from assetRules
            foreach (var assetRule in assetRules)
            {
                if (assetRule.AssetGuid == currentAsset)
                {
                    selected = i;
                    return;
                }
                i++;
            }
            // if we get to here we couldnt find and asset to be the currently selected
            // set it to -1 and ignore anything to be currently selected
            selected = -1;
            currentAsset = "";
        }

        //done
        private static void DoNewRuleGUI()
        {
            using (var changeCheck = new EditorGUI.ChangeCheckScope())
            {
                 newRule.RuleName = EditorGUILayout.TextField("Rule Name: ", newRule.RuleName);
                if (changeCheck.changed && AssetAuditor.RuleExists(newRule))
                {
                    newRule = ResFormatSetting.instance.assetRules.Find(x => x.RuleName == newRule.RuleName);
                    UpdateExistingRules();
                    AssetAuditor.queueComplete += AffectedAssetSearchComplete;
                    AssetAuditor.UpdateAffectedAssets(newRule);
                }
            }

            EditorGUI.BeginChangeCheck();
            newRule.WildCardMatchType = AssetAuditor.WildCardMatchType.Regex;
            // newRule.WildCardMatchType =
            //     (AssetAuditor.WildCardMatchType) EditorGUILayout.EnumPopup("Wild Card Matching Type: ",
            //         newRule.WildCardMatchType);
            // if (EditorGUI.EndChangeCheck())
            // {
            //     newRule.WildCard = "";
            // }

            EditorGUI.BeginChangeCheck();
            newRule.WildCard = EditorGUILayout.TextField("Regular: ", newRule.WildCard);
            if (EditorGUI.EndChangeCheck())
            {
                AssetAuditor.queueComplete += AffectedAssetSearchComplete;
                AssetAuditor.UpdateAffectedAssets(newRule);
            }


            newRule.SelectiveMode = true;// EditorGUILayout.Toggle("Selective Mode", newRule.SelectiveMode);
            newRule.autoImport = EditorGUILayout.Toggle("Auto Import", newRule.autoImport);

            if (newRule.SelectiveMode)
            {
                if(newRule.SelectiveProperties == null) newRule.SelectiveProperties = new List<string>();

                SerializedObject so = GetSerializedObject(newRule.assetType);
                var propertyNames = AssetAuditor.GetPropertyNames(so); // TODO need to cache this
                
                // loop through all the selective properties
                for (int i = 0 ; i < newRule.SelectiveProperties.Count ; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    EditorGUI.BeginChangeCheck();
                    newRule.SelectiveProperties[i] = propertyNames[EditorGUILayout.Popup(SelectedFromList(propertyNames , newRule.SelectiveProperties[i]), propertyNames)];
                    if (EditorGUI.EndChangeCheck())
                    {
                        AssetAuditor.queueComplete += AffectedAssetSearchComplete;
                        AssetAuditor.UpdateAffectedAssets(newRule);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("+", GUILayout.MaxWidth(20)))
                {
                    AddNewSelectiveRule();
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(20)))
                {
                    RemoveLastSelectiveRule();
                }

                EditorGUILayout.LabelField("Add and remove selective property overiding");
                EditorGUILayout.EndHorizontal();
            }
            
            // drop down for type
            EditorGUI.BeginChangeCheck();
            newRule.assetType = (AssetAuditor.AssetType) EditorGUILayout.IntPopup("Rule Type: ", (int)newRule.assetType, new[]{"Texture", "Model","Audio"},new[]{0,1,2} );
            if (EditorGUI.EndChangeCheck())
            {
                newRule.SelectiveProperties = new List<string>();
                AssetAuditor.queueComplete += AffectedAssetSearchComplete;
                AssetAuditor.UpdateAffectedAssets(newRule);
            }
            
            var createText = "Create New " + newRule.assetType + " Rule";
            if(AssetAuditor.RuleExists(newRule))
            {
                createText = "Update " + newRule.assetType + " Rule";
            }
            // if (!AssetAuditor.RuleExists(newRule))
            {
                if (GUILayout.Button(createText))
                {
                    switch (newRule.assetType)
                    {
                        case AssetAuditor.AssetType.Texture:
                            AssetAuditor.CreateProxyTexture(newRule, ref currentAsset);
                            break;
                        case AssetAuditor.AssetType.Audio:
                            AssetAuditor.CreateProxyAudio(newRule, ref currentAsset);
                            break;
                        case AssetAuditor.AssetType.Model:
                            AssetAuditor.CreateProxyModel(newRule, ref currentAsset);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    UpdateExistingRules();
                    AssetAuditor.queueComplete += AffectedAssetSearchComplete;
                    if(assetRules.Count > selected && selected >= 0)
                    {
                        AssetAuditor.UpdateAffectedAssets(assetRules[selected]);
                    }
                }
            }
            if (AssetAuditor.RuleExists(newRule))
            {
                if (GUILayout.Button("Delete Rule"))
                {
                    AssetAuditor.DeleteRule(newRule);
                    UpdateExistingRules();
                }
             }
            // else
            // {
            //     GUILayout.Label("Rule already exists in the project cannot create duplicates");
            // }

            GUILayout.Space(20);
            GUILayout.Label("Affect Assets Preview");
            GUILayout.Space(5);


            Rect rt = GUILayoutUtility.GetRect(5, window ? window.position.width-10 : 100f, 18, 18);
            EditorGUI.ProgressBar(rt,AssetAuditor.GetProgress(), "Affected Asset Search Progress " + (AssetAuditor.GetProgress() * 100f).ToString("0.00%"));

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            if (affectedAssets != null)
            {
                foreach (string affectedAsset in affectedAssets)
                {
                    EditorGUILayout.ObjectField(
                        AssetDatabase.LoadAssetAtPath(affectedAsset,
                            AssetAuditor.TypeFromAssetType(newRule.assetType)),
                        AssetAuditor.TypeFromAssetType(newRule.assetType), false);
                }
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Open Audit View"))
            {
                AssetAuditorWindow.GetWindow();
            }
        }

        private static SerializedObject GetSerializedObject(AssetAuditor.AssetType assetType)
        {
            SerializedObject so = null;
            switch (assetType)
            {
                case AssetAuditor.AssetType.Texture:
                    so = new SerializedObject(TextureImporter.GetAtPath(AssetAuditorPreferences.ProxyTexturePath));
                    break;
                case AssetAuditor.AssetType.Model:
                    so = new SerializedObject(ModelImporter.GetAtPath(AssetAuditorPreferences.ProxyModelPath));
                    break;
                case AssetAuditor.AssetType.Audio:
                    so = new SerializedObject(AudioImporter.GetAtPath(AssetAuditorPreferences.ProxyAudioPath));
                    break;
                case AssetAuditor.AssetType.Folder:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return so;
        }

        private static int SelectedFromList(string[] propertyNames , string current)
        {
            if (current == "") return 0; // hack to avoid the empty string problem
            
            int i = 0;
            while (propertyNames[i] != current)
            {
                i++;
            }
            return i;
        }

        private static void RemoveLastSelectiveRule()
        {
            newRule.SelectiveProperties.RemoveAt(newRule.SelectiveProperties.Count-1);
        }

        private static void AddNewSelectiveRule()
        {
            newRule.SelectiveProperties.Add(AssetAuditor.GetPropertyNames(GetSerializedObject(newRule.assetType))[0]);
        }

    }
}

