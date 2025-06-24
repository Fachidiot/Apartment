using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace GameAIfySDK
{
    public partial class GenerateWindow : EditorWindow
    {
        private int poseBasedGridSize = 2;
        private Texture2D[] poseBasedSampleTexture = new Texture2D[4];

        private List<string> characterPoseString = new List<string>
        {
            "queen, princess dress, cleavage, crown, black hair, wedge cut, short hair, short fringe, medium breasts",
            "orange print hoodie, shorts, green hair, absurdly long hair, medium hair, bangs, loli, small breasts",
            "black collared_shirt, long sleeves, skirt, pantyhose, lanyard, , blonde hair, unkempt, messy hair, long hair, asymmetrical bangs, mature_female, medium breasts",
            "blonde colored office lady, (black jacket, tight black pencil skirt, white collared shirt, black necktie, black pantyhose), burgundy hair, twintails, wavy hair, long hair, asymmetrical bangs, teenage, flat chest",
            "brown Bomber Jacket,yellow Fishing shorts, curly hair, short hair, baby bangs, two-tone hair, skinny, man",
            "demon, demon horn, demon tail, knight, holding sword, pink hair, (low ponytail, hair over shoulder), long hair, fringed bangs, small breasts",
            "Olive Green colored french coat,blonde side slit skirt, lavender colored High-top sneakers, silver hair, straight hair, wavy hair, long hair, round fringe, medium breasts",
            "Light blue Muscle Fit Hoodie,brown Hiking pants, dreadlocks, short hair, bangs, brown hair, skinny, young_boy",
            "green Field Jacket,green Tactical pants, straight hair, short hair, curtain bangs, orange hair, musclar",
            "burgundy colored kimono, green hair, two side up, wavy , very long hair, bangs, small breasts",
            "grey trench coat, skirt, multicolored hair, high ponytail, very long hair, bangs, medium breasts"
        };


        private const int PoseBasedCreateImgCount = 4;

        private void DoModdlePoseBasedUI()
        {
            if (poseBasedSampleTexture[0] == null)
            {
                LoadTexture();
            }

            EditorGUILayout.BeginVertical();
            float thumbnialSize = Constants.GENERATETAB_WIDTH / 2 - 20;

            for (int x = 0; x < poseBasedGridSize; ++x)
            {
                GUILayout.BeginHorizontal();
                for (int y = 0; y < poseBasedGridSize; ++y)
                {
                    int index = x * poseBasedGridSize + y;
                    GUILayout.BeginVertical(GUI.skin.box);
                    string gridTitle = Regex.Replace(((PoseBasedType)index).ToString(), "(?<!^)([A-Z])", " $1");
                    GUILayout.Label(gridTitle, EditorStyles.boldLabel);
                    string s = index == 3 ? "23 : 11 ( 1472 x 704px )" : "7 : 9 ( 896 x 1152px )";
                    GUILayout.Label(s);

                    if (poseBasedSampleTexture[index] != null)
                        GUILayout.Label(poseBasedSampleTexture[index], GUILayout.Width(thumbnialSize),
                            GUILayout.Height(thumbnialSize));
                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            bool isShowNegativePrompt = false;
            DoDrawPrompt(isShowNegativePrompt);

            GUILayout.Space(30);
            GeneratePoseBased();
        }

        private void GeneratePoseBased()
        {
            GUI.enabled = true;
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(inputPrompt));
            if (GUILayout.Button("Generate", GUILayout.Height(60)))
            {
                if (CheckException())
                {
                    ReferenceImageData referenceImageData = new ReferenceImageData();
                    if (_referenceImage)
                    {
                        referenceImageData.referenceImage = _referenceImage;
                        referenceImageData.strength = _referenceStrengthValue;
                        referenceImageData.mode = _textReferenceImageMode[(int)_selectedReferenceImageMode].ToLower();
                    }

                    GUI.enabled = false;
                    EditorCoroutineUtility.StartCoroutineOwnerless(APIHandler.RequestPoseBasedGenerate(inputPrompt,
                        (result) =>
                        {
                            if (result == "Success")
                                EditorCoroutineUtility.StartCoroutineOwnerless(LoadMyWorksDataAsync(true));


                            GUI.enabled = true;
                        }));
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void LoadTexture()
        {
            poseBasedSampleTexture[0] = Resources.Load<Texture2D>("PoseBased_FrontPose");
            poseBasedSampleTexture[1] = Resources.Load<Texture2D>("PoseBased_BackPose");
            poseBasedSampleTexture[2] = Resources.Load<Texture2D>("PoseBased_RunningPose");
            poseBasedSampleTexture[3] = Resources.Load<Texture2D>("PoseBased_LyingPose");
        }
    }
}