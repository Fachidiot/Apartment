using System;

namespace GameAIfySDK
{
    public class Enums
    {
        public static PresetModelMainCategory ConvertCategory(SideBarSubCategoryItem category)
        {
            if (Enum.TryParse<PresetModelMainCategory>(category.ToString(), out var mainCategory))
            {
                return mainCategory;
            }
            else
            {
                return PresetModelMainCategory.All;
            }
        }

        public static SideBarSubCategoryItem ConvertCategory(PresetModelMainCategory category)
        {
            if (Enum.TryParse<SideBarSubCategoryItem>(category.ToString(), out var mainCategory))
            {
                return mainCategory;
            }
            else
            {
                return SideBarSubCategoryItem.Character;
            }
        }
    }

    public enum PresetModelMainCategory
    {
        All = 0,
        Character = 1,
        Background = 2,
        UI = 3,
        Effect = 4,
    }

    public enum MarkerBrushType
    {
        AutoSelect,
        ManualBrush
    }

    // 1) 미리 정의된 비율 enum
    public enum AspectRatios
    {
        Ratio_1_2, // 1:2
        Ratio_9_16, // 9:16
        Ratio_2_3, // 2:3
        Ratio_3_4, // 3:4
        Ratio_4_5, // 4:5
        Ratio_1_1, // 1:1
        Ratio_5_4, // 5:4
        Ratio_4_3, // 4:3
        Ratio_3_2, // 3:2
        Ratio_16_9, // 16:9
        Ratio_2_1 // 2:1
    }

    public enum SortBy
    {
        Like = 0,
        Recent = 1,
    }

    public enum Resolution
    {
        Portrait_864x1152 = 0,
        LandScape_1152x864 = 1,
        Square_1024x1024 = 2
    }

    public enum PresetGroup
    {
        Standard = 0,
        Beta = 1,
        Private = 2,
        B2B = 3,
        Unknown = 4,
    }

    public enum ReferenceImageMode
    {
        Reference = 0,
        Openpose = 1,
        Depth = 2,
    }

    public enum SideBarMainCategoryItem
    {
        Gallery = 0,
        Generator = 1,
        Editor = 2,
        Extender = 3,
        _3D = 4
    }

    public enum SideBarSubCategoryItem
    {
        Posts = 0,
        MyWorks = 1,
        Character = 10,
        CharactersinBulk = 11,
        CharacterPose = 12,
        Background = 13,
        UI = 14,
        Effect = 15,
        FacialExpression = 20,
        Inpainting = 21,
        BackgroundRemove = 22,
        Enhance = 23,
        Outpainting = 30,
        BackgroundLoop = 31,
        DepthMeshConverter = 40,
        Text2Motion = 41,
    }

    public enum FaceType
    {
        smile,
        laugh,
        crying,
        kiss,
        sneer,
        cheeky,
        nervous,
        sullen,
        worried,
        doleful,
        bored,
        frightened,
        surprised,
        shocked,
        shame
    }

    public enum DepthScale
    {
        Shallow = 2,
        Standard = 3,
        Deep = 4,
    }

    public enum ExpresstionChangeStep
    {
        none,
        detectingFace,
        expresstionEdit,
        result
    }

    public enum ExpressionStyle
    {
        anime,
        realistic
    }

    public enum ExpressionType
    {
        Neutral,
        Happy,
        Sad,
        Angry
    }

    public enum PoseBasedType
    {
        FrontPose,
        BackPose,
        RunningPose,
        LyingPose
    }

    public enum CharacterMakerType
    {
        all,
        Male,
        Female
    }

    public enum PartType
    {
        Age = 1,
        Hair = 2,
        Clothes = 3,
        ClothesConcept = 4
    }

    public enum EnhanceStrengthType
    {
        none,
        low,
        medium,
        high
    }

    public enum Text2MotionStep
    {
        generateMotion,
        convert,
        converting,
        convertFinish,        
    }

    public enum SmplStatus
    { 
        none = 0,
        Creating = 1,
        Complete = 9,
    }    
}