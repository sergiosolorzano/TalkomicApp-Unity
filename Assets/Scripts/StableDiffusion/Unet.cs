using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace StableDiffusion
{
    public class Unet
    {
        public static UnityEvent<Texture2D> TextureCompletedCallback;

        private static InferenceSession unetEncoderInferenceSession;
        private static SchedulerBase scheduler;

        private const int batch_size = 1;
        private const int height = 512;
        private const int width = 512;

        private const float scale = 1.0f / 0.18215f;

        private static List<UnityEngine.Texture2D> listTex = new List<UnityEngine.Texture2D>();

        public static Texture2D tex;

        public static void LoadModel(string path)
        {
            //instantiate unity events
            TextureCompletedCallback = new UnityEvent<Texture2D>();
            
            //load models
            unetEncoderInferenceSession = new InferenceSession(path, Options());

            if (unetEncoderInferenceSession != null)
                UnityEngine.Debug.Log("UNet model Loaded.");
            else
                UnityEngine.Debug.Log("Failed to load UNet model.");
        }

        public static void OnDisableDispose() {
            
            Debug.Log("Destroying tex:" + listTex.Count);
            foreach (var tex in listTex)
                UnityEngine.Object.Destroy(tex);
            
            if (unetEncoderInferenceSession != null)
                unetEncoderInferenceSession.Dispose();
            else
                Debug.Log("unetEncoderInferenceSession was null.");
        }

        private static void WriteToFile(string text)
        {
            string filePath = "C:\\Temp\\output.txt";

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine(text);
            }
        }

        public static void SaveArrayToFile<T>(T[] array, string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, array);
            }
        }

        public static T[] LoadArrayFromFile<T>(string filePath)
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

        static Texture2D RotateTexture180(Texture2D original)
        {
            int width = original.width;
            int height = original.height;
            Texture2D rotated = new Texture2D(width, height);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    rotated.SetPixel(width - i - 1, height - j - 1, original.GetPixel(i, j));
                }
            }

            rotated.Apply();
            return rotated;
        }

        public static async Task<bool> Inference(int steps, DenseTensor<float> textEmbeddings, float cfg, int seed, RunDiffusion.Texture2DProperties tex2DProperties, bool useLMS)
        {
            scheduler = useLMS ? new LMSDiscreteScheduler() : new EulerAncestralDiscreteScheduler();
            var timesteps = scheduler.SetTimesteps(steps);
            //Debug.Log("Running steps " + steps);
            //UnityEngine.Debug.Log("SetTimeSteps Len:" + timesteps.Length);

            var latents = GenerateLatentSample(batch_size, seed, scheduler.InitNoiseSigma);

            var input = new List<NamedOnnxValue>();

            for (int t = 0; t < steps; t++)
            {
                // torch.cat([latents] * 2)
                var latentModelInput = TensorHelper.Duplicate(latents.ToArray(), new[] { 2, 4, height / 8, width / 8 });

                latentModelInput = scheduler.ScaleInput(latentModelInput, timesteps[t]);

                string samplesArrayTxt = string.Join(", ", latentModelInput.ToArray());
                string hiddenArrayTxt = string.Join(", ", textEmbeddings.ToArray());

                input = CreateUnetModelInput(textEmbeddings, latentModelInput, timesteps[t]);

                // Run Inference
                var output = unetEncoderInferenceSession.Run(input);
                var outputTensor = (output.ToList().First().Value as DenseTensor<float>);

                var splitTensors = TensorHelper.SplitTensor(outputTensor, new[] { 1, 4, height / 8, width / 8 });
                var noisePred = splitTensors.Item1;
                var noisePredText = splitTensors.Item2;

                noisePred = PerformGuidance(noisePred, noisePredText, cfg);

                latents = scheduler.Step(noisePred, timesteps[t], latents);
            }

            latents = TensorHelper.MultipleTensorByFloat(latents.ToArray(), scale, latents.Dimensions.ToArray());
            var decoderInput = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("latent_sample", latents) };

            tex = VAE.ConvertToImage(VAE.Decoder(decoderInput), width, height, tex2DProperties);
            tex = RotateTexture180(tex);
            listTex.Add(tex);
            //send texture to chatomic manager
            //Debug.Log("Invoke To Save")
            TextureCompletedCallback.Invoke(tex);

            return true;
        }

        public static Tensor<float> GenerateLatentSample(int batchSize, int seed, float initNoiseSigma)
        {
            UnityEngine.Random.InitState(seed);
            var channels = 4;
            var latents = new DenseTensor<float>(new[] { batchSize, channels, height / 8, width / 8 });
            var latentsArray = latents.ToArray();

            for (int i = 0; i < latentsArray.Length; i++)
            {
                // Generate a random number from a normal distribution with mean 0 and variance 1
                var u1 = UnityEngine.Random.Range(0.0f, 1.0f); // Uniform(0,1) random number
                //var u1 = 0.75f; // Uniform(0,1) random number
                var u2 = UnityEngine.Random.Range(0.0f, 1.0f); // Uniform(0,1) random number
                //var u2 = 0.55f; // Uniform(0,1) random number
                var radius = Mathf.Sqrt(-2.0f * Mathf.Log(u1)); // Radius of polar coordinates
                var theta = 2.0f * Mathf.PI * u2; // Angle of polar coordinates
                var standardNormalRand = radius * Mathf.Cos(theta); // Standard normal random number

                // add noise to latents with * scheduler.init_noise_sigma
                // generate randoms that are negative and positive
                latentsArray[i] = (float)standardNormalRand * initNoiseSigma;
            }

            latents = TensorHelper.CreateTensor(latentsArray, latents.Dimensions.ToArray());

            return latents;
        }

        public static List<NamedOnnxValue> CreateUnetModelInput(Tensor<float> encoderHiddenStates, Tensor<float> sample, long timeStep)
        {
            var input = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("encoder_hidden_states", encoderHiddenStates),
                NamedOnnxValue.CreateFromTensor("sample", sample),
                NamedOnnxValue.CreateFromTensor("timestep", new DenseTensor<long>(new long[] { timeStep }, new int[] { 1 }))
            };

            return input;
        }

        private static Tensor<float> PerformGuidance(Tensor<float> noisePred, Tensor<float> noisePredText, double guidanceScale)
        {
            Parallel.For(0, noisePred.Dimensions[0], i =>
            {
                for (int j = 0; j < noisePred.Dimensions[1]; j++)
                    for (int k = 0; k < noisePred.Dimensions[2]; k++)
                        for (int l = 0; l < noisePred.Dimensions[3]; l++)
                            noisePred[i, j, k, l] = noisePred[i, j, k, l] + (float)guidanceScale * (noisePredText[i, j, k, l] - noisePred[i, j, k, l]);
            });

            return noisePred;
        }

        private static SessionOptions Options()
        {
            var sessionOptions = new SessionOptions();

            try
            {
                sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                sessionOptions.AppendExecutionProvider_CUDA();
            }
            //catch
            //{
            //    sessionOptions.EnableMemoryPattern = false;
            //    sessionOptions.AppendExecutionProvider_DML();
            //}
            finally { sessionOptions.AppendExecutionProvider_CPU(); }

            return sessionOptions;
        }
    }
}