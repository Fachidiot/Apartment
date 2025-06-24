using System;
using System.Collections.Generic;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;


namespace GameAIfySDK
{
    public class InpaintingUIHandler : ISubCategoryUIHandler
    {
        private static List<Texture2D> canvasHistory = new List<Texture2D>();
        private static List<CustomStyle.DrawingMode> historyType = new List<CustomStyle.DrawingMode>();
        private static Stack<Vector2> segmentQueue = new Stack<Vector2>();
        private static Stack<int> segmentPositiveQueue = new Stack<int>();

        private List<Texture2D> redoCanvasHistory = new List<Texture2D>();
        private List<CustomStyle.DrawingMode> redoHistoryTypes = new List<CustomStyle.DrawingMode>();
        private static Stack<Vector2> segmentRedoQueue = new Stack<Vector2>();
        private static Stack<int> segmentPositiveRedoQueue = new Stack<int>();

        private static string inputPrompt;
        private static int selectedBrushSize = 32;
        private static float selectedOpacity = 1.0f;
        private static int canvasHistoryIndex = -1;
        private static Texture2D canvasImage;
        private static Texture2D brushCursor;
        private static Vector2 previousPos;
        private static bool newStroke = true;
        private static CustomStyle.DrawingMode currentDrawingMode = CustomStyle.DrawingMode.Draw;
        private static Color selectedColor = Color.white;

        private static HistoryData curSelectItem = null;

        public void InitCanvasImage()
        {
            ClearSegment();

            canvasHistoryIndex = -1;

            if (GenerateWindow.selectItem.texture == null)
            {
                canvasImage.SetPixels(Enumerable.Repeat(Color.clear, canvasImage.width * canvasImage.height).ToArray());
                canvasImage.Apply();
                
                AddToCanvasHistory();
                return;
            }

            canvasImage = new Texture2D(GenerateWindow.selectItem.texture.width,
                GenerateWindow.selectItem.texture.height, TextureFormat.RGBA32, false, true);
            canvasImage.SetPixels(Enumerable.Repeat(Color.clear, canvasImage.width * canvasImage.height).ToArray());
            canvasImage.Apply();

            AddToCanvasHistory();
        }

        public void DrawUI()
        {
            EditorGUILayout.BeginVertical();
            DoDrawingSection();
            
            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
            {
                DoToolSection();
                DoSendButton();
            }
            EditorGUILayout.EndVertical();
        }

        private void DoDrawingSection()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
            float thumbnailSize = GetThumbnailSize();

            Rect rect = GUILayoutUtility.GetRect(thumbnailSize, thumbnailSize, GUILayout.ExpandWidth(false),
                GUILayout.ExpandHeight(false));

