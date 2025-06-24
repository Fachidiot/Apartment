using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace GameAIfySDK
{
    public class DTO
    {
    }
    
    public class ServerErrorMessage
    {
        public string status;
        public string message;
    }


    #region GenerateImage

    public class Lora
    {
        public string content;
        public float strength;
    }

    public class PromptData
    {
        public string prompt;
        public string negativePrompt;
        public List<Lora> loras;
    }

    [System.Serializable]
    public class BaseInfo
    {
        public int seq;
        public string output_image;
        public int height;
        public int width;
        public int preset_num;
        public string img_path;
        public int option_seq;
        public Texture2D texture; // S3로부터 다운 받은 후 저장
    }

    [System.Serializable]
    public class MyWorkDataResponse
    {
        public string status;
        public List<ImageInfo> data;
        public Meta meta;
    }

    [System.Serializable]
    public class Meta
    {
        public int? nextPage;
        public int? prevPage;
    }

    [System.Serializable]
    public class ImageInfo : BaseInfo
    {
        public string create_date;
        public int history_count;
        public string post_seq;
    }

    [System.Serializable]
    public class PostInfo : BaseInfo
    {
        public int usn;
        public string title;
        public string description;
        public int type;
        public List<string> keyword;
        public string link;
        public int image_num;
        public int like;
        public int report_seq;
        public int is_deleted;
        public string nick_name;
        public bool isLike;
    }


    [System.Serializable]
    public class PostDataResponse
    {
        public string status;
        public List<PostInfo> data;
        public Meta meta;
    }

    public class DepthMapGenerateDataResponse
    {
        public string status;
        public DepthMapGenerateData data;
    }

    public class DepthMapGenerateData
    {
        public int seq;
        public string imgPath;
        public string objPath;
    }

    public class ExpressionConfigData
    {
        public int seq;
        public int groupNum;
        public string groupName;
        public string keyword;
        public string denoise;
        public string prompt;
        public int enable;
        public string keywordName;
    }

    public class ExpressionConfigDataResponse
    {
        public string status;
        public List<ExpressionConfigData> data;
    }

    public class ExpressionChangeDataResponse
    {
        public string status;
        public ExpressionChangeData data;
    }

    public class ExpressionChangeData
    {        
        public string imageName;
        public string imgPath;
    }

    public class ImageDataResponse
    {
        public string status;
        public ImageData data;
    }
    
    public class ImageData
    {
        public int seq;
        public string imageName;
        public string imgPath;
        public int width;
        public int height;
    }

    
    [System.Serializable]
    public struct AspectRatioEntry
    {
        public AspectRatios ratioEnum;
        public Vector2 ratioValue; // (width, height)
        public string label;
    }
    public class ReferenceImageData
    {
        public Texture2D referenceImage;
        public string mode;
        public float strength;
    }
    public class InputPromptData
    {
        public ModelInfo modelInfo;
        public int width;
        public int height;
        public string inputPrompt;
        public string inputNegativePrompt;
        public int remixSeq;
        public int batchSize;
        public ReferenceImageData referenceImageData;
    }
    
    public class EasyCharacterMakerListResponse
    {
        public string status;
        public List<EasyCharacterMakerListData> data;
    }

    public class EasyCharacterMakerListData
    {
        public int seq;
        public string gender;
        public int gender_num;
        public string part;
        public int part_num;
        public string name;
        public string path;
    }

    public class EasyCharacterMakerResponse
    {
        public string status;
        public List<EasyCharacterMakerData> data;
    }

    public class EasyCharacterMakerData
    {
        public string imageName;
        public string imagePath;
    }

    public class EnhanceDataResponse
    {
        public string status;
        public EnhanceData data;
    }

    public class EnhanceData
    {
        public int seq;
        public string imageName;
        public string imgPath;
        public int width;
        public int height;
    }
    #endregion


    #region Models

    public class ModelDataResponse
    {
        public string status;
        public List<ModelInfo> list;
    }

    public class ModelInfo
    {
        public int seq;
        public int groupNum; //0. All / 1. Character / 2. BackGround / 3. UI / 4. Effect
        public int presetNum;
        public string enName;
        public string koName;
        public string ckpt;
        public string prompt;
        public string negativePrompt;
        
        [JsonConverter(typeof(JsonStringToListConverter<AspectRatio>))]
        public List<AspectRatio> aspectRatio;

        [JsonConverter(typeof(JsonStringToListConverter<Lora>))]
        public List<Lora> loras;

        [JsonConverter(typeof(JsonStringToListConverter<ControlNet>))]
        public List<ControlNet> controlNets;

        public string workflowFile;
        public int credit;

        [JsonConverter(typeof(JsonStringToListConverter<string>))]
        public List<string> examPrompt;

        public int category; //0. Standard / 1. Beta / 2. Private / 3. B2B
        public string preset_img_path;
        public int enable;

        [JsonConverter(typeof(JsonStringToListConverter<Sampler>))]
        public List<Sampler> sampler;

        public Texture2D texture; // S3로부터 다운 받은 후 저장
    }

    public class AspectRatio
    {
        public string ratio;
        public int width;
        public int height;
    }

    public class ControlNet
    {
    }

    public class Sampler
    {
        public string name;
        public int step;
        public double denoise;
    }


    public class JointCoordinate
    {
        public float x;
        public float y;
        public float z;
    }
    [System.Serializable]
    public class SMPLFrame
    {
        public List<JointCoordinate> joints;
    }

    [System.Serializable]
    public class SMPLData
    {
        public float frame_rate;
        public List<SMPLFrame> frames;
    }

    public class TextToMotionData
    {
        public int seq;
        public int usn;
        public int sec;
        public string prompt;
        public string img_path;
        public string thumbnail_path;
        public string json_path;
        public string smpl_path;
        public int is_smpl_made;      // 0. 없음, 1. 진행중, 9. 완료됨         
        public string imgPath;
        public string thumbnailPath;
        public string jsonPath;
        public string smplPath;

        // 클라 사용
        public Texture2D texture;
        public List<Texture2D> gif;
        public SMPLData smplData;
    }

    public class TextToMotionDataResponse
    {
        public string status;
        public List<TextToMotionData> data;
    }

    public class TextToDetailMotionDataResponse
    {
        public string status;
        public TextToMotionData data;
    }

    public class JointData
    {
        public int seq;
        public string imagePath;
        public string thumbnailPath;
    }

    public class JointDataResponse
    {
        public string status;
        public JointData data;
    }

    public class ConvertSMPLResponse
    {
        public string status;
        public string reaminTime;
    }

    #endregion


    #region history

    public class ImageInfoResponse
    {
        public string status;
        public ImageOption imageOption;
    }

    public class ImageOption
    {
        public int seq;
        public string uploadImage;
        public string prompt;
        public string negativePrompt;
        public string aspectRatio;

        [JsonConverter(typeof(JsonStringToListConverter<Lora>))]
        public List<Lora> loras;

        [JsonConverter(typeof(JsonStringToListConverter<ControlNet>))]
        public List<ControlNet> controlNets;

        public int preset_num;
    }

    public class HistoryResponse
    {
        public string status;
        public List<HistoryData> data;
    }

    public class HistoryData
    {
        public int seq;
        public string output_image;
        public int output_image_width;
        public int output_image_height;
        public string func_num;
        public string img_path;
        public Texture2D texture;
    }

    #endregion


    #region Edit

    [System.Serializable]
    public class ServerData
    {
        public string mask;
    }

    [System.Serializable]
    public class ServerResponse
    {
        public string status;
        public ServerData data;
    }

    #endregion

    #region PluginSettings

    [System.Serializable]
    public class USNResponse
    {
        public string status;
        public int usn;
        public string sessionId;
    }

    #endregion
}