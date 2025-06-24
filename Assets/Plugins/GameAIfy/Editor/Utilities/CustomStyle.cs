using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GameAIfySDK
{
    public static class CustomStyle
    {
        public static GUIStyle titleLabelStyle;
        public static GUIStyle titleSubLabelStyle;
        public static GUIStyle bubbleStyle;
        public static GUIStyle centeredStyle;
        public static GUIStyle centeredWrapTextStyle;
        public static GUIStyle topLabelStyle;
        public static GUIStyle bottomLabelStyle;
        public static GUIStyle boldLabelStyle;
        public static GUIStyle wrapLabelStyle;
        public static GUIStyle textAreaStyle;
        public static GUIStyle selectedButtonStyle;

        public static Texture2D bubbleBackgroundTexture;
        public static Texture2D settingTexture;
        public static Texture2D greenLightTexture;
        public static Texture2D refreshTexture;
        public static Texture2D remixTexture;
        public static Texture2D trashTexture;
        public static Texture2D undoTexture;
        public static Texture2D redoTexture;
        public static Texture2D brushTexture;
        public static Texture2D eraserTexture;
        public static Texture2D addPinTexture;
        public static Texture2D informationTexture;
        public static Texture2D removePinTexture;
        public static Texture2D zoomTextrue;

        public static GUIContent remixButtonContent;
        public static GUIContent downloadButtonContent;
        public static GUIContent deleteButtonContent;
        public static GUIContent clearButtonContent;
        public static GUIContent settingButtonContent;

        public static Rect thumbnailRect;
        public static float thumbnailWidth;
        public static float thumbnailHeight;

        public static Color blueColor { get; private set; }
        public static Color darkGray { get; private set; }

        static CustomStyle()
        {
            InitializeStyles();
            InitializeTexture();
            InitializeContent();
            InitializeRect();
            InitializeInpainting();
            InitializeColor();
        }

        private static void InitializeStyles()
        {
            EditorStyles.helpBox.richText = true;
            
            titleLabelStyle = new GUIStyle(GUI.skin.label);
            titleLabelStyle.alignment = TextAnchor.MiddleLeft;
            titleLabelStyle.fontSize = 22;
            
            titleSubLabelStyle = new GUIStyle(GUI.skin.label);
            titleSubLabelStyle.fontSize = 18;
            
            centeredWrapTextStyle = new GUIStyle(GUI.skin.label);
            centeredWrapTextStyle.alignment = TextAnchor.MiddleCenter;
            centeredWrapTextStyle.wordWrap = true;
            
            // Initialize the bubble style
            bubbleStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white
                },
                fontSize = 10,
                border = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };

            // Create and cache the bubble background texture
            bubbleBackgroundTexture = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.15f, 0.9f));
            bubbleStyle.normal.background = bubbleBackgroundTexture;

            centeredStyle = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageOnly
            };

            topLabelStyle = new GUIStyle(EditorStyles.boldLabel);
            bottomLabelStyle = new GUIStyle(EditorStyles.label);

            boldLabelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };

            wrapLabelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };

            textAreaStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true
            };
        }

        public static GUIStyle GetSelectedButtonStyle()
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                normal =
                {
                    background = CreateColorTexture(new Color(0, 0.5333f, 0.8f, 1)),
                    textColor = Color.white
                },
                active =
                {
                    background = CreateColorTexture(new Color(0, 0.5333f, 0.75f, 1)),
                    textColor = Color.white
                },
                hover =
                {
                    background = CreateColorTexture(new Color(0, 0.5333f, 0.75f, 1)),
                    textColor = Color.white
                },
            };
            return style;
        }

        public static Texture2D CreateColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private static void InitializeTexture()
        {
            settingTexture = EditorGUIUtility.FindTexture("d__Popup");
            greenLightTexture = EditorGUIUtility.FindTexture("d_winbtn_mac_max");
            refreshTexture = EditorGUIUtility.FindTexture("d_Refresh");
            remixTexture = EditorGUIUtility.FindTexture("d_preAudioLoopOff");
            trashTexture = EditorGUIUtility.FindTexture("TreeEditor.Trash");
            informationTexture = EditorGUIUtility.FindTexture("d__Help");

            undoTexture = EditorGUIUtility.FindTexture("Animation.PrevKey");
            redoTexture = EditorGUIUtility.FindTexture("Animation.NextKey");
            brushTexture = EditorGUIUtility.FindTexture("Grid.PaintTool");
            eraserTexture = EditorGUIUtility.FindTexture("Grid.EraserTool");
            removePinTexture = EditorGUIUtility.FindTexture("d_winbtn_mac_close_h");
            addPinTexture = EditorGUIUtility.FindTexture("d_winbtn_mac_max_h");
            zoomTextrue = EditorGUIUtility.FindTexture("d_ViewToolZoom On");
        }

        private static void InitializeContent()
        {
            remixButtonContent = new GUIContent(remixTexture, "Remix");
            downloadButtonContent = new GUIContent("↓", "Download As");
            deleteButtonContent = new GUIContent("X", "Delete");
            clearButtonContent = new GUIContent(trashTexture, "Clear");
            settingButtonContent = new GUIContent(settingTexture, "Settings");
        }

        private static void InitializeColor()
        {
            blueColor = new Color(0.1f, 0.4f, 0.8f, 1f);
            darkGray = new Color(0.2f, 0.2f, 0.2f, 1f);
        }

        public struct ToolButton
        {
            public string Text;
            public string Tooltip;
            public DrawingMode Mode;
            public Action OnClick;
        }

        public static ToolButton[] toolButtons;

        public struct ActionButton
        {
            public string Text;
            public string Tooltip;
            public Action OnClick;
        }

        public static Texture2D uploadedImage;
        public static Texture2D brushCursor;
        public static Texture2D canvasImage;
        public static Texture2D transparentImage;
        public static Texture2D maskBuffer;
        public static int selectedBrushSize;
        public static List<Texture2D> canvasHistory;
        public static List<Texture2D> redoHistory;
        public static int canvasHistoryIndex;
        public static bool newStroke = true;
        public static string uploadedImagePath;

        public static float selectedOpacity = 1.0f;

        public enum DrawingMode
        {
            Add,
            Remove,
            Draw,
            Erase, /* Fill, Picker,*/
        }

        public static ActionButton[] actionButtons;
        public static GUIContent[] undoRedoOptions;
        public static GUIContent[] segmentMarkerOptions;
        public static GUIContent[] brushMakrerOptions;

        public static void InitializeInpainting()
        {
            transparentImage = new Texture2D(1, 1);
            selectedBrushSize = 10;
            canvasHistory = new List<Texture2D>();
            canvasHistoryIndex = -1;
            redoHistory = new List<Texture2D>();

            undoRedoOptions = new GUIContent[]
            {
                new GUIContent(CustomStyle.undoTexture, "undo"),
                new GUIContent(CustomStyle.redoTexture, "redo"),
                new GUIContent(CustomStyle.trashTexture, "clear")
            };
            brushMakrerOptions = new GUIContent[]
            {
                new GUIContent(CustomStyle.brushTexture, "Brush"),
                new GUIContent(CustomStyle.eraserTexture, "Erase")
            };
            segmentMarkerOptions = new GUIContent[]
            {
                new GUIContent(CustomStyle.addPinTexture, "Add Pin"),
                new GUIContent(CustomStyle.removePinTexture, "Remove Pin")
            };
        }


        private static void InitializeRect()
        {
            float padding = 10f;
            thumbnailWidth = 100f;
            thumbnailHeight = 100f;
            thumbnailRect = new Rect(padding, padding, thumbnailWidth, thumbnailHeight);
        }

        public static Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height, TextureFormat.ARGB32, false);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }


        public static void Label(string text, int fontSize = 12,
            TextAnchor alignment = TextAnchor.MiddleLeft,
            float width = 0,
            float height = 0,
            bool bold = false,
            params GUILayoutOption[] layoutOptions)
        {
            var style = new GUIStyle((bold) ? EditorStyles.boldLabel : EditorStyles.label)
            {
                normal =
                {
                    textColor = Color.white
                },
                fontSize = fontSize,
                alignment = alignment,
                fixedWidth = width,
                fixedHeight = height,
                wordWrap = true,
                hover =
                {
                    textColor = Color.white
                }
            };
            GUILayout.Label(text, style, layoutOptions);
        }
    }
}