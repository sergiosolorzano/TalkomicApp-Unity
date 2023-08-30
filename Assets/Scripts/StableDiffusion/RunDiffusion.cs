using Microsoft.ML.OnnxRuntime;
using StableDiffusion;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;

public class RunDiffusion : MonoBehaviour
{
    /*[HideInInspector]
    public UnityEvent<bool> aiModelsLoadedCompleteCallback;*/

    // Number of denoising steps (input)
    [HideInInspector]
    public int steps = ChatomicManager.steps;
    // Scale for classifier-free guidance
    private float ClassifierFreeGuidanceScaleValue = ChatomicManager.ClassifierFreeGuidanceScaleValue;
    private const bool useLMS = false;

    private const int resolution = 512;

    private Texture2D skyboxTexture;

    public class Texture2DProperties
    {
        public int resolution = RunDiffusion.resolution;
        public TextureFormat texFormat = TextureFormat.RGBA32;
        public bool mipChainBool = false;
    }
    [HideInInspector]
    public string promptPainting;

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

        skyboxTexture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
    }

    string TrimChatGPTResponse(string paintingDescription, int maxWordCount)
    {
        int wordCount = paintingDescription.Split(new char[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;

        if (wordCount > maxWordCount)
        {
            int reduce = wordCount - maxWordCount;
            string[] words = paintingDescription.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            paintingDescription = string.Join(" ", words.Take(words.Length - reduce));
            Debug.Log("***** ChatGPT Response TRIMMED by " + reduce + " words. Final Prompt for Diffusion:"+paintingDescription);
        }

        return paintingDescription;
    }

    public async Task<bool> StartDiffusionProcess(string paintingDescription, int maxWordCount)
    {
        //Ensure no more than limit words are sent for processing by Difussion Process
        promptPainting =TrimChatGPTResponse(ChatomicManager.custom_diffuser_pre_prompt+paintingDescription,maxWordCount);
        Debug.Log("***RunDiffusion requested to Start Diffusion process:" + promptPainting);
        //promptPainting = paintingDescription;

        //create empty embeddings
        await Main.Create_Empty_UncondInput_and_TextEmbeddings(promptPainting);
        //generate painting
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

        Debug.Log("GeneratePainting Sentis Steps Before:" + steps);
        steps = ChatomicManager.steps;
        Debug.Log("GeneratePainting Sentis Steps After:" + steps);
        await StableDiffusion.Main.Run(promptPainting, steps, ClassifierFreeGuidanceScaleValue, seed, texture2DProperties, useLMS);

        return true;
    }
    
    void OnDisable()
    {
        StableDiffusion.Main.Free();
    }
}