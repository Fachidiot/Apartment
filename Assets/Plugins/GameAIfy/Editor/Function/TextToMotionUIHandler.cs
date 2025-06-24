using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
namespace GameAIfySDK
{
    public class TextToMotionUIHandler : ISubCategoryUIHandler
    {
        private string[] randomString = new string[]
    {
            "A person in push-up position, crawling forward.",
            "A person walks with confidence.",
            "A person continues running without stopping.",
            "A person runs in a zigzag pattern.",
            "A person is watching his watch while standing.",
            "A person powerfully punch.",
            "A person kicks powerfully.",
            "A person waves high while saying hello.",
            "A person performs a handstand quickly.",
            "A person perform situps two times.",
            "A person leaps high into the air.",
            "A person throws an object forward using one arm.",
            "A person keeps walking forward in a crouching pose.",
            "A person kneels down abruptly.",
            "A person stands confidently with arms crossed.",
            "A person holds their head in frustration while bending forward.",
            "A person puts both hands on their waist.",
            "A person scratches their head.",
            "A person gets pushed hard and loses balance."            
    };

        protected const int promptWidth = Constants.GENERATETAB_WIDTH - 50;

        private const string NOTICE_MESSAGE = "Enter a sentence describing the motion. Start with \"A person\" and use a complete subject-verb sentence to produce more accurate results.";
        private const int MinMotionTime = 1;
        private const int MaxMotionTime = 5;

        private const int MinOutputCount = 1;
        private const int MaxOutputCount = 4;

        private List<Texture2D> gifFrames;        
        private int currentFrame = 0;
        private float frameDelay = 0.1f;
        private float nextFrameTime;
        private bool isPlaying = false;        

        private Vector2 scrollPosition = Vector2.zero;
        private string inputDescription = string.Empty;

        private int motionTime = 3;
        private int prevMotiontime = 3;

        private int outputCount = 1;
        private int prevOutputCount = 1;

        private Text2MotionStep curStep = Text2MotionStep.generateMotion;
        private SMPLData smplData = null;        
        private TextToMotionData curSelectMotionData = null;

        private bool isMakingSMPL = false;
        private int remainTime = 0;
        private int convertingSeq = 0;
        private bool isMakingMotion = false;
        private EditorCoroutine refreshCoroutine = null;

        #region SampleMotion Field

        private string[] sampleMotionList = new string[]
           {
               "Standing",
               "Walk",
               "Punch"
           };
      
        private List<TextToMotionData> sampleMotionDataList = new List<TextToMotionData>();

        #endregion

        public void DrawUI()
        {
            LoadSampleMotion();

            EditorGUILayout.BeginHorizontal();

            DrawMiddleArea();
            DrawRightArea();            
            EditorGUILayout.EndHorizontal();            
        }

        private void DrawMiddleArea()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(Constants.GENERATETAB_WIDTH));

            EditorGUILayout.HelpBox(NOTICE_MESSAGE, MessageType.None);

            DoDrawThumbnailImage();
            EditorGUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (curStep >= Text2MotionStep.convert)
            {
                bool disable = (isMakingMotion == true || isMakingSMPL == true || isPlaying == false) ? true : false;
                EditorGUI.BeginDisabledGroup(disable);
                if (sampleMotionDataList.Contains(curSelectMotionData) == false)
                {
                    if (GUILayout.Button("Delete", GUILayout.Width(80), GUILayout.Height(20)))
                    {
                        RequestDeleteMotion();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            if (curStep < Text2MotionStep.convert)
                DoDrawMyMotionsContents();
            else
                DoDrawMotionInfo();
            EditorGUILayout.Space(30);

            CheckConverting(ref convertingSeq);

            DoDrawButton();
            EditorGUILayout.Space(30);            
            EditorGUILayout.EndVertical();
        }
        private void DoDrawThumbnailImage()
        {
            if (gifFrames != null && gifFrames.Count > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                float windowWidth = Mathf.Min(GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor),
                    Constants.PREVIEW_IMG_MAX_WIDTH);
                float thumbnailWidth = windowWidth - 10;

                float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
                float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight) / 2, Constants.PREVIEW_IMG_MAX_WIDTH);
                float thumbnailHeight = windowHeight - 10;

                float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);

