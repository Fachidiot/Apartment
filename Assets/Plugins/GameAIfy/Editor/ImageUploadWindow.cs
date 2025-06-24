using System;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using Newtonsoft.Json;


namespace GameAIfySDK
{
    public class ImageUploadWindow : EditorWindow
    {
        public static ImageUploadWindow window;
        private static float minimumWidth = Constants.SETTING_WINDOW_MIN_WIDTH;
        public static PresetModelMainCategory selectedCategory = PresetModelMainCategory.All;

        private static Action<HistoryData> action;

        private readonly string[] _filteredCategoryNames =
            Enum.GetNames(typeof(PresetModelMainCategory)).Skip(1).ToArray();

        private Texture2D inputImage;
        private string inputPrompt;
        private bool isUploading = false;
        private string uploadText = "Upload";
        private string processingText = "Processing...";

        private static GUIStyle textStyle;
        private static GUIStyle boxStyle;

        public static void ShowWindow(Action<HistoryData> _action = null)
        {
            window = GetWindow<ImageUploadWindow>("Image Upload");
            window.minSize = new Vector2(minimumWidth, window.minSize.y);
            action = _action;

            textStyle = new GUIStyle();
            textStyle.alignment = TextAnchor.MiddleCenter;
            textStyle.normal.textColor = Color.white;

            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.MiddleCenter;            
        }

        [MenuItem("Window/GameAIfy/ImageUpload")]
        public static void ShowWindow()
        {
            window = GetWindow<ImageUploadWindow>("Image Upload");
            window.minSize = new Vector2(minimumWidth, window.minSize.y);
        }

        private void OnGUI()
        {
            if (!window)
            {
                ShowWindow();
            }

            DoUploadImage();
            DrawImageSettings();
            DoUploadButton();
        }

        private void OnEnable()
        {
            ClearInputField();
        }

        public void DoUploadImage()
        {
            GUILayout.Label("Upload Image", EditorStyles.boldLabel);
            if (GameAIfyUtilTools.CheckFailImageSize(inputImage))
            {                
                EditorGUILayout.HelpBox(HelpBoxMessages.TryEditInputOverSize, MessageType.Error);
            }
            else
            {
                EditorGUILayout.HelpBox(HelpBoxMessages.TryEditSizeInfo, MessageType.Info);
            }

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginDisabledGroup(isUploading);
            if (GUILayout.Button("Load Image"))
            {
                string imgPath = EditorUtility.OpenFilePanel("Upload image", EditorPrefs.GetString(Constants.SaveFolder), "png,jpeg,jpg");
                if (!string.IsNullOrEmpty(imgPath))
                {
                    byte[] datas = System.IO.File.ReadAllBytes(imgPath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(datas);

                    if(texture != null)
                    {                        
                        inputImage = texture;

                        textStyle = new GUIStyle();
                        textStyle.alignment = TextAnchor.MiddleCenter;
                        textStyle.normal.textColor = Color.white;

                        boxStyle = new GUIStyle(GUI.skin.box);                        
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
            if(inputImage != null)
            {
                GUILayout.Space(10);
                Rect rect = GUILayoutUtility.GetRect(100, 100, 100, 100);
                Rect imgRect = new Rect(
                        rect.x + (rect.width - 100) / 2,
                        rect.y + (rect.height - 100) / 2,
                        100,
                        100);
                GUI.DrawTexture(imgRect, inputImage, ScaleMode.ScaleToFit);
                GUILayout.Space(5);
                GUILayout.Label("  (" + inputImage.width + " x " + inputImage.height + ")", textStyle);
            }                        
            EditorGUILayout.EndVertical();

            //inputImage = (Texture2D)EditorGUILayout.ObjectField(inputImage, typeof(Texture2D), false);
            //if (inputImage != null)
            //{
            //    GUILayout.Label("  (" + inputImage.width + " x " + inputImage.height + ")");
            //}

            GUILayout.Space(10);
        }

        private void DrawImageSettings()
        {
            EditorGUI.BeginDisabledGroup(inputImage == null || isUploading);
            GUILayout.Label("Category", EditorStyles.boldLabel);
            int selectedIndex = GUILayout.Toolbar((int)selectedCategory - 1, _filteredCategoryNames,
                GUILayout.ExpandWidth(true));
            selectedCategory = (PresetModelMainCategory)(selectedIndex + 1);

            GUILayout.Space(20);
            EditorGUI.EndDisabledGroup();
            // GUILayout.Label("Describe the image - Optional", EditorStyles.boldLabel);
            // inputPrompt = EditorGUILayout.TextArea(inputPrompt, CustomStyle.textAreaStyle,
            //     GUILayout.Width(position.width - 5), GUILayout.Height(50));
            // GUILayout.Space(40);
        }

        private void DoUploadButton()
        {
            EditorGUI.BeginDisabledGroup(inputImage == null || selectedCategory == PresetModelMainCategory.All ||
                                         isUploading || GameAIfyUtilTools.CheckFailImageSize(inputImage));
            if (GUILayout.Button(isUploading ? processingText : uploadText))
            {
                GUI.FocusControl(null);
                isUploading = true;
                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.UploadImage(inputImage, inputPrompt,
                    (int)selectedCategory,
                    (callback) =>
                    {
                        isUploading = false;
                        if (callback != "Fail")
                        {
                            ClearInputField();
                            EditorCoroutineUtility.StartCoroutineOwnerless(
                                DataCache.LoadMyWorksDataAsync(0, 1, (c) => { GenerateWindow.window.Repaint(); }));

                            if (action != null)
                            {
                                ImageDataResponse expressionChangeInfoResponse =
                                    JsonConvert.DeserializeObject<ImageDataResponse>(callback);
                                var imageInfo = expressionChangeInfoResponse.data;
                                var history = DataCache.AddHistory(imageInfo, "Original Image");
                                GenerateWindow.window.Repaint();
                                action.Invoke(history);
                            }

                            window.Close();
                        }
                    }));
            }

            EditorGUI.EndDisabledGroup();
        }

        private void ClearInputField()
        {
            inputImage = null;
            inputPrompt = null;
            selectedCategory = PresetModelMainCategory.All;
        }
    }
}