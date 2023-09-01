using Microsoft.ML.OnnxRuntime;
using StableDiffusion;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

public class RunDiffusion : MonoBehaviour
{
    // Scale for classifier-free guidance
    private float ClassifierFreeGuidanceScaleValue = ChatomicManager.ClassifierFreeGuidanceScaleValue;
    private const bool useLMS = false;

    private const int resolution = 512;

    public class Texture2DProperties
    {
        public int resolution = RunDiffusion.resolution;
        public TextureFormat texFormat = TextureFormat.RGBA32;
        public bool mipChainBool = false;
    }
    [HideInInspector]
    public string promptImage;

    public void LoadModels()
    {
        //Load Models
        StableDiffusion.Main.Init(
            Application.dataPath + $"/Models/unet/model.onnx",
            Application.dataPath + $"/Models/text_encoder/text_encoder_model.onnx",
            Application.dataPath + $"/Models/tokenizer/cliptokenizer.onnx",
            Application.dataPath + $"/Plugins/ortextensions.dll",
            Application.dataPath + $"/Models/vae_decoder/vae_decoder.onnx"
        );
    }

    string TrimChatGPTResponse(string imageDescription, int maxWordCount)
    {
        int wordCount = imageDescription.Split(new char[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

        if (wordCount > maxWordCount)
        {
            int reduce = wordCount - maxWordCount;
            string[] words = imageDescription.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            imageDescription = string.Join(" ", words.Take(words.Length - reduce));
            Debug.Log("***** ChatGPT Response TRIMMED by " + reduce + " words. Final Prompt for Diffusion:"+imageDescription);
        }

        return imageDescription;
    }

    public async Task<bool> StartDiffusionProcess(string imageDescription, int maxWordCount)
    {
        //Ensure no more than limit words are sent for processing by Difussion Process
        promptImage =TrimChatGPTResponse(ChatomicManager.custom_diffuser_pre_prompt+imageDescription,maxWordCount);
        Debug.Log("***RunDiffusion requested to Start Diffusion process:" + promptImage);

        //create empty embeddings
        await Main.Create_Empty_UncondInput_and_TextEmbeddings(promptImage);
        //generate image
        await GenerateImage();
        return true;
    }

    public T[] LoadArrayFromFile<T>(string filePath)
    {
        if (File.Exists(filePath))
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T[])formatter.Deserialize(fileStream);
            }
        }

        return null;
    }

    public async Task<bool> GenerateImage()
    {
        var seed = UnityEngine.Random.Range(0, int.MaxValue);

        Texture2DProperties texture2DProperties = new Texture2DProperties();

        Debug.Log("***GenerateImage Sentis Steps Before:" + ChatomicManager.steps);
        Debug.Log("***GenerateImage Sentis Steps After:" + ChatomicManager.steps);
        await StableDiffusion.Main.Run(promptImage, ChatomicManager.steps, ClassifierFreeGuidanceScaleValue, seed, texture2DProperties, useLMS);

        return true;
    }
    
    void OnDisable()
    {
        StableDiffusion.Main.OnDisableDispose();
    }
}