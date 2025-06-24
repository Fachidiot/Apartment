using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.EditorCoroutines.Editor;
using Newtonsoft.Json;
using System.IO;

namespace GameAIfySDK
{
    public class DataCache : ScriptableSingleton<DataCache>
    {
        public static SortedDictionary<int, BaseInfo> myWorkDictionary =
            new SortedDictionary<int, BaseInfo>(Comparer<int>.Create((x, y) => y.CompareTo(x)));

        public static Dictionary<int, BaseInfo> postDataDictionary = new Dictionary<int, BaseInfo>();

        public static Dictionary<int, ModelInfo> modelDictionary = new Dictionary<int, ModelInfo>();
        public static Dictionary<int, Texture2D> textureCache = new Dictionary<int, Texture2D>();

        public static Dictionary<PresetModelMainCategory, ModelInfo> selectCategoryModelDictionary =
            new Dictionary<PresetModelMainCategory, ModelInfo>();

        //선택 이미지 1개에 대한 히스토리 데이터 (GenerateWindow 창에서 사용)        
        public static SortedDictionary<int, HistoryData> generateWindowHistoryMap =
            new SortedDictionary<int, HistoryData>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        //선택 이미지 1개에 대한 히스토리 데이터 (EditWindow 창에서 사용)
        public static SortedDictionary<int, HistoryData> editWindowHistoryMap =
            new SortedDictionary<int, HistoryData>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        public static MyWorkDataResponse myWorkDataResponse;
        public static PostDataResponse postDataResponse;

        public static int generateQueueCount = 0;

        public static List<EasyCharacterMakerListData> easyCharacterMakerList = new List<EasyCharacterMakerListData>();

        // Text2Motion        
        public static Dictionary<int, TextToMotionData> motionDictionary = new Dictionary<int, TextToMotionData>();
        
        //FacialExpression
        public static List<ExpressionConfigData> animeInfoList = new List<ExpressionConfigData>();
        public static List<ExpressionConfigData> realInfoList = new List<ExpressionConfigData>();
        
        public static ModelInfo GetModelInfo(int presetNum)
        {
            foreach (var item in modelDictionary.Values)
            {
                if (item.presetNum == presetNum)
                {
                    return item;
                }
            }

            return new ModelInfo();
        }

