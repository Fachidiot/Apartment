using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GameAIfySDK
{
    public class CommonUI : EditorWindow
    {
        public static void DoDrawPreset(ModelInfo selectPreset)
        {
            if (selectPreset.texture != null)
            {
                GUI.DrawTexture(CustomStyle.thumbnailRect, selectPreset.texture, ScaleMode.ScaleToFit);
            }
            else
            {
                GUI.Box(CustomStyle.thumbnailRect, "Loading...");
            }

            string categoryText = Enum.GetName(typeof(PresetGroup), selectPreset.category);
            string nameText = Enum.GetName(typeof(PresetModelMainCategory), selectPreset.groupNum);
            Vector2 categorySize = CustomStyle.bottomLabelStyle.CalcSize(new GUIContent(categoryText));
            Vector2 nameSize = CustomStyle.bottomLabelStyle.CalcSize(new GUIContent(nameText));
            float extraPadding = 10f;

            float boxCategoryWidth = categorySize.x + extraPadding;
            float boxCategoryHeight = categorySize.y + 4f;
            float boxNameWidth = nameSize.x + extraPadding;
            float boxNameHeight = nameSize.y + 4f;

            float boxX = CustomStyle.thumbnailWidth + 20f;
            float boxY = 10f;

            Rect boxRect1 = new Rect(boxX, boxY, boxCategoryWidth, boxCategoryHeight);
            Rect boxRect2 = new Rect(boxRect1.x + boxCategoryWidth + 5f, boxY, boxNameWidth, boxNameHeight);

            GUI.Box(boxRect1, "");
            GUI.Label(new Rect(boxRect1.x + 5, boxRect1.y + 2, boxRect1.width - 10, boxRect1.height - 4),
                categoryText, CustomStyle.bottomLabelStyle);

            GUI.Box(boxRect2, "");
            GUI.Label(new Rect(boxRect2.x + 5, boxRect2.y + 2, boxRect2.width - 10, boxRect2.height - 4),
                nameText, CustomStyle.bottomLabelStyle);

            float labelStartX = 120;
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(selectPreset.enName));
            GUI.Label(new Rect(labelStartX, 40, textSize.x + 10, textSize.y), selectPreset.enName,
                CustomStyle.topLabelStyle);
        }

        public static void DoSelectPreset(Rect buttonRect)
        {
            string message = "Select Model First";
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(message));
            Rect labelRect = new Rect(
                (buttonRect.width - textSize.x) * 0.5f,
                (buttonRect.height - textSize.y) * 0.5f,
                textSize.x,
                textSize.y);

            GUI.Label(labelRect, message);
        }

        public static void CenterTextureBox(Texture2D texture, float height)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            float windowWidth = Mathf.Min(GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor),
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailWidth = windowWidth - 10;
            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min(height - underExcludeHeight, Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 10;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);

            if (texture == null)
            {
                GUILayout.Box("No Image", GUILayout.Width(thumbnailSize), GUILayout.Height(thumbnailSize));
            }
            else
            {
                GUILayout.Box(texture, GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        public static Vector2 _scrollPos = Vector2.zero;

        private static int selectedHistoryKey = -1;

        public static void DoHistoryInfo(SortedDictionary<int, HistoryData> history, Action<HistoryData> callback)
        {
            GUILayout.Label("Image History", CustomStyle.boldLabelStyle);
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (GenerateWindow.selectItem != null)
                selectedHistoryKey = GenerateWindow.selectItem.seq;
            foreach (var kvp in history)
            {
                int key = kvp.Key;
                HistoryData item = kvp.Value;
                Rect rowRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));

                Color originalColor = GUI.backgroundColor;
                if (selectedHistoryKey == key)
                {
                    GUI.backgroundColor = new Color(0, 0, 0.8f, 1f);
                }

                GUI.Box(rowRect, GUIContent.none);
                GUI.backgroundColor = originalColor;

                GUI.BeginGroup(rowRect);
                {
                    float containerWidth = 100f;
                    float containerHeight = 30f;
                    Rect containerRect = new Rect(rowRect.width - containerWidth - 20, rowRect.height - containerHeight,
                        containerWidth,
                        containerHeight);
                    GUI.Box(containerRect, GUIContent.none);

                    float padding = 10f;
                    float imageSize = 100f;
                    Rect imageRect = new Rect(padding, padding, imageSize, imageSize);
                    if (item.texture != null)
                    {
                        GUI.DrawTexture(imageRect, item.texture, ScaleMode.ScaleToFit);

                        Rect smallButtonRect = new Rect(imageRect.x + imageSize - imageSize / 5, imageRect.y, 20, 20);
                        if (GUI.Button(smallButtonRect, CustomStyle.zoomTextrue))
                        {
                            ImageViewerWindow window =
                                (ImageViewerWindow)EditorWindow.GetWindow(typeof(ImageViewerWindow));
                            window.item = item;
                            window.Show();
                        }
                    }
                    else
                    {
                        GUI.Box(imageRect, "Loading...");
                    }

                    float labelStartX = imageRect.xMax + 10f;
                    float labelWidth = Constants.IMAGE_HISTORY_WIDTH - labelStartX - 10f;
                    GUI.Label(new Rect(labelStartX, 20, labelWidth, 20),
                        GetHistoryFuncName(item.func_num), CustomStyle.topLabelStyle);
                    string resolution = $"({item.output_image_width} x {item.output_image_height})";
                    GUI.Label(new Rect(labelStartX, 50, labelWidth, 20),
                        resolution, CustomStyle.bottomLabelStyle);

                    if (kvp.Value.texture != null)
                    {
                        GUI.BeginGroup(containerRect);
                        {
                            float halfWidth = containerRect.width / 2f;
                            Rect downloadRect = new Rect(0, 0, halfWidth, containerRect.height);

                            if (GUI.Button(downloadRect, CustomStyle.downloadButtonContent))
                            {
                                DownloadHandler.SaveAsTextureToPNG(item.texture);
                            }

                            Rect deleteRect = new Rect(halfWidth, 0, halfWidth, containerRect.height);
                            if (GUI.Button(deleteRect, CustomStyle.deleteButtonContent))
                            {
                                if (item.seq != history.Values.First().seq)
                                {
                                    GenerateWindow.window.DeleteImage(item.seq);
                                }
                                else if (DataCache.generateWindowHistoryMap.Count == 1)
                                {
                                    if (Alerts.CheckDialog(AlertMessages.TryRemoveOriginHistory))
                                    {
                                        GenerateWindow.window.DeleteImage(item.seq);
                                        GenerateWindow.selectItem = null;
                                    }
                                }
                            }
                        }
                        GUI.EndGroup();
                    }

                    Rect mainClickableRect = new Rect(0, 0, rowRect.width, rowRect.height);
                    Event e = Event.current;
                    if (e.type == EventType.MouseDown && mainClickableRect.Contains(e.mousePosition) &&
                        !containerRect.Contains(e.mousePosition) && item.texture != null)
                    {
                        selectedHistoryKey = key;
                        callback(item);
                        e.Use();
                    }
                }
                GUI.EndGroup();

                GUILayout.Space(5f);
            }

            EditorGUILayout.EndScrollView();
        }

        public static void DoInformationTooltip(string label, string tooltip, GUIStyle style = null)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(label, style ?? EditorStyles.boldLabel);
            GUIStyle infoStyle = new GUIStyle(EditorStyles.label);

            float offsetY =
                (((style ?? EditorStyles.boldLabel).lineHeight - CustomStyle.informationTexture.height) / 2f);
            infoStyle.contentOffset = new Vector2(0, offsetY);

            GUIContent infoContent = new GUIContent(CustomStyle.informationTexture, tooltip);
            GUILayout.Label(infoContent, infoStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        public static void DoTextureSizeText(int w, int h, int ow, int oh)
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            string formatString = "{0} x {1}  >  {2} x {3}";
            string displayText = string.Format(formatString, w, h, ow, oh);
            Vector2 textSize = labelStyle.CalcSize(new GUIContent(displayText));

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(displayText, labelStyle, GUILayout.Width(textSize.x));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            Rect lineRect = GUILayoutUtility.GetRect(0, 2, GUILayout.ExpandWidth(true));

            EditorGUI.DrawRect(lineRect, Color.white);
        }

        public static bool CheckEditOverSizeWithHelpBox(Texture2D texture)
        {
            if (GameAIfyUtilTools.CheckFailImageSize(texture))
            {
                EditorGUILayout.HelpBox(HelpBoxMessages.TryEditInputOverSize, MessageType.Error);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetHistoryFuncName(string func_num)
        {
            switch (func_num)
            {
                case "remove_bg":
                    return "BG Remove";
                case "enhance":
                    return "Enhance";
                case "expression_change":
                    return "Facial Expression";
                case "image_to_3d_map":
                    return "Image To 3D Map";
                case "seamless_extend":
                    return "Seamless Extend";
                case "inpaint":
                    return "Inpaint";
                case "outpaint":
                    return "Outpaint";
                default:
                    return func_num;
            }
        }
    }
}