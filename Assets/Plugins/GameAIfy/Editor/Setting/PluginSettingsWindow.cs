using System;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;
using System.Text;


namespace GameAIfySDK
{
    public class PluginSettingsWindow : EditorWindow
    {
        public static PluginSettingsWindow window; 
        private static float minimumWidth = Constants.SETTING_WINDOW_MIN_WIDTH;
        private string usn;
        private string token;
        private string apiKey;
        private string secretKey;
        private string saveFolder;
        private string sessionId;


        [MenuItem("Window/GameAIfy/Settings")]
        public static void ShowWindow()
        {
            window = GetWindow<PluginSettingsWindow>("GameAIfy Settings");
            window.minSize = new Vector2(minimumWidth, window.minSize.y);
        }

        private void OnEnable()
        {
            LoadSettings();
        }

        private void OnGUI()
        {
            if (!window)
            {
                ShowWindow();
            }
            
            GUILayout.Space(10);
            DrawAPISettings();
            if (GUILayout.Button("Create your access token"))
            {
                Application.OpenURL(Constants.ACCESS_TOKEN_URL);
            }

            GUILayout.Space(20);
            DrawImageSettings();
            GUILayout.Space(10);

            if (GUILayout.Button("Save"))
            {
                if (apiKey == "" || secretKey == "")
                {
                    DeleteSetting();
                    Alerts.SuccessDialog(AlertMessages.Reset);
                    return;
                }
                else
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(GetUSN());
                    return;
                }
            }

            GUILayout.FlexibleSpace();
        }


        private void DrawAPISettings()
        {
            GUILayout.Label("API Settings", EditorStyles.boldLabel);

            apiKey = EditorGUILayout.TextField("API Key:", apiKey);
            secretKey = EditorGUILayout.PasswordField("Secret Key:", secretKey);
        }

        private void DrawImageSettings()
        {
            GUILayout.Label("Download Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Save Folder: ", GUILayout.Width(80));
            saveFolder = EditorGUILayout.TextField(saveFolder);

            if (GUILayout.Button("Browse...", GUILayout.Width(80)))
            {
                string folder = EditorUtility.OpenFolderPanel("Select Save Folder", Application.dataPath, "");
                if (!string.IsNullOrEmpty(folder))
                {
                    if (folder.StartsWith(Application.dataPath))
                    {
                        saveFolder = "Assets" + folder.Replace(Application.dataPath, "");
                    }
                    else
                    {
                        saveFolder = folder;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(Constants.ApiKey, apiKey);
            EditorPrefs.SetString(Constants.SecretKey, secretKey);
            EditorPrefs.SetString(Constants.SaveFolder, saveFolder);
            CheckAndCreateFolder(saveFolder);
            EditorPrefs.SetString(Constants.USN, usn);
            EditorPrefs.SetString(Constants.Token, token);
        }


        private void DeleteSetting()
        {
            EditorPrefs.DeleteKey(Constants.ApiKey);
            EditorPrefs.DeleteKey(Constants.SecretKey);
            EditorPrefs.DeleteKey(Constants.USN);
            EditorPrefs.DeleteKey(Constants.Token);
            EditorPrefs.DeleteKey(Constants.SaveFolder);
        }

        private IEnumerator GetUSN()
        {
            string url = Constants.API_USER_INFO;
            UnityWebRequest request = new UnityWebRequest(url, "POST");

            string concatenatedKeys = apiKey + ":" + secretKey;
            // Add the Authorization header
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(concatenatedKeys);
            string base64Token = System.Convert.ToBase64String(bytesToEncode);

            // Prepare the Authorization header value
            token = "Basic " + base64Token;

            request.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Authorization", token);

            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Alerts.FailDialog(AlertMessages.APIKeyNotAvailable);
            }
            else
            {
                // Parse the response to get USN
                string responseText = request.downloadHandler.text;

                USNResponse usnResponse = JsonUtility.FromJson<USNResponse>(responseText);
                if (usnResponse != null && usnResponse.usn != -1)
                {
                    usn = usnResponse.usn.ToString();
                    sessionId = usnResponse.sessionId;
                    SaveSettings();
                    DataCache.RefreshData();
                    Alerts.SuccessDialog(AlertMessages.Saved);
                }
                else
                {
                    Alerts.FailDialog(AlertMessages.APIKeyNotAvailable);
                }
            }
        }

        private void LoadSettings()
        {
            apiKey = EditorPrefs.GetString(Constants.ApiKey);
            secretKey = EditorPrefs.GetString(Constants.SecretKey);
            saveFolder = EditorPrefs.GetString(Constants.SaveFolder, Constants.DefaultDownloadPath);
            CheckAndCreateFolder(saveFolder);
        }

        private static void CheckAndCreateFolder(string _folder)
        {
            if (!AssetDatabase.IsValidFolder(_folder))
            {
                string[] paths = _folder.Split('/');
                string parentFolder = string.Empty;
                foreach (string path in paths)
                {
                    if (!path.Equals("Assets"))
                    {
                        if (!AssetDatabase.IsValidFolder($"{parentFolder}/{path}"))
                        {
                            AssetDatabase.CreateFolder(parentFolder, path);
                            parentFolder += "/" + path;
                        }
                        else
                        {
                            parentFolder += "/" + path;
                        }
                    }
                    else
                    {
                        parentFolder = path;
                    }
                }
            }
        }
    }
}