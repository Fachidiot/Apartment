using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameAIfySDK;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace GameAIfySDK
{
    public partial class GenerateWindow : EditorWindow
    {
        public static GenerateWindow window;

        public static Dictionary<string, List<SubCategory>> textSideBarType;

        public static Dictionary<SideBarSubCategoryItem, SubCategory> textSideBarSubType =
            new Dictionary<SideBarSubCategoryItem, SubCategory>();

        public static SideBarSubCategoryItem _prevSubCategory = SideBarSubCategoryItem.Posts;

        public static float loraStrengthValue = 0.8f;
        public static ModelInfo selectPreset;
        public static HistoryData selectItem;

        private const float MinSliderValue = 0.0f;
        private const float MaxSliderValue = 1.0f;
        private const int MinImageCountValue = 1;
        private const int MaxImageCountValue = 4;

        private readonly string[] _textResolution = Enum.GetNames(typeof(Resolution));
        private readonly string[] _textModelMainCategory = Enum.GetNames(typeof(PresetModelMainCategory));
        private readonly string[] _textSortBy = Enum.GetNames(typeof(SortBy));
        private readonly string[] _textReferenceImageMode = Enum.GetNames(typeof(ReferenceImageMode));

        private static Dictionary<string, bool> _isSideBarExpanded = new Dictionary<string, bool>();
        private static SideBarMainCategoryItem _selectedMainCategory = SideBarMainCategoryItem.Gallery;
        private static SideBarSubCategoryItem _selectedSubCategory = SideBarSubCategoryItem.Posts;


        private Dictionary<SideBarSubCategoryItem, ISubCategoryUIHandler> uiHandlers;

        private static string inputPrompt = "";
        private static string inputNegativePrompt = "";
        private int inputRemixSeq = 0;

        private string _lastUpdateTime = "";
        private int _selectedResolution = 0;
        private PresetModelMainCategory _selectedPostCategory = PresetModelMainCategory.All;
        private PresetModelMainCategory _previousPostCategory;
        private PresetModelMainCategory _selectedGalleryCategory = PresetModelMainCategory.All;
        private PresetModelMainCategory _previousGalleryCategory;
        private SortBy _selectedSortByMenu = SortBy.Like;
        private SortBy _previousSortByMenu;
        private ReferenceImageMode _selectedReferenceImageMode = ReferenceImageMode.Reference;

        private string _functionDescription;
        private float _referenceStrengthValue = 0.8f;
        private Vector2 _scrollPosition = Vector2.zero;
        private Texture2D _referenceImage;
        private int _imageCountValue = 1;
        private bool _isClickSelectFromGallery = false;

        
        [MenuItem("Window/GameAIfy/Generate Assets &G")]
        public static void ShowWindow()
        {
            window = GetWindow<GenerateWindow>("GameAIfy");
            window.minSize = new Vector2(Constants.WINDOW_MIN_WIDTH, Constants.WINDOW_MIN_HEIGHT);

            if (!EditorPrefs.HasKey(Constants.USN))
            {
                if (Alerts.FailDialog(AlertMessages.APIKeyNull))
                {
                    PluginSettingsWindow.ShowWindow();
                }
            }
        }

        private void OnGUI()
        {
            if (!window)
            {
                ShowWindow();
            }

            DoTopBarGUI();

            GUILayout.BeginHorizontal();

            DoSideCategoryBar(Constants.SIDEBAR_WIDTH);
            DoContentArea();
            GUILayout.EndHorizontal();
        }


        private void OnEnable()
        {
            if (DataCache.modelDictionary.Count == 0)
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync(true));
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadPostDataAsync());
                EditorCoroutineUtility.StartCoroutineOwnerless(DataCache.LoadModelDataAsync());
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadEasyCharacterMakerList());
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadMotionDataAsync());
            }

            if (textSideBarType == null)
            {
                LoadJson();
            }

            if (DataCache.animeInfoList.Count == 0)
            {
                DataCache.LoadFacialExpressionData();
            }

            uiHandlers = new Dictionary<SideBarSubCategoryItem, ISubCategoryUIHandler>
            {
                { SideBarSubCategoryItem.FacialExpression, new FacialExpressionUIHandler() },
                { SideBarSubCategoryItem.BackgroundRemove, new BackgroundRemoveUIHandler() },
                { SideBarSubCategoryItem.Inpainting, new InpaintingUIHandler() },
                { SideBarSubCategoryItem.Outpainting, new OutpaintingUIHandler() },
                { SideBarSubCategoryItem.BackgroundLoop, new BackgroundLoopUIHandler() },
                { SideBarSubCategoryItem.DepthMeshConverter, new DepthMapUIHandler() },
                { SideBarSubCategoryItem.Enhance, new EnhanceUIHandler() },
                { SideBarSubCategoryItem.Text2Motion, new TextToMotionUIHandler() },
            };
            LoadTexture();
            OnModelChange();
        }

        private void LoadJson()
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>("data");

            if (jsonAsset != null)
            {
                string jsonString = jsonAsset.text;
                textSideBarType = JsonConvert.DeserializeObject<Dictionary<string, List<SubCategory>>>(jsonString);

                foreach (var key in textSideBarType.Keys)
                {
                    _isSideBarExpanded.Add(key, true);
                }

                foreach (var value in textSideBarType.Values)
                {
                    foreach (var subCategory in value)
                    {
                        textSideBarSubType.Add(subCategory.Index, subCategory);
                    }
                }
            }
        }

        private void DoTopBarGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("GameAIfy", CustomStyle.titleLabelStyle);
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(CustomStyle.settingButtonContent))
            {
                PluginSettingsWindow.ShowWindow();
            }

            GUILayout.EndHorizontal();

            GUILayout.Box("", GUILayout.Height(3), GUILayout.ExpandWidth(true));
        }

        private void DoContentArea()
        {
            switch (_selectedMainCategory)
            {
                case SideBarMainCategoryItem.Gallery:
                    DoPostTab();
                    break;
                case SideBarMainCategoryItem.Generator:
                    DoGeneratorTab();
                    DoPreviewImageTab();
                    break;
                case SideBarMainCategoryItem.Editor:
                case SideBarMainCategoryItem.Extender:
                case SideBarMainCategoryItem._3D:
                    if (selectItem == null && _selectedSubCategory != SideBarSubCategoryItem.Text2Motion)
                    {
                        DoEditSelectSection();
                    }
                    else
                    {
                        DoEditMainSection();
                        if (_selectedSubCategory != SideBarSubCategoryItem.Text2Motion)
                            DoHistoryInfo();
                    }

                    break;
                default:
                    DoPostTab();
                    break;
            }
        }

        private void DoSideCategoryBar(float width)
        {
            GUILayout.BeginVertical(GUILayout.Width(width));
            int mainCategoryIndex = 0;
            if (textSideBarType == null)
            {
                LoadJson();
            }

            foreach (KeyValuePair<string, List<SubCategory>> entry in textSideBarType)
            {
                if (GUILayout.Button(entry.Key))
                {
                    _isSideBarExpanded[entry.Key] = !_isSideBarExpanded[entry.Key];
                }

                Rect lastRect = GUILayoutUtility.GetLastRect();

                string arrow = _isSideBarExpanded[entry.Key] ? "▼" : "▶";

                Rect arrowRect = new Rect(lastRect.x + lastRect.width - 20, lastRect.y, 20, lastRect.height);
                GUI.Label(arrowRect, arrow);

                if (_isSideBarExpanded[entry.Key])
                {
                    EditorGUILayout.BeginVertical("box");
                    for (int i = 0; i < entry.Value.Count; i++)
                    {
                        //임시 코드 Text2Motion 비활성화
                        //EditorGUI.BeginDisabledGroup(entry.Value[i].Index == SideBarSubCategoryItem.Text2Motion);
                        var btn = new GUIContent(entry.Value[i].Name, entry.Value[i].Tooltip);
                        // var style = _selectedSubCategory == entry.Value[i].Index
                        //     ? CustomStyle.GetSelectedButtonStyle()
                        //     : GUI.skin.button;
                        var tempBG = GUI.skin.button.normal.background;
                        var tempTxt = GUI.skin.button.normal.textColor;
                        
                        if (_selectedSubCategory == entry.Value[i].Index)
                        {
                            //GUI.skin.button.normal.background =
                            //    CustomStyle.CreateColorTexture(new Color(0, 0.5333f, 0.8f, 1));
                            //GUI.skin.button.normal.textColor = Color.white;
                            GUIStyleState uIStyleState = new GUIStyleState();
                            uIStyleState.background = CustomStyle.CreateColorTexture(new Color(0, 0.5333f, 0.8f, 1));
                            GUI.skin.button.normal = uIStyleState;
                            GUI.skin.button.normal.textColor = Color.white;
                        }
                        
                        if (GUILayout.Button(btn, GUILayout.ExpandWidth(true)))
                        {                            
                            if (_selectedSubCategory != entry.Value[i].Index)
                            {
                                GUI.FocusControl(null);

                                _isClickSelectFromGallery = false;

                                ChangeCategory((SideBarSubCategoryItem)entry.Value[i].Index);

                                _functionDescription = entry.Value[i].Description;
                                if (_selectedMainCategory == SideBarMainCategoryItem.Generator)
                                {
                                    if (_selectedGalleryCategory != PresetModelMainCategory.All)
                                        EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync(true));

                                    if (_selectedSubCategory == SideBarSubCategoryItem.CharactersinBulk)
                                    {
                                        selectPreset = DataCache.GetSelectModel(PresetModelMainCategory.Character);
                                    }
                                    else
                                    {
                                        selectPreset =
                                            DataCache.GetSelectModel(Enums.ConvertCategory(_selectedSubCategory));
                                    }

                                    if (selectPreset != null)
                                        loraStrengthValue = selectPreset.loras[0].strength;
                                    _referenceImage = null;
                                }
                            }
                        }

                        GUI.skin.button.normal.background = tempBG;
                        GUI.skin.button.normal.textColor = tempTxt;
                    }

                    EditorGUILayout.EndVertical();
                }

                mainCategoryIndex++;
                EditorGUILayout.Space(30);
            }

            DoSideButtonGUI();
            DoContributeButtonGUI();
            GUILayout.EndVertical();
            GUILayout.Box("", GUILayout.ExpandHeight(true), GUILayout.Width(3));
        }


        public static void ChangeCategory(SideBarSubCategoryItem category)
        {
            window.inputRemixSeq = 0;

            _prevSubCategory = _selectedSubCategory;

            _selectedSubCategory = category;
            SideBarMainCategoryItem mainCategory = GetMainCategory(category);
            _selectedMainCategory = mainCategory;
        }

        private void DoGeneratorTab()
        {
            GUILayout.BeginVertical(GUILayout.Width(Constants.GENERATETAB_WIDTH));
            DoSubCategory();
            switch (_selectedSubCategory)
            {
                case SideBarSubCategoryItem.CharacterPose:
                    DoModdlePoseBasedUI();
                    break;
                case SideBarSubCategoryItem.CharactersinBulk:
                    DoDrawEasyCharacterCreator();
                    break;
                default:
                    DoGeneratorMiddleArea();
                    break;
            }

            GUILayout.EndVertical();
        }

        private void DoPostTab()
        {
            GUILayout.BeginVertical();
            switch (_selectedSubCategory)
            {
                case SideBarSubCategoryItem.Posts:
                    DoChoiceSortByDropDown();
                    DoChoiceModelToolbar();
                    break;
                case SideBarSubCategoryItem.MyWorks:

                    if (_isClickSelectFromGallery == true)
                    {
                        GUILayout.BeginVertical();
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("< Back", EditorStyles.boldLabel, GUILayout.Width(100),
                                GUILayout.Height(50)))
                        {
                            ChangeCategory(_prevSubCategory);
                        }

                        GUILayout.EndHorizontal();
                        DoChoiceModelToolbar();
                        GUILayout.EndVertical();
                    }
                    else
                    {
                        DoSubCategory();
                        DoChoiceModelToolbar();
                    }

                    break;
            }

            DoPreviewImageTab();

            GUILayout.EndVertical();
        }

        private void DoSubCategory()
        {
            if (textSideBarSubType.TryGetValue(_selectedSubCategory, out SubCategory s))
            {
                if (selectItem != null || _selectedMainCategory == SideBarMainCategoryItem.Gallery ||
                    _selectedMainCategory == SideBarMainCategoryItem.Generator ||
                    _selectedSubCategory == SideBarSubCategoryItem.Text2Motion)
                {
                    CommonUI.DoInformationTooltip(s.Name, s.Description, CustomStyle.titleSubLabelStyle);
                }
                else
                {
                    GUILayout.Label(s.Name, CustomStyle.titleSubLabelStyle);
                }
            }

            GUILayout.Space(5);
        }

        public static void OnModelChange()
        {
            if (window != null)
            {
                window.inputRemixSeq = 0;

                window.Repaint();
            }
        }

        private void DoGeneratorMiddleArea()
        {
            DoDrawModelSelect();

            GUILayout.Space(30);
            DoDrawPrompt();
            GUILayout.Space(30);
            DoReferenceImage();
            GUILayout.Space(30);


            GUILayout.Label("Settings", CustomStyle.boldLabelStyle);
            DoImageCount();
            GUILayout.Space(5);

            if (selectPreset != null && selectPreset.aspectRatio != null && selectPreset.aspectRatio.Count != 0)
            {
                string[] s = new string[selectPreset.aspectRatio.Count];
                for (int i = 0; i < selectPreset.aspectRatio.Count; i++)
                {
                    string sFormat = "{0} ( {1} x {2}px )";
                    s[i] = String.Format(sFormat, selectPreset.aspectRatio[i].ratio, selectPreset.aspectRatio[i].width,
                        selectPreset.aspectRatio[i].height);
                }

                GUILayout.Label("Aspect Ratio");
                _selectedResolution = EditorGUILayout.Popup((int)_selectedResolution, s, GUILayout.Height(30));
            }

            GUILayout.Space(30);
            DoGenerateButton();
        }

        private void DoDrawPrompt(bool isShowNegativePrompt = true)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Positive Prompt", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Random", GUILayout.Height(20), GUILayout.Width(100)))
            {
                inputPrompt = GetRandomPrompt();
                inputRemixSeq = 0;

                GUI.FocusControl(null);
            }

            if (GUILayout.Button(CustomStyle.clearButtonContent))
            {
                inputPrompt = "";
                inputRemixSeq = 0;
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            inputPrompt = EditorGUILayout.TextArea(inputPrompt, CustomStyle.textAreaStyle, GUILayout.Height(50));

            GUILayout.Space(30);

            if (isShowNegativePrompt)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Negative Prompt", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button(CustomStyle.clearButtonContent))
                {
                    inputNegativePrompt = "";
                    inputRemixSeq = 0;
                    GUI.FocusControl(null);
                }

                GUILayout.EndHorizontal();

                inputNegativePrompt =
                    EditorGUILayout.TextArea(inputNegativePrompt, CustomStyle.textAreaStyle, GUILayout.Height(50));
            }
        }

        protected void DoDrawModelSelect()
        {
            GUILayout.Label("Model Select", EditorStyles.boldLabel);
            Rect buttonRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(120));

            GUI.BeginGroup(buttonRect);
            if (GUI.Button(new Rect(0, 0, buttonRect.width, buttonRect.height), GUIContent.none))
            {
                PresetWindow.ShowWindow();

                if (_selectedSubCategory == SideBarSubCategoryItem.CharactersinBulk)
                    PresetWindow.presetModelMainCategory = PresetModelMainCategory.Character;
                else
                    PresetWindow.presetModelMainCategory = Enums.ConvertCategory(_selectedSubCategory);
            }

            if (selectPreset == null)
            {
                CommonUI.DoSelectPreset(buttonRect);
            }
            else
            {
                CommonUI.DoDrawPreset(selectPreset);
            }

            GUI.EndGroup();

            if (selectPreset != null && selectPreset.category == (int)PresetGroup.Beta)
            {
                DoLoraStrength();
            }
        }

        private void DoImageCount()
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label("Output Count", GUILayout.Width(100));

            int newSliderValue =
                (int)GUILayout.HorizontalSlider(_imageCountValue, MinImageCountValue, MaxImageCountValue);
            if (newSliderValue != _imageCountValue)
            {
                _imageCountValue = newSliderValue;
                GUI.FocusControl(null);
            }

            string textValue = GUILayout.TextField(_imageCountValue.ToString(), GUILayout.Width(50));
            if (int.TryParse(textValue, out int parsedValue))
            {
                int clampedValue = Mathf.Clamp(parsedValue, MinImageCountValue, MaxImageCountValue);
                if (clampedValue != _imageCountValue)
                {
                    _imageCountValue = clampedValue;
                }
            }

            GUILayout.EndHorizontal();
        }

        private void DoLoraStrength()
        {
            GUILayout.Label("Model Components", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            CommonUI.DoInformationTooltip("Strength", ToolTipMessages.LoraStrength, EditorStyles.label);
            loraStrengthValue =
                GUILayout.HorizontalSlider(loraStrengthValue, MinSliderValue, MaxSliderValue, GUILayout.Width(100));

            string textValue = EditorGUILayout.TextField(loraStrengthValue.ToString("F2"), GUILayout.Width(50));
            if (float.TryParse(textValue, out float parsedValue))
            {
                loraStrengthValue = Mathf.Clamp(parsedValue, MinSliderValue, MaxSliderValue);
            }

            GUILayout.EndHorizontal();
        }

        protected void DoGenerateButton()
        {
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(inputPrompt) &&
                                         _selectedSubCategory != SideBarSubCategoryItem.CharactersinBulk ||
                                         GameAIfyUtilTools.CheckFailImageSize(_referenceImage));
            if (GUILayout.Button("Generate", GUILayout.Height(60)))
            {
                if (CheckException())
                    GenerateImage();
            }

            EditorGUI.EndDisabledGroup();
        }

        private bool CheckException()
        {
            if (string.IsNullOrEmpty(EditorPrefs.GetString(Constants.USN)))
            {
                if (Alerts.FailDialog(AlertMessages.APIKeyNull))
                {
                    PluginSettingsWindow.ShowWindow();
                }

                return false;
            }

            if (selectPreset == null && _selectedSubCategory != SideBarSubCategoryItem.CharacterPose)
            {
                Alerts.FailDialog(AlertMessages.PresetNull);
                return false;
            }


            return true;
        }

        private void DoReferenceImage()
        {
            GUILayout.BeginHorizontal();            
            CommonUI.DoInformationTooltip("Reference Image", ToolTipMessages.ReferenceImage);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(CustomStyle.clearButtonContent))
            {
                if (_referenceImage != null)
                    _referenceImage = null;
            }
            GUILayout.EndHorizontal();
            //_referenceImage = (Texture2D)EditorGUILayout.ObjectField(_referenceImage, typeof(Texture2D), false);
            if (GUILayout.Button("Load Image"))
            {
                string imgPath = EditorUtility.OpenFilePanel("Upload image", EditorPrefs.GetString(Constants.SaveFolder), "png,jpeg,jpg");
                if (!string.IsNullOrEmpty(imgPath))
                {
                    byte[] datas = System.IO.File.ReadAllBytes(imgPath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(datas);

                    if (texture != null)
                    {
                        _referenceImage = texture;                        
                    }
                }
            }

            if (_referenceImage != null)
            {
                if (GameAIfyUtilTools.CheckFailImageSize(_referenceImage))
                {
                    EditorGUILayout.HelpBox(HelpBoxMessages.TryEditInputOverSize, MessageType.Error);
                }
                else
                {
                    GUILayout.Space(10);
                    Rect rect = GUILayoutUtility.GetRect(100, 100, 100, 100);
                    Rect imgRect = new Rect(
                            rect.x + (rect.width - 100) / 2,
                            rect.y + (rect.height - 100) / 2,
                            100,
                            100);
                    GUI.DrawTexture(imgRect, _referenceImage, ScaleMode.ScaleToFit);
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    CommonUI.DoInformationTooltip("Mode", ToolTipMessages.ReferenceMode, EditorStyles.label);
                    GUILayout.FlexibleSpace();
                    _selectedReferenceImageMode =
                        (ReferenceImageMode)EditorGUILayout.Popup((int)_selectedReferenceImageMode,
                            _textReferenceImageMode,
                            GUILayout.Width(150),
                            GUILayout.Height(30));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    CommonUI.DoInformationTooltip("Strength", ToolTipMessages.ReferenceStrength, EditorStyles.label);


                    _referenceStrengthValue =
                        GUILayout.HorizontalSlider(_referenceStrengthValue, MinSliderValue, MaxSliderValue,
                            GUILayout.Width(100));

                    string textValue =
                        EditorGUILayout.TextField(_referenceStrengthValue.ToString("F2"), GUILayout.Width(50));
                    if (float.TryParse(textValue, out float parsedValue))
                    {
                        _referenceStrengthValue = Mathf.Clamp(parsedValue, MinSliderValue, MaxSliderValue);
                    }

                    GUILayout.EndHorizontal();
                }
            }
        }

        private void DoSideButtonGUI()
        {
            GUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();
            GUILayout.Label("Refresh : " + _lastUpdateTime, EditorStyles.boldLabel, GUILayout.Height(30));
            if (GUILayout.Button(CustomStyle.refreshTexture, GUILayout.Width(30), GUILayout.Height(30)))
            {
                DataCache.RefreshData();
            }

            GUILayout.EndHorizontal();
        }

        private void DoContributeButtonGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Open Web", GUILayout.Width(200), GUILayout.Height(20)))
            {
                string url = Constants.WEB_URL;
                Application.OpenURL(url);
            }

            GUILayout.Space(10);
            GUILayout.EndVertical();
        }

        private void DoChoiceModelToolbar()
        {
            switch (_selectedSubCategory)
            {
                case SideBarSubCategoryItem.Posts:
                    _selectedPostCategory = (PresetModelMainCategory)GUILayout.Toolbar((int)_selectedPostCategory,
                        _textModelMainCategory, GUILayout.ExpandWidth(true));
                    if (_selectedPostCategory != _previousPostCategory)
                    {
                        _previousPostCategory = _selectedPostCategory;
                        EditorCoroutineUtility.StartCoroutineOwnerless(LoadPostDataAsync());
                    }

                    break;
                case SideBarSubCategoryItem.MyWorks:
                    _selectedGalleryCategory = (PresetModelMainCategory)GUILayout.Toolbar((int)_selectedGalleryCategory,
                        _textModelMainCategory, GUILayout.ExpandWidth(true));
                    if (_selectedGalleryCategory != _previousGalleryCategory)
                    {
                        _previousGalleryCategory = _selectedGalleryCategory;
                        EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync(true));
                    }

                    break;
            }

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
        }

        private void DoChoiceSortByDropDown()
        {
            GUILayout.BeginHorizontal();
            DoSubCategory();

            GUILayout.FlexibleSpace();

            GUILayout.Label("Sort by", EditorStyles.boldLabel, GUILayout.Width(100));

            _selectedSortByMenu =
                (SortBy)EditorGUILayout.Popup((int)_selectedSortByMenu, _textSortBy, GUILayout.Width(150));
            if (_selectedSortByMenu != _previousSortByMenu)
            {
                _previousSortByMenu = _selectedSortByMenu;
                EditorCoroutineUtility.StartCoroutineOwnerless(LoadPostDataAsync());
            }


            GUILayout.EndHorizontal();
        }


        private void DoPreviewImageTab()
        {
            GUILayout.Box("", GUILayout.Height(3));

            GUILayout.BeginHorizontal();

            //float leftExcludeWidth = Constants.SIDEBAR_WIDTH + (IsGenerateTab() ? Constants.GENERATETAB_WIDTH : 0);
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(_selectedMainCategory);
            int columns = Mathf.Min(4, Mathf.Max(2, Mathf.FloorToInt(windowWidth / Constants.IMAGE_MIN_WIDTH)));
            float thumbnailWidth = windowWidth / columns - 10;

            int itemsInRow = 0;

            var dictionary = IsPostsTab()
                ? (IDictionary<int, BaseInfo>)DataCache.postDataDictionary
                : DataCache.myWorkDictionary;
            if (dictionary != null)
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                if (_selectedSubCategory == SideBarSubCategoryItem.MyWorks)
                {
                    GUIStyle plusButtonStyle = new GUIStyle(GUI.skin.button);
                    plusButtonStyle.richText = true;
                    string buttonText = "<size=28>+</size>\n(Upload Image)";
                    GUIContent plusContent = new GUIContent(buttonText, ToolTipMessages.UserImageUpload);

                    if (GUILayout.Button(plusContent, plusButtonStyle, GUILayout.Width(thumbnailWidth),
                            GUILayout.Height(thumbnailWidth)))
                    {
                        ImageUploadWindow.ShowWindow();
                        ImageUploadWindow.selectedCategory = _selectedGalleryCategory;
                    }

                    itemsInRow++;
                }


                // 이미지들을 순회하면서 배치
                foreach (BaseInfo imageInfo in dictionary.Values)
                {
                    if (itemsInRow >= columns)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                        GUILayout.BeginHorizontal();
                        itemsInRow = 0;
                    }

                    GUILayout.BeginVertical();

                    if (GUILayout.Button(
                            imageInfo.texture != null
                                ? new GUIContent(imageInfo.texture)
                                : new GUIContent("Loading..."), GUILayout.Width(thumbnailWidth),
                            GUILayout.Height(thumbnailWidth)))
                    {
                        if (imageInfo.texture != null)
                        {
                            if (_isClickSelectFromGallery == true)
                            {
                                _isClickSelectFromGallery = false;
                                ChangeCategory(_prevSubCategory);

                                DataCache.generateWindowHistoryMap.Clear();
                                HistoryData item = new HistoryData();
                                item.seq = imageInfo.seq;
                                item.func_num = "Original Image";
                                item.output_image_width = imageInfo.width;
                                item.output_image_height = imageInfo.height;
                                item.texture = imageInfo.texture;
                                DataCache.generateWindowHistoryMap.Add(item.seq, item);
                                selectItem = item;
                                EditorCoroutineUtility.StartCoroutineOwnerless(
                                    DataCache.LoadHistoryAsync(imageInfo.seq, (callback) => { window.Repaint(); }));
                            }
                            else
                            {
                                // 미리보기 팝업 표시
                                ShowPreviewPopup(imageInfo);
                            }
                        }
                    }

                    GUILayout.EndVertical();

                    itemsInRow++;
                }

                if (itemsInRow < columns)
                    GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(300);

                int? nextPage = null;
                Func<int, IEnumerator> loadData = null;

                if (IsPostsTab())
                {
                    if (DataCache.postDataResponse != null && DataCache.postDataResponse.meta != null &&
                        DataCache.postDataResponse.meta.nextPage != null)
                    {
                        nextPage = DataCache.postDataResponse.meta.nextPage;
                        loadData = LoadPostDataAsync;
                    }
                }
                else
                {
                    if (DataCache.myWorkDataResponse != null && DataCache.myWorkDataResponse.meta != null &&
                        DataCache.myWorkDataResponse.meta.nextPage != null)
                    {
                        nextPage = DataCache.myWorkDataResponse.meta.nextPage;
                        loadData = (int page) => LoadMyWorksDataAsync(true, page);
                    }
                }

                if (nextPage.HasValue && loadData != null)
                {
                    if (GUILayout.Button("Load More", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(loadData(nextPage.Value));
                    }
                }

                GUILayout.EndVertical();

                GUILayout.EndScrollView();
            }

            GUILayout.EndHorizontal();
        }


        private void ShowPreviewPopup(BaseInfo imageInfo)
        {
            if (_selectedSubCategory == SideBarSubCategoryItem.Posts)
            {
                //PreviewWindow window = (PreviewWindow)EditorWindow.GetWindow(typeof(PreviewWindow));
                PreviewWindow window = (PreviewWindow)EditorWindow.GetWindow(typeof(PreviewWindow));
                PreviewWindow.originalItemInfo = imageInfo;
                //window.generateWindow = this;
                window.Show();
                window.isMyWorksItem = false;

                PreviewWindow.ShowWindow();
            }
            else
            {
                PreviewWindow window = (PreviewWindow)EditorWindow.GetWindow(typeof(PreviewWindow));
                PreviewWindow.originalItemInfo = imageInfo;
                window.Show();
                window.isMyWorksItem = true;
                PreviewWindow.ShowWindow();
            }
        }

        private void GenerateImage()
        {
            EditorPrefs.SetFloat(Constants.SelectedLoraStrength, loraStrengthValue);
            InputPromptData inputPromptData = new InputPromptData();
            inputPromptData.modelInfo = selectPreset;
            inputPromptData.width = selectPreset.aspectRatio[_selectedResolution].width;
            inputPromptData.height = selectPreset.aspectRatio[_selectedResolution].height;
            inputPromptData.inputPrompt = inputPrompt;
            inputPromptData.inputNegativePrompt = inputNegativePrompt;
            inputPromptData.remixSeq = inputRemixSeq;
            inputPromptData.batchSize = _imageCountValue;


            ReferenceImageData referenceImageData = new ReferenceImageData();
            if (_referenceImage)
            {
                referenceImageData.referenceImage = _referenceImage;
                referenceImageData.strength = _referenceStrengthValue;
                referenceImageData.mode = _textReferenceImageMode[(int)_selectedReferenceImageMode].ToLower();
            }

            inputPromptData.referenceImageData = referenceImageData;

            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.GenerateImage(inputPromptData,
                (onGenerateStatus) =>
                {
                    if (onGenerateStatus == "Success")
                        EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync(true));
                }));
        }

        public void SelectUpload(HistoryData item)
        {
            if (item != null)
                selectItem = item;

            Debug.Log("upload!");
        }

        public void DoEditSelectSection()
        {
            GUILayout.BeginVertical();
            DoSubCategory();
            GUILayout.Label(_functionDescription, CustomStyle.wrapLabelStyle);
            GUILayout.Space(30);

            float boxWidth = 300f;
            float boxHeight = 50f;
            GUILayout.Label("Please select an image.", GUILayout.Width(boxWidth), GUILayout.Height(boxHeight));
            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Select from My Works", GUILayout.Width(200), GUILayout.Height(30)))
            {
                _isClickSelectFromGallery = true;
                ChangeCategory(SideBarSubCategoryItem.MyWorks);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            var width = CustomStyle.bottomLabelStyle.CalcSize(new GUIContent("or")).x;
            GUILayout.Label("or", GUILayout.Width(width + 1));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Upload Image", GUILayout.Width(200), GUILayout.Height(30)))
            {
                ImageUploadWindow.ShowWindow(SelectUpload);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(20);

            GUILayout.EndVertical();
        }

        public void DoEditMainSection()
        {
            EditorGUILayout.BeginVertical();
            DoSubCategory();
            if (_selectedSubCategory != SideBarSubCategoryItem.Text2Motion)
            {
                GUILayout.BeginHorizontal();
                // 뒤로가기 버튼
                if (GUILayout.Button("< Back to Image Selection"))
                {
                    selectItem = null;
                    foreach (KeyValuePair<string, List<SubCategory>> entry in textSideBarType)
                    {
                        for (int i = 0; i < entry.Value.Count; i++)
                        {
                            if (_selectedSubCategory == (SideBarSubCategoryItem)entry.Value[i].Index)
                            {
                                _functionDescription = entry.Value[i].Description;
                            }
                        }
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            if (uiHandlers.TryGetValue(_selectedSubCategory, out var handler))
            {
                handler.DrawUI();
            }

            EditorGUILayout.EndVertical();
        }

        private void DoHistoryInfo()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Constants.IMAGE_HISTORY_WIDTH));
            if (selectItem != null)
            {
                CommonUI.DoHistoryInfo(DataCache.generateWindowHistoryMap, (item) => { selectItem = item; });
            }

            EditorGUILayout.EndVertical();
        }

        public void RemixImage(int option_seq)
        {
            _selectedSubCategory = SideBarSubCategoryItem.Character;
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.GetImageOptionAsync(option_seq, (jsonData) =>
            {
                if (string.IsNullOrEmpty(jsonData))
                    return;

                ImageInfoResponse imageInfoResponse = JsonConvert.DeserializeObject<ImageInfoResponse>(jsonData);

                ModelInfo useModelInfo = DataCache.GetModelInfo(imageInfoResponse.imageOption.preset_num);

                useModelInfo.loras = imageInfoResponse.imageOption.loras;
                DataCache.SelectPreset(useModelInfo);
                inputPrompt = imageInfoResponse.imageOption.prompt;
                inputNegativePrompt = imageInfoResponse.imageOption.negativePrompt;

                GUI.FocusControl(null);
                Repaint();
            }));
        }

        public void RemixImage(ModelInfo useModelInfo, ImageOption imageOption, int seq)
        {
            if (useModelInfo.groupNum == 99999)
                useModelInfo.groupNum = 1;
            ChangeCategory(Enums.ConvertCategory((PresetModelMainCategory)useModelInfo.groupNum));
            useModelInfo.loras = imageOption.loras;
            DataCache.SelectPreset(useModelInfo);
            inputPrompt = imageOption.prompt;
            inputNegativePrompt = imageOption.negativePrompt;
            inputRemixSeq = seq;

            GUI.FocusControl(null);
            Repaint();
        }

        public void DeleteImage(int seq)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.DeleteImage(seq, (onGenerateStatus) =>
            {
                if (onGenerateStatus == "Success")
                {
                    DataCache.myWorkDictionary.Remove(seq);
                    DataCache.editWindowHistoryMap.Remove(seq);
                    DataCache.generateWindowHistoryMap.Remove(seq);
                    DataCache.textureCache.Remove(seq);
                    Debug.Log("Deleted Complete");
                }

                Repaint();
            }));
        }

        private string GetRandomPrompt()
        {
            List<string> exam;
            //empty : character Pose
            if (selectPreset == null)
                exam = characterPoseString;
            else if (selectPreset.examPrompt == null || selectPreset.examPrompt.Count == 0)
                return "";
            else
                exam = selectPreset.examPrompt;

            int ranNum = UnityEngine.Random.Range(0, exam.Count);
            return exam[ranNum];
        }

        public static SideBarMainCategoryItem GetMainCategory(SideBarSubCategoryItem subCategory)
        {
            string subCategoryName = subCategory.ToString();

            foreach (var kv in textSideBarType)
            {
                if (kv.Value.Any(sc =>
                        sc.Name.Replace(" ", "").Equals(subCategoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    if (Enum.TryParse(kv.Key, true, out SideBarMainCategoryItem mainCategory))
                    {
                        return mainCategory;
                    }
                }
            }

            return SideBarMainCategoryItem.Editor;
        }


        private IEnumerator LoadMyWorksDataAsync(bool isForceUpdate = false, int page = 1)
        {
            int group = IsGenerateTab() ? 0 : (int)_selectedGalleryCategory;
            yield return DataCache.LoadMyWorksDataAsync(group, page, (callback) => { Repaint(); });
        }


        private IEnumerator LoadPostDataAsync(int page = 1)
        {
            string orderByValue = _selectedSortByMenu == SortBy.Like ? "like" : "createdAt";
            yield return DataCache.LoadPostDataAsync(orderByValue, (int)_selectedPostCategory, page,
                (s) => { Repaint(); });
        }

        private IEnumerator LoadMotionDataAsync()
        {
            yield return DataCache.LoadTextToMotionList();
        }

        private bool IsPostsTab()
        {
            return _selectedSubCategory == SideBarSubCategoryItem.Posts;
        }

        private bool IsGenerateTab()
        {
            return _selectedMainCategory == SideBarMainCategoryItem.Generator;
        }

        protected Rect GetFoldRect()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.box);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            titleStyle.normal.background = CustomStyle.MakeTex(2, 2, CustomStyle.darkGray);

            Rect titleRect = GUILayoutUtility.GetRect(0, 25);
            GUI.Box(titleRect, GUIContent.none, titleStyle);

            Rect foldoutRect = new Rect(titleRect.x + 5, titleRect.y, titleRect.width, titleRect.height);

            return foldoutRect;
        }
    }
}

public class SubCategory
{
    public SideBarSubCategoryItem Index { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Tooltip { get; set; }
}

public class GenerateImageType
{
    public ModelInfo modelInfo;
}