            curSelectItem = GenerateWindow.selectItem;
            if (curSelectItem != null && curSelectItem.texture != null)
            {
                Rect drawnRect = GetDrawnImageRect(rect, curSelectItem.texture);

                GUI.DrawTexture(rect, curSelectItem.texture, ScaleMode.ScaleToFit);
                if (canvasImage == null || canvasImage.width != curSelectItem.texture.width ||
                    canvasImage.height != curSelectItem.texture.height)
                {
                    InitCanvasImage();
                }

                GUI.DrawTexture(drawnRect, canvasImage, ScaleMode.ScaleToFit);

                if (drawnRect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.MouseDown)
                    {
                        Vector2 localMousePosition =
                            Event.current.mousePosition - new Vector2(drawnRect.x, drawnRect.y);
                        Vector2 textureCoords =
                            new Vector2(localMousePosition.x / drawnRect.width,
                                localMousePosition.y / drawnRect.height);

                        int x = (int)(textureCoords.x * curSelectItem.texture.width);
                        int y = (int)((1 - textureCoords.y) * curSelectItem.texture.height);

                        var segmentV = new Vector2(x, curSelectItem.texture.height - y);
                        if (Event.current.type == EventType.MouseDown)
                        {
                            if (currentDrawingMode == CustomStyle.DrawingMode.Add)
                            {
                                if (Constants.SEGMENT_MAX_PIN <= segmentPositiveQueue.Count(x => x == 1))
                                {
                                    Alerts.FailDialog(String.Format(AlertMessages.SegmentPinMax,Constants.SEGMENT_MAX_PIN));
                                }
                                else
                                {
                                    Debug.Log("Add Point : " + segmentV.x + "," + segmentV.y);
                                    segmentQueue.Push(segmentV);
                                    segmentPositiveQueue.Push(1);
                                    DrawOnTexture(canvasImage, new Vector2(x, y), 32, Color.green,
                                        selectedOpacity);

                                    AddToCanvasHistory();
                                }
                            }
                            else if (currentDrawingMode == CustomStyle.DrawingMode.Remove)
                            {
                                if (Constants.SEGMENT_MAX_PIN <= segmentPositiveQueue.Count(x => x == 0))
                                {
                                    Alerts.FailDialog(String.Format(AlertMessages.SegmentPinMax,Constants.SEGMENT_MAX_PIN));
                                }
                                else
                                {
                                    var v = new Vector2(x, y);
                                    segmentQueue.Push(segmentV);
                                    segmentPositiveQueue.Push(0);

                                    DrawOnTexture(canvasImage, new Vector2(x, y), 32, Color.red,
                                        selectedOpacity);
                                    Debug.Log("Remove Point : " + segmentV.x + "," + segmentV.y);

                                    AddToCanvasHistory();
                                }
                            }
                        }

                        if (currentDrawingMode == CustomStyle.DrawingMode.Draw)
                        {
                            DrawLine(canvasImage, new Vector2(x, y), selectedBrushSize, selectedColor,
                                selectedOpacity);
                        }
                        else if (currentDrawingMode == CustomStyle.DrawingMode.Erase)
                        {
                            DrawLine(canvasImage, new Vector2(x, y), selectedBrushSize, new Color(0, 0, 0, 0),
                                selectedOpacity);
                        }

                        Event.current.Use();
                    }

                    else if (Event.current.type == EventType.MouseUp)
                    {
                        newStroke = true;
                        previousPos = Vector2.zero;
                    }
                }
            }

