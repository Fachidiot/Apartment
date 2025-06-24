using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

namespace GameAIfySDK
{
    public class PresetWindow : EditorWindow
    {
        public readonly string[] _textModelMainCategory = Enum.GetNames(typeof(PresetModelMainCategory));
        private PresetModelMainCategory previousMainCategory = PresetModelMainCategory.All;
        public static PresetModelMainCategory presetModelMainCategory = PresetModelMainCategory.All;
        private static PresetWindow window;
        private static Dictionary<int, ModelInfo> _categoryDictionary = new Dictionary<int, ModelInfo>();
        private Vector2 _scrollPosition = Vector2.zero;
        private static bool _isStandardOn = true;
        private static bool _previousStandardOn = true;
        private static bool _isBetaOn = true;
        private static bool _previousBetaOn = true;
        private static bool _isPrivateOn = false;
        private static bool _previousPrivateOn = false;
        private static bool _isB2BOn = false;
        private static bool _previousB2BOn = false;

        private void OnGUI()
        {
            if (!window)
            {                
                ShowWindow();
            }
            GUILayout.BeginHorizontal();
            DoSideType();
            DoDrawModel();
            GUILayout.EndHorizontal();
        }

        private void OnEnable()
        {
            SetTab();
            if (DataCache.modelDictionary.Count == 0)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(DataCache.LoadModelDataAsync());
            }        
        }

        public static void ShowWindow()
        {
            window = GetWindow<PresetWindow>("Presets");
            window.minSize = new Vector2(Constants.WINDOW_MIN_WIDTH, window.minSize.y);
        }

        public void SetTab()
        {
            var filteredDictionary = new Dictionary<int, ModelInfo>();

            foreach (var kvp in DataCache.modelDictionary)
            {
                if ((!_isStandardOn && kvp.Value.category == (int)PresetGroup.Standard) || (!_isBetaOn && kvp.Value.category == (int)PresetGroup.Beta))
                    continue;
                if ((!_isPrivateOn && kvp.Value.category == (int)PresetGroup.Private) || (!_isB2BOn && kvp.Value.category == (int)PresetGroup.B2B))
                    continue;
                if (presetModelMainCategory != PresetModelMainCategory.All && kvp.Value.groupNum != (int)presetModelMainCategory)
                {
                    continue;
                }
                if(kvp.Value.category >= 99)
                    continue;
                filteredDictionary.Add(kvp.Key, kvp.Value);
            }

            _categoryDictionary = filteredDictionary;
        }

        private void DoSideType()
        {
            GUILayout.BeginVertical(GUILayout.Width(Constants.MODEL_TOGGLE_WIDTH));
            GUILayout.Space(50);
            GUILayout.Label("Model Type", EditorStyles.boldLabel);

            _isStandardOn = GUILayout.Toggle(_isStandardOn, "Standard");
            if (_previousStandardOn != _isStandardOn)
            {
                _previousStandardOn = _isStandardOn;
                SetTab();
            }

            _isBetaOn = GUILayout.Toggle(_isBetaOn, "Beta");
            if (_previousBetaOn != _isBetaOn)
            {
                _previousBetaOn = _isBetaOn;
                SetTab();
            }
            
            _isPrivateOn = GUILayout.Toggle(_isPrivateOn, "Private");
            if (_previousPrivateOn != _isPrivateOn)
            {
                _previousPrivateOn = _isPrivateOn;
                SetTab();
            }
            
            _isB2BOn = GUILayout.Toggle(_isB2BOn, "B2B");
            if (_previousB2BOn != _isB2BOn)
            {
                _previousB2BOn = _isB2BOn;
                SetTab();
            }

            GUILayout.EndVertical();
            GUILayout.Box("", GUILayout.ExpandHeight(true));
        }

        public void DoDrawModel()
        {
            GUILayout.BeginVertical();

            //presetModelMainCategory = (PresetModelMainCategory)GUILayout.Toolbar((int)presetModelMainCategory, _textModelMainCategory, GUILayout.ExpandWidth(true));

            if (presetModelMainCategory != previousMainCategory)
            {
                previousMainCategory = presetModelMainCategory;
                SetTab();
            }
            DoModelPreviewImageTab();
            GUILayout.EndVertical();
        }

        private void DoModelPreviewImageTab()
        {
            float leftExcludeWidth = Constants.MODEL_TOGGLE_WIDTH;
            float windowWidth = EditorGUIUtility.currentViewWidth - leftExcludeWidth;
            int columns = Mathf.Min(4, Mathf.Max(1, Mathf.FloorToInt(windowWidth / Constants.IMAGE_MIN_WIDTH)));
            float thumbnailWidth = windowWidth / columns - 10;

            int itemsInRow = 0;
            int itemsInCol = 0;

            if (_categoryDictionary != null && _categoryDictionary.Count > 0)
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
                GUILayout.BeginHorizontal();

                foreach (var modelInfo in _categoryDictionary.Values)
                {
                    if (itemsInRow >= columns)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                        GUILayout.BeginHorizontal();
                        itemsInRow = 0;
                        itemsInCol++;
                    }

                    GUILayout.BeginVertical();

                    if (GUILayout.Button(modelInfo.texture != null ? new GUIContent(modelInfo.texture) : new GUIContent("Loading..."), GUILayout.Width(thumbnailWidth), GUILayout.Height(thumbnailWidth)))
                    {
                        DataCache.SelectPreset(modelInfo);
                        window.Close();
                    }
                    Rect buttonRect = GUILayoutUtility.GetLastRect();

                    float bubbleWidth = 70f;
                    float bubbleHeight = 20f;
                    Rect bubbleRect = new Rect(buttonRect.xMax - bubbleWidth - 10, buttonRect.y + 10, bubbleWidth, bubbleHeight);
                    GUI.Box(bubbleRect, Enum.GetName(typeof(PresetGroup), modelInfo.category), CustomStyle.bubbleStyle);
                    GUILayout.Label(modelInfo.enName);
                    GUILayout.EndVertical();

                    itemsInRow++;
                }
                if (itemsInRow < columns)
                    GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.EndScrollView();
            }
        }

    }
}
