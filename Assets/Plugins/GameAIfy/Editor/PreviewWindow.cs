using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using Newtonsoft.Json;
using System.Collections;

namespace GameAIfySDK
{
    public class PreviewWindow : EditorWindow
    {
        public static BaseInfo originalItemInfo;
        public static HistoryData selectedItem;
        public static PreviewWindow window = null;

        private static ImageInfoResponse imageInfoResponse;
        private static ModelInfo _useModelInfo;
        private static string _promptLabel = "";
        private static string _negativePromptLabel = "";
        private static Vector2 _scrollPos;
        public bool isMyWorksItem = true;

        private Vector2 scrollPos1 = new Vector2(0, 0);
        private Vector2 scrollPos2 = new Vector2(0, 0);
        
        public static void ShowWindow()
        {
            window = GetWindow<PreviewWindow>("Edit Window");
            window.minSize = new Vector2(Constants.WINDOW_MIN_WIDTH, Constants.PREVIEW_WINDOW_MIN_HEIGHT);

            if (originalItemInfo == null)
            {
                window.Close();
                return;
            }

            HistoryData item = new HistoryData();
            item.seq = originalItemInfo.seq;
            item.func_num = "Original Image";
            item.output_image_width = originalItemInfo.width;
            item.output_image_height = originalItemInfo.height;
            item.texture = originalItemInfo.texture;
            selectedItem = item; 
            window.scrollPos1 = new Vector2(0, 0);
            window.scrollPos2 = new Vector2(0, 0);
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadImageOptionAsync());
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadHistoryAsync());

