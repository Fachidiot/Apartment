namespace GameAIfySDK
{
    public static class Constants
    {
        public const string WEB_URL = API_URL;
        public const string DISCORD_URL = "https://discord.gg/WePPtJkEY8";
        public const string ACCESS_TOKEN_URL = API_URL + "settings/api-keys";

        #region API End-Point

        public const string REAL_API_URL = "https://gameaify.com/";
        public const string API_URL = REAL_API_URL;
        public const string API_USER_INFO = API_URL + "member/sdk/get_user_info";
        public const string API_CREATE = API_URL + "images/sdk/create";
        public const string API_DELETE = API_URL + "images/sdk/delete";
        public const string API_EDIT = API_URL + "images/sdk/edit";
        public const string API_UPLOAD = API_URL + "images/sdk/upload";
        public const string API_REMOVEBG = API_URL + "images/sdk/edit/remove_bg";
        public const string API_UPSCALE = API_URL + "images/sdk/edit/upscale";
        public const string API_REFINE = API_URL + "images/sdk/edit/refine";
        public const string API_SEGMENTATION = API_URL + "images/sdk/edit/segmentation/mask";
        public const string API_INPAINT = API_URL + "images/sdk/edit/inpaint";
        public const string API_CANVAS_EXTEND = API_URL + "images/sdk/edit/outpaint/canvas_extend";
        public const string API_IMAGE_LIST = API_URL + "images/sdk/image_list?usn={0}&page={1}&limit={2}&group={3}";
        public const string API_POST_LIST = API_URL + "post/sdk/getPostList?orderBy={0}&group={1}&page={2}&limit={3}";
        public const string API_MODEL_LIST = API_URL + "post/sdk/agis_preset_list/{0}";
        public const string API_HISTORY_LIST = API_URL + "images/sdk/{0}/history";
        public const string API_IMG_INFO = API_URL + "images/sdk/get_image_option/{0}";
        public const string API_DEPTH_MAP = API_URL + "images/sdk/edit/image_to_3d/map";
        public const string API_EXPRESSION_CONFIG = API_URL + "images/sdk/get_expression_list/{0}";
        public const string API_EXPRESSION_CHANGE = API_URL + "images/sdk/edit/expression_change";
        public const string API_SEAMLESS_EXTEND = API_URL + "images/sdk/edit/outpaint/seamless_extend";
        public const string API_EASY_CHARACTER_MAKER_LIST = API_URL + "images/sdk/get_wildcard_list/{0}";
        public const string API_EASY_CHARACTER_MAKER = API_URL + "images/sdk/create/easy_character_maker";
        public const string API_POSE_BASED = API_URL + "images/sdk/create/pose_based_generator";
        public const string API_ENHANCE = API_URL + "images/sdk/edit/enhance";
        public const string API_TEXT_TO_MOTION_LIST = API_URL + "motion/sdk/motion_list?usn={0}&page={1}";
        public const string API_TEXT_TO_MOTION_DETAIL = API_URL + "motion/sdk/motion_detail";
        public const string API_TEXT_TO_MOTION_JOINT = API_URL + "motion/sdk/create/text_to_motion/joint";
        public const string API_TEXT_TO_MOTION_SMPL = API_URL + "motion/sdk/create/text_to_motion/smpl";
        public const string API_TEXT_TO_MOTION_DELETE = API_URL + "motion/sdk/delete";

        #endregion

        #region GameAIfy Constants

        public const int API_GENERATE_CALL_MAX_COUNT = 1;
        public const int API_LOAD_ITEM_AMOUNT = 100;
        public const int SEGMENT_MAX_PIN = 5;
        public const int CHARACTERPOSE_PRESET_NUM = 80002;

        #endregion

        #region Size Constants

        public const int SIDEBAR_WIDTH = 200;
        public const int GENERATETAB_WIDTH = 350;
        public const int IMAGE_MIN_WIDTH = 150;
        public const int RECT_WIDTH = 800;
        public const int RECT_HEIGHT = 900;
        public const int MODEL_TOGGLE_WIDTH = 100;

        public const int PREVIEW_IMG_MIN_WIDTH = 100;
        public const int PREVIEW_IMG_MIN_HEIGHT = 100;
        public const int PREVIEW_IMG_MAX_WIDTH = 2048;
        public const int PREVIEW_IMG_MAX_HEIGHT = 2048;
        public const int PREIVEW_WINDOW_OTHER_WIDTH = 400;
        public const int PREIVEW_WINDOW_OTHER_HEIGHT = 200;

        public const int WINDOW_MIN_WIDTH = 800;
        public const int WINDOW_MIN_HEIGHT = 800;

        public const int GENERATE_PREVIEW_WIDTH = 400;
        public const int EDIT_PRESET_INFO_WIDTH = 300;
        public const int IMAGE_HISTORY_WIDTH = 250;

        public const int SETTING_WINDOW_MIN_WIDTH = 400;

        public const int PREVIEW_WINDOW_MIN_HEIGHT = 600;
        public const int SEAMLESS_MAX_OUTPUT = 2048;
        public const int CANVAS_EXTEND_MAX_OUTPUT = 2048;
        public const int CANVAS_EXTEND_OVERAY = 4096;
        public const int CANVAS_MIN_INPUT = 512;
        public const int CANVAS_MAX_INPUT = 2048;

        #endregion


        #region EditorPrefabs

        public const string ApiKey = "GFApiKey";
        public const string SecretKey = "GFSecretKey";
        public const string SaveFolder = "GFSaveFolder";
        public const string USN = "GFUSN";
        public const string Token = "GFToken"; //Basic API key:Secret Key  
        public const string SelectedLoraStrength = "GFSelectedStrength";
        public const string SelectedPresetNum = "GFSelectedPresetNum";
        public const string DefaultDownloadPath = "Assets/Images";

        #endregion
    }

    public static class LabelMessages
    {
        public const string PoseBasedDescription =
            "Generates four images with different poses based on the entered prompt.";

        public const string FaceChangerDescription =
            "Selects the art style for the Input character.\n Choose the correct style to ensure more accurate facial expressions.";

        public const string RefineStrengthDescription =
            "Selecting Refine Strength may alter the details of the original image. Select \"None\" to keep the original.";
        public const string UploadImageDescription =
            " Upload an external image to use. To maintain quality, the Input Image size is recommended to be at least 1024 x 1024px.";
    }


    public static class HelpBoxMessages
    {
        public const string EditSizeTip = " The Input Image size must be between 512 x 512px and 2048 x 2048px.";
        public const string TryEditInputOverSize = " The Input Image size does not fit the supported dimensions (512 x 512px to 2048 x 2048px). Please upload the image within the size limit.";
        public const string TryEditSizeInfo = " The Input Image size must be between 512 x 512px and 2048 x 2048px. (recommend : 1024 x 1024px)";
        public const string TryEditOutputOverSize = " The Input Image size does not fit the supported dimensions (512 x 512px to 2048 x 2048px). Please upload the image within the size limit.";


        public const string PoseBasedToolTip =
            "- Currently, providing a more detailed prompt increases the likelihood of generating a consistent character. - Future updates will allow precise pose adjustments.\n";


        public const string CanvasExtendImageDragDescription =
            "Sets the position of the Input image within the Output Image.";

        public static readonly string[] PartialRedrawBrushType =
        {
            "Place Pins to specify editable areas.\nAt least one + Pin must be placed for Selection.\nFor better accuracy, place three or more + Pins.\n<color=#39FF14>+ Pin</color>: Indicates areas to be modified. ( Max : 5 )\n<color=red>- Pin</color>: Indicates areas to be excluded from modifications. ( Max : 5 )",
            "Use the brush to specify which areas to edit.\nUse the eraser to remove selected areas.\nAdjust the size to change the brush and eraser thickness."
        };

        public const string SMPLConvertStatus = "A task is in progress. Unable to start a new task. Estimated time remaining:{0}";
    }

    public static class AlertMessages
    {
        public const string QueueEnough = "Unable to proceed due to another task currently in progress. Please try again once the current task is completed.";
        public const string PresetNull = "Please select a model before proceeding with the task.";
        public const string APIKeyNull = "The API key you entered is incorrect. Please check Window > GameAIFy > Settings and enter a valid key.";
        public const string APIKeyNotAvailable = "The API key is either missing or unavailable. Please check Window > GameAIFy > Settings.";
        public const string Saved = "All options have been successfully saved.";
        public const string Reset = "All API keys have been reset. Please enter a new API key.";
        public const string InputTextureNull = "No image has been uploaded. Please select or upload an image.";
        public const string CategoryNull = "No category has been selected. Please choose the appropriate category.";
        public const string PartialRedrawReset = "Switching modes will remove the current selections. Do you want to proceed?";
        public const string TryRemoveOriginHistory = "The selected image is the original image. Deleting it cannot be undone. Do you want to proceed?";
        public const string SegmentPinMax = "A maximum of {0} pins can be placed for each type.";
        public const string HistoryDelete = "If you delete the original, all history will be deleted.";
        public const string RestrictedGenerated = "Unable to proceed due to an inclusion of a restricted term.\n";
    }

    public static class ToolTipMessages
    {
        public const string UserImageUpload = "Upload an external image to use.";
        public const string Regenerate = "Retrieve the previous prompt and settings to generate again";
        public const string LoraStrength = "Adjusts how strongly the selected Preset’s concept is applied to the Output image.";
        public const string ReferenceImage = "Sets the reference image to guide the Output image.\nThe Reference Image size must be between 512 x 512px and 2048 x 2048px.";
        public const string ReferenceMode = "- Reference Mode : Uses the color distribution from the reference image in the Output image.\n- Openpose Mode : Uses the pose of a character from the reference image in the Output image.\n- Depth Mode : Uses the depth of the reference image in the Output image.";
        public const string ReferenceStrength = "Adjusts how strongly the uploaded Reference Image influences the Output image.";
    }
}