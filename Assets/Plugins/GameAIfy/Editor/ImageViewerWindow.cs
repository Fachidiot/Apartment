using UnityEngine;
using UnityEditor;

namespace GameAIfySDK
{
    public class ImageViewerWindow : EditorWindow
    {
        public HistoryData item;

        void OnGUI()
        {
            if (item != null)
            {
                float windowWidth = EditorGUIUtility.currentViewWidth;
                Rect imageRect = new Rect(0, 0, windowWidth, position.height);
                GUI.DrawTexture(imageRect, item.texture, ScaleMode.ScaleToFit);
            }
        }
    }
}