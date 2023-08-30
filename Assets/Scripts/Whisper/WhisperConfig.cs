using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace AudioNoteTranscription.Whisper
{
    public class WhisperConfig
    {
        public enum ExecutionProvider
        {
            DirectML = 0,
            Cuda = 1,
            Cpu = 2,
            Azure = 3
        }
        // default props
        public ExecutionProvider ExecutionProviderTarget = ExecutionProvider.Cpu;
        public int DeviceId = 0;

        public int min_length = 0;
        // Max length per inference
        public int max_length = 448;
        public float repetition_penalty = 1.0f;
        public int no_repeat_ngram_size = 3;
        public int num_beams = 1;
        public int num_return_sequences = 1;
        public float length_penalty = 1.0f;
        public Tensor<int> attention_mask;
        public DenseTensor<byte> audio;
        //public DenseTensor<float> audio;

        public int nFrames = 3000;
        public int hopLength = 160;
        public int sampleRate = 16000;
        public int nMels = 80;

        public string WhisperOnnxPath = "";
        public string TestAudioPath = "";

        public SessionOptions GetSessionOptionsForEp()
        {
            var sessionOptions = new SessionOptions();

            switch (ExecutionProviderTarget)
            {
                //TODO
                case ExecutionProvider.DirectML:
                    sessionOptions.GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL;
                    sessionOptions.EnableMemoryPattern = false;
                    sessionOptions.AppendExecutionProvider_DML(DeviceId);
                    sessionOptions.AppendExecutionProvider_CPU();
                    return sessionOptions;

                //TODO
                case ExecutionProvider.Azure:
                    sessionOptions.AddSessionConfigEntry("azure.endpoint_type", "openai");
                    sessionOptions.AddSessionConfigEntry("azure.uri", "https://api.openai.com/v1/audio/translations");
                    sessionOptions.AddSessionConfigEntry("azure.model_name", "whisper-1");
                    sessionOptions.AppendExecutionProvider("AZURE");
                    return sessionOptions;
                default:
                case ExecutionProvider.Cpu:
                    sessionOptions.AppendExecutionProvider_CPU();
                    return sessionOptions;
            }

        }
    }
}

