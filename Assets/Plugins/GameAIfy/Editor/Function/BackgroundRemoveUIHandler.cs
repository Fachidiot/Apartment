using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;


namespace GameAIfySDK
{
    public class BackgroundRemoveUIHandler : ISubCategoryUIHandler
    {
        public void DrawUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DoDrawThumbnailImage();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();


            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
            {
                if (GUILayout.Button("Remove Background"))
                {
                    EditImage(SideBarSubCategoryItem.BackgroundRemove, "isnet-general-use");
                }
            }

        }


        public void DoDrawThumbnailImage()
        {
            // Image
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor);
            float thumbnailWidth = windowWidth - 20;

            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight)/2,
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 20;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);

            if (null != GenerateWindow.selectItem)
            {
                GUILayout.Box(GenerateWindow.selectItem.texture, GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));
            }
        }

        private void EditImage(SideBarSubCategoryItem editType, string value)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.EditImage(GenerateWindow.selectItem.seq, editType,
                value,
                (imageData) =>
                {
                    if (imageData == null)
                        return;

                    var history = DataCache.AddHistory(imageData, "remove_bg");
                    GenerateWindow.selectItem = history;
                    GenerateWindow.window.Repaint();
                }));
        }
    }
}