                GUILayout.Box(gifFrames[currentFrame], GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));

                if (isPlaying && Time.realtimeSinceStartup > nextFrameTime)
                {
                    currentFrame = (currentFrame + 1) % gifFrames.Count;
                    nextFrameTime = Time.realtimeSinceStartup + frameDelay;
                }
                GenerateWindow.window.Repaint();

                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
        }

        private void DoDrawMyMotionsContents()
        {
            EditorGUILayout.BeginVertical();
           
            // Description 
            GUILayout.BeginHorizontal();
            GUILayout.Label("Description", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            bool disable = (isMakingMotion == true || isMakingSMPL == true) ? true : false;
            EditorGUI.BeginDisabledGroup(disable);
            if (GUILayout.Button("Random", GUILayout.Height(20), GUILayout.Width(100)))
            {
                inputDescription = GetDummyRandomPrompt();

                GUI.FocusControl(null);
            }

            if (GUILayout.Button(CustomStyle.clearButtonContent))
            {
                inputDescription = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            inputDescription = EditorGUILayout.TextArea(inputDescription, CustomStyle.textAreaStyle, GUILayout.Width(promptWidth),
                GUILayout.Height(50));

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(20);
            
            EditorGUI.BeginDisabledGroup(true);

            // Motion Slider
            GUILayout.Label("Motion Duration (sec)", EditorStyles.boldLabel);

            motionTime = (int)EditorGUILayout.Slider(3, MinMotionTime, MaxMotionTime);
            motionTime = 3;
            if (motionTime != prevMotiontime)
            {                
                GUI.FocusControl(null);
            }

            EditorGUILayout.Space(5);
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;            
            labelStyle.normal.textColor = Color.white;

            GUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("Motion Duration is currently fixed at 3 seconds.", labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();

            EditorGUILayout.Space(20);

            // Output Slider
            GUILayout.Label("Output Count", EditorStyles.boldLabel);

            EditorGUILayout.Space(5);

            outputCount = (int)EditorGUILayout.Slider(1, MinOutputCount, MaxOutputCount);          
            outputCount = 1;
			
            if (outputCount != prevOutputCount)
            {
                GUI.FocusControl(null);
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();

        }

        private void DoDrawMotionInfo()
        {
            GUILayout.BeginVertical();

            GUILayout.Label("Motion Info", EditorStyles.boldLabel);
            GUILayout.Space(5);

            GUILayout.Label(string.Format("Description : \n {0}", curSelectMotionData == null ? "" : curSelectMotionData.prompt));
            GUILayout.Space(5);

            GUILayout.Label(string.Format("Motion Duration : {0}", curSelectMotionData == null ? "3" : "3"));

            GUILayout.EndVertical();
        }
        private void DrawRightArea()
        {
            GUILayout.BeginVertical();

            DoDrawSampleMotionList();
            EditorGUILayout.Space(20);

            DoDrawMyMotionList();

            GUILayout.EndVertical();
        }

        private void DoDrawSampleMotionList()
        {
            GUILayout.Label("Sample Motion", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Generator);
            int columns = Mathf.Min(4, Mathf.Max(2, Mathf.FloorToInt(windowWidth / Constants.IMAGE_MIN_WIDTH)));
            float thumbnailWidth = windowWidth / columns - 10;

            int itemsInRow = 0;
            int tempDrawCount = 0;            
            if (sampleMotionDataList != null && sampleMotionDataList.Count > 0)
            {                
                GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();
                
                for(int i = 0; i < sampleMotionDataList.Count; ++i)
                {
                    if (tempDrawCount < 3)
                    {
                        if (itemsInRow >= columns)
                        {
                            GUILayout.EndHorizontal();
                            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                            GUILayout.BeginHorizontal();
                            itemsInRow = 0;
                        }

                        if (GUILayout.Button(
                                sampleMotionDataList[i].texture!= null
                                    ? new GUIContent(sampleMotionDataList[i].texture)
                                    : new GUIContent("Loading..."), GUILayout.Width(thumbnailWidth),
                                GUILayout.Height(thumbnailWidth)))
                        {
                            curStep = Text2MotionStep.convertFinish;
                            curSelectMotionData = sampleMotionDataList[i];
                            LoadSampleGifTextureList();
                        }

                        Rect buttonRect = GUILayoutUtility.GetLastRect();
                        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(".anim"));
                        Rect textRect = new Rect(buttonRect.xMax - textSize.x - 5, buttonRect.y + 5, textSize.x, textSize.y);
                        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                        textStyle.normal.textColor = Color.white;
                        textStyle.fontSize = 10;
                        textStyle.normal.background = CustomStyle.CreateColorTexture(Color.green);

                        GUI.Label(textRect, ".anim", textStyle);

                        itemsInRow++;
                        tempDrawCount++;
                    }
                }
                
                GUILayout.EndHorizontal();
                


                GUILayout.EndVertical();                
            }

            GUILayout.EndHorizontal();
        }

        private void DoDrawMyMotionList()
        {
            GUILayout.Label("My Motion", EditorStyles.boldLabel);
            GUILayout.Box("", GUILayout.Height(3));

            GUILayout.BeginHorizontal();
            
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Generator);
            int columns = Mathf.Min(4, Mathf.Max(2, Mathf.FloorToInt(windowWidth / Constants.IMAGE_MIN_WIDTH)));
            float thumbnailWidth = windowWidth / columns - 10;

            int itemsInRow = 0;            

            var dictionary = DataCache.motionDictionary;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);

            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("+ Create New"), GUILayout.Width(thumbnailWidth),
            GUILayout.Height(thumbnailWidth)))
            {
                gifFrames = null;
                currentFrame = 0;
                isPlaying = false;
                curStep = Text2MotionStep.generateMotion;
            }
            itemsInRow++;

            foreach (KeyValuePair<int, TextToMotionData> pair in dictionary)
            {
                if (itemsInRow >= columns)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(3));
                    GUILayout.BeginHorizontal();
                    itemsInRow = 0;
                }

                if (pair.Value != null)
                {
                    if (GUILayout.Button(
                    pair.Value.texture != null
                        ? new GUIContent(pair.Value.texture)
                        : new GUIContent("Loading..."), GUILayout.Width(thumbnailWidth),
                    GUILayout.Height(thumbnailWidth)))
                    {
                        switch ((SmplStatus)pair.Value.is_smpl_made)
                        {
                            case SmplStatus.none:
                                curStep = Text2MotionStep.convert;
                                break;
                            case SmplStatus.Creating:
                                curStep = Text2MotionStep.converting;
                                break;
                            case SmplStatus.Complete:
                                curStep = Text2MotionStep.convertFinish;
                                break;
                        }

                        curSelectMotionData = pair.Value;
                        LoadGifTextureList();
                    }

                    if((SmplStatus)pair.Value.is_smpl_made == SmplStatus.Complete)
                    {
                        Rect buttonRect = GUILayoutUtility.GetLastRect();
                        Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(".anim"));
                        Rect textRect = new Rect(buttonRect.xMax - textSize.x - 5, buttonRect.y + 5, textSize.x, textSize.y);
                        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
                        textStyle.normal.textColor = Color.white;
                        textStyle.fontSize = 10;
                        textStyle.normal.background = CustomStyle.CreateColorTexture(Color.green);

                        GUI.Label(textRect, ".anim", textStyle);
                    }
                }

                itemsInRow++;
            }

            GUILayout.EndHorizontal();

            //GUILayout.Space(300);

            //if (DataCache.myWorkDataResponse.meta != null &&
            //         DataCache.myWorkDataResponse.meta.nextPage != null)
            //{
            //    if (GUILayout.Button("Load More", GUILayout.ExpandWidth(true), GUILayout.Height(30)))
            //    {
            //        //EditorCoroutineUtility.StartCoroutineOwnerless(
            //        //    LoadMyWorksDataAsync(true, (int)DataCache.myWorkDataResponse.meta.nextPage));
            //    }
            //}

            GUILayout.EndVertical();

            GUILayout.EndScrollView();

            GUILayout.EndHorizontal();
        }

        private void DoDrawButton()
        {
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                wordWrap = true // 자동 줄바꿈 활성화
            };

            switch (curStep)
            {
                case Text2MotionStep.generateMotion:
                    bool disable = (isMakingMotion == true || isMakingSMPL == true || string.IsNullOrEmpty(inputDescription) == true) ? true : false;
                    EditorGUI.BeginDisabledGroup(disable);
                    if(GUILayout.Button("Generate Motion Preview"))
                    {
                        // joint생성
                        RequestJoint();
                    }
                    EditorGUI.EndDisabledGroup();
                    break;
                case Text2MotionStep.convert:
                case Text2MotionStep.converting:
                    GUILayout.Label("Must convert to .anim to enable download. The conversion takes approximately 5 minutes", labelStyle, GUILayout.Width(Constants.GENERATETAB_WIDTH));
                    if (isMakingSMPL && convertingSeq > 0)
                    {
                        GUILayout.BeginVertical();                        
                        EditorGUILayout.HelpBox(string.Format(HelpBoxMessages.SMPLConvertStatus, remainTime), MessageType.Warning);
                        if(isMakingSMPL && convertingSeq > 0 && remainTime <= 0 && convertingSeq == curSelectMotionData.seq)
                        {
                            if (GUILayout.Button("Refresh"))
                            {
                                RequestMotionDetail(convertingSeq);
                            }
                        }                        
                        GUILayout.EndVertical();
                    }                    
                    EditorGUI.BeginDisabledGroup(isMakingSMPL || isMakingMotion);
                    if (GUILayout.Button("Convert to .anim"))
                    {
                        RequestConvert();
                    }
                    EditorGUI.EndDisabledGroup();
                    break;
                case Text2MotionStep.convertFinish:
                    GUILayout.Label("Download takes approximately 1 minute. Unity features will be unavailable during the download process.", labelStyle, GUILayout.Width(Constants.GENERATETAB_WIDTH));
                    if (GUILayout.Button("Download .anim (humanoid)"))
                    {
                        DownloadHumanoidAnim();
                    }
                    break;
            }
        }         

        private void RequestJoint()
        {
            if(string.IsNullOrEmpty(inputDescription) == false)
            {
                isMakingMotion = true;

                int key = GameAIfyUtilTools.GetRandomKey();
                TextToMotionData tempData = new TextToMotionData();
                tempData.seq = key;
                tempData.texture = null;
                DataCache.motionDictionary.Add(key, tempData);

                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestTextToMotionCreate(inputDescription, motionTime,
                (result) =>
                {
                    if (result == "fail")
                    {
                        isMakingMotion = false;
                        return;
                    }

                    JointDataResponse response = JsonConvert.DeserializeObject<JointDataResponse>(result);
                    
                    if (response != null)
                    {
                        if(response.data != null)
                        {
                            if(string.IsNullOrEmpty(response.data.thumbnailPath) == false)
                            {
                                EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.GetTextureAsync(response.data.thumbnailPath,
                                (texture) =>
                                {
                                    if(DataCache.motionDictionary.ContainsKey(response.data.seq) == false)
                                    {
                                        TextToMotionData tempData = new TextToMotionData();
                                        tempData.seq = response.data.seq;
                                        tempData.is_smpl_made = 0;
                                        tempData.prompt = inputDescription;
                                        tempData.thumbnailPath = response.data.thumbnailPath;
                                        tempData.texture = texture;
                                        tempData.imgPath = response.data.imagePath;
                                        
                                        curSelectMotionData = tempData;

                                        tempData.gif = LoadGifTextureList();

                                        DataCache.motionDictionary.Remove(key);
                                        DataCache.motionDictionary.Add(response.data.seq, tempData);

                                        curStep = Text2MotionStep.convert;
                                        isMakingMotion = false;
                                    }
                                }));
                            }                          
                        }                                              
                    }
                    
                }));
            }
        }

        private void RequestConvert()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestTextToMotionMakeSMPL(curSelectMotionData.seq,
            (result) =>
            {
                if(result != "fail" && result != null)
                {
                    ConvertSMPLResponse response = JsonConvert.DeserializeObject<ConvertSMPLResponse>(result);
                    if(response != null)
                    {
                        if(response.status == "success")
                        {
                            isMakingSMPL = true;
                            string strRemainTime = string.Empty;
                            
                            if (string.IsNullOrEmpty(response.reaminTime) == false)
                            {
                                strRemainTime = System.Text.RegularExpressions.Regex.Replace(response.reaminTime, @"\D", "");
                                remainTime = int.Parse(strRemainTime) * 60;                                
                            } 
                            if(remainTime <= 0)
                            {
                                remainTime = 300;
                            }

                            TextToMotionData motionData = null;
                            if(DataCache.motionDictionary.TryGetValue(curSelectMotionData.seq, out motionData))
                            {
                                motionData.is_smpl_made = 1;
                            }
                            
                            refreshCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(RefreshMotionData());
                        }
                    }
                }               
            }));
        }

        private void RequestMotionDetail(int seq)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestTextToMotionDetail(seq, (detailStatus) =>
            {
                if (detailStatus == "fail")
                    return;

                TextToDetailMotionDataResponse response = JsonConvert.DeserializeObject<TextToDetailMotionDataResponse>(detailStatus);

                if (response.status == "success")
                {
                    if (response.data != null)
                    {
                        if (DataCache.motionDictionary.ContainsKey(response.data.seq) == false)
                        {
                            DataCache.motionDictionary.Add(response.data.seq, response.data);
                        }
                        else
                        {
                            DataCache.motionDictionary[response.data.seq] = response.data;
                        }

                        if (CheckConverting(ref convertingSeq) == false)
                        {                            
                            if (refreshCoroutine != null)
                            {
                                EditorCoroutineUtility.StopCoroutine(refreshCoroutine);
                                refreshCoroutine = null;                                
                            }

                            isMakingSMPL = false;
                            remainTime = 0;
                            convertingSeq = 0;
                            curSelectMotionData = DataCache.motionDictionary[seq];
                            curStep = Text2MotionStep.convertFinish;
                        }
                    }
                }
            }));
        }

        private void DownloadHumanoidAnim()
        {                        
            if (curSelectMotionData == null)
                return;

            if(curSelectMotionData.smplData == null)
            {
                if (string.IsNullOrEmpty(curSelectMotionData.smplPath))
                    return;

                EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.LoadMotionJsonFile(curSelectMotionData.smplPath,
                (result) =>
                {
                    smplData = JsonConvert.DeserializeObject<SMPLData>(result);
                    curSelectMotionData.smplData = smplData;
                    CreateHumanoid(curSelectMotionData.seq, smplData);
                }));
            }  
            else
            {
                CreateHumanoid(curSelectMotionData.seq, curSelectMotionData.smplData);
            }
        }

        private void CreateHumanoid(int seq, SMPLData smplData)
        {
            if (smplData != null)
            {
                string assetPath = EditorUtility.SaveFilePanel("Save As", EditorPrefs.GetString(Constants.SaveFolder), "NewAnimationClip.anim", "anim");
                if (string.IsNullOrEmpty(assetPath))
                {
                    return;
                }

                string assetsFolderPath = Application.dataPath;

                if (assetPath.StartsWith(assetsFolderPath))
                {
                    string relativePath = "Assets" + assetPath.Substring(assetsFolderPath.Length);                    
                    if (smplData != null)
                    {
                        AnimationClip clip = TextToMotionProcessor.CreateAnimation(smplData);
                        if (clip != null)
                        {
                            AnimationClip newClip = new AnimationClip();

                            EditorUtility.CopySerialized(clip, newClip);

                            AssetDatabase.CreateAsset(newClip, relativePath);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    AssetDatabase.Refresh();
                }
                else
                {
                    string relativePath = assetsFolderPath + "/tempAnimation.anim";
                    relativePath = "Assets" + relativePath.Substring(assetsFolderPath.Length);                    
                    if (smplData != null)
                    {
                        AnimationClip clip = TextToMotionProcessor.CreateAnimation(smplData);
                        if (clip != null)
                        {
                            AnimationClip newClip = new AnimationClip();

                            EditorUtility.CopySerialized(clip, newClip);

                            AssetDatabase.CreateAsset(newClip, relativePath);
                            AssetDatabase.SaveAssets();

                            string newPath = AssetDatabase.GetAssetPath(newClip);
                            string targetPath = assetPath;

                            try
                            {
                                File.Copy(newPath, targetPath, true);
                                AssetDatabase.DeleteAsset(newPath);
                            }
                            catch
                            {
                                Debug.LogError("Save Fail");
                            }
                        }
                    }                    
                }
            }
        }

        private void RequestDeleteMotion()
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestTextToMotionDelete(curSelectMotionData.seq, 
                (result) =>
                {
                    if (string.IsNullOrEmpty(result) == false)
                    {
                        if(result.Equals("Success"))
                        {
                            if(DataCache.motionDictionary.ContainsKey(curSelectMotionData.seq))
                            {
                                DataCache.motionDictionary.Remove(curSelectMotionData.seq);
                                curSelectMotionData = null;
                                inputDescription = string.Empty;

                                gifFrames = null;
                                currentFrame = 0;
                                isPlaying = false;
                                
                                curStep = Text2MotionStep.generateMotion;                                
                            }
                        }
                    }
                }));
        }

        private void LoadSampleMotion()
        {
            if (sampleMotionDataList == null || (sampleMotionDataList != null && sampleMotionDataList.Count == 0))
            {
                for (int i = 0; i < sampleMotionList.Length; ++i)
                {
                    TextToMotionData data = new TextToMotionData();
                    
                    // set prompt
                    TextAsset textFile = Resources.Load<TextAsset>("SampleMotion/" + sampleMotionList[i] + "Text");
                    if(textFile != null)
                        data.prompt = textFile.text;

                    // set SMPL data
                    textFile = Resources.Load<TextAsset>("SampleMotion/" + sampleMotionList[i] + "Smpl");
                    if(textFile != null)
                    {
                        data.smplData = JsonConvert.DeserializeObject<SMPLData>(textFile.text);
                    }

                    // set gif
                    Texture2D texture = Resources.Load<Texture2D>("SampleMotion/" + sampleMotionList[i]);
                    if(texture != null)
                    {
                        data.texture = texture;
                        data.imgPath = Application.dataPath + "/GameAIfy/Resources/SampleMotion/" + sampleMotionList[i] + ".gif";
                    }
                    //byte[] bytes = File.ReadAllBytes(Application.dataPath + "/GameAIfy/Resources/SampleMotion/" + sampleMotionList[i] + ".gif");

                    //if (bytes.Length > 0)
                    //{
                    //    GifDecoder decoder = new GifDecoder(bytes);
                    //    data.gif = decoder.Frames;

                    //    data.texture = decoder.Frames[0];
                    //}

                    sampleMotionDataList.Add(data);
                }
            }
        }

        private List<Texture2D> LoadGifTextureList()
        {
            List<Texture2D> gifTextureList = null;
            if (curSelectMotionData != null)
            {
                if (curSelectMotionData.gif == null || (curSelectMotionData.gif != null && curSelectMotionData.gif.Count <= 0))
                {
                    if(string.IsNullOrEmpty(curSelectMotionData.imgPath) == false)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.LoadGifFile(curSelectMotionData.imgPath,
                        (result) =>
                        {
                            GifDecoder decoder = new GifDecoder(result);

                            List<Texture2D> frames = decoder.Frames;
                            gifTextureList = frames;
                            gifFrames = frames;
                            currentFrame = 0;
                            isPlaying = true;

                            curSelectMotionData.gif = frames;
                        }));
                    }                    
                }
                else
                {
                    gifTextureList = curSelectMotionData.gif;
                    gifFrames = gifTextureList;
                    currentFrame = 0;
                    isPlaying = true;
                }
            }
            return gifTextureList;
        }

        private void LoadSampleGifTextureList()
        {
            if (curSelectMotionData != null)
            {
                if (curSelectMotionData.gif == null)
                {
                    if (string.IsNullOrEmpty(curSelectMotionData.imgPath) == false)
                    {
                        if (File.Exists(curSelectMotionData.imgPath))
                        {
                            byte[] bytes = File.ReadAllBytes(curSelectMotionData.imgPath);

                            if (bytes.Length > 0)
                            {
                                GifDecoder decoder = new GifDecoder(bytes);
                                curSelectMotionData.gif = decoder.Frames;
                                
                                gifFrames = curSelectMotionData.gif;
                                currentFrame = 0;
                                isPlaying = true;
                            }
                        }
                    }
                }
                else
                {                    
                    gifFrames = curSelectMotionData.gif;
                    currentFrame = 0;
                    isPlaying = true;
                }
            }            
        }

        private IEnumerator RefreshMotionData()
        {                        
            while (isMakingSMPL && remainTime > 0)
            {                
                yield return new EditorWaitForSeconds(1);

                if(GenerateWindow.window == null)
                {
                    remainTime = 0;
                    EditorCoroutineUtility.StopCoroutine(refreshCoroutine);
                    refreshCoroutine = null;
                    yield break;
                }
                remainTime -= 1;                             

                if(remainTime <= 0)
                {
                    foreach (KeyValuePair<int, TextToMotionData> pair in DataCache.motionDictionary)
                    {
                        if (pair.Key > -1)
                        {
                            if (pair.Value.is_smpl_made == 1)
                            {
                                RequestMotionDetail(pair.Value.seq);

                                EditorCoroutineUtility.StopCoroutine(refreshCoroutine);
                                refreshCoroutine = null;
                                yield break;
                            }
                        }
                    }
                }
            }
        }


        private bool CheckConverting(ref int seq)
        {
            foreach (KeyValuePair<int, TextToMotionData> pair in DataCache.motionDictionary)
            {
                if (pair.Key > -1)
                {
                    if (pair.Value.is_smpl_made == 1)
                    {
                        isMakingSMPL = true;
                        seq = pair.Value.seq;
                        return true;                        
                    }
                }
            }
            return false;
        }

        private string GetDummyRandomPrompt()
        {
            if (randomString.Length == 0)
                return "";

            int ranNum = UnityEngine.Random.Range(0, randomString.Length);
            return randomString[ranNum];
        }
    }
}
