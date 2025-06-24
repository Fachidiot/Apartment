using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace GameAIfySDK
{
    public class EnhanceUIHandler : ISubCategoryUIHandler
    {
        private const float loraMinSliderValue = 0.0f;
        private const float loraMaxSliderValue = 1.0f;

        private const float upscaleRatioSliderMinValue = 100;
        private const float upscaleRatioSliderMaxValue = 400;

        private const int MaxScalePixel = 4096;

        private bool isResult = false;

        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle backgroundLabelStyle;

        //private bool isUpScaleRatioShow = true;
        //private bool isRefineStrengthShow = true;
        private bool isModelSelectShow = false;

        private float loraStrengthValue = 0.8f;
        private float _pastLoraStrengthValue = 0.8f;

        private HistoryData curSelectItem = null;
        private int upscaleRatioSliderValue = 150;
        private int upscaleRatioValue = 150;
        private int selectedRefineStrengthIndex = 0;
        private float maxRatio = upscaleRatioSliderMaxValue;

        public void DrawUI()
        {
            EditorGUILayout.BeginVertical();
            //DoDrawCurProcess();
            //EditorGUILayout.Space(10);

            DoDrawThumbnailImage();
            EditorGUILayout.Space(10);

            
            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
            {
                DoDrawUpscaleRatio();
                EditorGUILayout.Space(10);
                DoDrawRefineStrength();
                EditorGUILayout.Space(10);
                // DoDrawModelSelect();
                // EditorGUILayout.Space(10);

                DoDrawEnhanceButton();
            }
            
            EditorGUILayout.EndVertical();
        }

        public void DoDrawThumbnailImage()
        {
            // Image
            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor);
            float thumbnailWidth = windowWidth - 20;

            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight) / 2,
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 20;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;

            curSelectItem = GenerateWindow.selectItem;
            
            if (curSelectItem != null && curSelectItem.texture != null)
            {
                float imageAspect = (float)curSelectItem.texture.width / curSelectItem.texture.height;
                float computedRatio =
                    Mathf.Min(Constants.CANVAS_EXTEND_MAX_OUTPUT * 100f / curSelectItem.texture.width,
                        Constants.CANVAS_EXTEND_MAX_OUTPUT * 100f / curSelectItem.texture.height);
                maxRatio = Mathf.Min(computedRatio, upscaleRatioSliderMaxValue);

                if (upscaleRatioSliderValue > maxRatio)
                {
                    upscaleRatioValue = (int)maxRatio;
                }
                else
                {
                    upscaleRatioValue = upscaleRatioSliderValue;
                }

                float boxWidth, boxHeight;
                if (thumbnailWidth / thumbnailHeight > imageAspect)
                {
                    boxHeight = thumbnailHeight;
                    boxWidth = thumbnailHeight * imageAspect;
                }
                else
                {
                    boxWidth = thumbnailWidth;
                    boxHeight = thumbnailWidth / imageAspect;
                }

                Rect boxRect = GUILayoutUtility.GetRect(boxWidth, boxWidth, boxHeight, boxHeight,
                    GUILayout.ExpandWidth(false));

                float ratio = upscaleRatioValue / maxRatio;
                float imgWidth = boxRect.width * ratio;
                float imgHeight = boxRect.height * ratio;

                float defaultWidth = Mathf.Min(imgWidth,boxRect.width * (100 / maxRatio));
                float defaultHeight = Mathf.Min(imgHeight,boxRect.height * (100 / maxRatio));


                Rect outLineRect = new Rect(
                    boxRect.x + (boxRect.width - imgWidth) / 2,
                    boxRect.y + (boxRect.height - imgHeight) / 2,
                    imgWidth,
                    imgHeight
                );
                

                Rect imgRect = new Rect(
                    boxRect.x + (boxRect.width - defaultWidth) / 2,
                    boxRect.y + (boxRect.height - defaultHeight) / 2,
                    defaultWidth,
                    defaultHeight
                );
                
                GUI.DrawTexture(imgRect, curSelectItem.texture, ScaleMode.ScaleToFit);
                
                Handles.color = Color.white;
                Handles.DrawSolidRectangleWithOutline(boxRect, new Color(0, 0, 0, 0), Color.white);
                
                Handles.color = Color.green;
                Handles.DrawSolidRectangleWithOutline(imgRect, new Color(0, 0, 0, 0), Color.green);

                Handles.color = Color.magenta;
                Handles.DrawSolidRectangleWithOutline(outLineRect, new Color(0, 0, 0, 0), Color.magenta);
            }
            else
            {
                Rect boxRect = GUILayoutUtility.GetRect(thumbnailWidth, thumbnailWidth);
                Handles.DrawSolidRectangleWithOutline(boxRect, new Color(0, 0, 0, 0), Color.white);
            }
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        public void DoDrawRect()
        {
            // Image
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor);
            float thumbnailWidth = windowWidth - 20;

            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight) / 2,
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 20;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);

            if (null != GenerateWindow.selectItem)
            {
                GUILayout.Box(GenerateWindow.selectItem.texture, GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));
            }
        }

        private void DoDrawCurProcess()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 14;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.normal.textColor = Color.white;
            titleStyle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.darkGray); // dark gray color bg

            labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.fontSize = 12;
            labelStyle.alignment = TextAnchor.MiddleLeft;

            backgroundLabelStyle = new GUIStyle(EditorStyles.label);
            backgroundLabelStyle.alignment = TextAnchor.MiddleCenter;
            backgroundLabelStyle.normal.textColor = Color.white;
            backgroundLabelStyle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.blueColor); // blue color bg

            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUILayout.Label("Enhance", isResult == false ? backgroundLabelStyle : titleStyle,
                GUILayout.ExpandWidth(true), GUILayout.Height(25));
            GUILayout.Label(">>", titleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25));
            GUILayout.Label("Result", isResult == true ? backgroundLabelStyle : titleStyle, GUILayout.ExpandWidth(true),
                GUILayout.Height(25));

            GUILayout.EndHorizontal();
        }

        private void DoDrawUpscaleRatio()
        {
            GUIStyle ratiroTitle = new GUIStyle(GUI.skin.box);
            ratiroTitle.fontStyle = FontStyle.Bold;
            ratiroTitle.normal.textColor = Color.white;
            ratiroTitle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.darkGray);

            //if (isUpScaleRatioShow)
            {
                if (curSelectItem == null)
                    return;
                if (curSelectItem.texture == null)
                    return;

                float ratio = upscaleRatioValue * 0.01f;

                int width = (int)Mathf.Ceil(curSelectItem.texture.width * ratio);
                int height = (int)Mathf.Ceil(curSelectItem.texture.height * ratio);

                if (width / height > 1)
                {
                    if (width > MaxScalePixel)
                    {
                        float widthRatio = MaxScalePixel / (float)curSelectItem.texture.width;
                        width = MaxScalePixel;
                        height = (int)(curSelectItem.texture.height * widthRatio);
                        if (height > MaxScalePixel)
                            height = MaxScalePixel;
                    }
                }
                else if (width / height < 1)
                {
                    if (height > MaxScalePixel)
                    {
                        float heightRatio = MaxScalePixel / (float)curSelectItem.texture.height;
                        height = MaxScalePixel;
                        width = (int)(curSelectItem.texture.width * heightRatio);
                        if (width > MaxScalePixel)
                            width = MaxScalePixel;
                    }
                }

                width = GameAIfyUtilTools.RoundUpToMultipleOf8((int)width);
                height = GameAIfyUtilTools.RoundUpToMultipleOf8((int)height);

                GUILayout.BeginVertical();
                EditorGUILayout.Space(10);

                CommonUI.DoTextureSizeText(curSelectItem.texture.width, curSelectItem.texture.height, (int)width,
                    (int)height);

                EditorGUILayout.Space(10);

                GUIStyle titleStyle = new GUIStyle(EditorStyles.helpBox);
                titleStyle.fontSize = 13;
                titleStyle.fontStyle = FontStyle.Bold;
                GUILayout.Label("Upscale Ratio", titleStyle);

                // Draw Slider

                GUILayout.BeginHorizontal();
                upscaleRatioSliderValue = (int)EditorGUILayout.Slider(upscaleRatioSliderValue,
                    upscaleRatioSliderMinValue, upscaleRatioSliderMaxValue);
                GUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
                GUILayout.EndVertical();
            }
        }

        private void DoDrawRefineStrength()
        {
            //GUIStyle titleStyle = new GUIStyle(GUI.skin.box);
            //titleStyle.fontStyle = FontStyle.Bold;
            //titleStyle.normal.textColor = Color.white;
            //titleStyle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.darkGray);

            //GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.fontStyle = FontStyle.Bold;

            //Rect titleRect = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
            //GUI.Box(titleRect, GUIContent.none, titleStyle);

            //Rect foldoutRect = new Rect(titleRect.x + 5, titleRect.y, titleRect.width, titleRect.height);
            //isRefineStrengthShow = EditorGUI.Foldout(foldoutRect, isRefineStrengthShow, "Refine Strength", true, foldoutStyle);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.helpBox);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Refine Strength", titleStyle);

            //if (isRefineStrengthShow)
            {
                GUILayout.BeginVertical();
                EditorGUILayout.Space(10);
                // Info Label

                float labelWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor) - 30;
                GUILayout.Label(LabelMessages.RefineStrengthDescription, CustomStyle.centeredWrapTextStyle,
                    GUILayout.Width(labelWidth));

                EditorGUILayout.Space(10);

                // Button
                string[] options = { "None", "Low", "Medium", "High" };

                selectedRefineStrengthIndex = GUILayout.Toolbar(selectedRefineStrengthIndex, options);
                EditorGUILayout.Space(10);
                GUILayout.EndVertical();
            }
        }

        private void DoDrawModelSelect()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.box);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            titleStyle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.darkGray);

            GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            foldoutStyle.fontStyle = FontStyle.Bold;

            Rect titleRect = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
            GUI.Box(titleRect, GUIContent.none, titleStyle);

            Rect foldoutRect = new Rect(titleRect.x + 5, titleRect.y, titleRect.width, titleRect.height);
            isModelSelectShow = EditorGUI.Foldout(foldoutRect, isModelSelectShow, "Model Select", true, foldoutStyle);

            if (isModelSelectShow)
            {
                GUILayout.BeginVertical();
                EditorGUILayout.Space(10);
                // Info Label
                string strInfo =
                    "To use this feature, you must select a model. \n Choosing the right model improves the quality of the output.";

                GUILayout.Label(strInfo, CustomStyle.centeredWrapTextStyle, GUILayout.ExpandWidth(true));

                EditorGUILayout.Space(10);

                // Model Select Group
                Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));

                GUI.BeginGroup(buttonRect);
                if (GUI.Button(new Rect(0, 0, buttonRect.width, buttonRect.height), GUIContent.none))
                {
                    PresetWindow.ShowWindow();
                    PresetWindow.presetModelMainCategory =
                        Enums.ConvertCategory(SideBarSubCategoryItem.FacialExpression);
                }

                if (GenerateWindow.selectPreset == null)
                {
                    CommonUI.DoSelectPreset(buttonRect);
                }
                else
                {
                    CommonUI.DoDrawPreset(GenerateWindow.selectPreset);
                }

                GUI.EndGroup();
                EditorGUILayout.Space(10);
                DoLoraStrength();
                EditorGUILayout.Space(10);
                GUILayout.EndVertical();
            }
        }

        private void DoDrawEnhanceButton()
        {
            EditorGUI.BeginDisabledGroup(upscaleRatioSliderValue <= upscaleRatioSliderMinValue &&
                                         selectedRefineStrengthIndex == 0);
            if (GUILayout.Button("Enhance", GUILayout.ExpandWidth(true)))
            {
                EnhanceStrengthType strenghType = (EnhanceStrengthType)selectedRefineStrengthIndex;

                string strStrength = strenghType.ToString();
                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestEnhance(GenerateWindow.selectItem.seq,
                    strStrength, upscaleRatioValue,
                    (imageInfo) =>
                    {
                        if (imageInfo != null)
                        {
                            var history = DataCache.AddHistory(imageInfo, "enhance");
                            GenerateWindow.selectItem = history;
                            GenerateWindow.window.Repaint();
                        }
                    }));
            }

            EditorGUI.EndDisabledGroup();
        }

        private void DoLoraStrength()
        {
            GUILayout.Label("LoRA Components", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Strength");
            loraStrengthValue = GUILayout.HorizontalSlider(loraStrengthValue, loraMinSliderValue, loraMaxSliderValue);
            if (loraStrengthValue != _pastLoraStrengthValue)
            {
                _pastLoraStrengthValue = loraStrengthValue;
                GUI.FocusControl(null);
            }

            string textValue = EditorGUILayout.TextField(loraStrengthValue.ToString("F2"), GUILayout.Width(50));
            if (float.TryParse(textValue, out float parsedValue))
            {
                loraStrengthValue = Mathf.Clamp(parsedValue, loraMinSliderValue, loraMaxSliderValue);
            }

            GUILayout.EndHorizontal();
        }

        private void EditImage(SideBarSubCategoryItem editType, string value)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.EditImage(GenerateWindow.selectItem.seq, editType,
                value,
                (imageInfo) =>
                {
                    if (imageInfo != null)
                    {
                    }
                }));
        }
    }
}