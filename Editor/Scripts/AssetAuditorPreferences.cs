using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UFrame.ResFormat
{
    public class AssetAuditorPreferences
    {
        private static string proxyAssetDir;
        private const string proxyAssetDirDefault = "Assets/Editor/ResFormat";

        private static string proxyTexturePath;
        private const string proxyTexturePathDefault = "e762cc062d5c5411a9264437e5c9c09e";

        private static string proxyModelPath;
        private const string proxyModelPathDefault = "6ca747ad81483413fa1665afa5a9c79a";

        private static string proxyAudioPath;
        private const string proxyAudioPathDefault = "d13848c7015ad4078abfd3076f74c106";

        static AssetAuditorPreferences()
        {
            proxyAssetDir = ResFormatSetting.instance. proxyAssetDir;
            proxyTexturePath = ResFormatSetting.instance. proxyTexturePath;
            proxyModelPath =ResFormatSetting.instance. proxyModelPath;
            proxyAudioPath =ResFormatSetting.instance. proxyAudioPath;

            if(string.IsNullOrEmpty(proxyAssetDir)  )
                proxyAssetDir = proxyAssetDirDefault;
            if(string.IsNullOrEmpty(proxyTexturePath))
                proxyTexturePath = AssetDatabase.GUIDToAssetPath(proxyTexturePathDefault);
            if(string.IsNullOrEmpty(proxyModelPath))
                proxyModelPath = AssetDatabase.GUIDToAssetPath(proxyModelPathDefault);
            if(string.IsNullOrEmpty(proxyAudioPath))
                proxyAudioPath = AssetDatabase.GUIDToAssetPath(proxyAudioPathDefault);
        }

        public static string ProxyAssetsDirectory
        {
            get
            {
                createProxyAssetsDirectory();
                return proxyAssetDir;
            }
        }

        public static string ProxyTexturePath
        {
            get { return proxyTexturePath; }
        }

        public static string ProxyModelPath
        {
            get { return proxyModelPath; }
        }

        public static string ProxyAudioPath
        {
            get { return proxyAudioPath; }
        }

        private static void createProxyAssetsDirectory()
        {
            if( AssetDatabase.IsValidFolder( proxyAssetDir ) )
                return;

            AssetAuditorUtilities.CreatePath.Create( proxyAssetDir );
        }

        [SettingsProvider]
        public static SettingsProvider VersionSetting()
        {
            var provider = new SettingsProvider($"Project/{typeof(AssetAuditorPreferences).Namespace.Split(".")[0]}/Res Format Settings", SettingsScope.Project);
            provider.label = "Res Formatting";
            provider.guiHandler = PreferencesGUI;
            provider.deactivateHandler = () => ResFormatSetting.Save();
            provider.keywords = new string[] { "res", "format", "setting" };
            return provider;
        }
        
        public static void PreferencesGUI(string obj)
        {
            EditorGUILayout.LabelField("Proxy Assets Directory", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Browse", EditorStyles.miniButton))
            {
                string path = EditorUtility.OpenFolderPanel("Select Proxy Assets Directory", proxyAssetDir, "");

                if (!path.Contains(Application.dataPath))
                {
                    Debug.LogError("Selected path " + path + " is not a directory within the open project");
                }
                else if (path.Length > 0)
                {
                    proxyAssetDir =System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, path);
                    ResFormatSetting.instance.proxyAssetDir = proxyAssetDir;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (AssetDatabase.IsValidFolder(proxyAssetDir) == false)
            {
                EditorGUILayout.HelpBox("Folder does not exist at given path", MessageType.Warning);
                if (GUILayout.Button("Create Now", EditorStyles.miniButton))
                {
                    createProxyAssetsDirectory();
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Proxy Asset Paths", EditorStyles.boldLabel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Texture");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse", EditorStyles.miniButton))
            {
                string path = EditorUtility.OpenFilePanel("Select Proxy Texture", proxyTexturePath, "jpg,png,bmp,tga");

                if (path.Length > 0)
                {
                    if (!path.Contains(Application.dataPath))
                    {
                        Debug.LogError("Selected path <" + path + "> is not a directory within the open project");
                    }
                    else
                    {
                       proxyTexturePath =System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, path);
                       ResFormatSetting.instance.proxyAssetDir = proxyTexturePath;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            System.Type t = AssetDatabase.GetMainAssetTypeAtPath(proxyTexturePath);
            if (t == null || t != typeof(Texture2D))
                EditorGUILayout.HelpBox("Proxy Texture does not exist at given path", MessageType.Warning);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Model");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse", EditorStyles.miniButton))
            {
                string path = EditorUtility.OpenFilePanel("Select Proxy Model", proxyModelPath, "fbx,obj,3ds");

                if (path.Length > 0)
                {
                    if (!path.Contains(Application.dataPath))
                    {
                        Debug.LogError("Selected path <" + path + "> is not a directory within the open project");
                    }
                    else
                    {
                        proxyModelPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, path);
                       ResFormatSetting.instance.proxyModelPath = proxyModelPath;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            t = AssetDatabase.GetMainAssetTypeAtPath(proxyModelPath);
            if (t == null || t != typeof(GameObject))
                EditorGUILayout.HelpBox("Proxy Texture does not exist at given path", MessageType.Warning);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio");

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Browse", EditorStyles.miniButton))
            {
                string path = EditorUtility.OpenFilePanel("Select Proxy Audio", proxyAudioPath, "wav,mp3,ogg");

                if (path.Length > 0)
                {
                    if (!path.Contains(Application.dataPath))
                    {
                        Debug.LogError("Selected path <" + path + "> is not a directory within the open project");
                    }
                    else
                    {
                        proxyAudioPath = System.IO.Path.GetRelativePath(System.Environment.CurrentDirectory, path);
                        ResFormatSetting.instance.proxyAudioPath = proxyAudioPath;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            t = AssetDatabase.GetMainAssetTypeAtPath(proxyAudioPath);
            if (t == null || t != typeof(AudioClip))
                EditorGUILayout.HelpBox("Proxy AudioClip does not exist at given path", MessageType.Warning);
        }
    }
}