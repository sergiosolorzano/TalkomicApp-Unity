using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;

namespace StableDiffusion
{
    public class TextTokenizer
    {
        private static InferenceSession textTokenizerInferenceSession;
        // Vars to Create an array of empty tokens for the unconditional input.
        private const int modelMaxLength = 77;
        private const int blankTokenValue = 49407;

        public static void LoadModel(string path, string extension)
        {
            var sessionOptions = new SessionOptions();
            sessionOptions.RegisterCustomOpLibraryV2(extension, out _);
            textTokenizerInferenceSession = new InferenceSession(path, sessionOptions);

            if (textTokenizerInferenceSession != null)
                UnityEngine.Debug.Log("textTokenizerInferenceSession Initiated.");
            else
                UnityEngine.Debug.Log("Failed to initiate textTokenizerInferenceSession.");
        }

        public static void OnDisableDispose() { 
            if(textTokenizerInferenceSession != null) { textTokenizerInferenceSession.Dispose(); }
            else
            {
                UnityEngine.Debug.Log("textTokenizerModel has been freed.");
            }
        }

        public static int[] TokenizeText(string text)
        {

            var inputTensor = new DenseTensor<string>(new string[] { text }, new int[] { 1 });
            var inputString = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor<string>("string_input", inputTensor) };
            
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> tokens = null;
            if(textTokenizerInferenceSession!=null)
                tokens = textTokenizerInferenceSession.Run(inputString);

            var inputIds = (tokens.ToList().First().Value as IEnumerable<long>).ToArray();

            var InputIdsInt = inputIds.Select(x => (int)x).ToArray();
            //UnityEngine.Debug.Log("TokenizeText InputIds:"+String.Join(" ", inputIds));

            if (InputIdsInt.Length < modelMaxLength)
            {
                var pad = Enumerable.Repeat(blankTokenValue, modelMaxLength - InputIdsInt.Length).ToArray();
                InputIdsInt = InputIdsInt.Concat(pad).ToArray();
            }

            return InputIdsInt;
        }

        public static int[] CreateUnconditionalInput()
        {
            var inputIds = new List<int>() { blankTokenValue-1 };

            var pad = Enumerable.Repeat(blankTokenValue, modelMaxLength - inputIds.Count()).ToArray();
            inputIds.AddRange(pad);

            return inputIds.ToArray();
        }
    }
}