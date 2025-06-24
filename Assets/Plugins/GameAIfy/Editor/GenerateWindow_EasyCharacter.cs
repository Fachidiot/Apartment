using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Text;
using System.Collections;

namespace GameAIfySDK
{
    public partial class GenerateWindow : EditorWindow
    {
        private const int GenderBaseNum = 10000;
        private const int CategoryBaseNum = 1000;

        //private bool isBodyFeatureShow = true;

        //private bool isClothesShow = true;

        //private bool isSettingShow = true;

        private int outputCount = 1;
        private int prevOutputCount = 0;

        protected int selectedBodyTypeIndex = 0;
        protected int selectedAgeIndex = 0;
        protected int selectedHairLengthIndex = 0;

        protected int selectedClothesTypeIndex = 0;
        protected int selectedClothesConceptIndex = 0;

        //protected string[] bodyTypeOptions = { "Male", "Female" };
        //protected string[] ageOptions = { "Random", "Young", "Mature", "Senior" };
        //protected string[] hairLengthOptions = { "Random", "Long", "Medium", "Short" };


        //protected string[] clothesConcenptOptions = { "Random", "Casual", "Uniforms", "Office", "Dress", "Fantasy", "Swimsuit", "Military"};

        // Body Category string (use UI draw)
        private List<string> strGenderList = new List<string>();

        // Age, Hair, Clothes, Clothes Concept Category stringList (use UI draw)
        private Dictionary<PartType, List<string>> dicCategory = new Dictionary<PartType, List<string>>();

        // Toolbar Select Index list
        private Dictionary<PartType, int> optionsSelectIndexList = new Dictionary<PartType, int>();

        private List<EasyCharacterMakerListData> makerList = null;

        private Dictionary<CharacterMakerType, Dictionary<int, EasyCharacterMakerListData>> dicMakerList =
            new Dictionary<CharacterMakerType, Dictionary<int, EasyCharacterMakerListData>>();

        private IEnumerator LoadEasyCharacterMakerList()
        {
            yield return DataCache.LoadEasyCharacterMakerList();
        }


        private void SortMakerList()
        {
            if (DataCache.easyCharacterMakerList == null)
                return;

            for (int i = 0; i < makerList.Count; ++i)
            {
                CharacterMakerType genderType = (CharacterMakerType)makerList[i].gender_num;

                if (dicMakerList.ContainsKey(genderType) == false)
                {
                    Dictionary<int, EasyCharacterMakerListData> partList =
                        new Dictionary<int, EasyCharacterMakerListData>();
                    partList.Add(makerList[i].part_num, makerList[i]);

                    dicMakerList.Add(genderType, partList);
                }
                else
                {
                    Dictionary<int, EasyCharacterMakerListData> partList = dicMakerList[genderType];
                    if (partList.ContainsKey(makerList[i].part_num) == false)
                    {
                        partList.Add(makerList[i].part_num, makerList[i]);
                    }

                    dicMakerList[genderType] = partList;
                }

                if (strGenderList.Contains(makerList[i].gender) == false)
                    strGenderList.Add(makerList[i].gender);

                List<string> strList = null;

                int remainValue = makerList[i].part_num % GenderBaseNum;
                int categoryValue = remainValue / CategoryBaseNum;

                if (dicCategory.TryGetValue((PartType)categoryValue, out strList))
                {
                    if (strList.Contains(makerList[i].name) == false)
                        strList.Add(makerList[i].name);
                }
                else
                {
                    strList = new List<string>();
                    strList.Add(makerList[i].name);
                    dicCategory.Add((PartType)categoryValue, strList);
                }
            }

            foreach (KeyValuePair<PartType, List<string>> pair in dicCategory)
            {
                if (optionsSelectIndexList.ContainsKey(pair.Key) == false)
                {
                    optionsSelectIndexList.Add(pair.Key, 0);
                }
            }
        }

        protected void DoDrawEasyCharacterCreator()
        {
            makerList = DataCache.easyCharacterMakerList;
            
            if (DataCache.easyCharacterMakerList == null)
                return;
            
            SortMakerList();
            DoDrawModelSelect();
            
            EditorGUILayout.Space(10);
            DoDrawEasyCharacterCreatorBodyFeatures();
            
            EditorGUILayout.Space(10);
            DoDrawEasyCharacterCreatorClothes();
            
            EditorGUILayout.Space(10);
            DoDrawEasyCharacterCreatorSettings();
            
            GUILayout.Space(30);
            GenerateEasyCharacter();
        }