            if (window.isMyWorksItem)
            {
                DataCache.editWindowHistoryMap.Clear();
                DataCache.editWindowHistoryMap.Add(item.seq, item);
            }
        }

        private void OnGUI()
        {
            if (!window)
            {
                ShowWindow();
            }

            EditorGUILayout.BeginHorizontal();
            DoMainContent();
            DoRightSide();
            EditorGUILayout.EndHorizontal();
        }

        private void DoMainContent()
        {
            EditorGUILayout.BeginVertical();
            DoThumbnail();
            DoButtons();
            EditorGUILayout.EndVertical();
        }

        private void DoThumbnail()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(GUILayout.Height(position.height / 2));

            GUILayout.FlexibleSpace();
            float windowWidth = EditorGUIUtility.currentViewWidth - Constants.EDIT_PRESET_INFO_WIDTH;
            float thumbnailWidth = windowWidth - 20;

            float windowHeight = Mathf.Min(position.height / 2, Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 20;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);
            if (originalItemInfo != null && originalItemInfo.texture != null)
            {
                GUILayout.Label(selectedItem.texture, GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));
            }
            else
            {
                GUILayout.Box("No Image", GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
        }

        private void DoButtons()
        {
            int btnCnt = isMyWorksItem ? 4 : 3;
            var buttonWidth = (window.position.width - Constants.EDIT_PRESET_INFO_WIDTH - 40) / btnCnt;

            EditorGUILayout.BeginVertical(GUILayout.Height(position.height / 2));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Download", GUILayout.Width(buttonWidth)))
            {
                DownloadHandler.SaveTextureToPNG(selectedItem.texture, selectedItem.seq.ToString());
            }

            if (GUILayout.Button("Download As", GUILayout.Width(buttonWidth)))
            {
                DownloadHandler.SaveAsTextureToPNG(selectedItem.texture);
            }

            GUIContent regenerateBtn = new GUIContent("Regenerate", ToolTipMessages.Regenerate);
            if (GUILayout.Button(regenerateBtn, GUILayout.Width(buttonWidth)))
            {
                GenerateWindow.window.RemixImage(_useModelInfo, imageInfoResponse.imageOption, selectedItem.seq);
                GenerateWindow.ShowWindow();
            }

            if (window.isMyWorksItem)
            {
                if (GUILayout.Button("Delete", GUILayout.Width(buttonWidth)))
                {
                    if (selectedItem.seq != originalItemInfo.seq)
                    {
                        GenerateWindow.window.DeleteImage(selectedItem.seq);
                        DataCache.editWindowHistoryMap.Remove(selectedItem.seq);
                        Repaint();
                    }
                    else if (DataCache.editWindowHistoryMap.Count == 1 ||
                             Alerts.CheckDialog(AlertMessages.HistoryDelete))
                    {
                        GenerateWindow.window.DeleteImage(selectedItem.seq);
                        Close();
                    }
                }
            }

            GUILayout.Space(30);

            EditorGUILayout.EndHorizontal();
            if (window.isMyWorksItem)
                DoEditButtons();
            EditorGUILayout.EndVertical();
        }

        private void DoRightSide()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Constants.EDIT_PRESET_INFO_WIDTH - 5));
            DoModelInfo();

            if (window.isMyWorksItem)
                CommonUI.DoHistoryInfo(DataCache.editWindowHistoryMap, (item) => { selectedItem = item; });
            EditorGUILayout.EndVertical();
        }

        private void DoEditButtons()
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Space(30);
            GUILayout.Label("Edit", CustomStyle.boldLabelStyle);
            foreach (KeyValuePair<string, List<SubCategory>> entry in GenerateWindow.textSideBarType)
            {
                if (entry.Key == "Gallery" || entry.Key == "Generator")
                    continue;


                EditorGUILayout.BeginHorizontal();
                var buttonWidth = (window.position.width - Constants.EDIT_PRESET_INFO_WIDTH - 30) / 2;
                int cnt = 0;
                foreach (SubCategory subCategory in entry.Value)
                {
                    if (subCategory.Index == SideBarSubCategoryItem.Text2Motion)
                        continue;

                    cnt++;
                    if (GUILayout.Button(new GUIContent(subCategory.Name, subCategory.Tooltip),
                            GUILayout.Width(buttonWidth)))
                    {
                        GenerateWindow.ChangeCategory((SideBarSubCategoryItem)subCategory.Index);
                        SelectEditFunction();
                    }

                    if (cnt % 2 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void SelectEditFunction()
        {
            GenerateWindow.selectItem = selectedItem;
            if (DataCache.editWindowHistoryMap != DataCache.generateWindowHistoryMap)
            {
                DataCache.generateWindowHistoryMap =
                    new SortedDictionary<int, HistoryData>(DataCache.editWindowHistoryMap);
            }

            GenerateWindow.ShowWindow();
        }

        private static ModelInfo GetModelInfo(int presetNum)
        {
            if (presetNum == Constants.CHARACTERPOSE_PRESET_NUM)
            {
                presetNum = 66;
            }
            
            foreach (var item in DataCache.modelDictionary.Values)
            {
                if (item.presetNum == presetNum)
                {
                    return item;
                }
            }

            return new ModelInfo();
        }


        private void DoModelInfo()
        {
            GUILayout.Label("Preset", CustomStyle.boldLabelStyle);
            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));
            GUI.BeginGroup(buttonRect);
            if (_useModelInfo != null)
                CommonUI.DoDrawPreset(_useModelInfo);
            GUI.EndGroup();

            GUILayout.Space(10);
            GUILayout.Label("Positive Prompt", CustomStyle.boldLabelStyle);
            scrollPos1 = GUILayout.BeginScrollView(scrollPos1, GUILayout.Height(50));
            _promptLabel = GUILayout.TextArea(_promptLabel, CustomStyle.wrapLabelStyle);
            GUILayout.EndScrollView();
            GUILayout.Space(10);

            GUILayout.Label("Negative Prompt", CustomStyle.boldLabelStyle);
            scrollPos2 = GUILayout.BeginScrollView(scrollPos2, GUILayout.Height(50));
            _negativePromptLabel = GUILayout.TextArea(_negativePromptLabel, CustomStyle.wrapLabelStyle);
            GUILayout.EndScrollView();

            GUILayout.Space(10);
        }


        private static IEnumerator LoadImageOptionAsync()
        {
            yield return APIHandler.GetImageOptionAsync(originalItemInfo.option_seq, (jsonData) =>
            {
                if (string.IsNullOrEmpty(jsonData))
                    return;

                imageInfoResponse = JsonConvert.DeserializeObject<ImageInfoResponse>(jsonData);
                _useModelInfo = GetModelInfo(imageInfoResponse.imageOption.preset_num); 
                _promptLabel = imageInfoResponse.imageOption.prompt;
                _negativePromptLabel = imageInfoResponse.imageOption.negativePrompt;
                window.Repaint();
            });
        }

        private static IEnumerator LoadHistoryAsync()
        {
            yield return DataCache.LoadHistoryAsync(originalItemInfo.seq, (callback) => { window.Repaint(); },
                true);
        }
    }
}