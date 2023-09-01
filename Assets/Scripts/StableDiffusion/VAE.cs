using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StableDiffusion
{
    public class VAE
    {
        private static InferenceSession vaeDecoderModel;

        public static void LoadModel(string path)
        {
            vaeDecoderModel = new InferenceSession(path);
            if (vaeDecoderModel != null)
                UnityEngine.Debug.Log("vaeDecoderModel Loaded.");
            else
                UnityEngine.Debug.Log("Failed to load vaeDecoderModel.");
        }

        public static void OnDisableDispose() { 
            if(vaeDecoderModel!= null) { vaeDecoderModel.Dispose(); }
        }

        public static Tensor<float> Decoder(List<NamedOnnxValue> input)
        {
            var output = vaeDecoderModel.Run(input);
            return (output.ToList().First().Value as Tensor<float>);
        }

        public static Texture2D ConvertToImage(Tensor<float> output, int width, int height, RunDiffusion.Texture2DProperties tex2DProperties)
        {
            Texture2D result = new Texture2D(tex2DProperties.resolution, tex2DProperties.resolution, tex2DProperties.texFormat, tex2DProperties.mipChainBool);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result.SetPixel(x, y, new Color(output[0, 0, y, x] / 2 + 0.5f, output[0, 1, y, x] / 2 + 0.5f, output[0, 2, y, x] / 2 + 0.5f, 1.0f));

            result.Apply();
            return result;
        }

        /*public static void ConvertToImage(ref Texture2D result, Tensor<float> output, int width, int height)
        {
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    result.SetPixel(x, y, new Color(output[0, 0, y, x] / 2 + 0.5f, output[0, 1, y, x] / 2 + 0.5f, output[0, 2, y, x] / 2 + 0.5f, 1.0f));

            result.Apply();
        }*/
    }
}