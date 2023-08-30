using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StableDiffusion
{
    public class Main
    {
        private const int EmbeddingSize = 59136;    // 77 x 768

        private static float[] textPromptEmbeddings;

        private static int[] uncondInputTokens;
        private static float[] uncondEmbedding;

        private static DenseTensor<float> textEmbeddings;

        public static void Init(string unet, string textEncoder, string clipTokenizer, string extension, string vaeDecoder)
        {
            //Load UNet
            Unet.LoadModel(unet);
            //Load TextEncoder model
            TextEncoder.LoadModel(textEncoder);
            //Load VAE decoder
            VAE.LoadModel(vaeDecoder);
            //Load TextTokenizer Model - need this for sentis for now (can't tensor string to int64)
            TextTokenizer.LoadModel(clipTokenizer, extension);
        }

        public static async Task<bool> ClearUncondInput_and_TextEmbeddings()
        {
            //clear uncondInput uncondEmbeddings array and tensor
            if(uncondInputTokens!=null)
                Array.Clear(uncondInputTokens,0,uncondInputTokens.Length);
            if(uncondEmbedding!=null)
                Array.Clear(uncondEmbedding,0,uncondEmbedding.Length);
            if(textPromptEmbeddings!=null)
                Array.Clear(textPromptEmbeddings,0, textPromptEmbeddings.Length);
            
            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> Create_Empty_UncondInput_and_TextEmbeddings(string prompt)
        {
            // Create uncond_input of blank tokens
            uncondInputTokens = TextTokenizer.CreateUncondInput();
            uncondEmbedding = TextEncoder.Encode(uncondInputTokens).ToArray();

            // Concant textEmeddings and uncondEmbedding slice 0
            textEmbeddings = new DenseTensor<float>(new[] { 2, 77, 768 });
            for (int i = 0; i < EmbeddingSize; i++)
                textEmbeddings[0, i / 768, i % 768] = uncondEmbedding[i];

            //////Second Part of Embeddings////////
            // Load the tokenizer and text encoder to tokenize and encode the text.
            var textTokenized = TextTokenizer.TokenizeText(prompt);
            textPromptEmbeddings = TextEncoder.Encode(textTokenized).ToArray();

            //replace slice 1 textEmbeddings with the values of textPromptEmbeddings
            for (int i = 0; i < textPromptEmbeddings.Length; i++)
                textEmbeddings[1, i / 768, i % 768] = textPromptEmbeddings[i];

            await Task.Delay(1000);
            return true;
        }

        public static async Task<bool> Run(string prompt, int steps, float cfg, int seed, RunDiffusion.Texture2DProperties tex2DProperties, bool useLMS)
        {
            await Unet.Inference(steps, textEmbeddings, cfg, seed, tex2DProperties, useLMS);
            return true;
        }

        public static void Free()
        {
            Unet.Free();
            TextEncoder.Free();
            TextTokenizer.Free();
            VAE.Free();
        }
    }
}