            EditorGUILayout.EndVertical();            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);
        }


        private MarkerBrushType selectedMarkerBrushType = MarkerBrushType.AutoSelect;
        private MarkerBrushType previousMarkerBrushType = MarkerBrushType.AutoSelect;
        private int _selectMarkerPositiveNegative = 0;
        private int _previousUndoRedoIndex = -1;

        private void DoToolSection()
        {
            EditorGUILayout.BeginVertical();

            string[] options = Enum.GetNames(typeof(MarkerBrushType));

            selectedMarkerBrushType =
                (MarkerBrushType)GUILayout.Toolbar((int)selectedMarkerBrushType, options, GUILayout.ExpandWidth(true));

            if (selectedMarkerBrushType != previousMarkerBrushType)
            {
                bool changeAccepted = true;

                if (previousMarkerBrushType == MarkerBrushType.AutoSelect && segmentPositiveQueue.Count > 0
                    || previousMarkerBrushType == MarkerBrushType.ManualBrush && canvasHistoryIndex > 0)
                {
                    changeAccepted = Alerts.CheckDialog(AlertMessages.PartialRedrawReset);
                }

                if (changeAccepted)
                {
                    if (canvasHistoryIndex > 0)
                    {
                        ClearCanvas();
                    }

                    _selectMarkerPositiveNegative = 0;
                    previousMarkerBrushType = selectedMarkerBrushType;
                }
                else
                {
                    selectedMarkerBrushType = previousMarkerBrushType;
                }
            }

            EditorGUILayout.HelpBox(HelpBoxMessages.PartialRedrawBrushType[(int)selectedMarkerBrushType], MessageType.Info);

            EditorGUILayout.BeginHorizontal();

            if (selectedMarkerBrushType == MarkerBrushType.AutoSelect)
            {
                _selectMarkerPositiveNegative =
                    GUILayout.Toolbar(_selectMarkerPositiveNegative, CustomStyle.segmentMarkerOptions);
                currentDrawingMode = _selectMarkerPositiveNegative == 0
                    ? CustomStyle.DrawingMode.Add
                    : CustomStyle.DrawingMode.Remove;
            }
            else if (selectedMarkerBrushType == MarkerBrushType.ManualBrush)
            {
                _selectMarkerPositiveNegative =
                    GUILayout.Toolbar(_selectMarkerPositiveNegative, CustomStyle.brushMakrerOptions);
                currentDrawingMode = _selectMarkerPositiveNegative == 0
                    ? CustomStyle.DrawingMode.Draw
                    : CustomStyle.DrawingMode.Erase;
            }

            EditorGUILayout.Space(10);

            int currentIndex = GUILayout.Toolbar(_previousUndoRedoIndex, CustomStyle.undoRedoOptions,
                GUILayout.ExpandWidth(true));

            if (currentIndex != _previousUndoRedoIndex && currentIndex >= 0 &&
                currentIndex < CustomStyle.undoRedoOptions.Length)
            {
                switch (currentIndex)
                {
                    case 0:
                        UndoCanvas();
                        break;
                    case 1:
                        RedoCanvas();
                        break;
                    case 2:
                        ClearCanvas();
                        break;
                }

                _previousUndoRedoIndex = -1;
            }
            else
            {
                _previousUndoRedoIndex = currentIndex;
            }

            EditorGUILayout.EndHorizontal();

            if (selectedMarkerBrushType == MarkerBrushType.ManualBrush)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Size", EditorStyles.boldLabel, GUILayout.Width(80));
                GUILayout.Label(selectedBrushSize.ToString(), GUILayout.Width(40));
                selectedBrushSize =
                    (int)GUILayout.HorizontalSlider(selectedBrushSize, 10, 64, GUILayout.Width(200));
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(segmentQueue.Count == 0);
                if (GUILayout.Button("Segment"))
                {
                    List<Vector2> segmentList = segmentQueue.ToList();
                    List<int> segmentPositiveList = segmentPositiveQueue.ToList();

                    EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.SendSegment(curSelectItem.seq,
                        segmentList,
                        segmentPositiveList, (texture) =>
                        {
                            if (texture != null)
                            {
                                ClearCanvas();
                                previousMarkerBrushType = MarkerBrushType.ManualBrush;
                                selectedMarkerBrushType = MarkerBrushType.ManualBrush;
                                canvasImage = texture;
                                AddToCanvasHistory();
                                
                                GenerateWindow.window.Repaint();
                            }
                        }));
                }

                EditorGUI.EndDisabledGroup();
            }

            EditorGUILayout.Space(30);

            EditorGUILayout.EndVertical();
        }

        public float GetThumbnailSize()
        {
            // Image
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor);
            float thumbnailWidth = windowWidth - 20;

            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight)/2,
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 20;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);
            return thumbnailSize;
        }


        private void DoSendButton()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Positive Prompt", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(CustomStyle.clearButtonContent))
            {
                inputPrompt = "";
                GUI.FocusControl(null);
            }

            GUILayout.EndHorizontal();

            inputPrompt = EditorGUILayout.TextArea(inputPrompt, CustomStyle.textAreaStyle, GUILayout.Height(50));

            GUILayout.Space(30);
            EditorGUI.BeginDisabledGroup(canvasHistoryIndex <= segmentQueue.Count);
            if (GUILayout.Button("Edit"))
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.SendPartialRedraw(curSelectItem.seq,
                    inputPrompt,
                    canvasImage,
                    (callback) =>
                    {
                        if (callback != null)
                        {
                            ImageDataResponse expressionChangeInfoResponse =
                                JsonConvert.DeserializeObject<ImageDataResponse>(callback);
                            var imageInfo = expressionChangeInfoResponse.data;
                            var history = DataCache.AddHistory(imageInfo, "inpaint");
                            GenerateWindow.selectItem = history;
                            ClearCanvas();
                            GenerateWindow.window.Repaint();
                        }
                    }));
            }

            EditorGUI.EndDisabledGroup();
        }

        private Texture2D MakeOpacityTex(int width, int height, float opacity)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = new Color(1, 1, 1, opacity);
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void DoSegmentComplete(Texture2D tex)
        {
            Color[] pixels = tex.GetPixels();

            // 모든 픽셀에 대해 반복합니다.
            for (int i = 0; i < pixels.Length; i++)
            {
                if (pixels[i] == Color.green || pixels[i] == Color.red)
                {
                    pixels[i] = new Color(pixels[i].r, pixels[i].g, pixels[i].b, 0f);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
        }

        private Rect GetDrawnImageRect(Rect rect, Texture image)
        {
            if (image == null)
                return rect;

            float imageAspect = (float)image.width / image.height;
            float rectAspect = rect.width / rect.height;

            Rect drawnRect = rect;
            if (imageAspect > rectAspect)
            {
                float scale = rect.width / image.width;
                float drawnHeight = image.height * scale;
                float yOffset = (rect.height - drawnHeight) * 0.5f;
                drawnRect = new Rect(rect.x, rect.y + yOffset, rect.width, drawnHeight);
            }
            else
            {
                float scale = rect.height / image.height;
                float drawnWidth = image.width * scale;
                float xOffset = (rect.width - drawnWidth) * 0.5f;
                drawnRect = new Rect(rect.x + xOffset, rect.y, drawnWidth, rect.height);
            }

            return drawnRect;
        }

        private void DrawLine(Texture2D tex, Vector2 currentPos, int brushSize, Color color, float opacity)
        {
            if (newStroke && (currentDrawingMode == CustomStyle.DrawingMode.Draw ||
                              currentDrawingMode == CustomStyle.DrawingMode.Erase))
            {
                AddToCanvasHistory();
                newStroke = false;
            }

            if (previousPos != Vector2.zero)
            {
                float distance = Vector2.Distance(previousPos, currentPos);
                int steps = Mathf.CeilToInt(distance / (brushSize * 0.25f));
                for (int i = 0; i <= steps; i++)
                {
                    float t = (steps == 0) ? 0f : (float)i / steps;
                    Vector2 interpolatedPos = Vector2.Lerp(previousPos, currentPos, t);
                    DrawOnTexture(tex, interpolatedPos, brushSize, color, opacity);
                }
            }
            else
            {
                DrawOnTexture(tex, currentPos, brushSize, color, opacity);
            }

            previousPos = currentPos;
        }

        private void DrawOnTexture(Texture2D tex, Vector2 position, int brushSize, Color color, float opacity)
        {
            int xStart = Mathf.Clamp((int)position.x - brushSize / 2, 0, tex.width);
            int xEnd = Mathf.Clamp((int)position.x + brushSize / 2, 0, tex.width);
            int yStart = Mathf.Clamp((int)position.y - brushSize / 2, 0, tex.height);
            int yEnd = Mathf.Clamp((int)position.y + brushSize / 2, 0, tex.height);

            Color colorWithOpacity = new Color(color.r, color.g, color.b, opacity);

            for (int x = xStart; x < xEnd; x++)
            {
                for (int y = yStart; y < yEnd; y++)
                {
                    if (Vector2.Distance(new Vector2(x, y), position) <= brushSize / 2)
                    {
                        Color currentColor = tex.GetPixel(x, y);
                        Color finalColor;

                        if (currentDrawingMode == CustomStyle.DrawingMode.Erase)
                        {
                            finalColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0);
                        }
                        else
                        {
                            finalColor = Color.Lerp(currentColor, colorWithOpacity, colorWithOpacity.a);
                        }

                        tex.SetPixel(x, y, finalColor);
                    }
                }
            }

            tex.Apply();
        }

        private void AddToCanvasHistory()
        {
            if (canvasHistory.Count > canvasHistoryIndex + 1)
            {
                canvasHistory.RemoveRange(canvasHistoryIndex + 1, canvasHistory.Count - canvasHistoryIndex - 1);
                historyType.RemoveRange(canvasHistoryIndex + 1, canvasHistory.Count - canvasHistoryIndex - 1);
            }

            Texture2D newHistoryImage =
                new Texture2D(canvasImage.width, canvasImage.height, TextureFormat.RGBA32, false, true);
            newHistoryImage.SetPixels(canvasImage.GetPixels());
            newHistoryImage.Apply();
            canvasHistory.Add(newHistoryImage);
            historyType.Add(currentDrawingMode);
            canvasHistoryIndex++;
        }


        private void UndoCanvas()
        {
            if (canvasHistoryIndex <= 0) return;

            Texture2D redoImage =
                new Texture2D(canvasImage.width, canvasImage.height, TextureFormat.RGBA32, false, true);
            redoImage.SetPixels(canvasImage.GetPixels());
            redoImage.Apply();
            redoCanvasHistory.Add(redoImage);
            redoHistoryTypes.Add(historyType[canvasHistoryIndex]);

            if (historyType[canvasHistoryIndex] == CustomStyle.DrawingMode.Add ||
                historyType[canvasHistoryIndex] == CustomStyle.DrawingMode.Remove)
            {
                if (segmentQueue != null && segmentQueue.Count > 0 &&
                    segmentPositiveQueue != null && segmentPositiveQueue.Count > 0)
                {
                    var seg = segmentQueue.Pop();
                    var segPos = segmentPositiveQueue.Pop();
                    segmentRedoQueue.Push(seg);
                    segmentPositiveRedoQueue.Push(segPos);
                }
            }

            canvasHistoryIndex--;
            canvasImage.SetPixels(canvasHistory[canvasHistoryIndex].GetPixels());
            canvasImage.Apply();
        }

        private void RedoCanvas()
        {
            if (redoCanvasHistory.Count <= 0) return;

            Texture2D undoImage =
                new Texture2D(canvasImage.width, canvasImage.height, TextureFormat.RGBA32, false, true);
            undoImage.SetPixels(canvasImage.GetPixels());
            undoImage.Apply();
            canvasHistory.Add(undoImage);

            if (historyType[canvasHistoryIndex] == CustomStyle.DrawingMode.Add ||
                historyType[canvasHistoryIndex] == CustomStyle.DrawingMode.Remove)
            {
                if (segmentRedoQueue != null && segmentRedoQueue.Count > 0 &&
                    segmentPositiveRedoQueue != null && segmentPositiveRedoQueue.Count > 0)
                {
                    var seg = segmentRedoQueue.Pop();
                    var segPos = segmentPositiveRedoQueue.Pop();
                    segmentQueue.Push(seg);
                    segmentPositiveQueue.Push(segPos);
                }
            }

            canvasHistoryIndex++;

            Texture2D redoImage = redoCanvasHistory[redoCanvasHistory.Count - 1];
            redoCanvasHistory.RemoveAt(redoCanvasHistory.Count - 1);
            canvasImage.SetPixels(redoImage.GetPixels());
            canvasImage.Apply();

            CustomStyle.DrawingMode restoredMode = redoHistoryTypes[redoHistoryTypes.Count - 1];
            redoHistoryTypes.RemoveAt(redoHistoryTypes.Count - 1);
            historyType[canvasHistoryIndex] = restoredMode;
        }


        private void ClearCanvas()
        {
            InitCanvasImage();
        }

        private void ClearSegment()
        {
            segmentQueue.Clear();
            segmentPositiveQueue.Clear();
            segmentRedoQueue.Clear();
            segmentPositiveRedoQueue.Clear();

            canvasHistory.Clear();
            historyType.Clear();
            redoCanvasHistory.Clear();
            redoHistoryTypes.Clear();
        }
    }
}