        public static void RefreshData()
        {
            myWorkDictionary.Clear();
            postDataDictionary.Clear();
            modelDictionary.Clear();
            easyCharacterMakerList.Clear();
            textureCache.Clear();
            generateQueueCount = 0;
            
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync());
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadPostDataAsync());
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadModelDataAsync());
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadEasyCharacterMakerList());
            EditorCoroutineUtility.StartCoroutineOwnerless(LoadTextToMotionList());
        }


        public static IEnumerator LoadPostDataAsync(string orderBy = "like", int group = 1, int page = 1,
            Action<string> callback = null)
        {
            yield return APIHandler.GetPostDataAsync(orderBy, group, page, (jsonData) =>
            {
                if (jsonData == null)
                    return;

                var response = JsonConvert.DeserializeObject<PostDataResponse>(jsonData);
                if (postDataResponse == response)
                {
                    return;
                }

                postDataResponse = response;
                if (page == 1)
                    postDataDictionary.Clear();
                foreach (var data in postDataResponse.data)
                {
                    if (!postDataDictionary.TryGetValue(data.seq, out var value))
                    {
                        postDataDictionary.Add(data.seq, data);
                    }

                    if (textureCache.TryGetValue(data.seq, out Texture2D texture))
                    {
                        data.texture = texture;
                    }
                    else if (data.texture == null)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(LoadTextureAndRepaint(data, callback));
                    }
                }

                if (callback != null)
                    callback("success");
            });
        }

        public static IEnumerator LoadMyWorksDataAsync(int group = 1, int page = 1,
            Action<string> callback = null)
        {
            yield return APIHandler.GetMyWorksDataAsync(group, page, (jsonData) =>
            {
                if (jsonData == null)
                    return;

                var response = JsonConvert.DeserializeObject<MyWorkDataResponse>(jsonData);
                if (myWorkDataResponse == response)
                {
                    return;
                }

                myWorkDataResponse = response;
                if (page == 1)
                    myWorkDictionary.Clear();
                foreach (var data in myWorkDataResponse.data)
                {
                    if (!myWorkDictionary.TryGetValue(data.seq, out var value))
                    {
                        myWorkDictionary.Add(data.seq, data);
                    }

                    if (textureCache.TryGetValue(data.seq, out Texture2D texture))
                    {
                        data.texture = texture;
                    }
                    else if (data.texture == null)
                    {
                        EditorCoroutineUtility.StartCoroutineOwnerless(LoadTextureAndRepaint(data, callback));
                    }
                }
            });
        }

        public static IEnumerator LoadEasyCharacterMakerList(Action<string> callback = null)
        {
            yield return APIHandler.GetRequestEasyCharacterMakerList((int)CharacterMakerType.all, (jsonData) =>
            {
                if(string.IsNullOrEmpty(jsonData) == false && jsonData.Equals("Fail") == false)
                {
                    EasyCharacterMakerListResponse response =
                    JsonConvert.DeserializeObject<EasyCharacterMakerListResponse>(jsonData);

                    if (response.data != null)
                    {
                        easyCharacterMakerList = response.data;
                    }
                }
            });
        }

        public static IEnumerator LoadTextToMotionList(int page = 1, Action<string> callback = null)
        {
            yield return APIHandler.GetRequestMotionList(page, (jsonData) =>
            {
                if (jsonData == null)
                    return;

                if (jsonData == "Fail")
                    return;

                TextToMotionDataResponse response =
                    JsonConvert.DeserializeObject<TextToMotionDataResponse>(jsonData);

                if (response.status == "success")
                {
                    if (response.data != null)
                    {
                        motionDictionary.Clear();
                        for (int i = 0; i < response.data.Count; ++i)
                        {
                            motionDictionary.TryAdd(response.data[i].seq, response.data[i]);
                        }

                        foreach(KeyValuePair<int, TextToMotionData> pair in motionDictionary)
                        {
                            if(string.IsNullOrEmpty(pair.Value.thumbnailPath) == false)
                            {
                                EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.GetTextureAsync(pair.Value.thumbnailPath,
                                    (texture2D) => 
                                    {
                                        if (texture2D != null)
                                        {
                                            pair.Value.texture = texture2D;
                                        }
                                    }));
                            }                          
                        }
                    }
                }
            });
        }

        public static HistoryData AddHistory(ImageData itemData, string func_num)
        {
            if (func_num == "Original Image")
                DataCache.generateWindowHistoryMap.Clear();

            HistoryData item = new HistoryData();
            item.seq = itemData.seq;
            item.func_num = func_num;
            item.output_image_width = itemData.width;
            item.output_image_height = itemData.height;
            item.img_path = itemData.imgPath;

            EditorCoroutineUtility.StartCoroutineOwnerless(LoadTextureAndRepaint(item,
                (s) => { GenerateWindow.window.Repaint(); }));

            AddHistory(item);
            return item;
        }

        public static void AddHistory(HistoryData historyData)
        {
            generateWindowHistoryMap.TryAdd(historyData.seq, historyData);
            if (PreviewWindow.window != null && GenerateWindow.selectItem != null && PreviewWindow.selectedItem != null && GenerateWindow.selectItem.seq == PreviewWindow.selectedItem.seq)
            {
                editWindowHistoryMap.TryAdd(historyData.seq, historyData);
            }
        }

        private static HistoryResponse historyResponse;

        public static IEnumerator LoadHistoryAsync(int seq, Action<string> callback = null, bool isEditWindow = false)
        {
            yield return APIHandler.GetImageHistoryAsync(seq,
                (jsonData) =>
                {
                    if (string.IsNullOrEmpty(jsonData))
                        return;

                    historyResponse = JsonConvert.DeserializeObject<HistoryResponse>(jsonData);
                    if (historyResponse.data != null)
                    {
                        foreach (var data in historyResponse.data)
                        {
                            if (isEditWindow)
                            {
                                if (!DataCache.editWindowHistoryMap.TryAdd(data.seq, data))
                                    continue;
                            }
                            else
                            {
                                if (!DataCache.generateWindowHistoryMap.TryAdd(data.seq, data))
                                    continue;
                                if (PreviewWindow.selectedItem != null &&
                                    GenerateWindow.selectItem.seq == PreviewWindow.selectedItem.seq)
                                {
                                    if (!DataCache.editWindowHistoryMap.TryAdd(data.seq, data))
                                        continue;
                                }
                            }

                            if (data.texture == null)
                            {
                                EditorCoroutineUtility.StartCoroutineOwnerless(LoadTextureAndRepaint(data, callback));
                            }
                        }

                        if (callback != null)
                            callback("success");
                    }
                });
        }

        public static IEnumerator LoadTexture(int seq, string img_path, Action<Texture2D> callback)
        {
            if (textureCache.TryGetValue(seq, out Texture2D texture))
            {
                callback(texture);
            }
            else
            {
                yield return GameAIfyUtilTools.GetTextureAsync(img_path,
                    (texture2D) =>
                    {
                        textureCache.TryAdd(seq, texture2D);
                        if(callback != null)
                            callback(texture2D);
                    });
            }
        }

        public static IEnumerator LoadTextureAndRepaint(BaseInfo data, Action<string> callback)
        {
            yield return LoadTexture(data.seq, data.img_path, (texture2D) =>
            {
                data.texture = texture2D;
                if(callback != null)
                    callback("success");
            });
        }

        public static IEnumerator LoadTextureAndRepaint(HistoryData data, Action<string> callback)
        {
            yield return LoadTexture(data.seq, data.img_path, (texture2D) =>
            {
                data.texture = texture2D;
                if (callback != null)
                    callback("success");
            });
        }

        public static IEnumerator LoadModelDataAsync()
        {
            yield return APIHandler.GetModelDataAsync(0, (jsonData) =>
            {
                if (string.IsNullOrEmpty(jsonData))
                    return;

                ModelDataResponse modelDataResponse = JsonConvert.DeserializeObject<ModelDataResponse>(jsonData);
                foreach (var modelInfo in modelDataResponse.list)
                {
                    modelDictionary.TryAdd(modelInfo.presetNum, modelInfo);
                    if (null == modelInfo.texture)
                    {
                        if (textureCache.ContainsKey(-modelInfo.presetNum) &&
                            textureCache[-modelInfo.presetNum] != null)
                        {
                            modelInfo.texture = textureCache[-modelInfo.presetNum];
                        }
                        else
                        {
                            EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.GetTextureAsync(
                                modelInfo.preset_img_path,
                                (texture) =>
                                {
                                    modelInfo.texture = texture;
                                    textureCache.TryAdd(-modelInfo.presetNum, texture);
                                }));
                        }
                    }
                }

                InitLoadSelectModel();
                GenerateWindow.OnModelChange();
            });
        }

        public static void InitLoadSelectModel()
        {
            int count = Enum.GetValues(typeof(PresetModelMainCategory)).Length;
            for (int i = 1; i < count; i++)
            {
                if (EditorPrefs.HasKey(Constants.SelectedPresetNum + i))
                {
                    int model = EditorPrefs.GetInt(Constants.SelectedPresetNum + i);
                    
                    if(modelDictionary.TryGetValue(model, out ModelInfo modelInfo))
                        SetModel(modelInfo);
                }
            }
        }

        public static void SetModel(ModelInfo item)
        {
            if (item.loras == null)
            {
                item.loras = new List<Lora>();
            }

            if (item.loras.Count == 0 || item.loras[0] == null)
            {
                var newLora = new Lora
                {
                    content = "",
                    strength = 0.8f
                };

                if (item.loras.Count == 0)
                {
                    item.loras.Add(newLora);
                }
                else
                {
                    item.loras[0] = newLora;
                }
            }

            if (!selectCategoryModelDictionary.TryAdd((PresetModelMainCategory)item.groupNum, item))
                selectCategoryModelDictionary[(PresetModelMainCategory)item.groupNum] = item;
        }

        public static List<int> LoadEditTempKey(int batchSize = 1)
        {
            if (generateQueueCount >= Constants.API_GENERATE_CALL_MAX_COUNT)
            {
                Alerts.FailDialog(AlertMessages.QueueEnough);
                return null;
            }

            GenerateWindow.window.ShowNotification(new GUIContent("Send Request"), 1);
            generateQueueCount++;

            var keys = new List<int>();
            for (int i = 0; i < batchSize; i++)
            {
                int key = GameAIfyUtilTools.GetRandomKey();

                HistoryData tempItem = new HistoryData();
                tempItem.seq = key;
                tempItem.func_num = "Loading";
                generateWindowHistoryMap.Add(key, tempItem);
                keys.Add(key);
            }

            return keys;
        }

        public static List<int> LoadGenerateTempKey(int batchSize = 1)
        {
            if (generateQueueCount >= Constants.API_GENERATE_CALL_MAX_COUNT)
            {
                Alerts.FailDialog(AlertMessages.QueueEnough);
                return null;
            }

            GenerateWindow.window.ShowNotification(new GUIContent("Send Request"), 1);
            generateQueueCount++;

            var keys = new List<int>();
            for (int i = 0; i < batchSize; i++)
            {
                int key = GameAIfyUtilTools.GetRandomKey();
                myWorkDictionary.Add(key, new ImageInfo());
                keys.Add(key);
            }

            return keys;
        }

        public static void LoadFacialExpressionData(Action callback = null)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.GetExpressionConfig(ExpressionStyle.anime,
                (jsonData) =>
                {
                    if(string.IsNullOrEmpty(jsonData) == false && jsonData.Equals("Fail") == false)
                    {
                        ExpressionConfigDataResponse expressionChangeInfo =
                        JsonConvert.DeserializeObject<ExpressionConfigDataResponse>(jsonData);

                        if (expressionChangeInfo.data != null)
                        {
                            animeInfoList = expressionChangeInfo.data;
                        }
                    }
                }));

            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.GetExpressionConfig(ExpressionStyle.realistic,
                (jsonData) =>
                {
                    if (string.IsNullOrEmpty(jsonData) == false && jsonData.Equals("Fail") == false)
                    {
                        ExpressionConfigDataResponse expressionChangeInfo =
                        JsonConvert.DeserializeObject<ExpressionConfigDataResponse>(jsonData);

                        if (expressionChangeInfo.data != null)
                        {
                            realInfoList = expressionChangeInfo.data;
                            if (callback != null)
                                callback();
                        }
                    }
                }));
        }

        public static void DeleteEditTempKey(List<int> keys)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                generateWindowHistoryMap.Remove(keys[i]);
            }

            generateQueueCount--;
        }

        public static void DeleteGenerateTempKey(List<int> keys)
        {
            for (int i = 0; i < keys.Count; i++)
            {
                myWorkDictionary.Remove(keys[i]);
            }

            generateQueueCount--;
        }

        public static void SelectPreset(ModelInfo item)
        {
            EditorPrefs.SetInt(Constants.SelectedPresetNum + item.groupNum, item.presetNum);
            SetModel(item);

            GenerateWindow.selectPreset = item;
            GenerateWindow.loraStrengthValue = item.loras[0].strength;
            GenerateWindow.OnModelChange();
        }

        public static ModelInfo GetSelectModel(PresetModelMainCategory category)
        {
            selectCategoryModelDictionary.TryGetValue(category, out ModelInfo modelInfo);
            return modelInfo;
        }
    }
}