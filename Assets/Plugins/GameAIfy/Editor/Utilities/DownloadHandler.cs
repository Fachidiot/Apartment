using UnityEngine;
using System.IO;
using UnityEditor;

namespace GameAIfySDK
{
    public class DownloadHandler
    {
        public static void SaveTextureToPNG(Texture2D textureToSave, string name)
        {
            string folderPath = EditorPrefs.GetString(Constants.SaveFolder, Constants.DefaultDownloadPath);
            if (!Path.IsPathRooted(folderPath))
            {
                if (folderPath.StartsWith("Assets/"))
                {
                    folderPath = folderPath.Substring("Assets/".Length);
                }
                folderPath = Path.Combine(Application.dataPath, folderPath);
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, name + ".png");

            byte[] bytes = textureToSave.EncodeToPNG();
            File.WriteAllBytes(filePath, bytes);

            string relativePath = "Assets" + folderPath.Substring(Application.dataPath.Length);
            string assetPath = Path.Combine(relativePath, name + ".png").Replace("\\", "/");

            AssetDatabase.ImportAsset(assetPath);
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.SaveAndReimport();
                
            }
        }

        public static void SaveAsTextureToPNG(Texture2D textureToSave)
        {
            string assetPath = EditorUtility.SaveFilePanel("Save As", EditorPrefs.GetString(Constants.SaveFolder),"NewTexture.png", "png");

            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            string absolutePath = Path.GetFullPath(assetPath);

            byte[] bytes = textureToSave.EncodeToPNG();
            File.WriteAllBytes(absolutePath, bytes);

            string assetsFolderPath = Application.dataPath;
            if (assetPath.StartsWith(assetsFolderPath))
            {
                string relativePath = "Assets" + assetPath.Substring(assetsFolderPath.Length);
            
                AssetDatabase.ImportAsset(relativePath);
                AssetDatabase.Refresh();
            
                Object assetObj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                EditorGUIUtility.PingObject(assetObj);
                
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter != null)
                {
                    textureImporter.textureType = TextureImporterType.Sprite;
                    textureImporter.SaveAndReimport();
                }
            }
            else
            {
                EditorUtility.RevealInFinder(assetPath);
            }
        }
        
        public static void SaveAsObj(byte[] result)
        {
            string assetPath = EditorUtility.SaveFilePanel("Save As", EditorPrefs.GetString(Constants.SaveFolder),"NewObj.obj", "obj");

            if (string.IsNullOrEmpty(assetPath))
            {
                return;
            }

            string absolutePath = Path.GetFullPath(assetPath);

            byte[] bytes = result;
            File.WriteAllBytes(absolutePath, bytes);

            string assetsFolderPath = Application.dataPath;
            if (assetPath.StartsWith(assetsFolderPath))
            {
                string relativePath = "Assets" + assetPath.Substring(assetsFolderPath.Length);
            
                AssetDatabase.ImportAsset(relativePath);
                AssetDatabase.Refresh();
            
                Object assetObj = AssetDatabase.LoadAssetAtPath<Object>(relativePath);
                if (assetObj != null && UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                {
                    GameObject obj = assetObj as GameObject;
                    if (obj != null)
                    {
                        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                        foreach (Renderer rend in renderers)
                        {
                            foreach (Material mat in rend.sharedMaterials)
                            {
                                Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                                if (urpLitShader != null)
                                {
                                    mat.shader = urpLitShader;
                                    EditorUtility.SetDirty(mat);
                                }
                            }
                        }
                    }
                }
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(assetObj);
            }
            else
            {
                EditorUtility.RevealInFinder(assetPath);
            }
        }
    }
}