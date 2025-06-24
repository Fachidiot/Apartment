using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;


namespace GameAIfySDK
{
    public class OutpaintingUIHandler : ISubCategoryUIHandler
    {
        private static readonly AspectRatioEntry[] AspectRatioEntries = new AspectRatioEntry[]
        {
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_1_2, ratioValue = new Vector2(1f, 2f), label = "1 : 2" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_9_16, ratioValue = new Vector2(9f, 16f), label = "9 : 16" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_2_3, ratioValue = new Vector2(2f, 3f), label = "2 : 3" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_3_4, ratioValue = new Vector2(3f, 4f), label = "3 : 4" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_4_5, ratioValue = new Vector2(4f, 5f), label = "4 : 5" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_1_1, ratioValue = new Vector2(1f, 1f), label = "1 : 1" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_5_4, ratioValue = new Vector2(5f, 4f), label = "5 : 4" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_4_3, ratioValue = new Vector2(4f, 3f), label = "4 : 3" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_3_2, ratioValue = new Vector2(3f, 2f), label = "3 : 2" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_16_9, ratioValue = new Vector2(16f, 9f), label = "16 : 9" },
            new AspectRatioEntry()
                { ratioEnum = AspectRatios.Ratio_2_1, ratioValue = new Vector2(2f, 1f), label = "2 : 1" },
        };

        private int ratioIndex = 5;
        private float inputScale = 100;
        private float inputScaleSlider = 100;
        private float outputScale = 200f;
        private float _inputScaleMinValue = 50f;
        private float _inputScaleMaxValue = 100f;
        private Texture2D inputTexture;

        private Vector2 imageOffset = Vector2.zero;
        private bool isDragging = false;
        private Vector2 dragStartPos;
        private Vector2 offsetStartPos;

        private int labelInputWidth, labelInputHeight;
        private int labelPreviewWidth, labelPreviewHeight;

        private float maxContentWidth = 1000f;

        //private string debugInfo = "";
        private Vector2 debugInputCenter = Vector2.zero;
        private Rect previewRect;
        private Rect imgRect;

        public void DrawUI()
        {
            if (GenerateWindow.selectItem == null || GenerateWindow.selectItem.texture == null)
                return;

            inputTexture = GenerateWindow.selectItem.texture;

            EditorGUILayout.BeginVertical();

            DoExtendUI();

            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
                DoOption();

            EditorGUILayout.EndVertical();
        }

        private void DoExtendUI()
        {
            EditorGUILayout.HelpBox(HelpBoxMessages.CanvasExtendImageDragDescription, MessageType.None);
            GUILayout.Space(10);

            maxContentWidth = Mathf.Min(GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor),
                (GenerateWindow.window.position.height - Constants.PREIVEW_WINDOW_OTHER_HEIGHT) / 2);
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
            DrawPreview();
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Input Location", GUILayout.Width(100));
            if (GUILayout.Button("Reset"))
            {
                ResetLocation();
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            if (inputTexture != null)
            {
                CommonUI.DoTextureSizeText(labelInputWidth, labelInputHeight, labelPreviewWidth, labelPreviewHeight);
            }

            GUILayout.Space(10);
        }

