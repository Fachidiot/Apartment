using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace GameAIfySDK
{
    public static class GameAIfyUtilTools
    {
        public static int RoundUpToMultipleOf8(int value)
        {
            return ((value + 7) / 8) * 8;
        }

        public static Texture2D ConvertBase64ToTexture(string base64)
        {
            byte[] imageBytes = Convert.FromBase64String(base64);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(imageBytes);
            return tex;
        }

        public static Texture2D ScaleTexture(Texture2D texture, int targetWidth, int targetHeight)
        {
            RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.Default,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(texture, rt);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = rt;

            Texture2D scaledTexture = new Texture2D(targetWidth, targetHeight);
            scaledTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            scaledTexture.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);
            return scaledTexture;
        }

        public static Texture2D GetReadableTexture(Texture2D original)
        {
            RenderTexture rt = RenderTexture.GetTemporary(original.width, original.height, 0,
                RenderTextureFormat.Default, RenderTextureReadWrite.Linear);

            Graphics.Blit(original, rt);

            RenderTexture.active = rt;
            Texture2D readableTexture = new Texture2D(original.width, original.height, TextureFormat.RGBA32, false);
            readableTexture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            readableTexture.Apply();

            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(rt);

            return readableTexture;
        }

        public static IEnumerator GetTextureAsync(string url, System.Action<Texture2D> onTextureLoaded)
        {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                //Debug.Log("Failed to download image. Error: " + www.error);
                onTextureLoaded(null);
            }
            else
            {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture;

                int width = texture.width;
                int height = texture.height;
                texture = GameAIfyUtilTools.ScaleTexture(texture, width, height);

                onTextureLoaded(texture);
            }
        }

        public static IEnumerator LoadGeneratedObjectFileToString(string url, System.Action<string> result)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to download image. Error: " + www.error);
                result(null);
            }
            else
            {
                result(www.downloadHandler.text);
            }
        }

        public static IEnumerator DownloadGeneratedObjectFile(string url, System.Action<byte[]> result)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to download image. Error: " + www.error);
                result(null);
            }
            else
            {
                result(www.downloadHandler.data);
            }
        }
        
        public static bool CheckFailImageSize(Texture2D texture)
        {
            if (texture == null)
                return false;
            
            float intputMinSize = Mathf.Min(texture.width, texture.height);
            float intputMaxSize = Mathf.Max(texture.width, texture.height);
            if(intputMinSize < Constants.CANVAS_MIN_INPUT || intputMaxSize > Constants.CANVAS_MAX_INPUT)
                return true;
            return false;
        }

        public static IEnumerator LoadGifFile(string url, System.Action<byte[]> result)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to download image. Error: " + www.error);
                result(null);
            }
            else
            {
                result(www.downloadHandler.data);
            }
        }

        public static IEnumerator LoadMotionJsonFile(string url, System.Action<string> result)
        {
            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Failed to download image. Error: " + www.error);
                result(null);
            }
            else
            {
                result(www.downloadHandler.text);
            }
        }

        public static int GetRandomKey()
        {
            int min = 100000000;        
            int max = 200000000; 
            
            return UnityEngine.Random.Range(min, max); 
        }
        
        public static float CalcContentsTapWidth(SideBarMainCategoryItem category)
        {
            float width = EditorGUIUtility.currentViewWidth;
            
            width -= Constants.SIDEBAR_WIDTH;
            
            switch (category)
            {
                case SideBarMainCategoryItem.Gallery:
                    break;
                case SideBarMainCategoryItem.Generator:
                    width -= Constants.GENERATE_PREVIEW_WIDTH;
                    break;
                default:
                    width -= Constants.IMAGE_HISTORY_WIDTH;
                    break;
            }
            
            return width; 
        }
    }
}