using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Windows;

namespace GameAIfySDK
{
    public class FacialExpressionUIHandler : ISubCategoryUIHandler
    {
        private const float MinSliderValue = 0.0f;
        private const float MaxSliderValue = 1.0f;

        private const int MinOutputCount = 1;
        private const int MaxOutputCount = 4;

        private ExpresstionChangeStep curStep = ExpresstionChangeStep.none;
        private ExpressionStyle curStyle = ExpressionStyle.anime;
        //private ExpressionType curType = ExpressionType.Neutral;

        private List<ExpressionConfigData> curInfoList = null;
        //private bool isSend = false;

        private int selectedGridIdx = 0;
        private bool isShow = false;
        private float loraStrengthValue = 0.8f;
        private float _pastLoraStrengthValue = 0.8f;

        private GUIStyle titleStyle;
        private GUIStyle labelStyle;
        private GUIStyle backgroundLabelStyle;

        private bool isSettingShow = false;
        private int outputCount = 1;
        private int prevOutputCount = 0;

        public void DrawUI()
        {
            GUILayout.BeginVertical();
            //DoDrawCurProcess();
            //DoDrawImageUp();
            DoDrawThumbnailImage();
            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
                DoDrawImageUnder();
            GUILayout.EndVertical();
        }

        //private void DoDrawStepUI()
        //{
        //    GUILayout.BeginHorizontal();

        //    GUILayout.BeginHorizontal();

        //    //GUIStyle label1 = new GUIStyle(GUI.skin.label);
        //    //label1.normal.textColor = curStep == ExpresstionChangeStep.detectingFace ? Color.blue : Color.white;
        //    //GUILayout.Label("Detecting Face >>", label1);

        //    GUIStyle label2 = new GUIStyle(GUI.skin.label);
        //    label2.normal.textColor = curStep == ExpresstionChangeStep.expresstionEdit ? Color.blue : Color.white;
        //    GUILayout.Label("Expression Edit >> ", label2);

        //    GUIStyle label3 = new GUIStyle(GUI.skin.label);
        //    label3.normal.textColor = curStep == ExpresstionChangeStep.result ? Color.blue : Color.white;
        //    GUILayout.Label("Result", label3);
        //    GUILayout.EndHorizontal();


        //    GUILayout.EndHorizontal();
        //}

        public void DoDrawThumbnailImage()
        {
            // Image
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            float windowWidth = Mathf.Min(GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor),
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailWidth = windowWidth - 10;

            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight) / 2,
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 10;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);

