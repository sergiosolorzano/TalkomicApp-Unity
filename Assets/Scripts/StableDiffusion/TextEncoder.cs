using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace StableDiffusion
{
    public class TextEncoder
    {
        private static InferenceSession textEncoderInferenceSession;

        public static void LoadModel(string path)
        {
            textEncoderInferenceSession = new InferenceSession(path);

            if (textEncoderInferenceSession != null)
                UnityEngine.Debug.Log("TextEncoder model Loaded.");
            else
                UnityEngine.Debug.Log("Failed to load TextEncoder model.");
        }

        public static void OnDisableDispose()
        {
            if (textEncoderInferenceSession != null) 
            { textEncoderInferenceSession.Dispose(); }
            else
            {
                Debug.Log("textEncoderInferenceSession was null.");
            }
        }

        public static DenseTensor<float> Encode(int[] tokenizedInput)
        {
            var input_ids = TensorHelper.CreateTensor(tokenizedInput, new[] { 1, tokenizedInput.Count() });

            var input = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor<int>("input_ids", input_ids) };

            var encoded = textEncoderInferenceSession.Run(input);

            var lastHiddenState = (encoded.ToList().First().Value as IEnumerable<float>).ToArray();
            var lastHiddenStateTensor = TensorHelper.CreateTensor(lastHiddenState.ToArray(), new[] { 1, 77, 768 });

            return lastHiddenStateTensor;
        }
    }
}