using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;


namespace GameAIfySDK
{
    public class BackgroundLoopUIHandler : ISubCategoryUIHandler
    {
        private float outputScale = 150;
        private float outputScaleMin = 100;
        private float outputScaleMax = 200;
        private float outputScaleSlider = 150;
        private int outputWidth = 2048;

        [Range(0f, 100f)] private float overlapPercent = 25;

        private Texture2D selectTexture;

        public void DrawUI()
        {
            if (GenerateWindow.selectItem == null)
                return;

            selectTexture = GenerateWindow.selectItem.texture;
            if (selectTexture != null)
                DoDrawSeamlessInput();
            
            
            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
                DoOption();
        }

        private void DoDrawSeamlessInput()
        {
            Rect previewRect =
                GUILayoutUtility.GetRect(GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor) - 20,
                    200);
            GUI.Box(previewRect, GUIContent.none);

            Rect imageRect = CalculateFittedRect(previewRect, selectTexture.width, selectTexture.height);
            GUI.DrawTexture(imageRect, selectTexture, ScaleMode.ScaleToFit);

            Handles.color = Color.green;
            Handles.DrawSolidRectangleWithOutline(imageRect, new Color(0, 0, 0, 0), Color.green);

            Handles.BeginGUI();
            DrawOverlapAndOutput(imageRect);
            Handles.EndGUI();

            GUILayout.Space(10);
            CommonUI.DoTextureSizeText(selectTexture.width, selectTexture.height, outputWidth, selectTexture.height);
            GUILayout.Space(10);
        }

        private void DoOption()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Output Scale (%)");
            //string[] options = new string[] { "%", "px" };
            //selectedIndex = GUILayout.Toolbar(selectedIndex, options);
            GUILayout.EndHorizontal();
            // if (selectedIndex == 0)
            // {
            outputScaleSlider = EditorGUILayout.Slider(outputScaleSlider, outputScaleMin, outputScaleMax);
            // if (selectTexture.width * outputScale * 0.01 > Constants.SEAMLESS_MAX_OUTPUT)
            // {
            //     var s = Constants.SEAMLESS_MAX_OUTPUT * 100 / selectTexture.width;
            //     outputScale = s;
            // }

            if(selectTexture != null)
            {
                if (selectTexture.width * outputScaleSlider * 0.01 > Constants.SEAMLESS_MAX_OUTPUT)
                {
                    outputWidth = Constants.SEAMLESS_MAX_OUTPUT;
                    outputScale = ((float)Constants.SEAMLESS_MAX_OUTPUT / selectTexture.width) * 100;
                }
                else
                {
                    outputWidth = GameAIfyUtilTools.RoundUpToMultipleOf8((int)(outputScale * 0.01f * selectTexture.width));
                    outputScale = outputScaleSlider;
                }
            }

            //}
            // else
            // {
            //     outputWidth = EditorGUILayout.IntSlider(outputWidth, selectTexture.width,
            //         Mathf.Min(selectTexture.width * 2,Constants.SEAMLESS_MAX_OUTPUT_WIDTH));
            //     outputScale = outputWidth / selectTexture.width * 100;
            // }
            // GUILayout.Label("Overlap");
            // overlapPercent = EditorGUILayout.Slider(overlapPercent, 0f, 50f);
            EditorGUI.BeginDisabledGroup(outputScale <= 100);
            if (GUILayout.Button("Extend"))
            {
                Debug.Log("BackgroundLoop Send");
                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.SeamlessExtend(GenerateWindow.selectItem.seq,
                    outputWidth, (result) =>
                    {
                        if (result != null)
                        {
                            ImageDataResponse expressionChangeInfoResponse =
                                JsonConvert.DeserializeObject<ImageDataResponse>(result);
                            var imageInfo = expressionChangeInfoResponse.data;
                            var history = DataCache.AddHistory(imageInfo, "seamless_extend");
                            GenerateWindow.selectItem = history;
                            GenerateWindow.window.Repaint();
                        }
                    }));
            }

            EditorGUI.EndDisabledGroup();
        }

        private Rect CalculateFittedRect(Rect containerRect, float imageWidth, float imageHeight)
        {
            float imageMax = Mathf.Min(imageWidth * 2, Constants.SEAMLESS_MAX_OUTPUT);

            float ratio = Mathf.Min(2048 / imageWidth, outputScaleMax / 100);
            float effectiveWidth = Mathf.Min(containerRect.width / ratio, imageMax);

            float imageAspect = imageWidth / imageHeight;
            float drawWidth, drawHeight;

            drawWidth = effectiveWidth;
            drawHeight = drawWidth / imageAspect;

            if (drawHeight > containerRect.height)
            {
                drawHeight = containerRect.height;
                drawWidth = drawHeight * imageAspect;
            }

            float x = containerRect.x + (containerRect.width - drawWidth) * 0.5f;
            float y = containerRect.y + (containerRect.height - drawHeight) * 0.5f;
            return new Rect(x, y, drawWidth, drawHeight);
        }


        private void DrawOverlapAndOutput(Rect imageRect)
        {
            Color oldColor = Handles.color;

            // float overlapFraction = overlapPercent / 100f;
            // float overlapTotalWidth = imageRect.width * overlapFraction;
            // float overlapSide = overlapTotalWidth * 0.5f;
            //
            // Rect leftOverlapRect = new Rect(
            //     imageRect.xMin,
            //     imageRect.yMin,
            //     overlapSide,
            //     imageRect.height
            // );
            // Rect rightOverlapRect = new Rect(
            //     imageRect.xMax - overlapSide,
            //     imageRect.yMin,
            //     overlapSide,
            //     imageRect.height
            // );
            //
            // Handles.color = new Color(0f, 1f, 0f, 0.3f);
            // Handles.DrawSolidRectangleWithOutline(leftOverlapRect, Handles.color, Color.clear);
            // Handles.DrawSolidRectangleWithOutline(rightOverlapRect, Handles.color, Color.clear);

            float scaleFraction = outputScale / 100f;
            float extraRatio = scaleFraction - 1f;
            if (extraRatio > 0f)
            {
                float totalExtraWidth = imageRect.width * extraRatio;
                float extraSide = totalExtraWidth * 0.5f;

                Rect leftExtendRect = new Rect(
                    imageRect.xMin - extraSide,
                    imageRect.yMin,
                    extraSide,
                    imageRect.height
                );
                Rect rightExtendRect = new Rect(
                    imageRect.xMax,
                    imageRect.yMin,
                    extraSide,
                    imageRect.height
                );
                Rect combinedRect = new Rect(
                    imageRect.xMin - extraSide,
                    imageRect.yMin,
                    imageRect.width + extraSide * 2,
                    imageRect.height
                );
                Handles.color = new Color(0f, 0.5f, 1f, 0.3f);
                Handles.DrawSolidRectangleWithOutline(leftExtendRect, Handles.color, Color.clear);
                Handles.DrawSolidRectangleWithOutline(rightExtendRect, Handles.color, Color.clear);

                Handles.color = Color.white;
                Handles.DrawSolidRectangleWithOutline(combinedRect, new Color(0, 0, 0, 0), Color.white);
            }

            // 원래 색상 복원
            Handles.color = oldColor;
        }
    }
}