            if (null != GenerateWindow.selectItem)
            {
                GUILayout.Box(GenerateWindow.selectItem.texture, GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        private void DoDrawImageUp()
        {
            if (curStep == ExpresstionChangeStep.detectingFace)
            {
                //GUILayout.BeginHorizontal();
                //if(GUILayout.Button("Auto"))
                //{

                //}

                //if(GUILayout.Button("Manual"))
                //{

                //}
                //GUILayout.EndHorizontal();
            }
        }

        private void LoadChangeConfigList()
        {
            if (curStep == ExpresstionChangeStep.none)
            {
                if (DataCache.animeInfoList.Count == 0)
                {
                    DataCache.LoadFacialExpressionData(() => { LoadChangeConfigList(); });
                }
                else
                {
                    curStep = ExpresstionChangeStep.expresstionEdit;
                }
            }
        }

        private void DoDrawImageUnder()
        {
            DoDrawExpressionEditUI();
            // switch (curStep)
            // {
            //     case ExpresstionChangeStep.none:
            //         LoadChangeConfigList();
            //         break;
            //     case ExpresstionChangeStep.detectingFace:
            //         //GUILayout.Space(30);
            //         //if(GUILayout.Button("Detect"))
            //         //{
            //         //    curStep = ExpresstionChangeStep.expresstionEdit;
            //         //}
            //         break;
            //     case ExpresstionChangeStep.expresstionEdit:
            //         DoDrawExpressionEditUI();
            //         break;
            //     case ExpresstionChangeStep.result:
            //         break;
            //     default:
            //         break;
            // }
        }


        private void DoDrawExpressionEditUI()
        {
            GUILayout.BeginVertical();

            GUILayout.Space(10);
            CommonUI.DoInformationTooltip("Image Style", LabelMessages.FaceChangerDescription);

            GUILayout.BeginHorizontal();

            string[] options = { "Anime-Style", "Realistic-Style" };

            curStyle = (ExpressionStyle)GUILayout.Toolbar((int)curStyle, options);

            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.Label("Expression", EditorStyles.boldLabel);

            DoDrawExpressionButtonsUI();

            GUILayout.Space(20);

            //DoModelSelectUI();
            DoDrawSettings();

            GUILayout.BeginHorizontal();
            //if(GUILayout.Button("Detect Again", GUILayout.Width((thumbnailSize / 2))))
            //{
            //}

            //if(GUILayout.Button("Edit", GUILayout.Width((thumbnailSize / 2))))
            if (GUILayout.Button("Edit"))
            {
                if (curInfoList != null && curInfoList.Count > selectedGridIdx)
                {
                    ExpressionConfigData info = curInfoList[selectedGridIdx];
                    if (info != null)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.ExpressionChange(info,
                            GenerateWindow.selectItem, outputCount,
                            (imageData) =>
                            {
                                if (imageData != null)
                                {
                                    // var history = DataCache.AddHistory(imageData, "facechanger");
                                    // GenerateWindow.selectItem = history;
                                    // GenerateWindow.window.Repaint();
                                    EditorCoroutineUtility.StartCoroutineOwnerless(DataCache.LoadHistoryAsync(
                                        GenerateWindow.selectItem.seq,
                                        (c) => { GenerateWindow.window.Repaint(); }));
                                }
                            }));
                    }
                }
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DoDrawExpressionButtonsUI()
        {
            if (curStyle == ExpressionStyle.anime)
            {
                curInfoList = DataCache.animeInfoList;
            }
            else
            {
                curInfoList = DataCache.realInfoList;
            }

            if (curInfoList != null)
            {
                string[] gridStrings = new string[curInfoList.Count];
                for (int i = 0; i < gridStrings.Length; ++i)
                {
                    if (string.IsNullOrEmpty(curInfoList[i].keywordName) == true)
                    {
                        string[] arrStrs = curInfoList[i].keyword.Split('_');
                        if (arrStrs.Length > 0)
                        {
                            gridStrings[i] = arrStrs[arrStrs.Length - 1];
                        }
                    }
                    else
                    {
                        gridStrings[i] = curInfoList[i].keywordName;
                    }

                    if (gridStrings[i].Contains("calm") || gridStrings[i].Contains("normal"))
                        gridStrings[i] = gridStrings[i].Replace(gridStrings[i], "Neutral");

                    if (gridStrings[i].Contains("anger"))
                        gridStrings[i] = gridStrings[i].Replace(gridStrings[i], "Angry");

                    gridStrings[i] = Regex.Replace(gridStrings[i], @"\b[a-z]", m => m.Value.ToUpper());
                }

                selectedGridIdx = GUILayout.SelectionGrid(selectedGridIdx, gridStrings, 4);
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

            GUILayout.Label("Expression Edit",
                curStep == ExpresstionChangeStep.expresstionEdit ? backgroundLabelStyle : titleStyle,
                GUILayout.ExpandWidth(true), GUILayout.Height(25));
            GUILayout.Label(">>", titleStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25));
            GUILayout.Label("Result", curStep == ExpresstionChangeStep.result ? backgroundLabelStyle : titleStyle,
                GUILayout.ExpandWidth(true), GUILayout.Height(25));

            GUILayout.EndHorizontal();
        }

        private void DoDrawSettings()
        {
            GUIStyle ratiroTitle = new GUIStyle(GUI.skin.box);
            ratiroTitle.fontStyle = FontStyle.Bold;
            ratiroTitle.normal.textColor = Color.white;
            ratiroTitle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.darkGray);

            //GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.fontStyle = FontStyle.Bold;

            //Rect titleRect = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
            //GUI.Box(titleRect, GUIContent.none, ratiroTitle);

            //Rect foldoutRect = new Rect(titleRect.x + 5, titleRect.y, titleRect.width, titleRect.height);
            //isSettingShow = EditorGUI.Foldout(foldoutRect, isSettingShow, "Settings", true, foldoutStyle);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.helpBox);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Settings", titleStyle);

            //if (isSettingShow)
            {
                GUILayout.BeginVertical();
                EditorGUILayout.Space(10);

                // Draw Slider
                GUILayout.BeginHorizontal();
                GUILayout.Label("Output Count", EditorStyles.boldLabel);
                outputCount = (int)EditorGUILayout.Slider(outputCount, MinOutputCount, MaxOutputCount);
                if (outputCount != prevOutputCount)
                {
                    prevOutputCount = outputCount;
                    GUI.FocusControl(null);
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
                GUILayout.EndVertical();
            }
        }

        private void DoModelSelectUI()
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
            isShow = EditorGUI.Foldout(foldoutRect, isShow, "Model Select", true, foldoutStyle);

            if (isShow)
            {
                GUILayout.BeginVertical();
                EditorGUILayout.Space(10);
                // Info Label
                string strInfo =
                    "To use this feature, you must select a model. \n Choosing the right model improves the quality of the output.";

                GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
                centeredStyle.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label(strInfo, centeredStyle, GUILayout.ExpandWidth(true));

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

        private void DoLoraStrength()
        {
            GUILayout.Label("LoRA Components", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Strength");
            loraStrengthValue = GUILayout.HorizontalSlider(loraStrengthValue, MinSliderValue, MaxSliderValue);
            if (loraStrengthValue != _pastLoraStrengthValue)
            {
                _pastLoraStrengthValue = loraStrengthValue;
                GUI.FocusControl(null);
            }

            string textValue = EditorGUILayout.TextField(loraStrengthValue.ToString("F2"), GUILayout.Width(50));
            if (float.TryParse(textValue, out float parsedValue))
            {
                loraStrengthValue = Mathf.Clamp(parsedValue, MinSliderValue, MaxSliderValue);
            }

            GUILayout.EndHorizontal();
        }
    }
}