        private void DoOption()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Aspect Ratio", EditorStyles.boldLabel, GUILayout.Width(200));
            int newRatioIndex = (int)GUILayout.HorizontalSlider(ratioIndex, 0, AspectRatioEntries.Length - 1,
                GUILayout.ExpandWidth(true));
            Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(AspectRatioEntries[newRatioIndex].label));
            GUILayout.Label(AspectRatioEntries[newRatioIndex].label, GUILayout.Width(textSize.x));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Input Scale (%)", EditorStyles.boldLabel);
            inputScaleSlider = EditorGUILayout.Slider(inputScaleSlider, _inputScaleMinValue, _inputScaleMaxValue);
            EditorGUILayout.EndHorizontal();

            float computedRatio =
                Mathf.Max(Constants.CANVAS_MIN_INPUT * 100f / inputTexture.width,
                    Constants.CANVAS_MIN_INPUT * 100f / inputTexture.height);
            float minScaleRatio = Mathf.Max(computedRatio, _inputScaleMinValue);
            if (inputScaleSlider < minScaleRatio)
            {
                inputScale = minScaleRatio;
            }
            else
            {
                inputScale = inputScaleSlider;
            }


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Output Scale (%)", EditorStyles.boldLabel);
            outputScale = EditorGUILayout.Slider(outputScale, 100f, 200f);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                ForceRetarget(previewRect, imgRect);
            }

            if (newRatioIndex != ratioIndex)
            {
                ratioIndex = newRatioIndex;
                ResetLocation();
            }

            GUILayout.Space(10);
            
            if (labelPreviewWidth > Constants.PREVIEW_IMG_MAX_WIDTH ||
                labelPreviewHeight > Constants.PREVIEW_IMG_MAX_WIDTH)
            {
                EditorGUILayout.HelpBox(HelpBoxMessages.TryEditOutputOverSize, MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup((labelInputWidth == labelPreviewWidth &&
                                          labelInputHeight == labelPreviewHeight) ||
                                         labelPreviewWidth > Constants.PREVIEW_IMG_MAX_WIDTH ||
                                         labelPreviewHeight > Constants.PREVIEW_IMG_MAX_WIDTH);

            if (GUILayout.Button("Extend"))
            {
                Debug.Log("Extend");
                ForceRetarget(previewRect, imgRect);
                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.CanvasExtend(GenerateWindow.selectItem.seq,
                    (int)inputScale, debugInputCenter, labelPreviewWidth,
                    labelPreviewHeight,
                    (result) =>
                    {
                        if (result != null)
                        {
                            ImageDataResponse expressionChangeInfoResponse =
                                JsonConvert.DeserializeObject<ImageDataResponse>(result);
                            var imageInfo = expressionChangeInfoResponse.data;
                            var history = DataCache.AddHistory(imageInfo, "outpaint");
                            GenerateWindow.selectItem = history;
                            inputScale = 100;
                            outputScale = 100;
                            ResetLocation();
                            GenerateWindow.window.Repaint();
                        }
                    }));
            }

            EditorGUI.EndDisabledGroup();
        }

        private void ResetLocation()
        {
            imageOffset = Vector2.zero;
        }

        private void DrawPreview()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(maxContentWidth), GUILayout.Height(maxContentWidth));
            GUILayout.FlexibleSpace();

            float availableWidth = maxContentWidth - 50;

            if (inputTexture == null)
            {
                GUILayout.Box("", GUILayout.Width(maxContentWidth), GUILayout.Height(maxContentWidth));
                Rect emptyRect = GUILayoutUtility.GetRect(200, 200, GUILayout.Width(200), GUILayout.Height(200));
                EditorGUI.DrawRect(emptyRect, Color.gray * 1.2f);
                EditorGUI.LabelField(emptyRect, "No Texture Selected", new GUIStyle()
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                });

                Handles.color = Color.white;
                Handles.DrawSolidRectangleWithOutline(emptyRect, new Color(0, 0, 0, 0), Color.white);
                return;
            }

            float inFactor = inputScale / 100f;
            float drawnInputWidth = inputTexture.width * inFactor;
            float drawnInputHeight = inputTexture.height * inFactor;

            labelInputWidth = (int)drawnInputWidth;
            labelInputHeight = (int)drawnInputHeight;

            Vector2 ratio = AspectRatioEntries[ratioIndex].ratioValue;
            float desiredAspect = ratio.x / ratio.y;

            float candidateHeight = drawnInputWidth / desiredAspect;
            float previewWidth, previewHeight;
            if (candidateHeight >= drawnInputHeight)
            {
                previewWidth = drawnInputWidth;
                previewHeight = candidateHeight;
            }
            else
            {
                previewHeight = drawnInputHeight;
                previewWidth = drawnInputHeight * desiredAspect;
            }

            float outFactor = outputScale / 100f;
            previewWidth *= outFactor;
            previewHeight *= outFactor;

            if (previewWidth > Constants.CANVAS_EXTEND_OVERAY ||
                previewHeight > Constants.CANVAS_EXTEND_OVERAY)
            {
                float scale = Mathf.Min(Constants.CANVAS_EXTEND_OVERAY / previewWidth,
                    Constants.CANVAS_EXTEND_OVERAY / previewHeight);
                previewWidth = previewWidth * scale;
                previewHeight = previewHeight * scale;
            }

            labelPreviewWidth = GameAIfyUtilTools.RoundUpToMultipleOf8((int)(previewWidth));
            labelPreviewHeight = GameAIfyUtilTools.RoundUpToMultipleOf8((int)(previewHeight));

            float previewScale = Mathf.Min(availableWidth / previewWidth, availableWidth / previewHeight, 1f);
            float finalPreviewWidth = previewWidth * previewScale;
            float finalPreviewHeight = previewHeight * previewScale;

            float finalImageWidth = drawnInputWidth * previewScale;
            float finalImageHeight = drawnInputHeight * previewScale;

            previewRect = GUILayoutUtility.GetRect(finalPreviewWidth, finalPreviewHeight,
                GUILayout.Width(finalPreviewWidth), GUILayout.Height(finalPreviewHeight));
            EditorGUI.DrawRect(previewRect, Color.gray * 1.2f);

            Handles.color = Color.white;
            Handles.DrawSolidRectangleWithOutline(previewRect, new Color(0, 0, 0, 0), Color.white);
            Vector2 previewCenter = previewRect.center;
            imgRect = new Rect(
                previewCenter.x - finalImageWidth * 0.5f + imageOffset.x,
                previewCenter.y - finalImageHeight * 0.5f + imageOffset.y,
                finalImageWidth,
                finalImageHeight
            );
            GUI.DrawTexture(imgRect, inputTexture, ScaleMode.ScaleToFit);

            Handles.color = Color.green;
            Handles.DrawSolidRectangleWithOutline(imgRect, new Color(0, 0, 0, 0), Color.green);

            HandleImageDrag(previewRect, imgRect);

            debugInputCenter = new Vector2((imgRect.center.x - previewRect.xMin) / previewScale,
                (previewRect.yMax - imgRect.center.y) / previewScale);

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }

        private void HandleImageDrag(Rect previewRect, Rect imageRect)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (imageRect.Contains(e.mousePosition))
                {
                    isDragging = true;
                    dragStartPos = e.mousePosition;
                    offsetStartPos = imageOffset;
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseDrag && isDragging)
            {
                Vector2 delta = e.mousePosition - dragStartPos;
                imageOffset = offsetStartPos + delta;
                imageOffset = ClampImagePosition(previewRect, imageRect.size, imageOffset);
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                isDragging = false;
            }
        }

        private void ForceRetarget(Rect previewRect, Rect imageRect)
        {
            // 현재 imageOffset을 previewRect 내에서 유효한 값으로 제한합니다.
            imageOffset = ClampImagePosition(previewRect, imageRect.size, imageOffset);
        }

        private Vector2 ClampImagePosition(Rect previewRect, Vector2 imageSize, Vector2 currentOffset)
        {
            Vector2 previewCenter = previewRect.center;
            float halfWidth = imageSize.x * 0.5f;
            float halfHeight = imageSize.y * 0.5f;

            float minX = previewRect.xMin + halfWidth;
            float maxX = previewRect.xMax - halfWidth;
            float minY = previewRect.yMin + halfHeight;
            float maxY = previewRect.yMax - halfHeight;

            Vector2 imageCenter = previewCenter + currentOffset;
            float clampedX = Mathf.Clamp(imageCenter.x, minX, maxX);
            float clampedY = Mathf.Clamp(imageCenter.y, minY, maxY);

            return new Vector2(clampedX - previewCenter.x, clampedY - previewCenter.y);
        }
    }
}