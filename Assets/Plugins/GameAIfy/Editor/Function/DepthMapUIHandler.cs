using System;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

namespace GameAIfySDK
{
    public class DepthMapUIHandler : ISubCategoryUIHandler
    {
        private const float MinDepthScaleValue = 0.1f;
        private const float MaxDepthScaleValue = 5;

        //private DepthScale _selectDepthScale = DepthScale.Standard;
        private int _selectDepthScaleIndex = 1;
        private float _curDepthScaleValue = (float)DepthScale.Standard;

        private bool isRequestConvert = false;
        private Texture2D overlayTexture;
        private float _lastDepthScaleValue = (float)DepthScale.Standard;

        private bool isResult = false;
        private HistoryData curSelectItem = null;
        private string objPath = string.Empty;
        private float cameraDistance = 5f;
        private HistoryData curConvetingItem = null;
        private bool isMakingMesh = false;
        public void DrawUI()
        {
            if (curSelectItem != GenerateWindow.selectItem)
            {
                curSelectItem = GenerateWindow.selectItem;
                overlayTexture = null;
                isResult = false;
                if(isMakingMesh == false)
                {
                    curConvetingItem = GenerateWindow.selectItem;
                }
            }

            EditorGUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            DoDrawThumbnailImage();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GenerateWindow.selectItem != null &&
                !CommonUI.CheckEditOverSizeWithHelpBox(GenerateWindow.selectItem.texture))
            {
                EditorGUI.BeginDisabledGroup(isRequestConvert);
                {
                    if (isResult == false)
                    {
                        DoDrawConvert();
                    }
                    else
                    {
                        DoDrawResult();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            
            
            EditorGUILayout.EndVertical();
        }

        public void DoDrawThumbnailImage()
        {
            // Image
            float windowWidth = GameAIfyUtilTools.CalcContentsTapWidth(SideBarMainCategoryItem.Editor);
            float thumbnailWidth = windowWidth - 20;

            float underExcludeHeight = Constants.PREIVEW_WINDOW_OTHER_HEIGHT;
            float windowHeight = Mathf.Min((GenerateWindow.window.position.height - underExcludeHeight) / 2,
                Constants.PREVIEW_IMG_MAX_WIDTH);
            float thumbnailHeight = windowHeight - 20;

            float thumbnailSize = Mathf.Min(thumbnailWidth, thumbnailHeight);

            if (null != curSelectItem)
            {
                var texture = overlayTexture ? overlayTexture : curSelectItem.texture;
                GUILayout.Box(texture, GUILayout.Width(thumbnailSize),
                    GUILayout.Height(thumbnailSize));
            }
        }

        private void DoDrawConvert()
        {
            GUILayout.Label("Depth Scale", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();

            string[] depthScaleType = Enum.GetNames(typeof(DepthScale));

            EditorGUI.BeginChangeCheck();
            _selectDepthScaleIndex = GUILayout.Toolbar(_selectDepthScaleIndex, depthScaleType);
            if (_selectDepthScaleIndex >= 0 && EditorGUI.EndChangeCheck())
            {
                GUI.FocusControl(null);
                _curDepthScaleValue = (float)(int)Enum.GetValues(typeof(DepthScale)).GetValue(_selectDepthScaleIndex);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            float newSliderValue = EditorGUILayout.Slider((float)Math.Round(_curDepthScaleValue, 1), MinDepthScaleValue,
                MaxDepthScaleValue);
            if (newSliderValue != _curDepthScaleValue)
            {
                _selectDepthScaleIndex = -1;
                _curDepthScaleValue = newSliderValue;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Convert", GUILayout.ExpandWidth(true)))
            {
                _lastDepthScaleValue = _curDepthScaleValue;
                isRequestConvert = true;
                HistoryData selectItem = curSelectItem;
                EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.GenerateDepthMap(selectItem.seq,
                    _curDepthScaleValue,
                    (jsonData) =>
                    {
                        if (jsonData == null || (jsonData != null && jsonData == "fail"))
                        {
                            isRequestConvert = false;
                        }
                        else
                        {
                            if (jsonData != null)
                            {
                                DepthMapGenerateDataResponse depthMapGenerateInfo =
                                    JsonConvert.DeserializeObject<DepthMapGenerateDataResponse>(jsonData);

                                DepthMapGenerateData data = depthMapGenerateInfo.data;
                                if (data != null)
                                {
                                    EditorCoroutineUtility.StartCoroutineOwnerless(DataCache.LoadTexture(data.seq,
                                        data.imgPath,
                                        (texture) =>
                                        {
                                            if (texture != null)
                                            {
                                                objPath = data.objPath;
                                                overlayTexture = texture;
                                                curConvetingItem = selectItem;
                                            }
                                        }));

                                    isRequestConvert = false;
                                    isResult = true;
                                }
                            }
                        }
                    }));
            }
        }

        private void DoDrawResult()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Import to Scene"))
            {
                if (string.IsNullOrEmpty(objPath) == false)
                {
                    isMakingMesh = true;
                    EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.LoadGeneratedObjectFileToString(
                        objPath,
                        (result) =>
                        {
                            if (result != null)
                            {
                                HistoryData item = curSelectItem;
                                if(curConvetingItem != null && curConvetingItem.texture != null)
                                {
                                    item = curConvetingItem;
                                }
                                GameObject createdObj = MeshProcessor.GetGenerateMapObject(result.Split('\n'), item.texture);
                                Renderer renderer = createdObj.GetComponent<Renderer>();
                                if (renderer != null)
                                {
                                    if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
                                    {
                                        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
                                        if (urpLitShader != null)
                                        {
                                            renderer.sharedMaterial.shader = urpLitShader;
                                            renderer.sharedMaterial.SetTexture("_BaseMap", item.texture);
                                        }
                                    }
                                }
                                if (createdObj != null)
                                    AdjustCamera(createdObj);

                            }
                            else
                            {
                                isMakingMesh = false;
                            }
                        }));
                }
            }

            if (GUILayout.Button("Download As .obj"))
            {
                if (string.IsNullOrEmpty(objPath) == false)
                {
                    EditorCoroutineUtility.StartCoroutineOwnerless(GameAIfyUtilTools.DownloadGeneratedObjectFile(
                        objPath,
                        (result) =>
                        {
                            if (result != null)
                            {
                                DownloadHandler.SaveAsObj(result);
                            }
                        }));
                }
            }

            GUILayout.EndVertical();
        }

        #region Move Camera

        private void AdjustCamera(GameObject spawnedObject)
        {
            if (spawnedObject == null)
            {
                return;
            }

            Camera mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }

            Bounds objectBounds = GetObjectBounds(spawnedObject);

            Vector3 direction = mainCamera.transform.forward;
            mainCamera.transform.position =
                objectBounds.center - direction * (objectBounds.extents.magnitude + cameraDistance);

            mainCamera.transform.LookAt(objectBounds.center);
        }

        private Bounds GetObjectBounds(GameObject obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(obj.transform.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        #endregion
    }
}