        protected void DoDrawEasyCharacterCreatorBodyFeatures()
        {
            //Rect foldoutRect = GetFoldRect();

            //GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.fontStyle = FontStyle.Bold;

            //isBodyFeatureShow = EditorGUI.Foldout(foldoutRect, isBodyFeatureShow, "Body Features", true, foldoutStyle);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.helpBox);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Gender", titleStyle);

            //if (isBodyFeatureShow)
            {
                EditorGUILayout.BeginVertical();

                EditorGUILayout.Space(5);
                //GUILayout.Label("Body");
                selectedBodyTypeIndex = GUILayout.Toolbar(selectedBodyTypeIndex, strGenderList.ToArray());


                foreach (KeyValuePair<PartType, List<string>> pair in dicCategory)
                {
                    if (pair.Key < PartType.Clothes)
                    {
                        EditorGUILayout.Space(5);
                        string categoryTitle = Regex.Replace(((PartType)pair.Key).ToString(), "(?<!^)([A-Z])", " $1");
                        GUILayout.Label(categoryTitle, titleStyle);
                        optionsSelectIndexList[pair.Key] =
                            GUILayout.Toolbar(optionsSelectIndexList[pair.Key], pair.Value.ToArray());
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        protected void DoDrawEasyCharacterCreatorClothes()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.helpBox);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;            

            //Rect foldoutRect = GetFoldRect();

            //GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.fontStyle = FontStyle.Bold;

            //isClothesShow = EditorGUI.Foldout(foldoutRect, isClothesShow, "Clothes", true, foldoutStyle);

            //if (isClothesShow)
            {
                EditorGUILayout.BeginVertical();

                foreach (KeyValuePair<PartType, List<string>> pair in dicCategory)
                {
                    if (pair.Key >= PartType.Clothes)
                    {
                        EditorGUILayout.Space(5);
                        string categoryTitle = Regex.Replace(((PartType)pair.Key).ToString(), "(?<!^)([A-Z])", " $1");
                        GUILayout.Label(categoryTitle, titleStyle);
                        if (pair.Key == PartType.Clothes)
                        {
                            optionsSelectIndexList[pair.Key] = GUILayout.SelectionGrid(optionsSelectIndexList[pair.Key],
                                pair.Value.ToArray(), 4);
                        }
                        else
                        {
                            optionsSelectIndexList[pair.Key] = GUILayout.Toolbar(optionsSelectIndexList[pair.Key],
                                pair.Value.ToArray());
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }
        }

        protected void DoDrawEasyCharacterCreatorSettings()
        {
            //Rect foldoutRect = GetFoldRect();

            //GUIStyle foldoutStyle = new GUIStyle(EditorStyles.foldout);
            //foldoutStyle.fontStyle = FontStyle.Bold;

            GUIStyle titleStyle = new GUIStyle(EditorStyles.helpBox);
            titleStyle.fontSize = 13;
            titleStyle.fontStyle = FontStyle.Bold;
            GUILayout.Label("Settings", titleStyle);

            //isSettingShow = EditorGUI.Foldout(foldoutRect, isSettingShow, "Settings", true, foldoutStyle);

            //if (isSettingShow)
            {
                GUILayout.BeginVertical();

                EditorGUILayout.Space(5);

                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.alignment = TextAnchor.MiddleCenter;

                // Draw Slider
                GUILayout.BeginHorizontal();
                GUILayout.Label("Output Count", EditorStyles.boldLabel);
                outputCount = (int)EditorGUILayout.Slider(outputCount, MinImageCountValue, MaxImageCountValue);
                if (outputCount != prevOutputCount)
                {
                    prevOutputCount = outputCount;
                    GUI.FocusControl(null);
                }

                GUILayout.EndHorizontal();

                EditorGUILayout.Space(10);
                GUILayout.EndVertical();
            }
        }

        private void GenerateEasyCharacter()
        {
            GUI.enabled = true;
            if (GUILayout.Button("Generate", GUILayout.Height(60)))
            {
                if (CheckException())
                {
                    int mainKey = selectedBodyTypeIndex + 1;

                    List<int> seqList = new List<int>();

                    int baseNum = mainKey * GenderBaseNum;
                    foreach (PartType part in Enum.GetValues(typeof(PartType)))
                    {
                        if (part < PartType.ClothesConcept)
                        {
                            int partNum = baseNum + (optionsSelectIndexList[part] + 1) + ((int)part * CategoryBaseNum);
                            seqList.Add(GetSeq((CharacterMakerType)mainKey, partNum));
                        }
                    }
                
                    string strWildCard = GetSeqStringWithComma(seqList);
                    ReferenceImageData referenceImageData = new ReferenceImageData();
                    if (_referenceImage)
                    {
                        referenceImageData.referenceImage = _referenceImage;
                        referenceImageData.strength = _referenceStrengthValue;
                        referenceImageData.mode = _textReferenceImageMode[(int)_selectedReferenceImageMode].ToLower();
                    }

                    GUI.enabled = false;
                    EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestEasyCharacterMaker(outputCount,
                        selectPreset.seq, strWildCard,
                        (result) =>
                        {
                            if (result == "Success")
                                EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync(true));
                    
                            GUI.enabled = true;
                        }));
                }
            }
            

        }

        private int GetSeq(CharacterMakerType makerType, int partNum)
        {
            if (dicMakerList[makerType] != null)
            {
                if (dicMakerList[makerType][partNum] != null)
                {
                    return dicMakerList[makerType][partNum].seq;
                }
            }

            return 0;
        }

        private string GetSeqStringWithComma(List<int> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("[");
            for (int i = 0; i < list.Count; ++i)
            {
                sb.Append(list[i].ToString());
                if (i < list.Count - 1)
                    sb.Append(",");
            }

            sb.Append("]");

            return sb.ToString();
        }
    }
}