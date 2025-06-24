using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEditor;

namespace GameAIfySDK
{
    public class APIHandler
    {
        #region GET API

        public static IEnumerator GetMyWorksDataAsync(int group, int page, System.Action<string> onDataLoaded)
        {
            string usn = EditorPrefs.GetString(Constants.USN);
            int limit = Constants.API_LOAD_ITEM_AMOUNT;

            string formattedUrl = string.Format(Constants.API_IMAGE_LIST, usn, page, limit, group);
            UnityWebRequest www = UnityWebRequest.Get(formattedUrl);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                onDataLoaded(null);
            }
            else
            {
                onDataLoaded(www.downloadHandler.text);
            }
        }

        public static IEnumerator GetPostDataAsync(string orderBy, int group, int page,
            System.Action<string> onDataLoaded)
        {
            int limit = Constants.API_LOAD_ITEM_AMOUNT;

            string formattedUrl = string.Format(Constants.API_POST_LIST, orderBy, group, page, limit);
            UnityWebRequest www = UnityWebRequest.Get(formattedUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                onDataLoaded(null);
            }
            else
            {
                onDataLoaded(www.downloadHandler.text);
            }
        }

        public static IEnumerator GetModelDataAsync(int group, System.Action<string> onDataLoaded)
        {
            string formattedUrl = string.Format(Constants.API_MODEL_LIST, group);
            UnityWebRequest www = UnityWebRequest.Get(formattedUrl);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                onDataLoaded(null);
            }
            else
            {
                onDataLoaded(www.downloadHandler.text);
            }
        }

        public static IEnumerator GetImageOptionAsync(int option_seq, System.Action<string> onDataLoaded)
        {
            string formattedUrl = string.Format(Constants.API_IMG_INFO, option_seq);
            UnityWebRequest www = UnityWebRequest.Get(formattedUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                onDataLoaded(null);
            }
            else
            {
                onDataLoaded(www.downloadHandler.text);
            }
        }

        public static IEnumerator GetImageHistoryAsync(int seq, System.Action<string> onDataLoaded)
        {
            string formattedUrl = string.Format(Constants.API_HISTORY_LIST, seq);

            UnityWebRequest www = UnityWebRequest.Get(formattedUrl);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                onDataLoaded(null);
            }
            else
            {
                onDataLoaded(www.downloadHandler.text);
            }
        }

        public static IEnumerator GetExpressionConfig(ExpressionStyle style, System.Action<string> result)
        {
            string formattedUrl = string.Empty;
            formattedUrl = string.Format(Constants.API_EXPRESSION_CONFIG, (int)style);

            UnityWebRequest request = UnityWebRequest.Get(formattedUrl);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                result("Fail");
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator GetRequestEasyCharacterMakerList(int type, Action<string> result)
        {
            string formattedUrl = string.Empty;
            formattedUrl = string.Format(Constants.API_EASY_CHARACTER_MAKER_LIST, type);

            UnityWebRequest request = UnityWebRequest.Get(formattedUrl);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                result("Fail");
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator GetRequestMotionList(int pageNum, Action<string> result)
        {
            string formattedUrl = string.Format(Constants.API_TEXT_TO_MOTION_LIST, EditorPrefs.GetString(Constants.USN),
                pageNum);

            UnityWebRequest request = UnityWebRequest.Get(formattedUrl);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                result("Fail");
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        #endregion


        #region POST

        #region USE_AI

        public static IEnumerator GenerateImage(InputPromptData inputPromptData, System.Action<string> result)
        {
            List<int> tempKey = DataCache.LoadGenerateTempKey(inputPromptData.batchSize);
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_CREATE;

            PromptData data = new PromptData
            {
                prompt = inputPromptData.inputPrompt,
                negativePrompt = inputPromptData.inputNegativePrompt,
                loras = new List<Lora>
                {
                    new Lora
                    {
                        content = inputPromptData.modelInfo.loras[0].content,
                        strength = EditorPrefs.GetFloat(Constants.SelectedLoraStrength),
                    }
                }
            };
            string promptData = JsonConvert.SerializeObject(data);

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("presetId", inputPromptData.remixSeq == 0 ? inputPromptData.modelInfo.presetNum : Constants.CHARACTERPOSE_PRESET_NUM);
            form.AddField("width", inputPromptData.width);
            form.AddField("height", inputPromptData.height);
            form.AddField("prompt", promptData);
            form.AddField("batchSize", inputPromptData.batchSize);
            form.AddField("imageNum", inputPromptData.remixSeq);
            if (inputPromptData.referenceImageData.referenceImage != null)
            {
                Texture2D readableTexture =
                    GameAIfyUtilTools.GetReadableTexture(inputPromptData.referenceImageData.referenceImage);
                byte[] imageData = readableTexture.EncodeToPNG();

                form.AddBinaryData("image", imageData, "image.png", "image/png");

                form.AddField("strength", inputPromptData.referenceImageData.strength.ToString());
                form.AddField("mode", inputPromptData.referenceImageData.mode);
            }

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            // Add the Authorization header
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteGenerateTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                ServerErrorMessage errorInfo =
                    JsonConvert.DeserializeObject<ServerErrorMessage>(request.downloadHandler.text);
                if(errorInfo.message.Contains("not an allowed"))
                    Alerts.FailDialog(AlertMessages.RestrictedGenerated + errorInfo.message);
                result("Fail");
            }
            else
            {
                result("Success");
            }
        }

        public static IEnumerator SendSegment(int seq, List<Vector2> points, List<int> labels,
            System.Action<Texture2D> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }


            string url = Constants.API_SEGMENTATION;

            string result1 =
                "[" + string.Join(",", points.Select(p => $"[{p.x},{p.y}]")) + "]"; // string [[111,111],[222,222]]
            string result2 = "[" + string.Join(",", labels) + "]"; // string [1,0]

            Debug.Log(result1);
            Debug.Log(result2);

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("imageNum", seq);
            form.AddField("mode", "pointer");
            form.AddField("points", result1);
            form.AddField("labels", result2);

            UnityWebRequest request = UnityWebRequest.Post(url, form);

            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));
            Debug.Log("Send Segment Request");

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                result(null);
            }
            else
            {
                //Base 64 -> convert
                ServerResponse response = JsonUtility.FromJson<ServerResponse>(request.downloadHandler.text);
                if (response != null && response.data != null)
                {
                    string base64Mask = response.data.mask;
                    var texture = GameAIfyUtilTools.ConvertBase64ToTexture(base64Mask);
                    result(texture);
                }
                else
                {
                    result(null);
                }
            }
        }

        public static IEnumerator SendPartialRedraw(int seq, string inputPrompt, Texture2D texture,
            System.Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }


            string url = Constants.API_INPAINT;

            byte[] imageData = texture.EncodeToPNG();

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("prompt", inputPrompt ?? "");
            //form.AddField("presetId", modelInfo.presetNum);
            //form.AddField("strenth", (float)st);
            form.AddField("imageNum", seq);
            form.AddBinaryData("image", imageData, "image.png", "image/png");


            UnityWebRequest request = UnityWebRequest.Post(url, form);

            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));
            yield return request.SendWebRequest();

            DataCache.DeleteEditTempKey(tempKey);
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);

                ServerErrorMessage errorInfo =
                    JsonConvert.DeserializeObject<ServerErrorMessage>(request.downloadHandler.text);
                if(errorInfo.message.Contains("not an allowed"))
                    Alerts.FailDialog(AlertMessages.RestrictedGenerated + errorInfo.message);
                result(null);
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator CanvasExtend(int seq, int scale, Vector2 outputPosition, int outputWidth,
            int outputHeight, System.Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            Debug.Log("CanvasExtend");

            string url = Constants.API_CANVAS_EXTEND;

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("imageNum", seq);
            string pointer = string.Format("[{0}, {1}]", outputPosition.x, outputPosition.y);
            form.AddField("scale", scale);
            form.AddField("pointer", pointer);
            form.AddField("bgWidth", outputWidth);
            form.AddField("bgHeight", outputHeight);
            UnityWebRequest request = UnityWebRequest.Post(url, form);

            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));
            yield return request.SendWebRequest();

            DataCache.DeleteEditTempKey(tempKey);
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                result(null);
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator EditImage(int seq, SideBarSubCategoryItem editType, string value,
            System.Action<ImageData> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url;
            switch (editType)
            {
                case SideBarSubCategoryItem.BackgroundRemove:
                    url = Constants.API_REMOVEBG;
                    break;
                case SideBarSubCategoryItem.FacialExpression:
                    url = Constants.API_UPSCALE;
                    break;
                case SideBarSubCategoryItem.Enhance:
                    url = Constants.API_REFINE;
                    break;
                default:
                    url = Constants.API_EDIT;
                    break;
            }

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("imageNum", seq);
            form.AddField("value", value);
            Debug.Log(value);
            UnityWebRequest request = UnityWebRequest.Post(url, form);

            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));
            Debug.Log("Send Edit Request");

            yield return request.SendWebRequest();

            DataCache.DeleteEditTempKey(tempKey);
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                result(null);
            }
            else
            {
                ImageDataResponse expressionChangeInfoResponse =
                    JsonConvert.DeserializeObject<ImageDataResponse>(request.downloadHandler.text);
                var imageInfo = expressionChangeInfoResponse.data;

                result(imageInfo);
            }
        }

        public static IEnumerator SeamlessExtend(int seq, int outputWidth, System.Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_SEAMLESS_EXTEND;

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("imageNum", seq);
            form.AddField("width", outputWidth);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator ExpressionChange(ExpressionConfigData info, HistoryData selectImage, int batchSize,
            Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey(batchSize);
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_EXPRESSION_CHANGE;

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("expressionSeq", info.seq);
            form.AddField("batchSize", batchSize);
            form.AddField("imageNum", selectImage == null ? 0 : selectImage.seq);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
                result(null);
            }
            else
            {
                // Debug.Log(request.downloadHandler.text);
                // ImageDataResponse expressionChangeInfoResponse =
                //     JsonConvert.DeserializeObject<ImageDataResponse>(request.downloadHandler.text);
                // var imageInfo = expressionChangeInfoResponse.data;
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator RequestEasyCharacterMaker(int imgCount, int selectedImgSeq, string wildCardList,
            Action<string> result)
        {
            List<int> tempKey = DataCache.LoadGenerateTempKey(imgCount);
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_EASY_CHARACTER_MAKER;

            WWWForm form = new WWWForm();

            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("batchSize", imgCount);
            form.AddField("presetId", selectedImgSeq);
            form.AddField("wildcardList", wildCardList);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteGenerateTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                result("Success");
            }
        }

        public static IEnumerator RequestEnhance(int seq, string strength, int scale, Action<ImageData> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_ENHANCE;

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("imageNum", seq);
            form.AddField("strength", strength);
            form.AddField("scale", scale);
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                result(null);
            }
            else
            {
                ImageDataResponse expressionChangeInfoResponse =
                    JsonConvert.DeserializeObject<ImageDataResponse>(request.downloadHandler.text);
                var imageInfo = expressionChangeInfoResponse.data;

                result(imageInfo);
            }
        }

        public static IEnumerator GenerateDepthMap(int seq, float scale, System.Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_DEPTH_MAP;

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("scale", scale.ToString());
            form.AddField("imageNum", seq);
            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                result("fail");
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator RequestPoseBasedGenerate(string prompt, Action<string> result)
        {
            List<int> tempKey = DataCache.LoadGenerateTempKey(4);
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_POSE_BASED;

            WWWForm form = new WWWForm();

            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("prompt", prompt);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteGenerateTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
                ServerErrorMessage errorInfo =
                    JsonConvert.DeserializeObject<ServerErrorMessage>(request.downloadHandler.text);
                if(errorInfo.message.Contains("not an allowed"))
                    Alerts.FailDialog(AlertMessages.RestrictedGenerated + errorInfo.message);
            }
            else
            {
                result("Success");
            }
        }

        public static IEnumerator RequestTextToMotionDetail(int seq, Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result("fail");
                yield break;
            }

            string url = Constants.API_TEXT_TO_MOTION_DETAIL;

            WWWForm form = new WWWForm();

            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("seq", seq);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator RequestTextToMotionCreate(string prompt, int length, Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result("fail");
                yield break;
            }

            string url = Constants.API_TEXT_TO_MOTION_JOINT;

            WWWForm form = new WWWForm();

            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("sec", length);
            form.AddField("prompt", prompt);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                result("fail");
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator RequestTextToMotionMakeSMPL(int seq, Action<string> result)
        {
            List<int> tempKey = DataCache.LoadEditTempKey();
            if (tempKey == null)
            {
                result(null);
                yield break;
            }

            string url = Constants.API_TEXT_TO_MOTION_SMPL;

            WWWForm form = new WWWForm();

            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("seq", seq);

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();
            DataCache.DeleteEditTempKey(tempKey);

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(request.error);
                Debug.Log(request.downloadHandler.text);
            }
            else
            {
                result(request.downloadHandler.text);
            }
        }

        public static IEnumerator RequestTextToMotionDelete(int seq, Action<string> result)
        {
            string url = Constants.API_TEXT_TO_MOTION_DELETE;

            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("seq", seq);

            UnityWebRequest request = UnityWebRequest.Post(url, form);

            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                result("Fail");
            }
            else
            {
                result("Success");
            }
        }

        #endregion

        public static IEnumerator DeleteImage(int seq, System.Action<string> onDeleteStatus = null)
        {
            string url = Constants.API_DELETE;
            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("seq", seq);
            UnityWebRequest request = UnityWebRequest.Post(url, form);

            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                onDeleteStatus("Fail");
            }
            else
            {
                onDeleteStatus("Success");
            }
        }


        public static IEnumerator UploadImage(Texture2D texture, string inputPrompt, int category,
            System.Action<string> onUploadStatus = null)
        {
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            bool settingsChanged = false;
            bool originalReadable = false;
            TextureImporterCompression originalCompression = TextureImporterCompression.Uncompressed;

            if (importer != null)
            {
                originalReadable = importer.isReadable;
                originalCompression = importer.textureCompression;

                if (!importer.isReadable || importer.textureCompression != TextureImporterCompression.Uncompressed)
                {
                    importer.isReadable = true;
                    importer.textureCompression = TextureImporterCompression.Uncompressed;
                    importer.SaveAndReimport();
                    settingsChanged = true;
                }
            }
#endif

            byte[] imageData = texture.EncodeToPNG();

            string url = Constants.API_UPLOAD;
            WWWForm form = new WWWForm();
            form.AddField("usn", EditorPrefs.GetString(Constants.USN));
            form.AddField("category", category);
            if (!string.IsNullOrEmpty(inputPrompt))
                form.AddField("prompt", inputPrompt);
            form.AddBinaryData("image", imageData, "image.png", "image/png");

            UnityWebRequest request = UnityWebRequest.Post(url, form);
            request.SetRequestHeader("Authorization", EditorPrefs.GetString(Constants.Token));

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error generating image: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
                onUploadStatus?.Invoke("Fail");
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                onUploadStatus?.Invoke(request.downloadHandler.text);
            }

#if UNITY_EDITOR
            if (importer != null && settingsChanged)
            {
                importer.isReadable = originalReadable;
                importer.textureCompression = originalCompression;
                importer.SaveAndReimport();
            }
#endif
        }

        #endregion
    }
}