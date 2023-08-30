using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using NReco.VideoConverter;
using System.IO;
using System.Diagnostics;
using System.Net;
using AudioNoteTranscription.Model;
using System.Threading.Tasks;

namespace AudioNoteTranscription.Whisper
{
    public static class Inference
    {
        public static async Task<string> Start(string modelPath, string audioPath)
        {
            TranscriptionModel model = new TranscriptionModel();
            string result = await model.TranscribeAsync(modelPath, audioPath, false);
            return result;
        }

        public static List<NamedOnnxValue> CreateOnnxWhisperModelInput(WhisperConfig config)
        {

            var input = new List<NamedOnnxValue> {
                //choose stream or pcm
                 NamedOnnxValue.CreateFromTensor("audio_stream", config.audio),
                 //NamedOnnxValue.CreateFromTensor("audio_pcm", config.audio),
                NamedOnnxValue.CreateFromTensor("min_length", new DenseTensor<int>(new int[] {config.min_length}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("max_length", new DenseTensor<int>(new int[] {config.max_length}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_beams", new DenseTensor<int>(new int[] {config.num_beams}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("num_return_sequences", new DenseTensor<int>(new int[] {config.num_return_sequences}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("length_penalty", new DenseTensor<float>(new float[] {config.length_penalty}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("repetition_penalty", new DenseTensor<float>(new float[] {config.repetition_penalty}, new int[] { 1 })),
                NamedOnnxValue.CreateFromTensor("attention_mask", config.attention_mask)

            };

            return input;

        }
        public static string Run(WhisperConfig config, bool useCloudInference)
        {
            // load audio and pad/trim it to fit 30 seconds
            //choose stream byte or pcm float
            //float[] pcmAudioData = LoadAndProcessAudioFile(config.TestAudioPath, config.sampleRate);
            byte[] audioDataRaw = LoadAudioFileRaw(config.TestAudioPath);
            // Create audio data tensor of shape [1,480000]
            //config.audio = new DenseTensor<float>(pcmAudioData, new[] { 1, pcmAudioData.Length });
            config.audio = new DenseTensor<byte>(audioDataRaw, new[] { 1, audioDataRaw.Length });
            // Create tensor of zeros with shape [1,80,3000]
            config.attention_mask = new DenseTensor<int>(new[] { 1, config.nMels, config.nFrames });

            var input = CreateOnnxWhisperModelInput(config);

            //TODO
            // Check for internet connection
            // var isConnectivity = CheckForInternetConnection();

            // Run inference
            var run_options = new RunOptions();

            //TODO
            if (useCloudInference)
            {
                config.ExecutionProviderTarget = WhisperConfig.ExecutionProvider.Azure;
                run_options.AddRunConfigEntry("use_azure", "1");
                run_options.AddRunConfigEntry("azure.auth_key", "");
            }
            // Set EP
            var sessionOptions = config.GetSessionOptionsForEp();
            sessionOptions.RegisterOrtExtensions();
            sessionOptions.LogSeverityLevel = OrtLoggingLevel.ORT_LOGGING_LEVEL_ERROR;

            var session = new InferenceSession(config.WhisperOnnxPath, sessionOptions);

            List<string> outputs = new List<string>() { "str" };
            var result = session.Run(input, outputs, run_options);
            
            var stringOutput = (result.ToList().First().Value as IEnumerable<string>).ToArray();

            return stringOutput[0];
        }
        public static byte[] LoadAudioFileRaw(string file)
        {
            byte[] buff = null;
            FileStream fs = new FileStream(file,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            long numBytes = new FileInfo(file).Length;
            buff = br.ReadBytes((int)numBytes);
            return buff;
        }

        public static float[] LoadAndProcessAudioFile(string file, int sampleRate)
        {
            var ffmpeg = new FFMpegConverter();
            var output = new MemoryStream();

            var extension = Path.GetExtension(file).Substring(1);

            // Convert to PCM
            ffmpeg.ConvertMedia(inputFile: file,
                                inputFormat: extension,
                                outputStream: output,
                                //  DE s16le PCM signed 16-bit little-endian
                                outputFormat: "s16le",
                                new ConvertSettings()
                                {
                                    AudioCodec = "pcm_s16le",
                                    AudioSampleRate = sampleRate,
                                    // Convert to mono
                                    CustomOutputArgs = "-ac 1"
                                }); ;
            var buffer = output.ToArray();
            //The buffer length is divided by 2 because each sample in
            //the raw PCM format is encoded as a signed 16-bit integer,
            //which takes up 2 bytes of memory. Dividing the buffer
            //length by 2 gives the number of samples in the audio data.
            var result = new float[buffer.Length / 2];
            for (int i = 0; i < buffer.Length; i += 2)
            {
                short sample = (short)(buffer[i + 1] << 8 | buffer[i]);


                //The division by 32768 is used to normalize the audio data
                //to have values between -1.0 and 1.0.
                //The raw PCM format used by ffmpeg encodes audio samples
                //as signed 16-bit integers with a range from -32768
                //to 32767. Dividing by 32768 scales the samples to have
                //a range from -1.0 to 1.0 in floating-point format.
                result[i / 2] = sample / 32768.0f;
            }

            // Add padding to the audio file to make it 30 seconds long
            int paddingLength = sampleRate * 30 - result.Length;
            if (paddingLength > 0)
            {
                Array.Resize(ref result, result.Length + paddingLength);
            }
            else
            {
                //TODO: batch audio files that are longer than 30 seconds
                // Cut off anything over 30 seconds
                Array.Resize(ref result, 480000);
            }

            return result;
        }

        public static bool CheckForInternetConnection(int timeoutMs = 3000)
        {
            try
            {
                var url = "https://www.tapgaze.com/";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Timeout = timeoutMs;
                using (var response = (HttpWebResponse)request.GetResponse())
                    return true;
            }
            catch
            {
                return false;
            }
        }

    }
}
